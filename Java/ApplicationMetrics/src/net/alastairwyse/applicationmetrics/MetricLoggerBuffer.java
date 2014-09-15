/*
 * Copyright 2014 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package net.alastairwyse.applicationmetrics;

import java.lang.Thread.*;
import java.util.*;
import java.util.concurrent.CountDownLatch;

import net.alastairwyse.operatingsystemabstraction.*;

/**
 * Base class which acts as a buffer for implementations of interface IMetricLogger.  Stores logged metrics events in queues, so as to minimise the time taken to call the logging methods.
 * Derived classes must implement methods which process the buffered metric events (e.g. ProcessIntervalMetricEvent()).  These methods are called from a worker thread after dequeueing the stored metric events.
 * @author Alastair Wyse
 */
abstract class MetricLoggerBuffer implements IMetricLogger {

    // Constants
    /** The string abbreviation representing time zone used when storing and logging time stamps, and when calculating time intervals (e.g. for an interval metric). */
    protected final String timeZoneId = "UTC";
    
    // Queue objects 
    /** Queue used to buffer count metrics. */
    protected LinkedList<CountMetricEventInstance> countMetricEventQueue;
    /** Queue used to buffer amount metrics. */
    protected LinkedList<AmountMetricEventInstance> amountMetricEventQueue;
    /** Queue used to buffer status metrics. */
    protected LinkedList<StatusMetricEventInstance> statusMetricEventQueue;
    /** Queue used to buffer interval metrics. */
    protected LinkedList<IntervalMetricEventInstance> intervalMetricEventQueue;
    
    // Lock objects for queues
    /** Lock object which should be set before dequeuing from queue countMetricEventQueue. */
    protected Object countMetricEventQueueLock;
    /** Lock object which should be set before dequeuing from queue amountMetricEventQueue. */
    protected Object amountMetricEventQueueLock;
    /** Lock object which should be set before dequeuing from queue statusMetricEventQueue. */
    protected Object statusMetricEventQueueLock;
    /** Lock object which should be set before dequeuing from queue intervalMetricEventQueue. */
    protected Object intervalMetricEventQueueLock;
    
    /** Object which provides the current date and time. */
    protected ICalendarProvider calendarProvider;
    /** Handler for any uncaught exceptions occurring on the worker thread. */
    protected UncaughtExceptionHandler exceptionHandler;

    // HashMap object to temporarily store the start instance of any received interval metrics
    private HashMap<Class<?>, IntervalMetricEventInstance> startIntervalMetricEventStore;
    private Thread dequeueOperationThread;
    private int dequeueOperationLoopInterval;
    private volatile boolean cancelRequest;
    private boolean testConstructor = false;
    // CountDownLatch object used to signal client test code that an iteration of the loop in the dequeue operation thread has completed
    private CountDownLatch dequeueOperationLoopCompleteSignal;
    private boolean intervalMetricChecking;

    /**
     * Initialises a new instance of the MetricLoggerBuffer class.
     * @param dequeueOperationLoopInterval  The time to wait in between iterations of the worker thread which dequeues and processes metric events.
     * @param intervalMetricChecking        Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).
     * @param exceptionHandler              Handler for any uncaught exceptions occurring on the worker thread.
     */
    protected MetricLoggerBuffer(int dequeueOperationLoopInterval, boolean intervalMetricChecking, UncaughtExceptionHandler exceptionHandler) {
        if (dequeueOperationLoopInterval >= 0){
            this.dequeueOperationLoopInterval = dequeueOperationLoopInterval;
        }
        else {
            throw new IllegalArgumentException("Argument 'dequeueOperationLoopInterval' must be greater than or equal to 0.");
        }

        this.intervalMetricChecking = intervalMetricChecking;

        countMetricEventQueue = new LinkedList<CountMetricEventInstance>();
        amountMetricEventQueue = new LinkedList<AmountMetricEventInstance>();
        statusMetricEventQueue = new LinkedList<StatusMetricEventInstance>();
        intervalMetricEventQueue = new LinkedList<IntervalMetricEventInstance>();
        countMetricEventQueueLock = new Object();
        amountMetricEventQueueLock = new Object();
        statusMetricEventQueueLock = new Object();
        intervalMetricEventQueueLock = new Object();

        startIntervalMetricEventStore = new HashMap<Class<?>, IntervalMetricEventInstance>();
        calendarProvider = new CalendarProvider();
        this.exceptionHandler = exceptionHandler;
    }

    /**
     * Initialises a new instance of the ApplicationMetrics.MetricLoggerBuffer class.  
     * <b>Note</b> this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
     * @param dequeueOperationLoopInterval        The time to wait in between iterations of the worker thread which dequeues and processes metric events.
     * @param intervalMetricChecking              Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).
     * @param exceptionHandler                    Handler for any uncaught exceptions occurring on the worker thread.
     * @param calendarProvider                    A test (mock) ICalendarProvider object.
     * @param dequeueOperationLoopCompleteSignal  Notifies test code that an iteration of the worker thread which dequeues and processes metric events has completed.
     */
    protected MetricLoggerBuffer(int dequeueOperationLoopInterval, boolean intervalMetricChecking, UncaughtExceptionHandler exceptionHandler, ICalendarProvider calendarProvider, CountDownLatch dequeueOperationLoopCompleteSignal) {
        this(dequeueOperationLoopInterval, intervalMetricChecking, exceptionHandler);
        this.calendarProvider = calendarProvider;
        testConstructor = true;
        this.dequeueOperationLoopCompleteSignal = dequeueOperationLoopCompleteSignal;
    }

    /**
     * Starts a worker thread which calls methods to dequeue and process metric events, at an interval specified by constructor parameter 'dequeueOperationLoopInterval'.
     */
    public void Start() {
        cancelRequest = false;

        dequeueOperationThread = new Thread(new DequeueOperationHandler());
        dequeueOperationThread.setName("ApplicationMetrics.MetricLoggerBuffer metric event dequeue amd process worker thread.");
        dequeueOperationThread.setUncaughtExceptionHandler(exceptionHandler);
        dequeueOperationThread.start();
    }
    
    /**
     * Stops the thread which dequeues metric events from the internal buffers.
     */
    public void Stop() throws InterruptedException  {
        cancelRequest = true;
        if(dequeueOperationThread != null) {
            dequeueOperationThread.join();
        }
    }

    @Override
    public void Increment(CountMetric countMetric) {
        synchronized (countMetricEventQueueLock) {
            countMetricEventQueue.addLast(new CountMetricEventInstance(countMetric, calendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId))));
        }
    }

    @Override
    public void Add(AmountMetric amountMetric) {
        synchronized (amountMetricEventQueueLock) {
            amountMetricEventQueue.addLast(new AmountMetricEventInstance(amountMetric, calendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId))));
        }
    }

    @Override
    public void Set(StatusMetric statusMetric) {
        synchronized (statusMetricEventQueueLock) {
            statusMetricEventQueue.addLast(new StatusMetricEventInstance(statusMetric, calendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId))));
        }
    }

    @Override
    public void Begin(IntervalMetric intervalMetric) {
        synchronized (intervalMetricEventQueueLock) {
            intervalMetricEventQueue.addLast(new IntervalMetricEventInstance(intervalMetric, IntervalMetricEventTimePoint.Start, calendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId))));
        }
    }

    @Override
    public void End(IntervalMetric intervalMetric) {
        synchronized (intervalMetricEventQueueLock) {
            intervalMetricEventQueue.addLast(new IntervalMetricEventInstance(intervalMetric, IntervalMetricEventTimePoint.End, calendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId))));
        }
    }

    /**
     * Processes a logged count metric event.
     * Implementations of this method define how a count metric event should be processed after it has been retrieved from the internal buffer queue.  The event could for example be written to a database, or to the console.
     * @param   countMetricEvent  The count metric event to process.
     * @throws  Exception         if an error occurs processing the metric event.
     */
    protected abstract void ProcessCountMetricEvent(CountMetricEventInstance countMetricEvent) throws Exception;

    /**
     * Processes a logged amount metric event.
     * Implementations of this method define how an amount metric event should be processed after it has been retrieved from the internal buffer queue.  The event could for example be written to a database, or to the console.
     * @param   amountMetricEvent  The amount metric event to process.
     * @throws  Exception          if an error occurs processing the metric event.
     */
    protected abstract void ProcessAmountMetricEvent(AmountMetricEventInstance amountMetricEvent) throws Exception;

    /**
     * Processes a logged status metric event.
     * Implementations of this method define how a status metric event should be processed after it has been retrieved from the internal buffer queue.  The event could for example be written to a database, or to the console.
     * @param   statusMetricEvent  The status metric event to process.
     * @throws  Exception          if an error occurs processing the metric event.
     */
    protected abstract void ProcessStatusMetricEvent(StatusMetricEventInstance statusMetricEvent) throws Exception;

    /**
     * Processes a logged interval metric event.
     * Implementations of this method define how an interval metric event should be processed after it has been retrieved from the internal buffer queue.  The event could for example be written to a database, or to the console.
     * @param   intervalMetricEvent  The interval metric event to process.
     * @param   duration             The duration of the interval metric event in milliseconds.
     * @throws  Exception            if an error occurs processing the metric event.
     */
    protected abstract void ProcessIntervalMetricEvent(IntervalMetricEventInstance intervalMetricEvent, long duration) throws Exception;

    /**
     * Dequeues and processes metric events stored in the internal buffer.
     */
    protected void DequeueAndProcessMetricEvents() throws Exception {
        DequeueAndProcessCountMetricEvents();
        DequeueAndProcessAmountMetricEvents();
        DequeueAndProcessStatusMetricEvents();
        DequeueAndProcessIntervalMetricEvents();
    }

    /**
     * Dequeues count metric events stored in the internal buffer and calls abstract method ProcessCountMetricEvent() to process them.
     */
    private void DequeueAndProcessCountMetricEvents() throws Exception {
        LinkedList<CountMetricEventInstance> tempQueue;

        // Lock the count metric queue and move all items to the temporary queue
        synchronized (countMetricEventQueueLock) {
            tempQueue = new LinkedList<CountMetricEventInstance>(countMetricEventQueue);
            countMetricEventQueue.clear();
        }

        // Process all items in the temporary queue
        while (tempQueue.size() > 0) {
            CountMetricEventInstance currentCountMetricEvent = tempQueue.removeFirst();
            ProcessCountMetricEvent(currentCountMetricEvent);
        }
    }

    /**
     * Dequeues amount metric events stored in the internal buffer and calls abstract method ProcessAmountMetricEvent() to process them.
     */
    private void DequeueAndProcessAmountMetricEvents() throws Exception {
        LinkedList<AmountMetricEventInstance> tempQueue;

        // Lock the amount metric queue and move all items to the temporary queue
        synchronized (amountMetricEventQueueLock) {
            tempQueue = new LinkedList<AmountMetricEventInstance>(amountMetricEventQueue);
            amountMetricEventQueue.clear();
        }

        // Process all items in the temporary queue
        while (tempQueue.size() > 0) {
            ProcessAmountMetricEvent(tempQueue.removeFirst());
        }
    }

    /**
     * Dequeues status metric events stored in the internal buffer and calls abstract method ProcessStatusMetricEvent() to process them.
     */
    private void DequeueAndProcessStatusMetricEvents() throws Exception {
        LinkedList<StatusMetricEventInstance> tempQueue;

        // Lock the status metric queue and move all items to the temporary queue
        synchronized (statusMetricEventQueueLock) {
            tempQueue = new LinkedList<StatusMetricEventInstance>(statusMetricEventQueue);
            statusMetricEventQueue.clear();
        }

        // Process all items in the temporary queue
        while (tempQueue.size() > 0) {
            ProcessStatusMetricEvent(tempQueue.removeFirst());
        }
    }

    /**
     * Dequeues interval metric events stored in the internal buffer and calls abstract method ProcessIntervalMetricEvent() to process them.
     */
    private void DequeueAndProcessIntervalMetricEvents() throws Exception {
        LinkedList<IntervalMetricEventInstance> tempQueue;

        // Lock the interval metric queue and move all items to the temporary queue
        synchronized (intervalMetricEventQueueLock) {
            tempQueue = new LinkedList<IntervalMetricEventInstance>(intervalMetricEventQueue);
            intervalMetricEventQueue.clear();
        }

        // Process all items in the temporary queue
        while (tempQueue.size() > 0) {
            IntervalMetricEventInstance currentIntervalMetricEvent = tempQueue.removeFirst();

            // If the current interval metric represents the start of the interval, put it in the dictionary object 
            if (currentIntervalMetricEvent.getTimePoint() == IntervalMetricEventTimePoint.Start) {
                if (startIntervalMetricEventStore.containsKey(currentIntervalMetricEvent.getMetricType()) == true) {
                    // If a start interval event of this type was already received and checking is enabled, throw an exception
                    if (intervalMetricChecking == true) {
                        throw new Exception("Received duplicate begin '" + currentIntervalMetricEvent.getMetric().getName() + "' metrics.");
                    }
                    // If checking is not enabled, replace the currently stored begin interval event with the new one
                    else {
                        startIntervalMetricEventStore.remove(currentIntervalMetricEvent.getMetricType());
                        startIntervalMetricEventStore.put(currentIntervalMetricEvent.getMetricType(), currentIntervalMetricEvent);
                    }
                }
                else {
                    startIntervalMetricEventStore.put(currentIntervalMetricEvent.getMetricType(), currentIntervalMetricEvent);
                }
            }
            // If the current interval metric represents the end of the interval, call the method to process it
            else {
                if (startIntervalMetricEventStore.containsKey(currentIntervalMetricEvent.getMetricType()) == true) {
                    long intervalDuration = currentIntervalMetricEvent.getEventTime().getTimeInMillis() - startIntervalMetricEventStore.get(currentIntervalMetricEvent.getMetricType()).getEventTime().getTimeInMillis();
                    // If the duration is less then 0 set back to 0, as the start time could be after the end time in the case the metric event occurred across a system time update
                    if (intervalDuration < 0) {
                        intervalDuration = 0;
                    }
                    
                    ProcessIntervalMetricEvent(startIntervalMetricEventStore.get(currentIntervalMetricEvent.getMetricType()), intervalDuration);

                    startIntervalMetricEventStore.remove(currentIntervalMetricEvent.getMetricType());
                }
                else {
                    // If no corresponding start interval event of this type exists and checking is enabled, throw an exception
                    if (intervalMetricChecking == true) {
                        throw new Exception("Received end '" + currentIntervalMetricEvent.getMetric().getName() + "' with no corresponding start interval metric.");
                    }
                    // If checking is not enabled discard the interval event
                }
            }
        }
    }
    
    private class DequeueOperationHandler implements Runnable {
        
        public DequeueOperationHandler() {
        }
        
        @Override
        public void run() {
            try {
                while (cancelRequest == false) {
                    DequeueAndProcessMetricEvents();
                    if (dequeueOperationLoopInterval > 0) {
                        Thread.sleep(dequeueOperationLoopInterval);
                    }
                    // If the code is being tested, allow only a single iteration of the loop
                    if (testConstructor == true) {
                        dequeueOperationLoopCompleteSignal.countDown();
                        break;
                    }
                }
            }
            catch(Exception e) {
                throw new RuntimeException("Exception in DequeueOperationHandler thread.", e);
            }
        }
    }
}

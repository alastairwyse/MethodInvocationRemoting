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

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OperatingSystemAbstraction;

namespace ApplicationMetrics
{
    //******************************************************************************
    //
    // Class: MetricLoggerBuffer
    //
    //******************************************************************************
    /// <summary>
    /// Base class which acts as a buffer for implementations of interface IMetricLogger.  Stores logged metrics events in queues, so as to minimise the time taken to call the logging methods.
    /// </summary>
    /// <remarks>Derived classes must implement methods which process the buffered metric events (e.g. ProcessIntervalMetricEvent()).  These methods are called from a worker thread after dequeueing the buffered metric events.</remarks>
    abstract class MetricLoggerBuffer : IMetricLogger
    {
        // Queue objects 
        /// <summary>Queue used to buffer count metrics.</summary>
        protected Queue<CountMetricEventInstance> countMetricEventQueue;
        /// <summary>Queue used to buffer amount metrics.</summary>
        protected Queue<AmountMetricEventInstance> amountMetricEventQueue;
        /// <summary>Queue used to buffer status metrics.</summary>
        protected Queue<StatusMetricEventInstance> statusMetricEventQueue;
        /// <summary>Queue used to buffer interval metrics.</summary>
        protected Queue<IntervalMetricEventInstance> intervalMetricEventQueue;

        // Lock objects for queues
        /// <summary>Lock object which should be set before dequeuing from queue countMetricEventQueue.</summary>
        protected object countMetricEventQueueLock;
        /// <summary>Lock object which should be set before dequeuing from queue amountMetricEventQueue.</summary>
        protected object amountMetricEventQueueLock;
        /// <summary>Lock object which should be set before dequeuing from queue statusMetricEventQueue.</summary>
        protected object statusMetricEventQueueLock;
        /// <summary>Lock object which should be set before dequeuing from queue intervalMetricEventQueue.</summary>
        protected object intervalMetricEventQueueLock;

        /// <summary>Object which provides the current date and time.</summary>
        protected IDateTime dateTime;
        /// <summary>Object handles any exceptions.  Allows easier unit testing by pushing exceptions to the IExceptionHandler interface.</summary>
        protected IExceptionHandler exceptionHandler;

        // Dictionary object to temporarily store the start instance of any received interval metrics
        private Dictionary<Type, IntervalMetricEventInstance> startIntervalMetricEventStore;
        private Thread dequeueOperationThread;
        private int dequeueOperationLoopInterval;
        private volatile bool cancelRequest;
        private bool testConstructor = false;
        private bool intervalMetricChecking;

        //------------------------------------------------------------------------------
        //
        // Method: MetricLoggerBuffer (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MetricLoggerBuffer class.
        /// </summary>
        /// <param name="dequeueOperationLoopInterval">The time to wait in between iterations of the worker thread which dequeues and processes metric events.</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        protected MetricLoggerBuffer(int dequeueOperationLoopInterval, bool intervalMetricChecking)
        {
            if (dequeueOperationLoopInterval >= 0)
            {
                this.dequeueOperationLoopInterval = dequeueOperationLoopInterval;
            }
            else
            {
                throw new ArgumentOutOfRangeException("dequeueOperationLoopInterval", dequeueOperationLoopInterval, "Argument 'dequeueOperationLoopInterval' must be greater than or equal to 0.");
            }

            this.intervalMetricChecking = intervalMetricChecking;

            countMetricEventQueue = new Queue<CountMetricEventInstance>();
            amountMetricEventQueue = new Queue<AmountMetricEventInstance>();
            statusMetricEventQueue = new Queue<StatusMetricEventInstance>();
            intervalMetricEventQueue = new Queue<IntervalMetricEventInstance>();
            countMetricEventQueueLock = new object();
            amountMetricEventQueueLock = new object();
            statusMetricEventQueueLock = new object();
            intervalMetricEventQueueLock = new object();

            startIntervalMetricEventStore = new Dictionary<Type, IntervalMetricEventInstance>();
            dateTime = new OperatingSystemAbstraction.DateTime();
            exceptionHandler = new ExceptionThrower();
        }

        //------------------------------------------------------------------------------
        //
        // Method: MetricLoggerBuffer (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MetricLoggerBuffer class.  Note this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
        /// </summary>
        /// <param name="dequeueOperationLoopInterval">The time to wait in between iterations of the worker thread which dequeues and processes metric events.</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        /// <param name="dateTime">A test (mock) DateTime object.</param>
        /// <param name="exceptionHandler">A test (mock) exception handler object.</param>
        protected MetricLoggerBuffer(int dequeueOperationLoopInterval, bool intervalMetricChecking, IDateTime dateTime, IExceptionHandler exceptionHandler)
            : this(dequeueOperationLoopInterval, intervalMetricChecking)
        {
            this.dateTime = dateTime;
            this.exceptionHandler = exceptionHandler;
            testConstructor = true;
        }

        //------------------------------------------------------------------------------
        //
        // Method: Start
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Starts a worker thread which calls methods to dequeue and process metric events, at an interval specified by constructor parameter 'dequeueOperationLoopInterval'.
        /// </summary>
        public virtual void Start()
        {
            cancelRequest = false;

            dequeueOperationThread = new Thread(delegate()
            {
                while (cancelRequest == false)
                {
                    DequeueAndProcessMetricEvents();
                    if (dequeueOperationLoopInterval > 0)
                    {
                        Thread.Sleep(dequeueOperationLoopInterval);
                    }
                    // If the code is being tested, allow only a single iteration of the loop
                    if (testConstructor == true)
                    {
                        break;
                    }
                }
            });
            dequeueOperationThread.Name = "ApplicationMetrics.MetricLoggerBuffer metric event dequeue amd process worker thread.";
            dequeueOperationThread.Start();
        }

        //------------------------------------------------------------------------------
        //
        // Method: Stop
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Stops the thread which dequeues metric events from the internal buffers.
        /// </summary>
        public virtual void Stop()
        {
            cancelRequest = true;
            if (dequeueOperationThread != null)
            {
                dequeueOperationThread.Join();
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricLogger.Increment(ApplicationMetrics.CountMetric)"]/*'/>
        public void Increment(CountMetric countMetric)
        {
            lock (countMetricEventQueueLock)
            {
                countMetricEventQueue.Enqueue(new CountMetricEventInstance(countMetric, dateTime.UtcNow));
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricLogger.Add(ApplicationMetrics.AmountMetric)"]/*'/>
        public void Add(AmountMetric amountMetric)
        {
            lock (amountMetricEventQueueLock)
            {
                amountMetricEventQueue.Enqueue(new AmountMetricEventInstance(amountMetric, dateTime.UtcNow));
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricLogger.Set(ApplicationMetrics.StatusMetric)"]/*'/>
        public void Set(StatusMetric statusMetric)
        {
            lock (statusMetricEventQueueLock)
            {
                statusMetricEventQueue.Enqueue(new StatusMetricEventInstance(statusMetric, dateTime.UtcNow));
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricLogger.Begin(ApplicationMetrics.IntervalMetric)"]/*'/>
        public void Begin(IntervalMetric intervalMetric)
        {
            lock (intervalMetricEventQueueLock)
            {
                intervalMetricEventQueue.Enqueue(new IntervalMetricEventInstance(intervalMetric, IntervalMetricEventTimePoint.Start, dateTime.UtcNow));
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricLogger.End(ApplicationMetrics.IntervalMetric)"]/*'/>
        public void End(IntervalMetric intervalMetric)
        {
            lock (intervalMetricEventQueueLock)
            {
                intervalMetricEventQueue.Enqueue(new IntervalMetricEventInstance(intervalMetric, IntervalMetricEventTimePoint.End, dateTime.UtcNow));
            }
        }

        #region Abstract Methods

        //------------------------------------------------------------------------------
        //
        // Method: ProcessCountMetricEvent
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Processes a logged count metric event.
        /// </summary>
        /// <param name="countMetricEvent">The count metric event to process.</param>
        /// <remarks>Implementations of this method define how a count metric event should be processed after it has been retrieved from the internal buffer queue.  The event could for example be written to a database, or to the console.</remarks>
        protected abstract void ProcessCountMetricEvent(CountMetricEventInstance countMetricEvent);

        //------------------------------------------------------------------------------
        //
        // Method: ProcessAmountMetricEvent
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Processes a logged amount metric event.
        /// </summary>
        /// <param name="amountMetricEvent">The amount metric event to process.</param>
        /// <remarks>Implementations of this method define how an amount metric event should be processed after it has been retrieved from the internal buffer queue.  The event could for example be written to a database, or to the console.</remarks>
        protected abstract void ProcessAmountMetricEvent(AmountMetricEventInstance amountMetricEvent);

        //------------------------------------------------------------------------------
        //
        // Method: ProcessStatusMetricEvent
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Processes a logged status metric event.
        /// </summary>
        /// <param name="statusMetricEvent">The status metric event to process.</param>
        /// <remarks>Implementations of this method define how a status metric event should be processed after it has been retrieved from the internal buffer queue.  The event could for example be written to a database, or to the console.</remarks>
        protected abstract void ProcessStatusMetricEvent(StatusMetricEventInstance statusMetricEvent);

        //------------------------------------------------------------------------------
        //
        // Method: ProcessIntervalMetricEvent
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Processes a logged interval metric event.
        /// </summary>
        /// <param name="intervalMetricEvent">The interval metric event to process.</param>
        /// <param name="duration">The duration of the interval metric event in milliseconds.</param>
        /// <remarks>Implementations of this method define how an interval metric event should be processed after it has been retrieved from the internal buffer queue.  The event could for example be written to a database, or to the console.</remarks>
        protected abstract void ProcessIntervalMetricEvent(IntervalMetricEventInstance intervalMetricEvent, long duration);

        #endregion

        #region Private Methods

        //------------------------------------------------------------------------------
        //
        // Method: DequeueAndProcessMetricEvents
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Dequeues and processes metric events stored in the internal buffer.
        /// </summary>
        protected virtual void DequeueAndProcessMetricEvents()
        {
            DequeueAndProcessCountMetricEvents();
            DequeueAndProcessAmountMetricEvents();
            DequeueAndProcessStatusMetricEvents();
            DequeueAndProcessIntervalMetricEvents();
        }

        //------------------------------------------------------------------------------
        //
        // Method: DequeueAndProcessCountMetricEvents
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Dequeues count metric events stored in the internal buffer and calls abstract method ProcessCountMetricEvent() to process them.
        /// </summary>
        private void DequeueAndProcessCountMetricEvents()
        {
            Queue<CountMetricEventInstance> tempQueue;

            // Lock the count metric queue and move all items to the temporary queue
            lock (countMetricEventQueueLock)
            {
                tempQueue = new Queue<CountMetricEventInstance>(countMetricEventQueue);
                countMetricEventQueue.Clear();
            }

            // Process all items in the temporary queue
            while (tempQueue.Count > 0)
            {
                CountMetricEventInstance currentCountMetricEvent = tempQueue.Dequeue();
                ProcessCountMetricEvent(currentCountMetricEvent);
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: DequeueAndProcessAmountMetricEvents
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Dequeues amount metric events stored in the internal buffer and calls abstract method ProcessAmountMetricEvent() to process them.
        /// </summary>
        private void DequeueAndProcessAmountMetricEvents()
        {
            Queue<AmountMetricEventInstance> tempQueue;

            // Lock the amount metric queue and move all items to the temporary queue
            lock (amountMetricEventQueueLock)
            {
                tempQueue = new Queue<AmountMetricEventInstance>(amountMetricEventQueue);
                amountMetricEventQueue.Clear();
            }

            // Process all items in the temporary queue
            while (tempQueue.Count > 0)
            {
                ProcessAmountMetricEvent(tempQueue.Dequeue());
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: DequeueAndProcessStatusMetricEvents
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Dequeues status metric events stored in the internal buffer and calls abstract method ProcessStatusMetricEvent() to process them.
        /// </summary>
        private void DequeueAndProcessStatusMetricEvents()
        {
            Queue<StatusMetricEventInstance> tempQueue;

            // Lock the status metric queue and move all items to the temporary queue
            lock (statusMetricEventQueueLock)
            {
                tempQueue = new Queue<StatusMetricEventInstance>(statusMetricEventQueue);
                statusMetricEventQueue.Clear();
            }

            // Process all items in the temporary queue
            while (tempQueue.Count > 0)
            {
                ProcessStatusMetricEvent(tempQueue.Dequeue());
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: DequeueAndProcessIntervalMetricEvents
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Dequeues interval metric events stored in the internal buffer and calls abstract method ProcessIntervalMetricEvent() to process them.
        /// </summary>
        private void DequeueAndProcessIntervalMetricEvents()
        {
            Queue<IntervalMetricEventInstance> tempQueue;

            // Lock the interval metric queue and move all items to the temporary queue
            lock (intervalMetricEventQueueLock)
            {
                tempQueue = new Queue<IntervalMetricEventInstance>(intervalMetricEventQueue);
                intervalMetricEventQueue.Clear();
            }

            // Process all items in the temporary queue
            while (tempQueue.Count > 0)
            {
                IntervalMetricEventInstance currentIntervalMetricEvent = tempQueue.Dequeue();

                // If the current interval metric represents the start of the interval, put it in the dictionary object 
                if (currentIntervalMetricEvent.TimePoint == IntervalMetricEventTimePoint.Start)
                {
                    if (startIntervalMetricEventStore.ContainsKey(currentIntervalMetricEvent.MetricType) == true)
                    {
                        // If a start interval event of this type was already received and checking is enabled, throw an exception
                        if (intervalMetricChecking == true)
                        {
                            exceptionHandler.Handle(new Exception("Received duplicate begin '" + currentIntervalMetricEvent.Metric.Name + "' metrics."));
                        }
                        // If checking is not enabled, replace the currently stored begin interval event with the new one
                        else
                        {
                            startIntervalMetricEventStore.Remove(currentIntervalMetricEvent.MetricType);
                            startIntervalMetricEventStore.Add(currentIntervalMetricEvent.MetricType, currentIntervalMetricEvent);
                        }
                    }
                    else
                    {
                        startIntervalMetricEventStore.Add(currentIntervalMetricEvent.MetricType, currentIntervalMetricEvent);
                    }
                }
                // If the current interval metric represents the end of the interval, call the method to process it
                else
                {
                    if (startIntervalMetricEventStore.ContainsKey(currentIntervalMetricEvent.MetricType) == true)
                    {
                        TimeSpan intervalDuration = currentIntervalMetricEvent.EventTime.Subtract(startIntervalMetricEventStore[currentIntervalMetricEvent.MetricType].EventTime);
                        double intervalDurationMillisecondsDouble = intervalDuration.TotalMilliseconds;
                        // If the duration is less then 0 set back to 0, as the start time could be after the end time in the case the metric event occurred across a system time update
                        if (intervalDurationMillisecondsDouble < 0)
                        {
                            intervalDurationMillisecondsDouble = 0;
                        }
                        // Convert double to long
                        //   There should not be a risk of overflow here, as the number of milliseconds between DateTime.MinValue and DateTime.MaxValue is 315537897600000, which is a valid long value
                        long intervalDurationMillisecondsLong = Convert.ToInt64(intervalDurationMillisecondsDouble);

                        ProcessIntervalMetricEvent(startIntervalMetricEventStore[currentIntervalMetricEvent.MetricType], intervalDurationMillisecondsLong);

                        startIntervalMetricEventStore.Remove(currentIntervalMetricEvent.MetricType);
                    }
                    else
                    {
                        // If no corresponding start interval event of this type exists and checking is enabled, throw an exception
                        if (intervalMetricChecking == true)
                        {
                            exceptionHandler.Handle(new Exception("Received end '" + currentIntervalMetricEvent.Metric.Name + "' with no corresponding start interval metric."));
                        }
                        // If checking is not enabled discard the interval event
                    }
                }
            }
        }

        #endregion
    }
}

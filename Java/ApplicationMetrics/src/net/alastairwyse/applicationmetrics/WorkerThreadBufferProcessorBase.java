/*
 * Copyright 2015 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
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

import java.lang.Thread.UncaughtExceptionHandler;

/**
 * Provides common connection functionality for classes implementing interface IBufferProcessingStrategy, which use a worker thread to implement a buffer processing strategy.
 * @author Alastair Wyse
 */
abstract class WorkerThreadBufferProcessorBase implements IBufferProcessingStrategy {

    /** The number of count metric events currently stored in the buffer. */
    protected int countMetricEventsBuffered;
    /** The number of amount metric events currently stored in the buffer. */
    protected int amountMetricEventsBuffered;
    /** The number of status metric events currently stored in the buffer. */
    protected int statusMetricEventsBuffered;
    /** The number of interval metric events currently stored in the buffer. */
    protected int intervalMetricEventsBuffered;
    /** Worker thread which implements the strategy to process the contents of the buffers. */
    protected Thread bufferProcessingWorkerThread;
    /** Whether a stop/cancel request has been received. */
    protected volatile boolean cancelRequest;
    /** Whether any metric events remaining in the buffers when the Stop() method is called should be processed. */
    protected volatile boolean processRemainingBufferredMetricsOnStop;
    /** Contains callback methods to implement processing the buffered metric events. */
    protected IBufferProcessedEventHandler bufferProcessedEventHandler;
    /** Handler for any uncaught exceptions occurring on the worker thread. */
    protected UncaughtExceptionHandler exceptionHandler;
    
    @Override
    public void setBufferProcessedEventHandler(IBufferProcessedEventHandler bufferProcessedEventHandler) {
        this.bufferProcessedEventHandler = bufferProcessedEventHandler;
    }

    /**
     * Note that the counter members accessed in this method may be accessed by multiple threads (i.e. the worker thread in member bufferProcessingWorkerThread and the client code in the main thread).  This property should only be read from methods which have locks around the queues in the corresponding MetricLoggerBuffer class (e.g. overrides of the virtual 'Notify' methods defined in this class, which are called from the Add(), Set(), etc... methods in the MetricLoggerBuffer class).
     * @return  The total number of metric events currently stored across all buffers.
     */
    protected long getTotalMetricEventsBufferred() {
        return countMetricEventsBuffered + amountMetricEventsBuffered + statusMetricEventsBuffered + intervalMetricEventsBuffered;
    }
    
    /**
     * Initialises a new instance of the WorkerThreadBufferProcessorBase class.
     * @param  exceptionHandler  Handler for any uncaught exceptions occurring on the worker thread.
     */
    public WorkerThreadBufferProcessorBase(UncaughtExceptionHandler exceptionHandler) {
        countMetricEventsBuffered = 0;
        amountMetricEventsBuffered = 0;
        statusMetricEventsBuffered = 0;
        intervalMetricEventsBuffered = 0;
        processRemainingBufferredMetricsOnStop = true;
        this.exceptionHandler = exceptionHandler;
    }
    
    @Override
    public void Start() {
        cancelRequest = false;
        bufferProcessingWorkerThread.setName("net.alastairwyse.applicationmetrics.WorkerThreadBufferProcessorBase metric event buffer processing worker thread.");
        bufferProcessingWorkerThread.setDaemon(false);
        bufferProcessingWorkerThread.setUncaughtExceptionHandler(exceptionHandler);
        bufferProcessingWorkerThread.start();
    }

    @Override
    public void Stop() throws InterruptedException {
        cancelRequest = true;
        if (bufferProcessingWorkerThread != null) {
            bufferProcessingWorkerThread.join();
        }
    }

    @Override
    public void Stop(boolean processRemainingBufferedMetricEvents) throws Exception {
        this.processRemainingBufferredMetricsOnStop = processRemainingBufferedMetricEvents;
        Stop();
    }

    @Override
    public void NotifyCountMetricEventBuffered() {
        countMetricEventsBuffered++;
    }

    @Override
    public void NotifyAmountMetricEventBuffered() {
        amountMetricEventsBuffered++;
    }

    @Override
    public void NotifyStatusMetricEventBuffered() {
        statusMetricEventsBuffered++;
    }

    @Override
    public void NotifyIntervalMetricEventBuffered() {
        intervalMetricEventsBuffered++;
    }

    @Override
    public void NotifyCountMetricEventBufferCleared() {
        countMetricEventsBuffered = 0;
    }

    @Override
    public void NotifyAmountMetricEventBufferCleared() {
        amountMetricEventsBuffered = 0;
    }

    @Override
    public void NotifyStatusMetricEventBufferCleared() {
        statusMetricEventsBuffered = 0;
    }

    @Override
    public void NotifyIntervalMetricEventBufferCleared() {
        intervalMetricEventsBuffered = 0;
    }

}

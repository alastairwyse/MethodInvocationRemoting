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
 * Implements a buffer processing strategy for MetricLoggerBuffer classes, whereby when the total size of the buffers reaches a defined limit, a worker thread is signaled to process the buffers.
 * The class uses the Object class wait() and notify() methods to signal between the main thread and the worker thread.  Because of the way these methods work, it's possible that calls to notify() (initiated from the classes' public 'Notify' methods) could be missed (i.e. not signal), if the worker thread is still processing the buffers when the call to notify() occurs.  This situation is more likely to arise if the 'bufferSizeLimit' is set very small. 
 * @author Alastair Wyse
 */
public class SizeLimitedBufferProcessor extends WorkerThreadBufferProcessorBase {

    private int bufferSizeLimit;
    private Object bufferProcessSignal;
    
    /**
     * Initialises a new instance of the SizeLimitedBufferProcessor class.
     * @param  bufferSizeLimit   The total size of the buffers which when reached, triggers processing of the buffer contents.
     * @param  exceptionHandler  Handler for any uncaught exceptions occurring on the worker thread.
     */
    public SizeLimitedBufferProcessor(int bufferSizeLimit, UncaughtExceptionHandler exceptionHandler) {
        super(exceptionHandler);
        this.bufferSizeLimit = bufferSizeLimit;
        bufferProcessSignal = new Object();
    }
    
    @Override
    public void Start() {
        bufferProcessingWorkerThread = new Thread(new WorkerThread(this));
        super.Start();
    }
    
    @Override
    public void Stop() throws InterruptedException {
        cancelRequest = true;
        synchronized(bufferProcessSignal) {
            bufferProcessSignal.notify();
        }
        if (bufferProcessingWorkerThread != null) {
            bufferProcessingWorkerThread.join();
        }
    }

    @Override
    public void NotifyCountMetricEventBuffered() {
        super.NotifyCountMetricEventBuffered();
        CheckBufferLimitReached();
    }

    @Override
    public void NotifyAmountMetricEventBuffered() {
        super.NotifyAmountMetricEventBuffered();
        CheckBufferLimitReached();
    }

    @Override
    public void NotifyStatusMetricEventBuffered() {
        super.NotifyStatusMetricEventBuffered();
        CheckBufferLimitReached();
    }

    @Override
    public void NotifyIntervalMetricEventBuffered() {
        super.NotifyIntervalMetricEventBuffered();
        CheckBufferLimitReached();

    }

    /**
     * Checks whether the size limit for the buffers has been reached, and if so, signals the worker thread to process the buffers.
     */
    private void CheckBufferLimitReached() {
        if (getTotalMetricEventsBufferred() >= bufferSizeLimit) {
            try {
                synchronized(bufferProcessSignal) {
                    bufferProcessSignal.notify();
                }
            }
            catch (Exception e) {
                throw new RuntimeException("Error attempting to signal worker thread.", e);
            }
        }
    }
    
    private class WorkerThread implements Runnable {

        private SizeLimitedBufferProcessor outerClass;
        
        /**
         * Initialises a new instance of the WorkerThread class.
         * @param  outerClass  The outer SizeLimitedBufferProcessor object.
         */
        public WorkerThread(SizeLimitedBufferProcessor outerClass) {
            this.outerClass = outerClass;
        }
        
        @Override
        public void run() {
            while (cancelRequest == false) {
                try {
                    synchronized(bufferProcessSignal) {
                        bufferProcessSignal.wait();
                    }
                    if (cancelRequest == false) {
                        bufferProcessedEventHandler.BufferProcessed();
                    }
                }
                catch(Exception e) {
                    throw new RuntimeException("Exception in buffer processing thread.", e);
                }
            }
            if (outerClass.getTotalMetricEventsBufferred() > 0 && processRemainingBufferredMetricsOnStop == true) {
                try {
                    bufferProcessedEventHandler.BufferProcessed();
                }
                catch(Exception e) {
                    throw new RuntimeException("Exception processing buffers when exiting buffer processing thread.", e);
                }
            }
        }
    }
}

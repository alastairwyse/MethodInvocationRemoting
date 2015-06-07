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
import java.util.concurrent.CountDownLatch;

/**
 * Implements a buffer processing strategy for MetricLoggerBuffer classes, using a worker thread which dequeues and processes buffered metric events at a regular interval.
 * @author Alastair Wyse
 */
public class LoopingWorkerThreadBufferProcessor extends WorkerThreadBufferProcessorBase {

    private int dequeueOperationLoopInterval;
    private boolean testConstructor = false;
    // CountDownLatch object used to signal client test code that an iteration of the loop in the dequeue operation thread has completed
    private CountDownLatch dequeueOperationLoopCompleteSignal;
    
    /**
     * Initialises a new instance of the LoopingWorkerThreadBufferProcessor class.
     * @param  dequeueOperationLoopInterval  The time to wait (in milliseconds) between iterations of the worker thread which dequeues and processes metric events.
     * @param  exceptionHandler              Handler for any uncaught exceptions occurring on the worker thread.
     */
    public LoopingWorkerThreadBufferProcessor(int dequeueOperationLoopInterval, UncaughtExceptionHandler exceptionHandler) {
        super(exceptionHandler);
        if (dequeueOperationLoopInterval < 0) {
            throw new IllegalArgumentException("Argument 'dequeueOperationLoopInterval' must be greater than or equal to 0.");
        }

        this.dequeueOperationLoopInterval = dequeueOperationLoopInterval;
    }

    /**
     * Initialises a new instance of the LoopingWorkerThreadBufferProcessor class.
     * <b>Note</b> this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
     * @param  dequeueOperationLoopInterval        The time to wait (in milliseconds) between iterations of the worker thread which dequeues and processes metric events.
     * @param  exceptionHandler                    Handler for any uncaught exceptions occurring on the worker thread.
     * @param  dequeueOperationLoopCompleteSignal  Notifies test code that an iteration of the worker thread which dequeues and processes metric events has completed.
     */
    public LoopingWorkerThreadBufferProcessor(int dequeueOperationLoopInterval, UncaughtExceptionHandler exceptionHandler, CountDownLatch dequeueOperationLoopCompleteSignal) {
        this(dequeueOperationLoopInterval, exceptionHandler);
        testConstructor = true;
        this.dequeueOperationLoopCompleteSignal = dequeueOperationLoopCompleteSignal;
    }
    
    @Override
    public void Start() {
        bufferProcessingWorkerThread = new Thread(new LoopingWorkerThread(this));
        super.Start();
    }

    private class LoopingWorkerThread implements Runnable {

        private LoopingWorkerThreadBufferProcessor outerClass;
        
        /**
         * Initialises a new instance of the WorkerThreadBufferProcessorBase class.
         * @param  outerClass  The outer LoopingWorkerThreadBufferProcessor object.
         */
        public LoopingWorkerThread(LoopingWorkerThreadBufferProcessor outerClass) {
            this.outerClass = outerClass;
        }
        
        @Override
        public void run() {
            while (cancelRequest == false ){
                try {
                    bufferProcessedEventHandler.BufferProcessed();
                    if (dequeueOperationLoopInterval > 0) {
                        Thread.sleep(dequeueOperationLoopInterval);
                    }
                    // If the code is being tested, allow only a single iteration of the loop
                    if (testConstructor == true) {
                        dequeueOperationLoopCompleteSignal.countDown();
                        break;
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

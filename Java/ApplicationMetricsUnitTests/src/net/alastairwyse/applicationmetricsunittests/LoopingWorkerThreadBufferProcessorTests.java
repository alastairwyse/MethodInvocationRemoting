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

package net.alastairwyse.applicationmetricsunittests;

import static org.junit.Assert.*;

import org.junit.Before;
import org.junit.Test;

import java.util.concurrent.CountDownLatch;

import net.alastairwyse.applicationmetrics.*;


/**
 * Unit tests for class applicationmetrics.LoopingWorkerThreadBufferProcessor.
 * @author Alastair Wyse
 */
public class LoopingWorkerThreadBufferProcessorTests {

    private ExceptionStorer exceptionStorer;
    private CountDownLatch dequeueOperationLoopCompleteSignal;
    private LoopingWorkerThreadBufferProcessor testLoopingWorkerThreadBufferProcessor;
   
    @Before
    public void setUp() throws Exception {
        exceptionStorer = new ExceptionStorer();
        dequeueOperationLoopCompleteSignal = new CountDownLatch(1);
        testLoopingWorkerThreadBufferProcessor = new LoopingWorkerThreadBufferProcessor(500, exceptionStorer, dequeueOperationLoopCompleteSignal);
    }
    
    @Test
    public void BufferProcessedEventRaisedAfterStop() throws Exception {
        // Tests that the IBufferProcessedEventHandler.BufferProcessed() callback is called if the buffers still contain metric events after the Stop() method is called.
        //   Unfortunately this unit test is not deterministic, and assumes that the operating system will schedule the main and worker threads so that the calls to NotifyCountMetricEventBuffered() and Stop() will occur before the worker thread has completed one iteration of its loop.

        BufferProcessedEventCounter testBufferProcessedEventCounter = new BufferProcessedEventCounter();
        testLoopingWorkerThreadBufferProcessor.setBufferProcessedEventHandler(testBufferProcessedEventCounter);
        
        testLoopingWorkerThreadBufferProcessor.Start();
        Thread.sleep(250);
        testLoopingWorkerThreadBufferProcessor.NotifyCountMetricEventBuffered();
        testLoopingWorkerThreadBufferProcessor.Stop();
        
        assertEquals(2, testBufferProcessedEventCounter.getBufferProcessedEventRaisedCount());
    }
}

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

import net.alastairwyse.applicationmetrics.*;

/**
 * Unit tests for class applicationmetrics.SizeLimitedBufferProcessor.
 * @author Alastair Wyse
 */
public class SizeLimitedBufferProcessorTests {

    private ExceptionStorer exceptionStorer;
    private BufferProcessedEventCounter testBufferProcessedEventCounter;
    private SizeLimitedBufferProcessor testSizeLimitedBufferProcessor;
    
    @Before
    public void setUp() throws Exception {
        exceptionStorer = new ExceptionStorer();
        testBufferProcessedEventCounter = new BufferProcessedEventCounter();
        testSizeLimitedBufferProcessor = new SizeLimitedBufferProcessor(5, exceptionStorer);
        testSizeLimitedBufferProcessor.setBufferProcessedEventHandler(testBufferProcessedEventCounter);
    }
    
    /* 
     * NOTE: The below tests are potentially non-deterministic, as the 'assert' statements could potentially be executed before the thread signalled inside the SizeLimitedBufferProcessor class has had a chance to iterate its loop (depending on thread scheduling in the operating system).
     *       The Thread.yield() method is used in some cases to overcome this (although still does not guarantee the results will be deterministic).
     */
    
    @Test
    public void BufferProcessedEventCalledAfterEventsBufferred() throws Exception {
        testSizeLimitedBufferProcessor.Start();
        Thread.yield();
        testSizeLimitedBufferProcessor.NotifyCountMetricEventBuffered();
        testSizeLimitedBufferProcessor.NotifyAmountMetricEventBuffered();
        testSizeLimitedBufferProcessor.NotifyStatusMetricEventBuffered();
        testSizeLimitedBufferProcessor.NotifyIntervalMetricEventBuffered();
        testSizeLimitedBufferProcessor.NotifyCountMetricEventBuffered();
        // After 5 metric events are buffered the buffer size limit is reached, and the metric events should be processed and the buffers cleared
        testSizeLimitedBufferProcessor.NotifyCountMetricEventBufferCleared();
        testSizeLimitedBufferProcessor.NotifyAmountMetricEventBufferCleared();
        testSizeLimitedBufferProcessor.NotifyStatusMetricEventBufferCleared();
        testSizeLimitedBufferProcessor.NotifyIntervalMetricEventBufferCleared();
        testSizeLimitedBufferProcessor.Stop();

        assertEquals(1, testBufferProcessedEventCounter.getBufferProcessedEventRaisedCount());
    }
    
    @Test
    public void BufferProcessedEventRaisedAfterStopWithNoParameter() throws Exception {
        testSizeLimitedBufferProcessor.Start();
        Thread.yield();
        testSizeLimitedBufferProcessor.NotifyCountMetricEventBuffered();
        testSizeLimitedBufferProcessor.NotifyAmountMetricEventBuffered();
        testSizeLimitedBufferProcessor.NotifyStatusMetricEventBuffered();
        testSizeLimitedBufferProcessor.NotifyIntervalMetricEventBuffered();
        testSizeLimitedBufferProcessor.Stop();

        assertEquals(1, testBufferProcessedEventCounter.getBufferProcessedEventRaisedCount());
    }
    
    @Test
    public void BufferProcessedEventRaisedAfterStopWithTrueParameter() throws Exception {
        testSizeLimitedBufferProcessor.Start();
        Thread.yield();
        testSizeLimitedBufferProcessor.NotifyCountMetricEventBuffered();
        testSizeLimitedBufferProcessor.NotifyAmountMetricEventBuffered();
        testSizeLimitedBufferProcessor.NotifyStatusMetricEventBuffered();
        testSizeLimitedBufferProcessor.NotifyIntervalMetricEventBuffered();
        testSizeLimitedBufferProcessor.Stop(true);

        assertEquals(1, testBufferProcessedEventCounter.getBufferProcessedEventRaisedCount());
    }
    
    @Test
    public void BufferProcessedEventNotRaisedAfterStopWithFalseParameter() throws Exception {
        testSizeLimitedBufferProcessor.Start();
        Thread.yield();
        testSizeLimitedBufferProcessor.NotifyCountMetricEventBuffered();
        testSizeLimitedBufferProcessor.NotifyAmountMetricEventBuffered();
        testSizeLimitedBufferProcessor.NotifyStatusMetricEventBuffered();
        testSizeLimitedBufferProcessor.NotifyIntervalMetricEventBuffered();
        testSizeLimitedBufferProcessor.Stop(false);

        assertEquals(0, testBufferProcessedEventCounter.getBufferProcessedEventRaisedCount());
    }
    
    @Test
    public void BufferProcessedEventNotRaisedAfterStopWithNoBufferedMetricEvents() throws Exception {
        testSizeLimitedBufferProcessor.Start();
        Thread.yield();
        testSizeLimitedBufferProcessor.Stop(false);

        assertEquals(0, testBufferProcessedEventCounter.getBufferProcessedEventRaisedCount());
    }
}

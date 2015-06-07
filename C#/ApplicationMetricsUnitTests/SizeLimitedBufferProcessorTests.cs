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

#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ApplicationMetrics;

namespace ApplicationMetricsUnitTests
{
    //******************************************************************************
    //
    // Class: SizeLimitedBufferProcessorTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for class ApplicationMetrics.SizeLimitedBufferProcessorTests.
    /// </summary>
    class SizeLimitedBufferProcessorTests
    {
        private SizeLimitedBufferProcessor testSizeLimitedBufferProcessor;
        private int bufferProcessedEventRaisedCount;

        [SetUp]
        protected void SetUp()
        {
            testSizeLimitedBufferProcessor = new SizeLimitedBufferProcessor(5);
            testSizeLimitedBufferProcessor.BufferProcessed += delegate(object sender, EventArgs e) { bufferProcessedEventRaisedCount++; };
            bufferProcessedEventRaisedCount = 0;
        }

        [TearDown]
        protected void TearDown()
        {
            testSizeLimitedBufferProcessor.Dispose();
        }

        /* 
         * NOTE: The below tests are potentially non-deterministic, as the 'Assert' statements could potentially be executed before the thread signalled inside the SizeLimitedBufferProcessor class has had a chance to iterate its loop (depending on thread scheduling in the operating system).
         *       On some systems it may be necessary to put Thread.Sleep() instructions between the test steps, and the 'Assert'(s).
         */

        [Test]
        public void BufferProcessedEventRaisedAfterEventsBufferred()
        {
            testSizeLimitedBufferProcessor.Start();
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

            Assert.AreEqual(1, bufferProcessedEventRaisedCount);
        }

        [Test]
        public void BufferProcessedEventRaisedAfterStopWithNoParameter()
        {
            testSizeLimitedBufferProcessor.Start();
            testSizeLimitedBufferProcessor.NotifyCountMetricEventBuffered();
            testSizeLimitedBufferProcessor.NotifyAmountMetricEventBuffered();
            testSizeLimitedBufferProcessor.NotifyStatusMetricEventBuffered();
            testSizeLimitedBufferProcessor.NotifyIntervalMetricEventBuffered();
            testSizeLimitedBufferProcessor.Stop();

            Assert.AreEqual(1, bufferProcessedEventRaisedCount);
        }

        [Test]
        public void BufferProcessedEventRaisedAfterStopWithTrueParameter()
        {
            testSizeLimitedBufferProcessor.Start();
            testSizeLimitedBufferProcessor.NotifyCountMetricEventBuffered();
            testSizeLimitedBufferProcessor.NotifyAmountMetricEventBuffered();
            testSizeLimitedBufferProcessor.NotifyStatusMetricEventBuffered();
            testSizeLimitedBufferProcessor.NotifyIntervalMetricEventBuffered();
            testSizeLimitedBufferProcessor.Stop(true);

            Assert.AreEqual(1, bufferProcessedEventRaisedCount);
        }

        [Test]
        public void BufferProcessedEventNotRaisedAfterStopWithFalseParameter()
        {
            testSizeLimitedBufferProcessor.Start();
            testSizeLimitedBufferProcessor.NotifyCountMetricEventBuffered();
            testSizeLimitedBufferProcessor.NotifyAmountMetricEventBuffered();
            testSizeLimitedBufferProcessor.NotifyStatusMetricEventBuffered();
            testSizeLimitedBufferProcessor.NotifyIntervalMetricEventBuffered();
            testSizeLimitedBufferProcessor.Stop(false);

            Assert.AreEqual(0, bufferProcessedEventRaisedCount);
        }

        [Test]
        public void BufferProcessedEventNotRaisedAfterStopWithNoBufferedMetricEvents()
        {
            testSizeLimitedBufferProcessor.Start();
            testSizeLimitedBufferProcessor.Stop(false);

            Assert.AreEqual(0, bufferProcessedEventRaisedCount);
        }
    }
}

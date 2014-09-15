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
using NUnit.Framework;
using NMock2;
using ApplicationMetrics;
using OperatingSystemAbstraction;

namespace ApplicationMetricsUnitTests
{
    //******************************************************************************
    //
    // Class: FileMetricLoggerTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for class ApplicationMetrics.FileMetricLogger.
    /// </summary>
    [TestFixture]
    class FileMetricLoggerTests
    {
        /* 
         * NOTE: See notes in class MicrosoftAccessMetricLoggerTests regarding testing of underlying worker threads.  The same comments apply to this test class
         */

        private Mockery mocks;
        private IStreamWriter mockStreamWriter;
        private IDateTime mockDateTime;
        private ExceptionStorer exceptionStorer;
        private AutoResetEvent workerThreadLoopCompleteSignal;
        private FileMetricLogger testFileMetricLogger;
        private const string dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockStreamWriter = mocks.NewMock<IStreamWriter>();
            mockDateTime = mocks.NewMock<IDateTime>();
            exceptionStorer = new ExceptionStorer();
            workerThreadLoopCompleteSignal = new AutoResetEvent(false);
            testFileMetricLogger = new FileMetricLogger('|', 10, true, mockStreamWriter, mockDateTime, exceptionStorer);
        }

        [Test]
        public void IncrementSuccessTest()
        {
            TimeSpan utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(System.DateTime.Now);
            System.DateTime timeStamp1 = new System.DateTime(2014, 6, 14, 12, 45, 31);
            System.DateTime timeStamp2 = new System.DateTime(2014, 6, 14, 12, 45, 43);
            System.DateTime timeStamp3 = new System.DateTime(2014, 6, 15, 23, 58, 47);

            using (mocks.Ordered)
            {
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(timeStamp1));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(timeStamp2));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(timeStamp3));
                Expect.Once.On(mockStreamWriter).Method("WriteLine").With(timeStamp1.Add(utcOffset).ToString(dateTimeFormat) + " | " + new TestMessageReceivedMetric().Name);
                Expect.Once.On(mockStreamWriter).Method("Flush");
                Expect.Once.On(mockStreamWriter).Method("WriteLine").With(timeStamp2.Add(utcOffset).ToString(dateTimeFormat) + " | " + new TestDiskReadOperationMetric().Name);
                Expect.Once.On(mockStreamWriter).Method("Flush");
                Expect.Once.On(mockStreamWriter).Method("WriteLine").With(timeStamp3.Add(utcOffset).ToString(dateTimeFormat) + " | " + new TestMessageReceivedMetric().Name);
                Expect.Once.On(mockStreamWriter).Method("Flush").Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testFileMetricLogger.Increment(new TestMessageReceivedMetric());
            testFileMetricLogger.Increment(new TestDiskReadOperationMetric());
            testFileMetricLogger.Increment(new TestMessageReceivedMetric());
            testFileMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void AddSuccessTest()
        {
            TimeSpan utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(System.DateTime.Now);
            System.DateTime timeStamp1 = new System.DateTime(2014, 6, 14, 12, 45, 31);
            System.DateTime timeStamp2 = new System.DateTime(2014, 6, 14, 12, 45, 43);
            System.DateTime timeStamp3 = new System.DateTime(2014, 6, 15, 23, 58, 47);

            using (mocks.Ordered)
            {
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(timeStamp1));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(timeStamp2));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(timeStamp3));
                Expect.Once.On(mockStreamWriter).Method("WriteLine").With(timeStamp1.Add(utcOffset).ToString(dateTimeFormat) + " | " + new TestMessageBytesReceivedMetric(0).Name + " | " + 12345);
                Expect.Once.On(mockStreamWriter).Method("Flush");
                Expect.Once.On(mockStreamWriter).Method("WriteLine").With(timeStamp2.Add(utcOffset).ToString(dateTimeFormat) + " | " + new TestDiskBytesReadMetric(0).Name + " | " + 160307);
                Expect.Once.On(mockStreamWriter).Method("Flush");
                Expect.Once.On(mockStreamWriter).Method("WriteLine").With(timeStamp3.Add(utcOffset).ToString(dateTimeFormat) + " | " + new TestMessageBytesReceivedMetric(0).Name + " | " + 12347);
                Expect.Once.On(mockStreamWriter).Method("Flush").Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testFileMetricLogger.Add(new TestMessageBytesReceivedMetric(12345));
            testFileMetricLogger.Add(new TestDiskBytesReadMetric(160307));
            testFileMetricLogger.Add(new TestMessageBytesReceivedMetric(12347));
            testFileMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SetSuccessTest()
        {
            TimeSpan utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(System.DateTime.Now);
            System.DateTime timeStamp1 = new System.DateTime(2014, 6, 17, 23, 42, 33);
            System.DateTime timeStamp2 = new System.DateTime(2014, 6, 17, 23, 44, 35);
            System.DateTime timeStamp3 = new System.DateTime(2014, 6, 17, 23, 59, 01);

            using (mocks.Ordered)
            {
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(timeStamp1));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(timeStamp2));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(timeStamp3));
                Expect.Once.On(mockStreamWriter).Method("WriteLine").With(timeStamp1.Add(utcOffset).ToString(dateTimeFormat) + " | " + new TestAvailableMemoryMetric(0).Name + " | " + 301156000);
                Expect.Once.On(mockStreamWriter).Method("Flush");
                Expect.Once.On(mockStreamWriter).Method("WriteLine").With(timeStamp2.Add(utcOffset).ToString(dateTimeFormat) + " | " + new TestFreeWorkerThreadsMetric(0).Name + " | " + 12);
                Expect.Once.On(mockStreamWriter).Method("Flush");
                Expect.Once.On(mockStreamWriter).Method("WriteLine").With(timeStamp3.Add(utcOffset).ToString(dateTimeFormat) + " | " + new TestAvailableMemoryMetric(0).Name + " | " + 301155987);
                Expect.Once.On(mockStreamWriter).Method("Flush").Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testFileMetricLogger.Set(new TestAvailableMemoryMetric(301156000));
            testFileMetricLogger.Set(new TestFreeWorkerThreadsMetric(12));
            testFileMetricLogger.Set(new TestAvailableMemoryMetric(301155987));
            testFileMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void BeginEndSuccessTests()
        {
            TimeSpan utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(System.DateTime.Now);
            System.DateTime timeStamp1 = new System.DateTime(2014, 6, 14, 12, 45, 31, 000);
            System.DateTime timeStamp2 = new System.DateTime(2014, 6, 14, 12, 45, 31, 034);
            System.DateTime timeStamp3 = new System.DateTime(2014, 6, 14, 12, 45, 43, 500);
            System.DateTime timeStamp4 = new System.DateTime(2014, 6, 14, 12, 45, 43, 499);
            System.DateTime timeStamp5 = new System.DateTime(2014, 6, 15, 23, 58, 47, 750);
            System.DateTime timeStamp6 = new System.DateTime(2014, 6, 15, 23, 58, 48, 785);

            using (mocks.Ordered)
            {
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(timeStamp1));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(timeStamp2));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(timeStamp3));
                // Note below expect makes the end time before the begin time.  Class should insert the resulting milliseconds interval as 0.
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(timeStamp4));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(timeStamp5));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(timeStamp6));
                Expect.Once.On(mockStreamWriter).Method("WriteLine").With(timeStamp1.Add(utcOffset).ToString(dateTimeFormat) + " | " + new TestDiskReadTimeMetric().Name + " | " + 34);
                Expect.Once.On(mockStreamWriter).Method("Flush");
                Expect.Once.On(mockStreamWriter).Method("WriteLine").With(timeStamp3.Add(utcOffset).ToString(dateTimeFormat) + " | " + new TestMessageProcessingTimeMetric().Name + " | " + 0);
                Expect.Once.On(mockStreamWriter).Method("Flush");
                Expect.Once.On(mockStreamWriter).Method("WriteLine").With(timeStamp5.Add(utcOffset).ToString(dateTimeFormat) + " | " + new TestMessageProcessingTimeMetric().Name + " | " + 1035);
                Expect.Once.On(mockStreamWriter).Method("Flush").Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testFileMetricLogger.Begin(new TestDiskReadTimeMetric());
            testFileMetricLogger.End(new TestDiskReadTimeMetric());
            testFileMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testFileMetricLogger.End(new TestMessageProcessingTimeMetric());
            testFileMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testFileMetricLogger.End(new TestMessageProcessingTimeMetric());
            testFileMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void CloseSuccessTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockStreamWriter).Method("Close").WithNoArguments();
            }

            testFileMetricLogger.Close();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void DisposeSuccessTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockStreamWriter).Method("Dispose").WithNoArguments();
            }

            testFileMetricLogger.Dispose();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}

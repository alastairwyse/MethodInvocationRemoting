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
    // Class: ConsoleMetricLoggerTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for class ApplicationMetrics.ConsoleMetricLogger.
    /// </summary>
    class ConsoleMetricLoggerTests
    {
        /* 
         * NOTE: See notes in class MicrosoftAccessMetricLoggerTests regarding testing of underlying worker threads.  The same comments apply to this test class
         */

        private Mockery mocks;
        private IConsole mockConsole;
        private IDateTime mockDateTime;
        private ExceptionStorer exceptionStorer;
        private AutoResetEvent workerThreadLoopCompleteSignal;
        private ConsoleMetricLogger testConsoleMetricLogger;
        private const string separatorString = ": ";

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockConsole = mocks.NewMock<IConsole>();
            mockDateTime = mocks.NewMock<IDateTime>();
            exceptionStorer = new ExceptionStorer();
            workerThreadLoopCompleteSignal = new AutoResetEvent(false);
            testConsoleMetricLogger = new ConsoleMetricLogger(10, true, mockConsole, mockDateTime, exceptionStorer);
        }

        [Test]
        public void LogCountMetricTotalSuccessTest()
        {
            using (mocks.Ordered)
            {
                // Expects for calls to Increment()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 20, 21)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 20, 23)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 20, 25)));
                // Expects for calls to Start()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 20, 27)));
                SetWriteTitleExpectations();
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestMessageReceivedMetric().Name + separatorString + "2");
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestDiskReadOperationMetric().Name + separatorString + "1").Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testConsoleMetricLogger.Increment(new TestMessageReceivedMetric());
            testConsoleMetricLogger.Increment(new TestDiskReadOperationMetric());
            testConsoleMetricLogger.Increment(new TestMessageReceivedMetric());
            testConsoleMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogAmounMetricTotalSuccessTest()
        {
            using (mocks.Ordered)
            {
                // Expects for calls to Add()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 20, 21)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 20, 23)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 20, 25)));
                // Expects for calls to Start()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 20, 27)));
                SetWriteTitleExpectations();
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestMessageBytesReceivedMetric(0).Name + separatorString + "3072");
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestDiskBytesReadMetric(0).Name + separatorString + "3049").Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(1024));
            testConsoleMetricLogger.Add(new TestDiskBytesReadMetric(3049));
            testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(2048));
            testConsoleMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogStatusMetricValueSuccessTest()
        {
            using (mocks.Ordered)
            {
                // Expects for calls to Set()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 14, 22, 54, 01)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 14, 22, 54, 03)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 14, 22, 54, 06)));
                // Expects for calls to Start()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 14, 22, 54, 00)));
                SetWriteTitleExpectations();
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestAvailableMemoryMetric(0).Name + separatorString + "714768384");
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestFreeWorkerThreadsMetric(0).Name + separatorString + "8").Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testConsoleMetricLogger.Set(new TestAvailableMemoryMetric(80740352));
            testConsoleMetricLogger.Set(new TestFreeWorkerThreadsMetric(8));
            testConsoleMetricLogger.Set(new TestAvailableMemoryMetric(714768384));
            testConsoleMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogIntervalMetricTotalSuccessTest()
        {
            using (mocks.Ordered)
            {
                // Expects for calls to Begin() and End()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 14, 22, 54, 01, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 14, 22, 54, 03, 250)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 14, 22, 54, 06, 987)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 14, 22, 54, 07, 123)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 14, 22, 56, 59, 501)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 14, 22, 58, 01, 267)));
                // Expects for calls to Start()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 14, 22, 54, 00)));
                SetWriteTitleExpectations();
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestDiskReadTimeMetric().Name + separatorString + "3737");
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestMessageProcessingTimeMetric().Name + separatorString + "67889").Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testConsoleMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testConsoleMetricLogger.Begin(new TestDiskReadTimeMetric());
            testConsoleMetricLogger.End(new TestDiskReadTimeMetric());
            testConsoleMetricLogger.End(new TestMessageProcessingTimeMetric());
            testConsoleMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testConsoleMetricLogger.End(new TestMessageProcessingTimeMetric());
            testConsoleMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogCountOverTimeUnitAggregateSuccessTest()
        {
            using (mocks.Ordered)
            {
                // Expects for the calls to Increment()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 10, 250)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 10, 500)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 10, 750)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 11, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 11, 250)));
                // Expects for calls to Start()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 10, 000)));
                SetWriteTitleExpectations();
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestMessageReceivedMetric().Name + separatorString + "5");
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 12, 000)));
                Expect.Once.On(mockConsole).Method("WriteLine").With("MessagesReceivedPerSecond" + separatorString + "2.5").Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testConsoleMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Second, "MessagesReceivedPerSecond", "The number of messages received per second");
            for (int i = 0; i < 5; i++)
            {
                testConsoleMetricLogger.Increment(new TestMessageReceivedMetric());
            }
            testConsoleMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogCountOverTimeUnitAggregateNoInstancesSuccessTest()
        {
            // Tests defining a count over time unit aggregate, where no instances of the underlying count metric have been logged

            using (mocks.Ordered)
            {
                // Expects for calls to Start()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 11, 23, 30, 42, 000)));
                SetWriteTitleExpectations();
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 11, 23, 30, 47, 000)));
                Expect.Once.On(mockConsole).Method("WriteLine").With("MessagesReceivedPerSecond" + separatorString + "0").Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testConsoleMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Second, "MessagesReceivedPerSecond", "The number of messages received per second");
            testConsoleMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();
            // Wait a few more milliseconds so that any unexpected method calls after the signal are caught
            Thread.Sleep(50);

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogAmountOverCountAggregateSuccessTest()
        {
            using (mocks.Ordered)
            {
                // Expects for the calls to Add() and Increment()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 17, 56, 19, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 17, 56, 20, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 17, 56, 21, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 17, 56, 22, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 17, 56, 23, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 17, 56, 24, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 17, 56, 25, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 17, 56, 26, 000)));
                // Expects for calls to Start()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 17, 56, 18, 000)));
                SetWriteTitleExpectations();
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestMessageReceivedMetric().Name + separatorString + "4");
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestMessageBytesReceivedMetric(0).Name + separatorString + "18");
                Expect.Once.On(mockConsole).Method("WriteLine").With("BytesReceivedPerMessage" + separatorString + "4.5").Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testConsoleMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestMessageReceivedMetric(), "BytesReceivedPerMessage", "The number of bytes received per message");
            testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(2));
            testConsoleMetricLogger.Increment(new TestMessageReceivedMetric());
            testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(6));
            testConsoleMetricLogger.Increment(new TestMessageReceivedMetric());
            testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(3));
            testConsoleMetricLogger.Increment(new TestMessageReceivedMetric());
            testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(7));
            testConsoleMetricLogger.Increment(new TestMessageReceivedMetric());
            testConsoleMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogAmountOverCountAggregateNoInstancesSuccessTest()
        {
            // Tests defining an amount over count aggregate, where no instances of the underlying count metric have been logged

            using (mocks.Ordered)
            {
                // Expects for the calls to Add()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 17, 56, 19, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 17, 56, 20, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 17, 56, 21, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 17, 56, 22, 000)));
                // Expects for calls to Start()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 17, 56, 18, 000)));
                SetWriteTitleExpectations();
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestMessageBytesReceivedMetric(0).Name + separatorString + "18").Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testConsoleMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestMessageReceivedMetric(), "BytesReceivedPerMessage", "The number of bytes received per message");
            testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(2));
            testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(6));
            testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(3));
            testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(7));
            testConsoleMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();
            // Wait a few more milliseconds so that any unexpected method calls after the signal are caught
            Thread.Sleep(50);

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogAmountOverTimeUnitAggregateSuccessTest()
        {
            using (mocks.Ordered)
            {
                // Expects for the calls to Add()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 10, 250)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 10, 500)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 10, 750)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 11, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 11, 250)));
                // Expects for calls to Start()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 10, 000)));
                SetWriteTitleExpectations();
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestMessageBytesReceivedMetric(0).Name + separatorString + "1345");
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 12, 000)));
                Expect.Once.On(mockConsole).Method("WriteLine").With("MessageBytesPerSecond" + separatorString + "672.5").Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testConsoleMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), TimeUnit.Second, "MessageBytesPerSecond", "The number of message bytes received per second");
            testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(149));
            testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(257));
            testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(439));
            testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(271));
            testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(229));
            testConsoleMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogAmountOverTimeUnitAggregateNoInstancesSuccessTest()
        {
            // Tests defining an amount over time unit aggregate, where no instances of the underlying amount metric have been logged

            using (mocks.Ordered)
            {
                // Expects for calls to Start()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 11, 23, 30, 42, 000)));
                SetWriteTitleExpectations();
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 11, 23, 30, 47, 000)));
                Expect.Once.On(mockConsole).Method("WriteLine").With("MessageBytesPerSecond" + separatorString + "0").Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testConsoleMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), TimeUnit.Second, "MessageBytesPerSecond", "The number of message bytes received per second");
            testConsoleMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();
            // Wait a few more milliseconds so that any unexpected method calls after the signal are caught
            Thread.Sleep(50);

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogAmountOverAmountAggregateSuccessTest()
        {
            using (mocks.Ordered)
            {
                // Expects for the calls to Add()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 10, 250)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 10, 500)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 10, 750)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 11, 000)));
                // Expects for calls to Start()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 10, 000)));
                SetWriteTitleExpectations();
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestMessageBytesReceivedMetric(0).Name + separatorString + "588");
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestDiskBytesReadMetric(0).Name + separatorString + "528");
                Expect.Once.On(mockConsole).Method("WriteLine").With("MessageBytesReceivedPerDiskBytesRead" + separatorString + "1.11363636363636").Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testConsoleMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestDiskBytesReadMetric(0), "MessageBytesReceivedPerDiskBytesRead", "The number of message bytes received per disk bytes read");
            testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(149));
            testConsoleMetricLogger.Add(new TestDiskBytesReadMetric(257));
            testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(439));
            testConsoleMetricLogger.Add(new TestDiskBytesReadMetric(271));
            testConsoleMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogAmountOverAmountAggregateNoNumeratorInstancesSuccessTest()
        {
            using (mocks.Ordered)
            {
                // Expects for the calls to Add()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 10, 250)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 10, 500)));
                // Expects for calls to Start()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 10, 000)));
                SetWriteTitleExpectations();
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestDiskBytesReadMetric(0).Name + separatorString + "528");
                Expect.Once.On(mockConsole).Method("WriteLine").With("MessageBytesReceivedPerDiskBytesRead" + separatorString + "0").Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testConsoleMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestDiskBytesReadMetric(0), "MessageBytesReceivedPerDiskBytesRead", "The number of message bytes received per disk bytes read");
            testConsoleMetricLogger.Add(new TestDiskBytesReadMetric(257));
            testConsoleMetricLogger.Add(new TestDiskBytesReadMetric(271));
            testConsoleMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogAmountOverAmountAggregateNoDenominatorInstancesSuccessTest()
        {
            using (mocks.Ordered)
            {
                // Expects for the calls to Add()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 10, 250)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 10, 500)));
                // Expects for calls to Start()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 39, 10, 000)));
                SetWriteTitleExpectations();
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestMessageBytesReceivedMetric(0).Name + separatorString + "588").Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testConsoleMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestDiskBytesReadMetric(0), "MessageBytesReceivedPerDiskBytesRead", "The number of message bytes received per disk bytes read");
            testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(149));
            testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(439));
            testConsoleMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();
            // Wait a few more milliseconds so that any unexpected method calls after the signal are caught
            Thread.Sleep(50);


            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogIntervalOverCountAggregateSuccessTest()
        {
            using (mocks.Ordered)
            {
                // Expects for the calls to Begin(), End(), and Increment()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 23, 01, 17, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 23, 01, 17, 120)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 23, 01, 17, 120)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 23, 01, 19, 850)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 23, 01, 20, 975)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 23, 01, 20, 980)));
                // Expects for calls to Start()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 23, 01, 16, 999)));
                SetWriteTitleExpectations();
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestMessageReceivedMetric().Name + separatorString + "2");
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestMessageProcessingTimeMetric().Name + separatorString + "1245");
                Expect.Once.On(mockConsole).Method("WriteLine").With("ProcessingTimePerMessage" + separatorString + "622.5").Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testConsoleMetricLogger.DefineMetricAggregate(new TestMessageProcessingTimeMetric(), new TestMessageReceivedMetric(), "ProcessingTimePerMessage", "The average time to process each message");
            testConsoleMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testConsoleMetricLogger.End(new TestMessageProcessingTimeMetric());
            testConsoleMetricLogger.Increment(new TestMessageReceivedMetric());
            testConsoleMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testConsoleMetricLogger.End(new TestMessageProcessingTimeMetric());
            testConsoleMetricLogger.Increment(new TestMessageReceivedMetric());
            testConsoleMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogIntervalOverCountAggregateNoInstancesSuccessTest()
        {
            // Tests defining an interval over count aggregate, where no instances of the underlying count metric have been logged

            using (mocks.Ordered)
            {
                // Expects for the calls to Begin(), End(), and Increment()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 23, 01, 17, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 23, 01, 17, 120)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 23, 01, 19, 850)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 23, 01, 20, 975)));
                // Expects for calls to Start()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 23, 01, 16, 999)));
                SetWriteTitleExpectations();
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestMessageProcessingTimeMetric().Name + separatorString + "1245").Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testConsoleMetricLogger.DefineMetricAggregate(new TestMessageProcessingTimeMetric(), new TestMessageReceivedMetric(), "ProcessingTimePerMessage", "The average time to process each message");
            testConsoleMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testConsoleMetricLogger.End(new TestMessageProcessingTimeMetric());
            testConsoleMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testConsoleMetricLogger.End(new TestMessageProcessingTimeMetric());
            testConsoleMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();
            // Wait a few more milliseconds so that any unexpected method calls after the signal are caught
            Thread.Sleep(50);

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogIntervalOverTotalRunTimeAggregateSuccessTest()
        {
            using (mocks.Ordered)
            {
                // Expects for the calls to Begin() and End()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 19, 17, 33, 51, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 19, 17, 33, 51, 789)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 19, 17, 33, 52, 058)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 19, 17, 33, 56, 032)));
                // Expects for calls to Start()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 19, 17, 33, 50, 000)));
                SetWriteTitleExpectations();
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestMessageProcessingTimeMetric().Name + separatorString + "4763");
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 19, 17, 33, 56, 300)));
                Expect.Once.On(mockConsole).Method("WriteLine").With("MessageProcessingTimePercentage" + separatorString + "0.756031746031746").Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testConsoleMetricLogger.DefineMetricAggregate(new TestMessageProcessingTimeMetric(), "MessageProcessingTimePercentage", "The amount of time spent processing messages as a percentage of total run time");
            testConsoleMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testConsoleMetricLogger.End(new TestMessageProcessingTimeMetric());
            testConsoleMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testConsoleMetricLogger.End(new TestMessageProcessingTimeMetric());
            testConsoleMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogIntervalOverTotalRunTimeAggregateZeroElapsedTimeSuccessTest()
        {
            // Tests that an aggregate is not logged when no time has elapsed

            using (mocks.Ordered)
            {
                // Expects for the calls to Begin() and End()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 19, 17, 33, 51, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 19, 17, 33, 51, 789)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 19, 17, 33, 52, 058)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 19, 17, 33, 56, 032)));
                // Expects for calls to Start()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 19, 17, 33, 50, 000)));
                SetWriteTitleExpectations();
                Expect.Once.On(mockConsole).Method("WriteLine").With(new TestMessageProcessingTimeMetric().Name + separatorString + "4763");
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 19, 17, 33, 50, 000)), Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testConsoleMetricLogger.DefineMetricAggregate(new TestMessageProcessingTimeMetric(), "MessageProcessingTimePercentage", "The amount of time spent processing messages as a percentage of total run time");
            testConsoleMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testConsoleMetricLogger.End(new TestMessageProcessingTimeMetric());
            testConsoleMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testConsoleMetricLogger.End(new TestMessageProcessingTimeMetric());
            testConsoleMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        //------------------------------------------------------------------------------
        //
        // Method: SetWriteTitleExpectations
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Sets mock expectations for clearing the console and writing the title banner.
        /// </summary>
        private void SetWriteTitleExpectations()
        {
            Expect.Once.On(mockConsole).Method("Clear").WithNoArguments();
            Expect.Once.On(mockConsole).Method("WriteLine").With("---------------------------------------------------");
            Expect.Once.On(mockDateTime).GetProperty("Now").Will(Return.Value(new System.DateTime(2014, 07, 12, 15, 20, 29)));
            Expect.Once.On(mockConsole).Method("WriteLine").With("-- Application metrics as of 2014-07-12 15:20:29 --");
            Expect.Once.On(mockConsole).Method("WriteLine").With("---------------------------------------------------");
        }
    }
}

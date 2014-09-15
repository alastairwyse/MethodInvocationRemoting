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
    // Class: MicrosoftAccessMetricLoggerTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for class ApplicationMetrics.MicrosoftAccessMetricLogger.
    /// </summary>
    /// <remarks>This class also implicity tests the functionality in abstract class MetricLoggerBuffer.  Functionality from MetricLoggerBuffer (e.g. behaviour of parameter 'intervalMetricChecking') does not need to be tested in other classes deriving from MetricLoggerBuffer.</remarks>
    public class MicrosoftAccessMetricLoggerTests
    {
        /* 
         * NOTE: As most of the work of the MicrosoftAccessMetricLogger class is done by a worker thread, many of the tests in this class rely on checking the behavior of the worker thread.
         *       This creates an issue in the unit test code as NUnit does not catch exceptions thrown on the worker thread via the Assert.Throws() method.
         *         To work around this an ExceptionStorer object is injected into the test class.  Any exceptions thrown are checked in this object rather than via the Assert.Throws() method (see test method IncrementDatabaseInsertException() as an example).
         */

        const string accessConnectionStringPrefix = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=";

        private Mockery mocks;
        private IOleDbConnection mockDbConnection;
        private IOleDbCommand mockDbCommand;
        private IDateTime mockDateTime;
        private ExceptionStorer exceptionStorer;
        private AutoResetEvent workerThreadLoopCompleteSignal;
        private string testDbFilePath = @"C:\Temp\TestAccessDb.mdb";
        private string testMetricCategoryName = "DefaultCategory";
        private MicrosoftAccessMetricLogger testMicrosoftAccessMetricLogger;

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockDbConnection = mocks.NewMock<IOleDbConnection>();
            mockDbCommand = mocks.NewMock<IOleDbCommand>();
            mockDateTime = mocks.NewMock<IDateTime>();
            exceptionStorer = new ExceptionStorer();
            workerThreadLoopCompleteSignal = new AutoResetEvent(false);
            testMicrosoftAccessMetricLogger = new MicrosoftAccessMetricLogger(testDbFilePath, testMetricCategoryName, 10, true, mockDbConnection, mockDbCommand, mockDateTime, exceptionStorer);
        }

        [Test]
        public void InvalidDequeueOperationLoopIntervalArgument()
        {
            ArgumentOutOfRangeException e = Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                testMicrosoftAccessMetricLogger = new MicrosoftAccessMetricLogger(testDbFilePath, testMetricCategoryName, -1, true);
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Argument 'dequeueOperationLoopInterval' must be greater than or equal to 0."));
            Assert.AreEqual("dequeueOperationLoopInterval", e.ParamName);
            Assert.AreEqual(-1, (int)e.ActualValue);
        }

        [Test]
        public void InvalidMetricCategoryNameArgument()
        {
            ArgumentException e = Assert.Throws<ArgumentException>(delegate
            {
                testMicrosoftAccessMetricLogger = new MicrosoftAccessMetricLogger(testDbFilePath, "  ", 10, true);
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Argument 'metricCategoryName' cannot be blank."));
            Assert.AreEqual("metricCategoryName", e.ParamName);
        }

        [Test]
        public void ConnectWhenAlreadyConnected()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                SetCheckConnectedExpectations(true);
            }

            testMicrosoftAccessMetricLogger.Connect();
            Exception e = Assert.Throws<Exception>(delegate
            {
                testMicrosoftAccessMetricLogger.Connect();
            });

            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Connection to database has already been established."));
        }

        [Test]
        public void ConnectException()
        {
            using (mocks.Ordered)
            {
                SetCheckConnectedExpectations(false);
                Expect.Once.On(mockDbConnection).SetProperty("ConnectionString").To(accessConnectionStringPrefix + testDbFilePath);
                Expect.Once.On(mockDbConnection).Method("Open").WithNoArguments().Will(Throw.Exception(new Exception("Mock Connection Failure")));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testMicrosoftAccessMetricLogger.Connect();
            });

            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to connect to database at path '" + testDbFilePath + "'."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("Mock Connection Failure"));
        }

        [Test]
        public void ConnectSuccessTest()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
            }

            testMicrosoftAccessMetricLogger.Connect();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void DisconnectWhenNotConnected()
        {
            using (mocks.Ordered)
            {
                SetCheckConnectedExpectations(false);
            }

            testMicrosoftAccessMetricLogger.Disconnect();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void DisconnectException()
        {
            using (mocks.Ordered)
            {
                SetCheckConnectedExpectations(true);
                Expect.Once.On(mockDbConnection).Method("Close").WithNoArguments().Will(Throw.Exception(new Exception("Mock Disconnection Failure")));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testMicrosoftAccessMetricLogger.Disconnect();
            });

            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to connect disconnect from database."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("Mock Disconnection Failure"));
        }

        [Test]
        public void DisconnectSuccessTest()
        {
            using (mocks.Ordered)
            {
                SetCheckConnectedExpectations(true);
                Expect.Once.On(mockDbConnection).Method("Close").WithNoArguments();
            }

            testMicrosoftAccessMetricLogger.Disconnect();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void IncrementDatabaseInsertException()
        {
            String expectedSqlStatement = CreateCountMetricInsertSql(2014, 6, 14, 12, 45, 31, new TestMessageReceivedMetric().Name, testMetricCategoryName);

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014,6,14,12,45,31)));
                Expect.Once.On(mockDbCommand).SetProperty("CommandText").To(expectedSqlStatement.ToString());
                Expect.Once.On(mockDbCommand).Method("ExecuteNonQuery").WithNoArguments().Will(Throw.Exception(new Exception("Mock Query Execution Failure")), Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testMicrosoftAccessMetricLogger.Connect();
            testMicrosoftAccessMetricLogger.Increment(new TestMessageReceivedMetric());
            testMicrosoftAccessMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
            // Using the standard Assert.Throws() syntax NUnit is not able to detect exceptions which occur on worker threads.
            //   Hence to check the correct exceptions have occurred they are written to member exceptionStorer, which is also injected into the test object.
            Assert.IsNotNull(exceptionStorer.StoredException);
            Assert.IsInstanceOf(typeof(Exception), exceptionStorer.StoredException);
            Assert.That(exceptionStorer.StoredException.Message, NUnit.Framework.Is.StringStarting("Failed to insert instance of count metric 'MessageReceived'."));
            Assert.That(exceptionStorer.StoredException.InnerException.Message, NUnit.Framework.Is.StringStarting("Mock Query Execution Failure"));
        }

        [Test]
        public void IncrementSuccessTest()
        {
            String firstExpectedSqlStatement = CreateCountMetricInsertSql(2014, 6, 14, 12, 45, 31, new TestMessageReceivedMetric().Name, testMetricCategoryName);
            String secondExpectedSqlStatement = CreateCountMetricInsertSql(2014, 6, 14, 12, 45, 43, new TestDiskReadOperationMetric().Name, testMetricCategoryName);
            String thirdExpectedSqlStatement = CreateCountMetricInsertSql(2014, 6, 15, 23, 58, 47, new TestMessageReceivedMetric().Name, testMetricCategoryName);

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 14, 12, 45, 31)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 14, 12, 45, 43)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 15, 23, 58, 47)));
                Expect.Once.On(mockDbCommand).SetProperty("CommandText").To(firstExpectedSqlStatement.ToString());
                Expect.Once.On(mockDbCommand).Method("ExecuteNonQuery").WithNoArguments().Will(Return.Value(1));
                Expect.Once.On(mockDbCommand).SetProperty("CommandText").To(secondExpectedSqlStatement.ToString());
                Expect.Once.On(mockDbCommand).Method("ExecuteNonQuery").WithNoArguments().Will(Return.Value(1));
                Expect.Once.On(mockDbCommand).SetProperty("CommandText").To(thirdExpectedSqlStatement.ToString());
                Expect.Once.On(mockDbCommand).Method("ExecuteNonQuery").WithNoArguments().Will(Return.Value(1), Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testMicrosoftAccessMetricLogger.Connect();
            testMicrosoftAccessMetricLogger.Increment(new TestMessageReceivedMetric());
            testMicrosoftAccessMetricLogger.Increment(new TestDiskReadOperationMetric());
            testMicrosoftAccessMetricLogger.Increment(new TestMessageReceivedMetric());
            testMicrosoftAccessMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void AddDatabaseInsertException()
        {
            String expectedSqlStatement = CreateAmountMetricInsertSql(2014, 6, 14, 12, 45, 31, 12345, new TestMessageBytesReceivedMetric(0).Name, testMetricCategoryName);

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 14, 12, 45, 31)));
                Expect.Once.On(mockDbCommand).SetProperty("CommandText").To(expectedSqlStatement.ToString());
                Expect.Once.On(mockDbCommand).Method("ExecuteNonQuery").WithNoArguments().Will(Throw.Exception(new Exception("Mock Query Execution Failure")), Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testMicrosoftAccessMetricLogger.Connect();
            testMicrosoftAccessMetricLogger.Add(new TestMessageBytesReceivedMetric(12345));
            testMicrosoftAccessMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.IsNotNull(exceptionStorer.StoredException);
            Assert.IsInstanceOf(typeof(Exception), exceptionStorer.StoredException);
            Assert.That(exceptionStorer.StoredException.Message, NUnit.Framework.Is.StringStarting("Failed to insert instance of amount metric 'MessageBytesReceived'."));
            Assert.That(exceptionStorer.StoredException.InnerException.Message, NUnit.Framework.Is.StringStarting("Mock Query Execution Failure"));
        }

        [Test]
        public void AddSuccessTest()
        {
            String firstExpectedSqlStatement = CreateAmountMetricInsertSql(2014, 6, 14, 12, 45, 31, 12345, new TestMessageBytesReceivedMetric(0).Name, testMetricCategoryName);
            String secondExpectedSqlStatement = CreateAmountMetricInsertSql(2014, 6, 14, 12, 45, 43, 160307, new TestDiskBytesReadMetric(0).Name, testMetricCategoryName);
            String thirdExpectedSqlStatement = CreateAmountMetricInsertSql(2014, 6, 15, 23, 58, 47, 12347, new TestMessageBytesReceivedMetric(0).Name, testMetricCategoryName);

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 14, 12, 45, 31)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 14, 12, 45, 43)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 15, 23, 58, 47)));
                Expect.Once.On(mockDbCommand).SetProperty("CommandText").To(firstExpectedSqlStatement.ToString());
                Expect.Once.On(mockDbCommand).Method("ExecuteNonQuery").WithNoArguments().Will(Return.Value(1));
                Expect.Once.On(mockDbCommand).SetProperty("CommandText").To(secondExpectedSqlStatement.ToString());
                Expect.Once.On(mockDbCommand).Method("ExecuteNonQuery").WithNoArguments().Will(Return.Value(1));
                Expect.Once.On(mockDbCommand).SetProperty("CommandText").To(thirdExpectedSqlStatement.ToString());
                Expect.Once.On(mockDbCommand).Method("ExecuteNonQuery").WithNoArguments().Will(Return.Value(1), Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testMicrosoftAccessMetricLogger.Connect();
            testMicrosoftAccessMetricLogger.Add(new TestMessageBytesReceivedMetric(12345));
            testMicrosoftAccessMetricLogger.Add(new TestDiskBytesReadMetric(160307));
            testMicrosoftAccessMetricLogger.Add(new TestMessageBytesReceivedMetric(12347));
            testMicrosoftAccessMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SetDatabaseInsertException()
        {
            String expectedSqlStatement = CreateStatusMetricInsertSql(2014, 6, 17, 23, 42, 33, 301156000, new TestAvailableMemoryMetric(0).Name, testMetricCategoryName);

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 17, 23, 42, 33)));
                Expect.Once.On(mockDbCommand).SetProperty("CommandText").To(expectedSqlStatement.ToString());
                Expect.Once.On(mockDbCommand).Method("ExecuteNonQuery").WithNoArguments().Will(Throw.Exception(new Exception("Mock Query Execution Failure")), Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testMicrosoftAccessMetricLogger.Connect();
            testMicrosoftAccessMetricLogger.Set(new TestAvailableMemoryMetric(301156000));
            testMicrosoftAccessMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.IsNotNull(exceptionStorer.StoredException);
            Assert.IsInstanceOf(typeof(Exception), exceptionStorer.StoredException);
            Assert.That(exceptionStorer.StoredException.Message, NUnit.Framework.Is.StringStarting("Failed to insert instance of status metric 'AvailableMemory'."));
            Assert.That(exceptionStorer.StoredException.InnerException.Message, NUnit.Framework.Is.StringStarting("Mock Query Execution Failure"));
        }

        [Test]
        public void SetSuccessTest()
        {
            String firstExpectedSqlStatement = CreateStatusMetricInsertSql(2014, 6, 17, 23, 42, 33, 301156000, new TestAvailableMemoryMetric(0).Name, testMetricCategoryName);
            String secondExpectedSqlStatement = CreateStatusMetricInsertSql(2014, 6, 17, 23, 44, 35, 12, new TestFreeWorkerThreadsMetric(0).Name, testMetricCategoryName);
            String thirdExpectedSqlStatement = CreateStatusMetricInsertSql(2014, 6, 17, 23, 59, 01, 301155987, new TestAvailableMemoryMetric(0).Name, testMetricCategoryName);

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 17, 23, 42, 33)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 17, 23, 44, 35)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 17, 23, 59, 01)));
                Expect.Once.On(mockDbCommand).SetProperty("CommandText").To(firstExpectedSqlStatement.ToString());
                Expect.Once.On(mockDbCommand).Method("ExecuteNonQuery").WithNoArguments().Will(Return.Value(1));
                Expect.Once.On(mockDbCommand).SetProperty("CommandText").To(secondExpectedSqlStatement.ToString());
                Expect.Once.On(mockDbCommand).Method("ExecuteNonQuery").WithNoArguments().Will(Return.Value(1));
                Expect.Once.On(mockDbCommand).SetProperty("CommandText").To(thirdExpectedSqlStatement.ToString());
                Expect.Once.On(mockDbCommand).Method("ExecuteNonQuery").WithNoArguments().Will(Return.Value(1), Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testMicrosoftAccessMetricLogger.Connect();
            testMicrosoftAccessMetricLogger.Set(new TestAvailableMemoryMetric(301156000));
            testMicrosoftAccessMetricLogger.Set(new TestFreeWorkerThreadsMetric(12));
            testMicrosoftAccessMetricLogger.Set(new TestAvailableMemoryMetric(301155987));
            testMicrosoftAccessMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void BeginEndDatabaseInsertException()
        {
            String expectedSqlStatement = CreateIntervalMetricInsertSql(2014, 6, 14, 12, 45, 50, 987, new TestMessageProcessingTimeMetric().Name, testMetricCategoryName);

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 14, 12, 45, 50, 31)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 14, 12, 45, 51, 18)));
                Expect.Once.On(mockDbCommand).SetProperty("CommandText").To(expectedSqlStatement.ToString());
                Expect.Once.On(mockDbCommand).Method("ExecuteNonQuery").WithNoArguments().Will(Throw.Exception(new Exception("Mock Query Execution Failure")), Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testMicrosoftAccessMetricLogger.Connect();
            testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
            testMicrosoftAccessMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.IsNotNull(exceptionStorer.StoredException);
            Assert.IsInstanceOf(typeof(Exception), exceptionStorer.StoredException);
            Assert.That(exceptionStorer.StoredException.Message, NUnit.Framework.Is.StringStarting("Failed to insert instance of interval metric 'MessageProcessingTime'."));
            Assert.That(exceptionStorer.StoredException.InnerException.Message, NUnit.Framework.Is.StringStarting("Mock Query Execution Failure"));
        }

        [Test]
        public void BeginEndDuplicateBeginIntervalEvents()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 14, 12, 45, 50, 31)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 14, 12, 45, 51, 18)), Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testMicrosoftAccessMetricLogger.Connect();
            testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testMicrosoftAccessMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.IsNotNull(exceptionStorer.StoredException);
            Assert.IsInstanceOf(typeof(Exception), exceptionStorer.StoredException);
            Assert.That(exceptionStorer.StoredException.Message, NUnit.Framework.Is.StringStarting("Received duplicate begin 'MessageProcessingTime' metrics."));
        }

        [Test]
        public void BeginEndEndIntervalEventWithNoBegin()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 14, 12, 45, 50, 31)), Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testMicrosoftAccessMetricLogger.Connect();
            testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
            testMicrosoftAccessMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.IsNotNull(exceptionStorer.StoredException);
            Assert.IsInstanceOf(typeof(Exception), exceptionStorer.StoredException);
            Assert.That(exceptionStorer.StoredException.Message, NUnit.Framework.Is.StringStarting("Received end 'MessageProcessingTime' with no corresponding start interval metric."));
        }

        [Test]
        public void BeginEndSuccessTests()
        {
            String firstExpectedSqlStatement = CreateIntervalMetricInsertSql(2014, 6, 14, 12, 45, 31, 34, new TestDiskReadTimeMetric().Name, testMetricCategoryName);
            String secondExpectedSqlStatement = CreateIntervalMetricInsertSql(2014, 6, 14, 12, 45, 43, 0, new TestMessageProcessingTimeMetric().Name, testMetricCategoryName);
            String thirdExpectedSqlStatement = CreateIntervalMetricInsertSql(2014, 6, 15, 23, 58, 47, 1035, new TestMessageProcessingTimeMetric().Name, testMetricCategoryName);

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 14, 12, 45, 31, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 14, 12, 45, 31, 034)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 14, 12, 45, 43, 500)));
                // Note below expect makes the end time before the begin time.  Class should insert the resulting milliseconds interval as 0.
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 14, 12, 45, 43, 499)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 15, 23, 58, 47, 750)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 15, 23, 58, 48, 785)));
                Expect.Once.On(mockDbCommand).SetProperty("CommandText").To(firstExpectedSqlStatement.ToString());
                Expect.Once.On(mockDbCommand).Method("ExecuteNonQuery").WithNoArguments().Will(Return.Value(1));
                Expect.Once.On(mockDbCommand).SetProperty("CommandText").To(secondExpectedSqlStatement.ToString());
                Expect.Once.On(mockDbCommand).Method("ExecuteNonQuery").WithNoArguments().Will(Return.Value(1));
                Expect.Once.On(mockDbCommand).SetProperty("CommandText").To(thirdExpectedSqlStatement.ToString());
                Expect.Once.On(mockDbCommand).Method("ExecuteNonQuery").WithNoArguments().Will(Return.Value(1), Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testMicrosoftAccessMetricLogger.Connect();
            testMicrosoftAccessMetricLogger.Begin(new TestDiskReadTimeMetric());
            testMicrosoftAccessMetricLogger.End(new TestDiskReadTimeMetric());
            testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
            testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
            testMicrosoftAccessMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void BeginEndNestedSuccessTest()
        {
            // Tests correct logging of metrics where an interval metric's begin and end events are wholly nested within the begin and end events of another type of interval metric
            String firstExpectedSqlStatement = CreateIntervalMetricInsertSql(2014, 6, 14, 22, 48, 10, 100, new TestDiskReadTimeMetric().Name, testMetricCategoryName);
            String secondExpectedSqlStatement = CreateIntervalMetricInsertSql(2014, 6, 14, 22, 48, 09, 60005, new TestMessageProcessingTimeMetric().Name, testMetricCategoryName);

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 14, 22, 48, 09, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 14, 22, 48, 10, 250)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 14, 22, 48, 10, 350)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 14, 22, 49, 09, 005)));
                Expect.Once.On(mockDbCommand).SetProperty("CommandText").To(firstExpectedSqlStatement.ToString());
                Expect.Once.On(mockDbCommand).Method("ExecuteNonQuery").WithNoArguments().Will(Return.Value(1));
                Expect.Once.On(mockDbCommand).SetProperty("CommandText").To(secondExpectedSqlStatement.ToString());
                Expect.Once.On(mockDbCommand).Method("ExecuteNonQuery").WithNoArguments().Will(Return.Value(1), Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testMicrosoftAccessMetricLogger.Connect();
            testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testMicrosoftAccessMetricLogger.Begin(new TestDiskReadTimeMetric());
            testMicrosoftAccessMetricLogger.End(new TestDiskReadTimeMetric());
            testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
            testMicrosoftAccessMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void BeginEndNoCheckingSuccessTests()
        {
            // Tests the class with parameter 'intervalMetricChecking' set to false;
            testMicrosoftAccessMetricLogger = new MicrosoftAccessMetricLogger(testDbFilePath, testMetricCategoryName, 10, false, mockDbConnection, mockDbCommand, mockDateTime, exceptionStorer);

            String firstExpectedSqlStatement = CreateIntervalMetricInsertSql(2014, 6, 14, 12, 45, 31, 3600001, new TestMessageProcessingTimeMetric().Name, testMetricCategoryName);
            String secondExpectedSqlStatement = CreateIntervalMetricInsertSql(2014, 6, 15, 23, 58, 47, 1035, new TestMessageProcessingTimeMetric().Name, testMetricCategoryName);

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2013, 6, 14, 12, 45, 31)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 14, 12, 45, 31, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 14, 13, 45, 31, 001)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 14, 13, 45, 31, 002)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 15, 23, 58, 47, 750)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 15, 23, 58, 48, 785)));
                Expect.Once.On(mockDbCommand).SetProperty("CommandText").To(firstExpectedSqlStatement.ToString());
                Expect.Once.On(mockDbCommand).Method("ExecuteNonQuery").WithNoArguments().Will(Return.Value(1));
                Expect.Once.On(mockDbCommand).SetProperty("CommandText").To(secondExpectedSqlStatement.ToString());
                Expect.Once.On(mockDbCommand).Method("ExecuteNonQuery").WithNoArguments().Will(Return.Value(1), Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testMicrosoftAccessMetricLogger.Connect();
            // Tests sending sequential begin events of the same type.  The first should be ignored.
            testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            // Tests correct logging of an interval metric following sequential begin events
            testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
            // Tests sending an end event with no corresponding begin.  This should be ignored.
            testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
            // Tests correct logging of an interval metric following and end with no begin
            testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
            testMicrosoftAccessMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void DisposeSuccessTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockDbCommand).Method("Dispose").WithNoArguments();
                Expect.Once.On(mockDbConnection).Method("Dispose").WithNoArguments();
            }

            testMicrosoftAccessMetricLogger.Dispose();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        //------------------------------------------------------------------------------
        //
        // Method: SetConnectExpectations
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Sets mock expectations for the Connect() method.
        /// </summary>
        private void SetConnectExpectations()
        {
            // Private method CheckConnected() tests the connection state against 4 different values, hence have to mock the GetProperty operation 4 times
            SetCheckConnectedExpectations(false);
            Expect.Once.On(mockDbConnection).SetProperty("ConnectionString").To(accessConnectionStringPrefix + testDbFilePath);
            Expect.Once.On(mockDbConnection).Method("Open").WithNoArguments();
        }

        //------------------------------------------------------------------------------
        //
        // Method: SetCheckConnectedExpectations
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Sets mock expectations for private method CheckConnected().
        /// </summary>
        /// <param name="expectedReturnValue">If true simulates mock returns which will return the connnected status as true.</param>
        private void SetCheckConnectedExpectations(bool expectedReturnValue)
        {
            if (expectedReturnValue == true)
            {
                Expect.Exactly(4).On(mockDbConnection).GetProperty("State").Will(Return.Value(System.Data.ConnectionState.Open));
            }
            else
            {
                Expect.Exactly(4).On(mockDbConnection).GetProperty("State").Will(Return.Value(System.Data.ConnectionState.Closed));
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: CreateCountMetricInsertSql
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Creates a valid Access SQL insert statement for inserting a count metric instance, based on the inputted parameters.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <param name="hour">The hours.</param>
        /// <param name="minute">The minutes.</param>
        /// <param name="second">The seconds.</param>
        /// <param name="countMetricName">The name of the count metric.</param>
        /// <param name="categoryName">The name of the metric category.</param>
        /// <returns>The SQL statement.</returns>
        private string CreateCountMetricInsertSql(int year, int month, int day, int hour, int minute, int second, string countMetricName, string categoryName)
        {
            StringBuilder sqlStatement = new StringBuilder();
            sqlStatement.Append("INSERT ");
            sqlStatement.Append("INTO    CountMetricInstances ");
            sqlStatement.Append("        ( CmetId, ");
            sqlStatement.Append("          CtgrId, ");
            sqlStatement.Append("          [Timestamp] ");
            sqlStatement.Append("          ) ");
            sqlStatement.Append("SELECT  Cmet.CmetId, ");
            sqlStatement.Append("        Ctgr.CtgrId, ");
            sqlStatement.Append("        '" + new System.DateTime(year, month, day, hour, minute, second).ToString("yyyy-MM-dd HH:mm:ss") + "' ");
            sqlStatement.Append("FROM    CountMetrics Cmet, ");
            sqlStatement.Append("        Categories Ctgr ");
            sqlStatement.Append("WHERE   Cmet.Name = '" + countMetricName + "' ");
            sqlStatement.Append("  AND   Ctgr.Name = '" + categoryName + "';");

            return sqlStatement.ToString();
        }

        //------------------------------------------------------------------------------
        //
        // Method: CreateAmountMetricInsertSql
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Creates a valid Access SQL insert statement for inserting an amount metric instance, based on the inputted parameters.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <param name="hour">The hours.</param>
        /// <param name="minute">The minutes.</param>
        /// <param name="second">The seconds.</param>
        /// <param name="amount">The amount value associated with the amount metric.</param>
        /// <param name="amountMetricName">The name of the amount metric.</param>
        /// <param name="categoryName">The name of the metric category.</param>
        /// <returns>The SQL statement.</returns>
        private string CreateAmountMetricInsertSql(int year, int month, int day, int hour, int minute, int second, long amount, string amountMetricName, string categoryName)
        {
            StringBuilder sqlStatement = new StringBuilder();
            sqlStatement.Append("INSERT ");
            sqlStatement.Append("INTO    AmountMetricInstances ");
            sqlStatement.Append("        ( CtgrId, ");
            sqlStatement.Append("          AmetId, ");
            sqlStatement.Append("          Amount, ");
            sqlStatement.Append("          [Timestamp] ");
            sqlStatement.Append("          ) ");
            sqlStatement.Append("SELECT  Ctgr.CtgrId, ");
            sqlStatement.Append("        Amet.AmetId, ");
            sqlStatement.Append("        " + amount.ToString() + ", ");
            sqlStatement.Append("        '" + new System.DateTime(year, month, day, hour, minute, second).ToString("yyyy-MM-dd HH:mm:ss") + "' ");
            sqlStatement.Append("FROM    AmountMetrics Amet, ");
            sqlStatement.Append("        Categories Ctgr ");
            sqlStatement.Append("WHERE   Amet.Name = '" + amountMetricName + "' ");
            sqlStatement.Append("  AND   Ctgr.Name = '" + categoryName + "';");

            return sqlStatement.ToString();
        }

        //------------------------------------------------------------------------------
        //
        // Method: CreateStatusMetricInsertSql
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Creates a valid Access SQL insert statement for inserting a status metric instance, based on the inputted parameters.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <param name="hour">The hours.</param>
        /// <param name="minute">The minutes.</param>
        /// <param name="second">The seconds.</param>
        /// <param name="value">The amount value associated with the status metric.</param>
        /// <param name="statusMetricName">The name of the status metric.</param>
        /// <param name="categoryName">The name of the metric category.</param>
        /// <returns>The SQL statement.</returns>
        private string CreateStatusMetricInsertSql(int year, int month, int day, int hour, int minute, int second, long value, string statusMetricName, string categoryName)
        {
            StringBuilder sqlStatement = new StringBuilder();
            sqlStatement.Append("INSERT ");
            sqlStatement.Append("INTO    StatusMetricInstances ");
            sqlStatement.Append("        ( CtgrId, ");
            sqlStatement.Append("          SmetId, ");
            sqlStatement.Append("          [Value], ");
            sqlStatement.Append("          [Timestamp] ");
            sqlStatement.Append("          ) ");
            sqlStatement.Append("SELECT  Ctgr.CtgrId, ");
            sqlStatement.Append("        Smet.SmetId, ");
            sqlStatement.Append("        " + value.ToString() + ", ");
            sqlStatement.Append("        '" + new System.DateTime(year, month, day, hour, minute, second).ToString("yyyy-MM-dd HH:mm:ss") + "' ");
            sqlStatement.Append("FROM    StatusMetrics Smet, ");
            sqlStatement.Append("        Categories Ctgr ");
            sqlStatement.Append("WHERE   Smet.Name = '" + statusMetricName + "' ");
            sqlStatement.Append("  AND   Ctgr.Name = '" + categoryName + "';");

            return sqlStatement.ToString();
        }

        //------------------------------------------------------------------------------
        //
        // Method: CreateIntervalMetricInsertSql
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Creates a valid Access SQL insert statement for inserting an interval metric instance, based on the inputted parameters.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <param name="hour">The hours.</param>
        /// <param name="minute">The minutes.</param>
        /// <param name="second">The seconds.</param>
        /// <param name="milliseconds">The number of milliseconds in the interval.</param>
        /// <param name="intervalMetricName">The name of the amount metric.</param>
        /// <param name="categoryName">The name of the metric category.</param>
        /// <returns>The SQL statement.</returns>
        private string CreateIntervalMetricInsertSql(int year, int month, int day, int hour, int minute, int second, int milliseconds, string intervalMetricName, string categoryName)
        {
            StringBuilder sqlStatement = new StringBuilder();
            sqlStatement.Append("INSERT ");
            sqlStatement.Append("INTO    IntervalMetricInstances ");
            sqlStatement.Append("        ( CtgrId, ");
            sqlStatement.Append("          ImetId, ");
            sqlStatement.Append("          MilliSeconds, ");
            sqlStatement.Append("          [Timestamp] ");
            sqlStatement.Append("          ) ");
            sqlStatement.Append("SELECT  Ctgr.CtgrId, ");
            sqlStatement.Append("        Imet.ImetId, ");
            sqlStatement.Append("        " + milliseconds.ToString() + ", ");
            sqlStatement.Append("        '" + new System.DateTime(year, month, day, hour, minute, second).ToString("yyyy-MM-dd HH:mm:ss") + "' ");
            sqlStatement.Append("FROM    IntervalMetrics Imet, ");
            sqlStatement.Append("        Categories Ctgr ");
            sqlStatement.Append("WHERE   Imet.Name = '" + intervalMetricName + "' ");
            sqlStatement.Append("  AND   Ctgr.Name = '" + categoryName + "';");

            return sqlStatement.ToString();
        }
    }
}

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
    // Class: PerformanceCounterMetricLoggerTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for class ApplicationMetrics.PerformanceCounterMetricLogger.
    /// </summary>
    /// <remarks>This class also implicity tests the functionality in abstract classes MetricLoggerStorer and MetricAggregateLogger.  Functionality from MetricLoggerStorer and MetricAggregateLogger (e.g. storing of totals of metrics, and calulation of aggregates) does not need to be tested in other classes deriving from MetricAggregateLogger.</remarks>
    class PerformanceCounterMetricLoggerTests
    {
        /* 
         * NOTE: See notes in class MicrosoftAccessMetricLoggerTests regarding testing of underlying worker threads.  The same comments apply to this test class
         */

        private Mockery mocks;
        private ICounterCreationDataCollection mockCounterCreationDataCollection;
        private ICounterCreationDataFactory mockCounterCreationDataFactory;
        private IPerformanceCounterCategory mockPerformanceCounterCategory;
        private IPerformanceCounterFactory mockPerformanceCounterFactory;
        private IPerformanceCounter mockPerformanceCounter;
        private IDateTime mockDateTime;
        private ExceptionStorer exceptionStorer;
        private ICounterCreationData mockCounterCreationData;
        private AutoResetEvent workerThreadLoopCompleteSignal;
        private string testMetricCategoryName = "TestCategory";
        private string testMetricCategoryDescription = "Description of Test Category";
        private PerformanceCounterMetricLogger testPerformanceCounterMetricLogger;

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockCounterCreationDataCollection = mocks.NewMock<ICounterCreationDataCollection>();
            mockCounterCreationDataFactory = mocks.NewMock<ICounterCreationDataFactory>();
            mockPerformanceCounterCategory = mocks.NewMock<IPerformanceCounterCategory>();
            mockPerformanceCounterFactory = mocks.NewMock<IPerformanceCounterFactory>();
            mockPerformanceCounter = mocks.NewMock<IPerformanceCounter>();
            mockDateTime = mocks.NewMock<IDateTime>();
            exceptionStorer = new ExceptionStorer();
            mockCounterCreationData = mocks.NewMock<ICounterCreationData>();
            workerThreadLoopCompleteSignal = new AutoResetEvent(false);
            testPerformanceCounterMetricLogger = new PerformanceCounterMetricLogger(testMetricCategoryName, testMetricCategoryDescription, 10, true, mockCounterCreationDataCollection, mockCounterCreationDataFactory, mockPerformanceCounterCategory, mockPerformanceCounterFactory, mockDateTime, exceptionStorer);
        }

        [Test]
        public void InvalidMetricCategoryNameArgument()
        {
            ArgumentException e = Assert.Throws<ArgumentException>(delegate
            {
                testPerformanceCounterMetricLogger = new PerformanceCounterMetricLogger(" ", testMetricCategoryDescription, 10, true, mockCounterCreationDataCollection, mockCounterCreationDataFactory, mockPerformanceCounterCategory, mockPerformanceCounterFactory, mockDateTime, exceptionStorer);
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Argument 'metricCategoryName' cannot be blank."));
            Assert.AreEqual("metricCategoryName", e.ParamName);
        }

        [Test]
        public void InvalidMetricCategoryDescriptionArgument()
        {
            ArgumentException e = Assert.Throws<ArgumentException>(delegate
            {
                testPerformanceCounterMetricLogger = new PerformanceCounterMetricLogger(testMetricCategoryName, " ", 10, true, mockCounterCreationDataCollection, mockCounterCreationDataFactory, mockPerformanceCounterCategory, mockPerformanceCounterFactory, mockDateTime, exceptionStorer);
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Argument 'metricCategoryDescription' cannot be blank."));
            Assert.AreEqual("metricCategoryDescription", e.ParamName);
        }

        [Test]
        public void DefineMetricAggregateInvalidName()
        {
            // Tests exception when classes deriving from MetricAggregateContainerBase are constructed with a blank name
            ArgumentException e = Assert.Throws<ArgumentException>(delegate
            {
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Second, "", "The number of messages received per second.");
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Argument 'name' cannot be blank."));
            Assert.AreEqual("name", e.ParamName);
        }

        [Test]
        public void DefineMetricAggregateInvalidDescription()
        {
            // Tests exception when classes deriving from MetricAggregateContainerBase are constructed with a blank description
            ArgumentException e = Assert.Throws<ArgumentException>(delegate
            {
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Second, "MessagesReceivedPerSecond", " ");
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Argument 'description' cannot be blank."));
            Assert.AreEqual("description", e.ParamName);
        }

        [Test]
        public void DefineMetricAggregateDuplicateName()
        {
            Exception e = Assert.Throws<Exception>(delegate
            {
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Second, "MessagesReceivedPerSecond", "The number of messages received per second.");
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Second, "MessagesReceivedPerSecond", "The number of messages received per second.");
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Metric aggregate with name 'MessagesReceivedPerSecond' has already been defined."));

            e = Assert.Throws<Exception>(delegate
            {
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestMessageReceivedMetric(), "DuplicateAggregateName", "Duplicate metric aggregate name.");
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Second, "DuplicateAggregateName", "Duplicate metric aggregate name.");
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Metric aggregate with name 'DuplicateAggregateName' has already been defined."));

            e = Assert.Throws<Exception>(delegate
            {
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Second, "DuplicateAggregateName", "Duplicate metric aggregate name.");
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestMessageReceivedMetric(), "DuplicateAggregateName", "Duplicate metric aggregate name.");
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Metric aggregate with name 'DuplicateAggregateName' has already been defined."));

            e = Assert.Throws<Exception>(delegate
            {
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), TimeUnit.Second, "DuplicateAggregateName", "Duplicate metric aggregate name.");
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Second, "DuplicateAggregateName", "Duplicate metric aggregate name.");
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Metric aggregate with name 'DuplicateAggregateName' has already been defined."));

            e = Assert.Throws<Exception>(delegate
            {
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Second, "DuplicateAggregateName", "Duplicate metric aggregate name.");
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), TimeUnit.Second, "DuplicateAggregateName", "Duplicate metric aggregate name.");
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Metric aggregate with name 'DuplicateAggregateName' has already been defined."));

            e = Assert.Throws<Exception>(delegate
            {
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), TimeUnit.Second, "DuplicateAggregateName", "Duplicate metric aggregate name.");
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestDiskBytesReadMetric(0), "DuplicateAggregateName", "Duplicate metric aggregate name.");
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Metric aggregate with name 'DuplicateAggregateName' has already been defined."));

            e = Assert.Throws<Exception>(delegate
            {
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestDiskBytesReadMetric(0), "DuplicateAggregateName", "Duplicate metric aggregate name.");
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), TimeUnit.Second, "DuplicateAggregateName", "Duplicate metric aggregate name.");
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Metric aggregate with name 'DuplicateAggregateName' has already been defined."));

            e = Assert.Throws<Exception>(delegate
            {
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), TimeUnit.Second, "DuplicateAggregateName", "Duplicate metric aggregate name.");
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestDiskReadTimeMetric(), new TestDiskReadOperationMetric(), "DuplicateAggregateName", "Duplicate metric aggregate name.");
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Metric aggregate with name 'DuplicateAggregateName' has already been defined."));

            e = Assert.Throws<Exception>(delegate
            {
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestDiskReadTimeMetric(), new TestDiskReadOperationMetric(), "DuplicateAggregateName", "Duplicate metric aggregate name.");
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), TimeUnit.Second, "DuplicateAggregateName", "Duplicate metric aggregate name.");
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Metric aggregate with name 'DuplicateAggregateName' has already been defined."));
        }

        [Test]
        public void CreatePerformanceCountersExistsTestException()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessagesReceivedPerSecond", "The number of messages received per second", System.Diagnostics.PerformanceCounterType.NumberOfItems64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(1));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessagesReceivedPerSecondInstantaneous", "The number of messages received per second (instantaneous counter)", System.Diagnostics.PerformanceCounterType.RateOfCountsPerSecond64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(2));
                Expect.Once.On(mockPerformanceCounterCategory).Method("Exists").With(testMetricCategoryName).Will(Throw.Exception(new UnauthorizedAccessException("Test inner exception")));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Second, "MessagesReceivedPerSecond", "The number of messages received per second");
                testPerformanceCounterMetricLogger.CreatePerformanceCounters();
            });

            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to create performance counter category."));
            Assert.AreEqual(typeof(UnauthorizedAccessException), e.InnerException.GetType());
        }

        [Test]
        public void CreatePerformanceCountersException()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessagesReceivedPerSecond", "The number of messages received per second", System.Diagnostics.PerformanceCounterType.NumberOfItems64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(1));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessagesReceivedPerSecondInstantaneous", "The number of messages received per second (instantaneous counter)", System.Diagnostics.PerformanceCounterType.RateOfCountsPerSecond64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(2));
                Expect.Once.On(mockPerformanceCounterCategory).Method("Exists").With(testMetricCategoryName).Will(Return.Value(true));
                Expect.Once.On(mockPerformanceCounterCategory).Method("Delete").With(testMetricCategoryName);
                Expect.Once.On(mockPerformanceCounterCategory).Method("Create").With(testMetricCategoryName, testMetricCategoryDescription, System.Diagnostics.PerformanceCounterCategoryType.SingleInstance, mockCounterCreationDataCollection).Will(Throw.Exception(new System.ComponentModel.Win32Exception("Test inner exception")));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Second, "MessagesReceivedPerSecond", "The number of messages received per second");
                testPerformanceCounterMetricLogger.CreatePerformanceCounters();
            });

            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to create performance counter category."));
            Assert.AreEqual(typeof(System.ComponentModel.Win32Exception), e.InnerException.GetType());
        }

        [Test]
        public void CreatePerformanceCountersInvalidMetricName()
        {
            Exception e = Assert.Throws<Exception>(delegate
            {
                testPerformanceCounterMetricLogger.RegisterMetric(new BlankNameMetric());
                testPerformanceCounterMetricLogger.CreatePerformanceCounters();
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("The 'Name' property of metric " + new BlankNameMetric().GetType().FullName + " is blank."));

            e = Assert.Throws<Exception>(delegate
            {
                testPerformanceCounterMetricLogger = new PerformanceCounterMetricLogger(testMetricCategoryName, testMetricCategoryDescription, 10, true, mockCounterCreationDataCollection, mockCounterCreationDataFactory, mockPerformanceCounterCategory, mockPerformanceCounterFactory, mockDateTime, exceptionStorer);
                testPerformanceCounterMetricLogger.RegisterMetric(new LongNameMetric());
                testPerformanceCounterMetricLogger.CreatePerformanceCounters();
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("The 'Name' property of metric " + new LongNameMetric().GetType().FullName + " exceeds the 80 character limit imposed by Windows performance counters."));

            e = Assert.Throws<Exception>(delegate
            {
                testPerformanceCounterMetricLogger = new PerformanceCounterMetricLogger(testMetricCategoryName, testMetricCategoryDescription, 10, true, mockCounterCreationDataCollection, mockCounterCreationDataFactory, mockPerformanceCounterCategory, mockPerformanceCounterFactory, mockDateTime, exceptionStorer);
                testPerformanceCounterMetricLogger.RegisterMetric(new WhitespaceNameMetric());
                testPerformanceCounterMetricLogger.CreatePerformanceCounters();
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("The 'Name' property of metric " + new WhitespaceNameMetric().GetType().FullName + " cannot contain leading or trailing whitespace."));

            e = Assert.Throws<Exception>(delegate
            {
                testPerformanceCounterMetricLogger = new PerformanceCounterMetricLogger(testMetricCategoryName, testMetricCategoryDescription, 10, true, mockCounterCreationDataCollection, mockCounterCreationDataFactory, mockPerformanceCounterCategory, mockPerformanceCounterFactory, mockDateTime, exceptionStorer);
                testPerformanceCounterMetricLogger.RegisterMetric(new DoubleQuoteNameMetric());
                testPerformanceCounterMetricLogger.CreatePerformanceCounters();
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("The 'Name' property of metric " + new DoubleQuoteNameMetric().GetType().FullName + " cannot contain the '\"' character."));

            e = Assert.Throws<Exception>(delegate
            {
                testPerformanceCounterMetricLogger = new PerformanceCounterMetricLogger(testMetricCategoryName, testMetricCategoryDescription, 10, true, mockCounterCreationDataCollection, mockCounterCreationDataFactory, mockPerformanceCounterCategory, mockPerformanceCounterFactory, mockDateTime, exceptionStorer);
                testPerformanceCounterMetricLogger.RegisterMetric(new ControlCharacterNameMetric());
                testPerformanceCounterMetricLogger.CreatePerformanceCounters();
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("The 'Name' property of metric " + new ControlCharacterNameMetric().GetType().FullName + " cannot contain control characters."));
        }

        [Test]
        public void CreatePerformanceCountersInvalidMetricAggregateName()
        {
            ArgumentException e = Assert.Throws<ArgumentException>(delegate
            {
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Second, "1234567890123456789012345678901234567890123456789012345678901234", "Test metric");
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Argument 'name' cannot exceed 63 characters."));
            Assert.AreEqual("name", e.ParamName);

            e = Assert.Throws<ArgumentException>(delegate
            {
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestMessageReceivedMetric(), " WhitespaceName ", "Test metric");
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Argument 'name' cannot contain leading or trailing whitespace."));
            Assert.AreEqual("name", e.ParamName);

            e = Assert.Throws<ArgumentException>(delegate
            {
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), TimeUnit.Second, "DoubleQuote\"Name", "Test metric");
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Argument 'name' cannot contain the '\"' character."));
            Assert.AreEqual("name", e.ParamName);

            e = Assert.Throws<ArgumentException>(delegate
            {
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestDiskBytesReadMetric(0), "12345678901234567890123456789012345678901234567890123456789012345678901234567", "Test metric");
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Argument 'name' cannot exceed 76 characters."));
            Assert.AreEqual("name", e.ParamName);

            e = Assert.Throws<ArgumentException>(delegate
            {
                char controlCharacter = (char)0x02;
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestDiskReadTimeMetric(), new TestMessageReceivedMetric(), "ControlCharacter" + controlCharacter.ToString() + "Name", "Test metric");
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Argument 'name' cannot contain control characters."));
            Assert.AreEqual("name", e.ParamName);

            e = Assert.Throws<ArgumentException>(delegate
            {
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestDiskReadTimeMetric(), "12345678901234567890123456789012345678901234567890123456789012345678901234567", "Test metric");
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Argument 'name' cannot exceed 76 characters."));
            Assert.AreEqual("name", e.ParamName);
        }

        [Test]
        public void CountMetricCreatePerformanceCountersSuccessTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With(new TestMessageReceivedMetric().Name, new TestMessageReceivedMetric().Description, System.Diagnostics.PerformanceCounterType.NumberOfItems64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(1));
                Expect.Once.On(mockPerformanceCounterCategory).Method("Exists").With(testMetricCategoryName).Will(Return.Value(true));
                Expect.Once.On(mockPerformanceCounterCategory).Method("Delete").With(testMetricCategoryName);
                Expect.Once.On(mockPerformanceCounterCategory).Method("Create").With(testMetricCategoryName, testMetricCategoryDescription, System.Diagnostics.PerformanceCounterCategoryType.SingleInstance, mockCounterCreationDataCollection);
            }

            testPerformanceCounterMetricLogger.RegisterMetric(new TestMessageReceivedMetric());
            testPerformanceCounterMetricLogger.CreatePerformanceCounters();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void CountOverTimeUnitMetricAggregateCreatePerformanceCountersSuccessTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessagesReceivedPerSecond", "The number of messages received per second", System.Diagnostics.PerformanceCounterType.NumberOfItems64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(1));
                // Aggregates with a time unit denominator of seconds should create an instantaneous counter of type RateOfCountsPerSecond64
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessagesReceivedPerSecondInstantaneous", "The number of messages received per second (instantaneous counter)", System.Diagnostics.PerformanceCounterType.RateOfCountsPerSecond64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(2));
                // Aggregates with a time unit denominator of minute, day, or hour should create an instantaneous counter of type AverageCount64 and corresponding base counter
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessagesReceivedPerMinute", "The number of messages received per minute", System.Diagnostics.PerformanceCounterType.NumberOfItems64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(3));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessagesReceivedPerMinuteInstantaneous", "The number of messages received per minute (instantaneous counter)", System.Diagnostics.PerformanceCounterType.AverageCount64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(4));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessagesReceivedPerMinuteInstantaneousBase", "The number of messages received per minute (instantaneous base counter)", System.Diagnostics.PerformanceCounterType.AverageBase).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(5));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessagesReceivedPerHour", "The number of messages received per hour", System.Diagnostics.PerformanceCounterType.NumberOfItems64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(6));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessagesReceivedPerHourInstantaneous", "The number of messages received per hour (instantaneous counter)", System.Diagnostics.PerformanceCounterType.AverageCount64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(7));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessagesReceivedPerHourInstantaneousBase", "The number of messages received per hour (instantaneous base counter)", System.Diagnostics.PerformanceCounterType.AverageBase).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(8));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessagesReceivedPerDay", "The number of messages received per day", System.Diagnostics.PerformanceCounterType.NumberOfItems64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(9));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessagesReceivedPerDayInstantaneous", "The number of messages received per day (instantaneous counter)", System.Diagnostics.PerformanceCounterType.AverageCount64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(10));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessagesReceivedPerDayInstantaneousBase", "The number of messages received per day (instantaneous base counter)", System.Diagnostics.PerformanceCounterType.AverageBase).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(11));
                Expect.Once.On(mockPerformanceCounterCategory).Method("Exists").With(testMetricCategoryName).Will(Return.Value(true));
                Expect.Once.On(mockPerformanceCounterCategory).Method("Delete").With(testMetricCategoryName);
                Expect.Once.On(mockPerformanceCounterCategory).Method("Create").With(testMetricCategoryName, testMetricCategoryDescription, System.Diagnostics.PerformanceCounterCategoryType.SingleInstance, mockCounterCreationDataCollection);
            }

            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Second, "MessagesReceivedPerSecond", "The number of messages received per second");
            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Minute, "MessagesReceivedPerMinute", "The number of messages received per minute");
            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Hour, "MessagesReceivedPerHour", "The number of messages received per hour");
            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Day, "MessagesReceivedPerDay", "The number of messages received per day");
            testPerformanceCounterMetricLogger.CreatePerformanceCounters();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void AmountMetricCreatePerformanceCountersSuccessTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With(new TestMessageBytesReceivedMetric(0).Name, new TestMessageBytesReceivedMetric(0).Description, System.Diagnostics.PerformanceCounterType.NumberOfItems64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(1));
                Expect.Once.On(mockPerformanceCounterCategory).Method("Exists").With(testMetricCategoryName).Will(Return.Value(true));
                Expect.Once.On(mockPerformanceCounterCategory).Method("Delete").With(testMetricCategoryName);
                Expect.Once.On(mockPerformanceCounterCategory).Method("Create").With(testMetricCategoryName, testMetricCategoryDescription, System.Diagnostics.PerformanceCounterCategoryType.SingleInstance, mockCounterCreationDataCollection);
            }

            testPerformanceCounterMetricLogger.RegisterMetric(new TestMessageBytesReceivedMetric(0));
            testPerformanceCounterMetricLogger.CreatePerformanceCounters();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void AmountOverCountMetricAggregateCreatePerformanceCountersSuccessTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("BytesReceivedPerMessage", "The number of bytes received per message", System.Diagnostics.PerformanceCounterType.NumberOfItems64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(1));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("BytesReceivedPerMessageInstantaneous", "The number of bytes received per message (instantaneous counter)", System.Diagnostics.PerformanceCounterType.AverageCount64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(2));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("BytesReceivedPerMessageInstantaneousBase", "The number of bytes received per message (instantaneous base counter)", System.Diagnostics.PerformanceCounterType.AverageBase).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(3));
                Expect.Once.On(mockPerformanceCounterCategory).Method("Exists").With(testMetricCategoryName).Will(Return.Value(true));
                Expect.Once.On(mockPerformanceCounterCategory).Method("Delete").With(testMetricCategoryName);
                Expect.Once.On(mockPerformanceCounterCategory).Method("Create").With(testMetricCategoryName, testMetricCategoryDescription, System.Diagnostics.PerformanceCounterCategoryType.SingleInstance, mockCounterCreationDataCollection);
            }

            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestMessageReceivedMetric(), "BytesReceivedPerMessage", "The number of bytes received per message");
            testPerformanceCounterMetricLogger.CreatePerformanceCounters();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void AmountOverTimeUnitMetricAggregateCreatePerformanceCountersSuccessTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessageBytesReceivedPerSecond", "The number of message bytes received per second", System.Diagnostics.PerformanceCounterType.NumberOfItems64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(1));
                // Aggregates with a time unit denominator of seconds should create an instantaneous counter of type RateOfCountsPerSecond64
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessageBytesReceivedPerSecondInstantaneous", "The number of message bytes received per second (instantaneous counter)", System.Diagnostics.PerformanceCounterType.RateOfCountsPerSecond64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(2));
                // Aggregates with a time unit denominator of minute, day, or hour should create an instantaneous counter of type AverageCount64 and corresponding base counter
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessageBytesReceivedPerMinute", "The number of message bytes received per minute", System.Diagnostics.PerformanceCounterType.NumberOfItems64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(3));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessageBytesReceivedPerMinuteInstantaneous", "The number of message bytes received per minute (instantaneous counter)", System.Diagnostics.PerformanceCounterType.AverageCount64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(4));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessageBytesReceivedPerMinuteInstantaneousBase", "The number of message bytes received per minute (instantaneous base counter)", System.Diagnostics.PerformanceCounterType.AverageBase).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(5));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessageBytesReceivedPerHour", "The number of message bytes received per hour", System.Diagnostics.PerformanceCounterType.NumberOfItems64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(6));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessageBytesReceivedPerHourInstantaneous", "The number of message bytes received per hour (instantaneous counter)", System.Diagnostics.PerformanceCounterType.AverageCount64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(7));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessageBytesReceivedPerHourInstantaneousBase", "The number of message bytes received per hour (instantaneous base counter)", System.Diagnostics.PerformanceCounterType.AverageBase).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(8));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessageBytesReceivedPerDay", "The number of message bytes received per day", System.Diagnostics.PerformanceCounterType.NumberOfItems64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(9));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessageBytesReceivedPerDayInstantaneous", "The number of message bytes received per day (instantaneous counter)", System.Diagnostics.PerformanceCounterType.AverageCount64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(10));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessageBytesReceivedPerDayInstantaneousBase", "The number of message bytes received per day (instantaneous base counter)", System.Diagnostics.PerformanceCounterType.AverageBase).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(11));
                Expect.Once.On(mockPerformanceCounterCategory).Method("Exists").With(testMetricCategoryName).Will(Return.Value(true));
                Expect.Once.On(mockPerformanceCounterCategory).Method("Delete").With(testMetricCategoryName);
                Expect.Once.On(mockPerformanceCounterCategory).Method("Create").With(testMetricCategoryName, testMetricCategoryDescription, System.Diagnostics.PerformanceCounterCategoryType.SingleInstance, mockCounterCreationDataCollection);
            }

            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), TimeUnit.Second, "MessageBytesReceivedPerSecond", "The number of message bytes received per second");
            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), TimeUnit.Minute, "MessageBytesReceivedPerMinute", "The number of message bytes received per minute");
            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), TimeUnit.Hour, "MessageBytesReceivedPerHour", "The number of message bytes received per hour");
            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), TimeUnit.Day, "MessageBytesReceivedPerDay", "The number of message bytes received per day");
            testPerformanceCounterMetricLogger.CreatePerformanceCounters();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void AmountOverAmountMetricAggregateCreatePerformanceCountersSuccessTest()
        {
            Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessageBytesReceivedPerDiskBytesRead", "The number of message bytes received per disk bytes read", System.Diagnostics.PerformanceCounterType.RawFraction).Will(Return.Value(mockCounterCreationData));
            Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(1));
            Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessageBytesReceivedPerDiskBytesReadBase", "The number of message bytes received per disk bytes read (base counter)", System.Diagnostics.PerformanceCounterType.RawBase).Will(Return.Value(mockCounterCreationData));
            Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(2));
            Expect.Once.On(mockPerformanceCounterCategory).Method("Exists").With(testMetricCategoryName).Will(Return.Value(true));
            Expect.Once.On(mockPerformanceCounterCategory).Method("Delete").With(testMetricCategoryName);
            Expect.Once.On(mockPerformanceCounterCategory).Method("Create").With(testMetricCategoryName, testMetricCategoryDescription, System.Diagnostics.PerformanceCounterCategoryType.SingleInstance, mockCounterCreationDataCollection);

            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestDiskBytesReadMetric(0), "MessageBytesReceivedPerDiskBytesRead", "The number of message bytes received per disk bytes read");
            testPerformanceCounterMetricLogger.CreatePerformanceCounters();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void IntervalOverCountMetricAggregateCreatePerformanceCountersSuccessTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("ProcessingTimePerMessage", "The average time to process each message", System.Diagnostics.PerformanceCounterType.NumberOfItems64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(1));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("ProcessingTimePerMessageInstantaneous", "The average time to process each message (instantaneous counter)", System.Diagnostics.PerformanceCounterType.AverageCount64).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(2));
                Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("ProcessingTimePerMessageInstantaneousBase", "The average time to process each message (instantaneous base counter)", System.Diagnostics.PerformanceCounterType.AverageBase).Will(Return.Value(mockCounterCreationData));
                Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(3));
                Expect.Once.On(mockPerformanceCounterCategory).Method("Exists").With(testMetricCategoryName).Will(Return.Value(true));
                Expect.Once.On(mockPerformanceCounterCategory).Method("Delete").With(testMetricCategoryName);
                Expect.Once.On(mockPerformanceCounterCategory).Method("Create").With(testMetricCategoryName, testMetricCategoryDescription, System.Diagnostics.PerformanceCounterCategoryType.SingleInstance, mockCounterCreationDataCollection);
            }

            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageProcessingTimeMetric(), new TestMessageReceivedMetric(), "ProcessingTimePerMessage", "The average time to process each message");
            testPerformanceCounterMetricLogger.CreatePerformanceCounters();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void IntervalOverTotalRunTimeMetricAggregateCreatePerformanceCountersSuccessTest()
        {
            Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessageProcessingTimePercentage", "The amount of time spent processing messages as a percentage of total run time", System.Diagnostics.PerformanceCounterType.RawFraction).Will(Return.Value(mockCounterCreationData));
            Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(1));
            Expect.Once.On(mockCounterCreationDataFactory).Method("Create").With("MessageProcessingTimePercentageBase", "The amount of time spent processing messages as a percentage of total run time (base counter)", System.Diagnostics.PerformanceCounterType.RawBase).Will(Return.Value(mockCounterCreationData));
            Expect.Once.On(mockCounterCreationDataCollection).Method("Add").With(mockCounterCreationData).Will(Return.Value(2));
            Expect.Once.On(mockPerformanceCounterCategory).Method("Exists").With(testMetricCategoryName).Will(Return.Value(true));
            Expect.Once.On(mockPerformanceCounterCategory).Method("Delete").With(testMetricCategoryName);
            Expect.Once.On(mockPerformanceCounterCategory).Method("Create").With(testMetricCategoryName, testMetricCategoryDescription, System.Diagnostics.PerformanceCounterCategoryType.SingleInstance, mockCounterCreationDataCollection);

            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageProcessingTimeMetric(), "MessageProcessingTimePercentage", "The amount of time spent processing messages as a percentage of total run time");
            testPerformanceCounterMetricLogger.CreatePerformanceCounters();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void RegisterMetricAlreadyRegisteredException()
        {
            ArgumentException e = Assert.Throws<ArgumentException>(delegate
            {
                testPerformanceCounterMetricLogger.RegisterMetric(new TestMessageReceivedMetric());
                testPerformanceCounterMetricLogger.RegisterMetric(new TestMessageReceivedMetric());
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Metric of type '" + typeof(TestMessageReceivedMetric).Name + "' has already been registered."));
            Assert.AreEqual("metric", e.ParamName);
        }

        [Test]
        public void RegisterMetricAggregateWithSameNameAlreadyRegisteredException()
        {
            Exception e = Assert.Throws<Exception>(delegate
            {
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Second, new TestDiskReadOperationMetric().Name, "Test description.");
                testPerformanceCounterMetricLogger.RegisterMetric(new TestDiskReadOperationMetric());
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Metric or metric aggregate with name '" + new TestDiskReadOperationMetric().Name + "' has already been registered."));
        }

        [Test]
        public void DefineMetricAggregateMetricWithSameNameAlreadyRegisteredException()
        {
            Exception e = Assert.Throws<Exception>(delegate
            {
                testPerformanceCounterMetricLogger.RegisterMetric(new TestDiskReadOperationMetric());
                testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Second, new TestDiskReadOperationMetric().Name, "Test description.");
            });

            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Metric or metric aggregate with name '" + new TestDiskReadOperationMetric().Name + "' has already been registered."));
        }

        [Test]
        public void DisposeSuccessTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, new TestDiskReadOperationMetric().Name, false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, new TestMessageReceivedMetric().Name, false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "BytesReceivedPerMessage", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "BytesReceivedPerMessageInstantaneous", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "BytesReceivedPerMessageInstantaneousBase", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 3, 21, 20, 39)));
                Expect.Exactly(5).On(mockPerformanceCounter).Method("Dispose").WithNoArguments();
            }

            testPerformanceCounterMetricLogger.RegisterMetric(new TestDiskReadOperationMetric());
            testPerformanceCounterMetricLogger.RegisterMetric(new TestMessageReceivedMetric());
            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestMessageReceivedMetric(), "BytesReceivedPerMessage", "The number of bytes received per message");
            testPerformanceCounterMetricLogger.Start();
            testPerformanceCounterMetricLogger.Stop();
            testPerformanceCounterMetricLogger.Dispose();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogCountMetricTotalSuccessTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 26, 22, 49, 01)));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, (new TestMessageReceivedMetric()).Name, false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 26, 22, 49, 03)));
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(1L).Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testPerformanceCounterMetricLogger.RegisterMetric(new TestMessageReceivedMetric());
            testPerformanceCounterMetricLogger.Increment(new TestMessageReceivedMetric());
            testPerformanceCounterMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogAmountMetricTotalSuccessTest()
        {
            using (mocks.Ordered)
            {
                // Expects for calls to Add()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 12, 09, 32, 02)));
                // Expects for calls to Start()
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, (new TestMessageBytesReceivedMetric(0)).Name, false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 12, 09, 34, 13)));
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(1024L).Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testPerformanceCounterMetricLogger.RegisterMetric(new TestMessageBytesReceivedMetric(0));
            testPerformanceCounterMetricLogger.Add(new TestMessageBytesReceivedMetric(1024));
            testPerformanceCounterMetricLogger.Start();
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
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, (new TestAvailableMemoryMetric(0)).Name, false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, (new TestFreeWorkerThreadsMetric(0)).Name, false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 14, 22, 54, 00)));
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(714768384L);
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(8L).Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testPerformanceCounterMetricLogger.RegisterMetric(new TestAvailableMemoryMetric(0));
            testPerformanceCounterMetricLogger.RegisterMetric(new TestFreeWorkerThreadsMetric(0));
            testPerformanceCounterMetricLogger.Set(new TestAvailableMemoryMetric(80740352));
            testPerformanceCounterMetricLogger.Set(new TestFreeWorkerThreadsMetric(8));
            testPerformanceCounterMetricLogger.Set(new TestAvailableMemoryMetric(714768384));
            testPerformanceCounterMetricLogger.Start();
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
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, (new TestMessageProcessingTimeMetric()).Name, false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, (new TestDiskReadTimeMetric()).Name, false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 14, 22, 54, 00)));
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(3737L);
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(67889L).Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testPerformanceCounterMetricLogger.RegisterMetric(new TestMessageProcessingTimeMetric());
            testPerformanceCounterMetricLogger.RegisterMetric(new TestDiskReadTimeMetric());
            testPerformanceCounterMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testPerformanceCounterMetricLogger.Begin(new TestDiskReadTimeMetric());
            testPerformanceCounterMetricLogger.End(new TestDiskReadTimeMetric());
            testPerformanceCounterMetricLogger.End(new TestMessageProcessingTimeMetric());
            testPerformanceCounterMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testPerformanceCounterMetricLogger.End(new TestMessageProcessingTimeMetric());
            testPerformanceCounterMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void DontLogMetricWhenNotRegisteredSuccessTest()
        {
            using (mocks.Ordered)
            {
                // Expects for the calls to Increment(), Add() etc...
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 26, 22, 49, 01)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 26, 22, 49, 03)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 26, 22, 49, 05)));
                // Expects for calls to Start()
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, (new TestMessageReceivedMetric()).Name, false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 6, 26, 22, 49, 55)));
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(1L).Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testPerformanceCounterMetricLogger.RegisterMetric(new TestMessageReceivedMetric());
            // Tests that below metrics other than TestMessageReceivedMetric are not logged
            testPerformanceCounterMetricLogger.Increment(new TestDiskReadOperationMetric());
            testPerformanceCounterMetricLogger.Add(new TestMessageBytesReceivedMetric(1024));
            testPerformanceCounterMetricLogger.Increment(new TestMessageReceivedMetric());
            testPerformanceCounterMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogCountOverTimeUnitAggregateSuccessTest()
        {
            using (mocks.Ordered)
            {
                // Expects for the calls to Increment()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 3, 22, 52, 39, 500)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 3, 22, 52, 40, 500)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 3, 22, 52, 41, 333)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 3, 22, 52, 41, 666)));
                // Expects for calls to Start()
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessagesReceivedPerSecond", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessagesReceivedPerSecondInstantaneous", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessagesReceivedPerMinute", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessagesReceivedPerMinuteInstantaneous", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessagesReceivedPerMinuteInstantaneousBase", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessagesReceivedPerDay", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessagesReceivedPerDayInstantaneous", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessagesReceivedPerDayInstantaneousBase", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 3, 22, 52, 39, 000)));
                // Expects for calls to MetricAggregateLogger.LogCountOverTimeUnitAggregates() and PerformanceCounterMetricLoggerImplementation.LogCountOverTimeUnitAggregate()
                //   ... simulates 4 messages received over elapsed time of 3 seconds (average of 1 message per second)
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 3, 22, 52, 42, 000)));
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(1L);
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(4L);
                //   ... simluates 4 messages received over elapsed time of 2 minutes (average of 2 messages per minute)
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 3, 22, 54, 42, 000)));
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(2L);
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(4L);
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(2L);
                //   ... simluates elapsed time of 0 days, hence no counter values are written
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 3, 22, 54, 42, 000)), Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Second, "MessagesReceivedPerSecond", "The number of messages received per second");
            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Minute, "MessagesReceivedPerMinute", "The number of messages received per minute");
            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Day, "MessagesReceivedPerDay", "The number of messages received per day");
            for (int i = 0; i < 4; i++)
            {
                testPerformanceCounterMetricLogger.Increment(new TestMessageReceivedMetric());
            }
            testPerformanceCounterMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();
            // Wait a few more milliseconds so that any unexpected method calls after the signal are caught
            Thread.Sleep(50);

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogCountOverTimeUnitAggregateNoInstancesSuccessTest()
        {
            // Tests defining a count over time unit aggregate, where no instances of the underlying count metric have been logged

            using (mocks.Ordered)
            {
                // Expects for calls to Start()
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessagesReceivedPerSecond", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessagesReceivedPerSecondInstantaneous", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 11, 23, 30, 42, 000)));
                // Expects for calls to MetricAggregateLogger.LogCountOverTimeUnitAggregates() and PerformanceCounterMetricLoggerImplementation.LogCountOverTimeUnitAggregate()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 11, 23, 30, 47, 000)));
                //   ... simluates 0 messages received over elapsed time of 5 seconds
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(0L);
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(0L).Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.Second, "MessagesReceivedPerSecond", "The number of messages received per second");
            testPerformanceCounterMetricLogger.Start();
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
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 13, 10, 41, 31, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 13, 10, 41, 31, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 13, 10, 41, 44, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 13, 10, 41, 44, 000)));
                // Expects for calls to Start()
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "BytesReceivedPerMessage", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "BytesReceivedPerMessageInstantaneous", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "BytesReceivedPerMessageInstantaneousBase", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 13, 10, 40, 05, 000)));
                // Expects for calls to PerformanceCounterMetricLoggerImplementation.LogAmountOverCountAggregate()
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(254L);
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(509L);
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(2L).Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestMessageReceivedMetric(), "BytesReceivedPerMessage", "The number of bytes received per message");
            testPerformanceCounterMetricLogger.Add(new TestMessageBytesReceivedMetric(125));
            testPerformanceCounterMetricLogger.Increment(new TestMessageReceivedMetric());
            testPerformanceCounterMetricLogger.Add(new TestMessageBytesReceivedMetric(384));
            testPerformanceCounterMetricLogger.Increment(new TestMessageReceivedMetric());
            testPerformanceCounterMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();
            // Wait a few more milliseconds so that any unexpected method calls after the signal are caught
            Thread.Sleep(50);

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogAmountOverCountAggregateNoInstancesSuccessTest()
        {
            // Tests defining an amount over count aggregate, where no instances of the underlying count metric have been logged

            using (mocks.Ordered)
            {
                // Expects for the calls to Add()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 13, 10, 41, 31, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 13, 10, 41, 44, 000)));
                // Expects for calls to Start()
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "BytesReceivedPerMessage", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "BytesReceivedPerMessageInstantaneous", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "BytesReceivedPerMessageInstantaneousBase", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 13, 10, 40, 05, 000)), Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
                // Expects for calls to PerformanceCounterMetricLoggerImplementation.LogAmountOverCountAggregate()
                Expect.Never.On(mockPerformanceCounter);
            }

            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestMessageReceivedMetric(), "BytesReceivedPerMessage", "The number of bytes received per message");
            testPerformanceCounterMetricLogger.Add(new TestMessageBytesReceivedMetric(125));
            testPerformanceCounterMetricLogger.Add(new TestMessageBytesReceivedMetric(384));
            testPerformanceCounterMetricLogger.Start();
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
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 3, 22, 52, 39, 500)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 3, 22, 52, 40, 500)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 3, 22, 52, 41, 333)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 3, 22, 52, 41, 666)));
                // Expects for calls to Start()
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessageBytesReceivedPerSecond", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessageBytesReceivedPerSecondInstantaneous", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessageBytesReceivedPerMinute", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessageBytesReceivedPerMinuteInstantaneous", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessageBytesReceivedPerMinuteInstantaneousBase", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessageBytesReceivedPerDay", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessageBytesReceivedPerDayInstantaneous", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessageBytesReceivedPerDayInstantaneousBase", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 3, 22, 52, 39, 000)));
                // Expects for calls to MetricAggregateLogger.LogAmountOverTimeUnitAggregates() and PerformanceCounterMetricLoggerImplementation.LogAmountOverTimeUnitAggregate()
                //   ... simulates 1118 bytes received over elapsed time of 3 seconds (average of 373 bytes per second)
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 3, 22, 52, 42, 000)));
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(373L);
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(1118L);
                //   ... simluates 1118 bytes received over elapsed time of 2 minutes (average of 559 bytes per minute)
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 3, 22, 54, 42, 000)));
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(559L);
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(1118L);
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(2L);
                //   ... simluates elapsed time of 0 days, hence no counter values are written
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 3, 22, 54, 42, 000)), Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), TimeUnit.Second, "MessageBytesReceivedPerSecond", "The number of message bytes received per second");
            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), TimeUnit.Minute, "MessageBytesReceivedPerMinute", "The number of message bytes received per minute");
            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), TimeUnit.Day, "MessageBytesReceivedPerDay", "The number of message bytes received per day");
            testPerformanceCounterMetricLogger.Add(new TestMessageBytesReceivedMetric(149));
            testPerformanceCounterMetricLogger.Add(new TestMessageBytesReceivedMetric(257));
            testPerformanceCounterMetricLogger.Add(new TestMessageBytesReceivedMetric(439));
            testPerformanceCounterMetricLogger.Add(new TestMessageBytesReceivedMetric(273));
            testPerformanceCounterMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();
            // Wait a few more milliseconds so that any unexpected method calls after the signal are caught
            Thread.Sleep(50);

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogAmountOverTimeUnitAggregateNoInstancesSuccessTest()
        {
            // Tests defining an amount over time unit aggregate, where no instances of the underlying amount metric have been logged

            using (mocks.Ordered)
            {
                // Expects for calls to Start()
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessageBytesReceivedPerSecond", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessageBytesReceivedPerSecondInstantaneous", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 11, 23, 30, 42, 000)));
                // Expects for calls to MetricAggregateLogger.LogCountOverTimeUnitAggregates() and PerformanceCounterMetricLoggerImplementation.LogCountOverTimeUnitAggregate()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 7, 11, 23, 30, 47, 000)));
                //   ... simluates 0 messages received over elapsed time of 5 seconds
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(0L);
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(0L).Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), TimeUnit.Second, "MessageBytesReceivedPerSecond", "The number of message bytes received per second");
            testPerformanceCounterMetricLogger.Start();
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
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 21, 45, 39, 500)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 21, 45, 39, 570)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 21, 45, 58, 333)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 21, 45, 58, 603)));
                // Expects for calls to Start()
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessageBytesReceivedPerDiskBytesRead", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessageBytesReceivedPerDiskBytesReadBase", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 21, 45, 38, 770)));
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(588L);
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(528L).Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestDiskBytesReadMetric(0), "MessageBytesReceivedPerDiskBytesRead", "The number of message bytes received per disk bytes read");
            testPerformanceCounterMetricLogger.Add(new TestMessageBytesReceivedMetric(149));
            testPerformanceCounterMetricLogger.Add(new TestDiskBytesReadMetric(257));
            testPerformanceCounterMetricLogger.Add(new TestMessageBytesReceivedMetric(439));
            testPerformanceCounterMetricLogger.Add(new TestDiskBytesReadMetric(271));
            testPerformanceCounterMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();
            // Wait a few more milliseconds so that any unexpected method calls after the signal are caught
            Thread.Sleep(50);

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogAmountOverAmountAggregateNoInstancesSuccessTest()
        {
            // Tests defining an amount over amount aggregate, where no instances of the underlying denominator amount metric have been logged

            using (mocks.Ordered)
            {
                // Expects for the calls to Add()
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 21, 45, 39, 500)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 21, 45, 58, 333)));
                // Expects for calls to Start()
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessageBytesReceivedPerDiskBytesRead", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessageBytesReceivedPerDiskBytesReadBase", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 21, 45, 38, 770)), Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestDiskBytesReadMetric(0), "MessageBytesReceivedPerDiskBytesRead", "The number of message bytes received per disk bytes read");
            testPerformanceCounterMetricLogger.Add(new TestMessageBytesReceivedMetric(149));
            testPerformanceCounterMetricLogger.Add(new TestMessageBytesReceivedMetric(439));
            testPerformanceCounterMetricLogger.Start();
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
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "ProcessingTimePerMessage", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "ProcessingTimePerMessageInstantaneous", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "ProcessingTimePerMessageInstantaneousBase", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 23, 01, 16, 999)));
                // Expects for calls to PerformanceCounterMetricLoggerImplementation.LogIntervalOverCountAggregate()
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(622L);
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(1245L);
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(2L).Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageProcessingTimeMetric(), new TestMessageReceivedMetric(), "ProcessingTimePerMessage", "The average time to process each message");
            testPerformanceCounterMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testPerformanceCounterMetricLogger.End(new TestMessageProcessingTimeMetric());
            testPerformanceCounterMetricLogger.Increment(new TestMessageReceivedMetric());
            testPerformanceCounterMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testPerformanceCounterMetricLogger.End(new TestMessageProcessingTimeMetric());
            testPerformanceCounterMetricLogger.Increment(new TestMessageReceivedMetric());
            testPerformanceCounterMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();
            // Wait a few more milliseconds so that any unexpected method calls after the signal are caught
            Thread.Sleep(50);

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
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "ProcessingTimePerMessage", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "ProcessingTimePerMessageInstantaneous", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "ProcessingTimePerMessageInstantaneousBase", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 16, 23, 01, 16, 999)), Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
                // Expects for calls to PerformanceCounterMetricLoggerImplementation.LogIntervalOverCountAggregate()
                Expect.Never.On(mockPerformanceCounter);
            }

            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageProcessingTimeMetric(), new TestMessageReceivedMetric(), "ProcessingTimePerMessage", "The average time to process each message");
            testPerformanceCounterMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testPerformanceCounterMetricLogger.End(new TestMessageProcessingTimeMetric());
            testPerformanceCounterMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testPerformanceCounterMetricLogger.End(new TestMessageProcessingTimeMetric());
            testPerformanceCounterMetricLogger.Start();
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
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessageProcessingTimePercentage", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessageProcessingTimePercentageBase", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 19, 17, 33, 50, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 19, 17, 33, 56, 300)));
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(4763L);
                Expect.Once.On(mockPerformanceCounter).SetProperty("RawValue").To(6300L).Will(Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageProcessingTimeMetric(), "MessageProcessingTimePercentage", "The amount of time spent processing messages as a percentage of total run time");
            testPerformanceCounterMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testPerformanceCounterMetricLogger.End(new TestMessageProcessingTimeMetric());
            testPerformanceCounterMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testPerformanceCounterMetricLogger.End(new TestMessageProcessingTimeMetric());
            testPerformanceCounterMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();
            // Wait a few more milliseconds so that any unexpected method calls after the signal are caught
            Thread.Sleep(50);

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
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessageProcessingTimePercentage", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockPerformanceCounterFactory).Method("Create").With(testMetricCategoryName, "MessageProcessingTimePercentageBase", false).Will(Return.Value(mockPerformanceCounter));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 19, 17, 33, 50, 000)));
                Expect.Once.On(mockDateTime).GetProperty("UtcNow").Will(Return.Value(new System.DateTime(2014, 07, 19, 17, 33, 50, 000)), Signal.EventWaitHandle(workerThreadLoopCompleteSignal));
            }

            testPerformanceCounterMetricLogger.DefineMetricAggregate(new TestMessageProcessingTimeMetric(), "MessageProcessingTimePercentage", "The amount of time spent processing messages as a percentage of total run time");
            testPerformanceCounterMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testPerformanceCounterMetricLogger.End(new TestMessageProcessingTimeMetric());
            testPerformanceCounterMetricLogger.Begin(new TestMessageProcessingTimeMetric());
            testPerformanceCounterMetricLogger.End(new TestMessageProcessingTimeMetric());
            testPerformanceCounterMetricLogger.Start();
            workerThreadLoopCompleteSignal.WaitOne();
            // Wait a few more milliseconds so that any unexpected method calls after the signal are caught
            Thread.Sleep(50);

            mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}

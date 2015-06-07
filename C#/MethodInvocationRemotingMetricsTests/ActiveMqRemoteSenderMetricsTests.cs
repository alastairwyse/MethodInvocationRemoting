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
using NMock2;
using Apache.NMS;
using ApplicationLogging;
using ApplicationMetrics;
using MethodInvocationRemoting;
using MethodInvocationRemotingMetrics;

namespace MethodInvocationRemotingMetricsTests
{
    //******************************************************************************
    //
    // Class: ActiveMqRemoteSenderMetricsTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for the metrics logging functionality in class MethodInvocationRemoting.ActiveMqRemoteSender.
    /// </summary>
    [TestFixture]
    class ActiveMqRemoteSenderMetricsTests
    {
        private Mockery mocks;
        private IConnectionFactory mockConnectionFactory;
        private IConnection mockConnection;
        private ISession mockSession;
        private IDestination mockDestination;
        private IMessageProducer mockProducer;
        private IMetricLogger mockMetricLogger;
        private ActiveMqRemoteSender testActiveMqRemoteSender;

        private const string filterIdentifier = "Filter";
        private const string connectUriName = "activemq:tcp://localhost:61616";
        private const string queueName = "TestQueueName";
        private const string messageFilter = "TestMessageFilter";

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockConnectionFactory = mocks.NewMock<IConnectionFactory>();
            mockConnection = mocks.NewMock<IConnection>();
            mockSession = mocks.NewMock<ISession>();
            mockDestination = mocks.NewMock<IDestination>();
            mockProducer = mocks.NewMock<IMessageProducer>();
            mockMetricLogger = mocks.NewMock<IMetricLogger>();
            testActiveMqRemoteSender = new ActiveMqRemoteSender(connectUriName, queueName, messageFilter, new ConsoleApplicationLogger(LogLevel.Critical, '|', "  "), mockMetricLogger, mockConnectionFactory, mockConnection, mockSession, mockDestination, mockProducer);
        }

        [Test]
        public void SendMetricsTest()
        {
            ITextMessage mockTextMessage = mocks.NewMock<ITextMessage>();
            IPrimitiveMap mockTextMessageProperties = mocks.NewMock<IPrimitiveMap>();

            Expect.AtLeastOnce.On(mockConnection);
            Expect.AtLeastOnce.On(mockProducer);
            Expect.Once.On(mockSession).Method("CreateTextMessage").Will(Return.Value(mockTextMessage));
            Expect.Once.On(mockTextMessage).GetProperty("Properties").Will(Return.Value(mockTextMessageProperties));
            Expect.Once.On(mockTextMessageProperties).Method("SetString").With(new object[2] { filterIdentifier, messageFilter });
            using (mocks.Ordered)
            {
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new MessageSendTime()));
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new MessageSendTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new MessageSent()));
            }

            testActiveMqRemoteSender.Connect();
            testActiveMqRemoteSender.Send("<TestMessage>Test message content</TestMessage>");

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SendExceptionMetricsTest()
        {
            ITextMessage mockTextMessage = mocks.NewMock<ITextMessage>();
            IPrimitiveMap mockTextMessageProperties = mocks.NewMock<IPrimitiveMap>();

            Expect.AtLeastOnce.On(mockConnection);
            Expect.Once.On(mockProducer).Method("Send").WithAnyArguments().Will(Throw.Exception(new Exception("Mock Send Failure")));
            Expect.Once.On(mockSession).Method("CreateTextMessage").Will(Return.Value(mockTextMessage));
            Expect.Once.On(mockTextMessage).GetProperty("Properties").Will(Return.Value(mockTextMessageProperties));
            Expect.Once.On(mockTextMessageProperties).Method("SetString").With(new object[2] { filterIdentifier, messageFilter });
            using (mocks.Ordered)
            {
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new MessageSendTime()));
                Expect.Once.On(mockMetricLogger).Method("CancelBegin").With(IsMetric.Equal(new MessageSendTime()));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testActiveMqRemoteSender.Connect();
                testActiveMqRemoteSender.Send("<TestMessage>Test message content</TestMessage>");
            });
        }
    }
}

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
    // Class: ActiveMqRemoteReceiverMetricsTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for the metrics logging functionality in class MethodInvocationRemoting.ActiveMqRemoteReceiver.
    /// </summary>
    [TestFixture]
    class ActiveMqRemoteReceiverMetricsTests
    {
        private Mockery mocks;
        private IConnectionFactory mockConnectionFactory;
        private IConnection mockConnection;
        private ISession mockSession;
        private IDestination mockDestination;
        private IMessageConsumer mockConsumer;
        private IMetricLogger mockMetricLogger;
        private ActiveMqRemoteReceiver testActiveMqRemoteReceiver;

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
            mockConsumer = mocks.NewMock<IMessageConsumer>();
            mockMetricLogger = mocks.NewMock<IMetricLogger>();
            testActiveMqRemoteReceiver = new ActiveMqRemoteReceiver(connectUriName, queueName, messageFilter, 1000, new ConsoleApplicationLogger(LogLevel.Critical, '|', "  "), mockMetricLogger, mockConnectionFactory, mockConnection, mockSession, mockDestination, mockConsumer);
        }

        [Test]
        public void ReceiveMetricsTest()
        {
            ITextMessage mockTextMessage = mocks.NewMock<ITextMessage>();
            IPrimitiveMap mockTextMessageProperties = mocks.NewMock<IPrimitiveMap>();
            string receivedMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><Data>abc</Data></Parameter><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter /><Parameter><DataType>double</DataType><Data>4.5678899999999999e+002</Data></Parameter></Parameters><ReturnType /></MethodInvocation>";

            using (mocks.Ordered)
            {
                // Expects for Connect()
                Expect.AtLeastOnce.On(mockConnection);
                // Expects for first Receive()
                Expect.Once.On(mockConsumer).Method("Receive").WithAnyArguments().Will(Return.Value(mockTextMessage));
                Expect.Once.On(mockTextMessage).GetProperty("Properties").Will(Return.Value(mockTextMessageProperties));
                Expect.Once.On(mockTextMessageProperties).Method("GetString").With("Text").Will(Return.Value(receivedMessage));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new MessageReceived()));
                Expect.Once.On(mockMetricLogger).Method("Add").With(IsAmountMetric.Equal(new ReceivedMessageSize(381)));
            }

            testActiveMqRemoteReceiver.Connect();
            testActiveMqRemoteReceiver.Receive();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}

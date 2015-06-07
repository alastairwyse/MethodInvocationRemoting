/*
 * Copyright 2013 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
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

namespace MethodInvocationRemotingUnitTests
{
    //******************************************************************************
    //
    // Class: ActiveMqRemoteSenderTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for class MethodInvocationRemoting.ActiveMqRemoteSender.
    /// </summary>
    [TestFixture]
    public class ActiveMqRemoteSenderTests
    {
        private Mockery mocks;
        private IConnectionFactory mockConnectionFactory;
        private IConnection mockConnection;
        private ISession mockSession;
        private IDestination mockDestination;
        private IMessageProducer mockProducer;
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
            mockSession= mocks.NewMock<ISession>();
            mockDestination= mocks.NewMock<IDestination>();
            mockProducer = mocks.NewMock<IMessageProducer>();
            testActiveMqRemoteSender = new ActiveMqRemoteSender(connectUriName, queueName, messageFilter, new ConsoleApplicationLogger(LogLevel.Warning, '|', "  "), new NullMetricLogger(), mockConnectionFactory, mockConnection, mockSession, mockDestination, mockProducer);
        }

        [Test]
        public void SendConnectionClosed()
        {
            Exception e = Assert.Throws<Exception>(delegate
            {
                testActiveMqRemoteSender.Send("<TestMessage>Test message content</TestMessage>");
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Connection to message queue is not open."));
        }

        [Test]
        public void SendAfterDispose()
        {
            const string testMessage = "<TestMessage>Test message content</TestMessage>";

            SetDisposeExpectations();
            testActiveMqRemoteSender.Dispose();
            ObjectDisposedException e = Assert.Throws<ObjectDisposedException>(delegate
            {
                testActiveMqRemoteSender.Send(testMessage);
            });
            Assert.AreEqual(e.ObjectName, "ActiveMqRemoteSender");
        }

        [Test]
        public void SendException()
        {
            ITextMessage mockTextMessage = mocks.NewMock<ITextMessage>();
            IPrimitiveMap mockTextMessageProperties = mocks.NewMock<IPrimitiveMap>();
            const string testMessage = "<TestMessage>Test message content</TestMessage>";

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockSession).Method("CreateTextMessage").With(testMessage).Will(Return.Value(mockTextMessage));
                Expect.Once.On(mockTextMessage).GetProperty("Properties").Will(Return.Value(mockTextMessageProperties));
                Expect.Once.On(mockTextMessageProperties).Method("SetString").With(new object[2] { filterIdentifier, messageFilter });
                Expect.Once.On(mockProducer).Method("Send").With(mockTextMessage).Will(Throw.Exception(new Exception("Mock Send Failure")));
            }

            testActiveMqRemoteSender.Connect();
            Exception e = Assert.Throws<Exception>(delegate
            {
                testActiveMqRemoteSender.Send(testMessage);
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Error sending message."));
        }

        [Test]
        public void SendSuccessTest()
        {
            ITextMessage mockTextMessage = mocks.NewMock<ITextMessage>();
            IPrimitiveMap mockTextMessageProperties = mocks.NewMock<IPrimitiveMap>();
            const string testMessage = "<TestMessage>Test message content</TestMessage>";

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockSession).Method("CreateTextMessage").With(testMessage).Will(Return.Value(mockTextMessage));
                Expect.Once.On(mockTextMessage).GetProperty("Properties").Will(Return.Value(mockTextMessageProperties));
                Expect.Once.On(mockTextMessageProperties).Method("SetString").With(new object[2] { filterIdentifier, messageFilter });
                Expect.Once.On(mockProducer).Method("Send").With(mockTextMessage);
            }

            testActiveMqRemoteSender.Connect();
            testActiveMqRemoteSender.Send(testMessage);
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void DisposeSuccessTest()
        {
            using (mocks.Ordered)
            {
                SetDisposeExpectations();
            }

            testActiveMqRemoteSender.Dispose();
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void DisconnnectNotAttemptedWhenNotConnected()
        {
            Expect.Never.On(mockProducer);

            testActiveMqRemoteSender.Disconnect();
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.AreEqual(false, testActiveMqRemoteSender.Connected);
        }

        [Test]
        public void DisconnectSuccessTest()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockProducer).Method("Close").WithNoArguments();
                Expect.Once.On(mockSession).Method("Close").WithNoArguments();
                Expect.Once.On(mockConnection).Method("Stop").WithNoArguments();
                Expect.Once.On(mockConnection).Method("Close").WithNoArguments();
            }

            testActiveMqRemoteSender.Connect();
            testActiveMqRemoteSender.Disconnect();
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        //------------------------------------------------------------------------------
        //
        // Method: SetConnectExpectations
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Sets mock expectations for the Connect method.
        /// </summary>
        private void SetConnectExpectations()
        {
            Expect.Once.On(mockConnection).Method("Start").WithNoArguments();
        }

        //------------------------------------------------------------------------------
        //
        // Method: SetDisposeExpectations
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Sets mock expectations for the Dispose method.
        /// </summary>
        private void SetDisposeExpectations()
        {
            Expect.Once.On(mockProducer).Method("Dispose").WithNoArguments();
            Expect.Once.On(mockSession).Method("Dispose").WithNoArguments();
            Expect.Once.On(mockConnection).Method("Dispose").WithNoArguments();
        }
    }
}

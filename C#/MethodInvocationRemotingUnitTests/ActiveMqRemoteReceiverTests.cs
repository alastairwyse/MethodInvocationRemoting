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

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NMock2;
using Apache.NMS;
using MethodInvocationRemoting;
using ApplicationLogging;

namespace MethodInvocationRemotingUnitTests
{
    //******************************************************************************
    //
    // Class: ActiveMqRemoteReceiverTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for class MethodInvocationRemoting.ActiveMqRemoteReceiver.
    /// </summary>
    [TestFixture]
    public class ActiveMqRemoteReceiverTests
    {
        private Mockery mocks;
        private IConnectionFactory mockConnectionFactory;
        private IConnection mockConnection;
        private ISession mockSession;
        private IDestination mockDestination;
        private IMessageConsumer mockConsumer;
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
            testActiveMqRemoteReceiver = new ActiveMqRemoteReceiver(connectUriName, queueName, messageFilter, 1000, new ConsoleApplicationLogger(LogLevel.Warning, '|', "  "), mockConnectionFactory, mockConnection, mockSession, mockDestination, mockConsumer);
        }

        [Test]
        public void ReceiveConnectionClosed()
        {
            Exception e = Assert.Throws<Exception>(delegate
            {
                testActiveMqRemoteReceiver.Receive();
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Connection to message queue is not open."));
        }

        [Test]
        public void ReceiveAfterDispose()
        {
            SetDisposeExpectations();
            testActiveMqRemoteReceiver.Dispose();
            ObjectDisposedException e = Assert.Throws<ObjectDisposedException>(delegate
            {
                testActiveMqRemoteReceiver.Receive();
            });
            Assert.AreEqual(e.ObjectName, "ActiveMqRemoteReceiver");
        }

        [Test]
        public void ReceiveException()
        {
            ITextMessage mockTextMessage = mocks.NewMock<ITextMessage>();

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockConsumer).Method("Receive").WithAnyArguments().Will(Throw.Exception(new Exception("Mock Receive Failure")));
            }

            testActiveMqRemoteReceiver.Connect();
            Exception e = Assert.Throws<Exception>(delegate
            {
                string receivedMessage = testActiveMqRemoteReceiver.Receive();
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Error receiving message."));
        }

        [Test]
        public void ReceiveSuccessTests()
        {
            ITextMessage mockTextMessage = mocks.NewMock<ITextMessage>();
            IPrimitiveMap mockTextMessageProperties = mocks.NewMock<IPrimitiveMap>();
            const string testMessage = "<TestMessage>Test message content</TestMessage>";

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockConsumer).Method("Receive").WithAnyArguments().Will(Return.Value(mockTextMessage));
                Expect.Once.On(mockTextMessage).GetProperty("Properties").Will(Return.Value(mockTextMessageProperties));
                Expect.Once.On(mockTextMessageProperties).Method("GetString").With("Text").Will(Return.Value(testMessage));
            }

            testActiveMqRemoteReceiver.Connect();
            string receivedMessage = testActiveMqRemoteReceiver.Receive();
            Assert.AreEqual(testMessage, receivedMessage);
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void DisposeSuccessTest()
        {
            using (mocks.Ordered)
            {
                SetDisposeExpectations();
            }

            testActiveMqRemoteReceiver.Dispose();
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void DisconnnectNotAttemptedWhenNotConnected()
        {
            Expect.Never.On(mockConsumer);

            testActiveMqRemoteReceiver.Disconnect();
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.AreEqual(false, testActiveMqRemoteReceiver.Connected);
        }

        [Test]
        public void DisconnectSuccessTest()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockConsumer).Method("Close").WithNoArguments();
                Expect.Once.On(mockSession).Method("Close").WithNoArguments();
                Expect.Once.On(mockConnection).Method("Stop").WithNoArguments();
                Expect.Once.On(mockConnection).Method("Close").WithNoArguments();
            }

            testActiveMqRemoteReceiver.Connect();
            testActiveMqRemoteReceiver.Disconnect();
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
            Expect.Once.On(mockConsumer).Method("Dispose").WithNoArguments();
            Expect.Once.On(mockSession).Method("Dispose").WithNoArguments();
            Expect.Once.On(mockConnection).Method("Dispose").WithNoArguments();
        }
    }
}

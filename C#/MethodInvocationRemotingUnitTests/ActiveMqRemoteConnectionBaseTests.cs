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
    // Class: ActiveMqRemoteConnectionBaseTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for class MethodInvocationRemoting.ActiveMqRemoteConnectionBase.
    /// </summary>
    /// <remarks>As ActiveMqRemoteConnectionBase is an abstract base class, functionality will be tested through derived class ActiveMqRemoteSender.</remarks>
    public class ActiveMqRemoteConnectionBaseTests
    {
        private Mockery mocks;
        private IConnectionFactory mockConnectionFactory;
        private IConnection mockConnection;
        private ISession mockSession;
        private IDestination mockDestination;
        private IMessageProducer mockProducer;
        private ActiveMqRemoteSender testActiveMqRemoteSender;

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
            testActiveMqRemoteSender = new ActiveMqRemoteSender(connectUriName, queueName, messageFilter, new ConsoleApplicationLogger(LogLevel.Warning, '|', "  "), new NullMetricLogger(), mockConnectionFactory, mockConnection, mockSession, mockDestination, mockProducer);
        }

        [Test]
        public void ConnectException()
        {
            Expect.Once.On(mockConnection).Method("Start").Will(Throw.Exception(new Exception("Mock Connection Failure")));

            Exception e = Assert.Throws<Exception>(delegate
            {
                testActiveMqRemoteSender.Connect();
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Error connecting to message queue."));
        }

        [Test]
        public void ConnectAfterDispose()
        {
            SetDisposeExpectations();
            testActiveMqRemoteSender.Dispose();
            ObjectDisposedException e = Assert.Throws<ObjectDisposedException>(delegate
            {
                testActiveMqRemoteSender.Connect();
            });
            Assert.AreEqual(e.ObjectName, "ActiveMqRemoteSender");
        }

        [Test]
        public void ConnectWhenAlreadyConnected()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
            }

            testActiveMqRemoteSender.Connect();
            Exception e = Assert.Throws<Exception>(delegate
            {
                testActiveMqRemoteSender.Connect();
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Connection to message queue has already been opened."));
        }

        [Test]
        public void ConnectSuccessTest()
        {
            Assert.AreEqual(false, testActiveMqRemoteSender.Connected);
            SetConnectExpectations();

            testActiveMqRemoteSender.Connect();
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.AreEqual(true, testActiveMqRemoteSender.Connected);
        }

        [Test]
        public void DisconnectAfterDispose()
        {
            SetDisposeExpectations();
            testActiveMqRemoteSender.Dispose();
            ObjectDisposedException e = Assert.Throws<ObjectDisposedException>(delegate
            {
                testActiveMqRemoteSender.Disconnect();
            });
            Assert.AreEqual(e.ObjectName, "ActiveMqRemoteSender");
        }

        [Test]
        public void DisconnnectNotAttemptedWhenNotConnected()
        {
            Expect.Never.On(mockConnection);
            Expect.Never.On(mockSession);

            testActiveMqRemoteSender.Disconnect();
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.AreEqual(false, testActiveMqRemoteSender.Connected);
        }

        [Test]
        public void DisconnnectSuccessTest()
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
            Assert.AreEqual(true, testActiveMqRemoteSender.Connected);
            testActiveMqRemoteSender.Disconnect();
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.AreEqual(false, testActiveMqRemoteSender.Connected);
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

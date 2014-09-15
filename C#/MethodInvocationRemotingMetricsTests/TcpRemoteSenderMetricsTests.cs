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
using OperatingSystemAbstraction;
using ApplicationLogging;
using ApplicationMetrics;
using MethodInvocationRemoting;
using MethodInvocationRemotingMetrics;


namespace MethodInvocationRemotingMetricsTests
{
    //******************************************************************************
    //
    // Class: TcpRemoteSenderMetricsTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for the metrics logging functionality in class MethodInvocationRemoting.TcpRemoteSender.
    /// </summary>
    [TestFixture]
    class TcpRemoteSenderMetricsTests
    {
        private Mockery mocks;
        private ITcpClient mockTcpClient;
        private INetworkStream mockNetworkStream;
        private IMetricLogger mockMetricLogger;
        private TcpRemoteSender testTcpRemoteSender;
        private System.Net.IPAddress testIpAddress;
        private int testPort = 55000;

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockTcpClient = mocks.NewMock<ITcpClient>();
            mockNetworkStream = mocks.NewMock<INetworkStream>();
            mockMetricLogger = mocks.NewMock<IMetricLogger>();
            testIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            testTcpRemoteSender = new TcpRemoteSender(testIpAddress.ToString(), testPort, 3, 10, 25, 10, new ConsoleApplicationLogger(LogLevel.Critical, '|', "  "), mockMetricLogger, mockTcpClient);
        }

        [Test]
        public void SendMetricsTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new MessageSendTime()));
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockNetworkStream).Method("Write");
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(1));
                Expect.Once.On(mockNetworkStream).Method("ReadByte").WithNoArguments().Will(Return.Value(6));
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new MessageSendTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new MessageSent()));
            }

            testTcpRemoteSender.Send("<TestMessage>Test message content</TestMessage>");

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SendReconnectMetricsTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new MessageSendTime()));
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockNetworkStream).Method("Write").WithAnyArguments().Will(Throw.Exception(new System.IO.IOException("Mock IOException")));
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(false));
                // Expectations for attempt to reconnect
                Expect.Once.On(mockTcpClient).Method("Close").WithAnyArguments();
                Expect.Once.On(mockTcpClient).Method("Connect").WithAnyArguments();
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new TcpRemoteSenderReconnected()));
                // Expectations for resend
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockNetworkStream).Method("Write");
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(1));
                Expect.Once.On(mockNetworkStream).Method("ReadByte").WithNoArguments().Will(Return.Value(6));
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new MessageSendTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new MessageSent()));
            }

            testTcpRemoteSender.Send("<TestMessage>Test message content</TestMessage>");
            mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}

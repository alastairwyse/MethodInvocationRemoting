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
using MethodInvocationRemoting;
using OperatingSystemAbstraction;
using ApplicationLogging;
using ApplicationMetrics;

namespace MethodInvocationRemotingLoggingTests
{
    //******************************************************************************
    //
    // Class: TcpRemoteSenderLoggingTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for the logging functionality in class MethodInvocationRemoting.TcpRemoteSender.
    /// </summary>
    [TestFixture]
    public class TcpRemoteSenderLoggingTests
    {
        private Mockery mocks;
        private ITcpClient mockTcpClient;
        private INetworkStream mockNetworkStream;
        private IApplicationLogger mockApplicationLogger;
        private TcpRemoteSender testTcpRemoteSender;
        private System.Net.IPAddress testIpAddress;
        private int testPort = 55000;

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockTcpClient = mocks.NewMock<ITcpClient>();
            mockNetworkStream = mocks.NewMock<INetworkStream>();
            mockApplicationLogger = mocks.NewMock<IApplicationLogger>();
            testIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            testTcpRemoteSender = new TcpRemoteSender(testIpAddress.ToString(), testPort, 3, 10, 25, 10, mockApplicationLogger, new NullMetricLogger(), mockTcpClient);
        }

        [Test]
        public void ConnectLoggingTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).Method("Connect");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testTcpRemoteSender, LogLevel.Information, "Connected to " + testIpAddress.ToString() + ":" + testPort + ".");
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
            }

            testTcpRemoteSender.Connect();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void DisconnectLoggingTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
                Expect.AtLeastOnce.On(mockTcpClient);
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testTcpRemoteSender, LogLevel.Information, "Disconnected.");
            }

            testTcpRemoteSender.Disconnect();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SendLoggingTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockNetworkStream).Method("Write");
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(1));
                Expect.Once.On(mockNetworkStream).Method("ReadByte").WithNoArguments().Will(Return.Value(6));
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testTcpRemoteSender, LogLevel.Information, "Message sent and acknowledged.");
            }

            testTcpRemoteSender.Send("<TestMessage>Test message content</TestMessage>");

            mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}

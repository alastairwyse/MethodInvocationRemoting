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
using NMock2.Actions;
using OperatingSystemAbstraction;
using ApplicationLogging;
using ApplicationMetrics;
using MethodInvocationRemoting;

namespace MethodInvocationRemotingLoggingTests
{
    //******************************************************************************
    //
    // Class: TcpRemoteReceiverLoggingTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for the logging functionality in class MethodInvocationRemoting.TcpRemoteReceiver.
    /// </summary>
    [TestFixture]
    public class TcpRemoteReceiverLoggingTests
    {
        private Mockery mocks;
        private ITcpListener mockTcpListener;
        private ITcpClient mockTcpClient;
        private INetworkStream mockNetworkStream;
        private IApplicationLogger mockApplicationLogger;
        private TcpRemoteReceiver testTcpRemoteReceiver;
        private int testPort = 55000;

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockTcpListener = mocks.NewMock<ITcpListener>();
            mockTcpClient = mocks.NewMock<ITcpClient>();
            mockNetworkStream = mocks.NewMock<INetworkStream>();
            mockApplicationLogger = mocks.NewMock<IApplicationLogger>();
            testTcpRemoteReceiver = new TcpRemoteReceiver(testPort, 3, 10, 20, mockApplicationLogger, new NullMetricLogger(), mockTcpListener);
        }

        [Test]
        public void ConnectLoggingTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockTcpListener).GetProperty("Active").Will(Return.Value(false));
                Expect.Once.On(mockTcpListener).Method("Start");
                Expect.Once.On(mockTcpListener).Method("Pending").Will(Return.Value(true));
                Expect.Once.On(mockTcpListener).Method("AcceptTcpClient").Will(Return.Value(mockTcpClient));
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testTcpRemoteReceiver, LogLevel.Information, "Connection received on port " + testPort + ".");
            }

            testTcpRemoteReceiver.Connect();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void DisconnectLoggingTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockTcpListener).GetProperty("Active").Will(Return.Value(false));
                Expect.Once.On(mockTcpListener).Method("Start");
                Expect.Once.On(mockTcpListener).Method("Pending").Will(Return.Value(true));
                Expect.Once.On(mockTcpListener).Method("AcceptTcpClient").Will(Return.Value(mockTcpClient));
                Expect.Once.On(mockApplicationLogger).Method("Log");
                Expect.Once.On(mockTcpClient).Method("Close");
                Expect.Once.On(mockTcpClient).Method("Dispose");
                Expect.Once.On(mockTcpListener).Method("Stop");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testTcpRemoteReceiver, LogLevel.Information, "Disconnected.");
            }

            testTcpRemoteReceiver.Connect();
            testTcpRemoteReceiver.Disconnect();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveLoggingTest()
        {
            // Setup test messages
            string testMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><Data>abc</Data></Parameter><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter /><Parameter><DataType>double</DataType><Data>4.5678899999999999e+002</Data></Parameter></Parameters><ReturnType /></MethodInvocation>";
            byte[] testMessageBody = System.Text.Encoding.UTF8.GetBytes(testMessage);
            byte[] testMessageSequenceNumber = BitConverter.GetBytes(123);
            byte[] testMessageSizeHeader = BitConverter.GetBytes(testMessageBody.LongLength);
            if (BitConverter.IsLittleEndian == false)
            {
                Array.Reverse(testMessageSequenceNumber);
                Array.Reverse(testMessageSizeHeader);
            }
            byte[] testMessageByteArray = new byte[testMessageSequenceNumber.Length + testMessageSizeHeader.Length + testMessageBody.Length + 2];
            testMessageByteArray[0] = 0x02;  // Set the start delimiter
            Array.Copy(testMessageSequenceNumber, 0, testMessageByteArray, 1, testMessageSequenceNumber.Length);
            Array.Copy(testMessageSizeHeader, 0, testMessageByteArray, 1 + testMessageSequenceNumber.Length, testMessageSizeHeader.Length);
            Array.Copy(testMessageBody, 0, testMessageByteArray, 1 + testMessageSequenceNumber.Length + testMessageSizeHeader.Length, testMessageBody.Length);
            testMessageByteArray[testMessageByteArray.Length - 1] = 0x03;  // Set the end delimiter

            string testSmallMessage = "<TestMessage>Test message content</TestMessage>";
            byte[] testSmallMessageBody = System.Text.Encoding.UTF8.GetBytes(testSmallMessage);
            byte[] testSmallMessageSequenceNumber = BitConverter.GetBytes(124);
            byte[] testSmallMessageSizeHeader = BitConverter.GetBytes(testSmallMessageBody.LongLength);
            if (BitConverter.IsLittleEndian == false)
            {
                Array.Reverse(testSmallMessageSequenceNumber);
                Array.Reverse(testSmallMessageSizeHeader);
            }
            byte[] testSmallMessageByteArray = new byte[testSmallMessageSequenceNumber.Length + testSmallMessageSizeHeader.Length + testSmallMessageBody.Length + 2];
            testSmallMessageByteArray[0] = 0x02;  // Set the start delimiter
            Array.Copy(testSmallMessageSequenceNumber, 0, testSmallMessageByteArray, 1, testSmallMessageSequenceNumber.Length);
            Array.Copy(testSmallMessageSizeHeader, 0, testSmallMessageByteArray, 1 + testSmallMessageSequenceNumber.Length, testSmallMessageSizeHeader.Length);
            Array.Copy(testSmallMessageBody, 0, testSmallMessageByteArray, 1 + testSmallMessageSequenceNumber.Length + testSmallMessageSizeHeader.Length, testSmallMessageBody.Length);
            testSmallMessageByteArray[testSmallMessageByteArray.Length - 1] = 0x03;  // Set the end delimiter

            using (mocks.Ordered)
            {
                // Expects for Connect()
                Expect.Once.On(mockTcpListener).GetProperty("Active").Will(Return.Value(false));
                Expect.Once.On(mockTcpListener).Method("Start");
                Expect.Once.On(mockTcpListener).Method("Pending").Will(Return.Value(true));
                Expect.Once.On(mockTcpListener).Method("AcceptTcpClient").Will(Return.Value(mockTcpClient));
                Expect.Once.On(mockApplicationLogger).Method("Log");
                // Expects for first Receive()
                Expect.Once.On(mockTcpListener).Method("Pending").Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockTcpClient).Method("GetStream").Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockTcpListener).Method("Pending").Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockNetworkStream).Method("Read").Will(new SetNamedParameterAction("buffer", testMessageByteArray), Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockTcpListener).Method("Pending").Will(Return.Value(false));
                Expect.Once.On(mockNetworkStream).Method("WriteByte");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testTcpRemoteReceiver, LogLevel.Information, "Received message '<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataT' (truncated).");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testTcpRemoteReceiver, LogLevel.Debug, "Complete message content: '" + testMessage + "'.");
                // Expects for second Receive()
                Expect.Once.On(mockTcpListener).Method("Pending").Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testSmallMessageByteArray.Length));
                Expect.Once.On(mockTcpClient).Method("GetStream").Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockTcpListener).Method("Pending").Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testSmallMessageByteArray.Length));
                Expect.Once.On(mockNetworkStream).Method("Read").Will(new SetNamedParameterAction("buffer", testSmallMessageByteArray), Return.Value(testSmallMessageByteArray.Length));
                Expect.Once.On(mockTcpListener).Method("Pending").Will(Return.Value(false));
                Expect.Once.On(mockNetworkStream).Method("WriteByte");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testTcpRemoteReceiver, LogLevel.Information, "Received message '" + testSmallMessage + "'.");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testTcpRemoteReceiver, LogLevel.Debug, "Complete message content: '" + testSmallMessage + "'.");
            }

            testTcpRemoteReceiver.Connect();
            testTcpRemoteReceiver.Receive();
            testTcpRemoteReceiver.Receive();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void CancelReceiveLoggingTests()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockTcpListener).GetProperty("Active").Will(Return.Value(false));
                Expect.Once.On(mockTcpListener).Method("Start");
                Expect.Once.On(mockTcpListener).Method("Pending").Will(Return.Value(true));
                Expect.Once.On(mockTcpListener).Method("AcceptTcpClient").Will(Return.Value(mockTcpClient));
                Expect.Once.On(mockApplicationLogger).Method("Log");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testTcpRemoteReceiver, LogLevel.Information, "Receive operation cancelled.");
            }

            testTcpRemoteReceiver.Connect();
            testTcpRemoteReceiver.CancelReceive();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}

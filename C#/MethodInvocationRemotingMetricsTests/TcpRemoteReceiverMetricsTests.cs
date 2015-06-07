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
using NMock2.Actions;
using OperatingSystemAbstraction;
using ApplicationLogging;
using ApplicationMetrics;
using MethodInvocationRemoting;
using MethodInvocationRemotingMetrics;

namespace MethodInvocationRemotingMetricsTests
{
    //******************************************************************************
    //
    // Class: TcpRemoteReceiverMetricsTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for the metrics logging functionality in class MethodInvocationRemoting.TcpRemoteReceiver.
    /// </summary>
    [TestFixture]
    class TcpRemoteReceiverMetricsTests
    {
        private Mockery mocks;
        private ITcpListener mockTcpListener;
        private ITcpClient mockTcpClient;
        private INetworkStream mockNetworkStream;
        private IMetricLogger mockMetricLogger;
        private TcpRemoteReceiver testTcpRemoteReceiver;
        private int testPort = 55000;
        private byte[] testMessageByteArray;

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockTcpListener = mocks.NewMock<ITcpListener>();
            mockTcpClient = mocks.NewMock<ITcpClient>();
            mockNetworkStream = mocks.NewMock<INetworkStream>();
            mockMetricLogger = mocks.NewMock<IMetricLogger>();
            testTcpRemoteReceiver = new TcpRemoteReceiver(testPort, 3, 10, 20, new ConsoleApplicationLogger(LogLevel.Critical, '|', "  "), mockMetricLogger, mockTcpListener);
            // Setup test message
            byte[] testMessageBody = System.Text.Encoding.UTF8.GetBytes("<Data>ABC</Data>");
            byte[] testMessageSequenceNumber = BitConverter.GetBytes(123);
            byte[] testMessageSizeHeader = BitConverter.GetBytes(testMessageBody.LongLength);
            if (BitConverter.IsLittleEndian == false)
            {
                Array.Reverse(testMessageSequenceNumber);
                Array.Reverse(testMessageSizeHeader);
            }
            testMessageByteArray = new byte[testMessageSequenceNumber.Length + testMessageSizeHeader.Length + testMessageBody.Length + 2];
            testMessageByteArray[0] = 0x02;  // Set the start delimiter
            Array.Copy(testMessageSequenceNumber, 0, testMessageByteArray, 1, testMessageSequenceNumber.Length);
            Array.Copy(testMessageSizeHeader, 0, testMessageByteArray, 1 + testMessageSequenceNumber.Length, testMessageSizeHeader.Length);
            Array.Copy(testMessageBody, 0, testMessageByteArray, 1 + testMessageSequenceNumber.Length + testMessageSizeHeader.Length, testMessageBody.Length);
            testMessageByteArray[testMessageByteArray.Length - 1] = 0x03;  // Set the end delimiter
        }

        [Test]
        public void ReceiveMetricsTest()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                // Expects for Receive()
                SetBeginMessageReceiveExpectations(testMessageByteArray.Length);
                Expect.Once.On(mockNetworkStream).Method("Read").Will(new SetNamedParameterAction("buffer", testMessageByteArray), Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockTcpListener).Method("Pending").Will(Return.Value(false));
                Expect.Once.On(mockNetworkStream).Method("WriteByte");
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new MessageReceiveTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new MessageReceived()));
                Expect.Once.On(mockMetricLogger).Method("Add").With(IsAmountMetric.Equal(new ReceivedMessageSize(16)));
            }

            testTcpRemoteReceiver.Connect();
            testTcpRemoteReceiver.Receive();
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveUnhandledExceptionMetricsTest()
        {
            // Tests that the CancelBegin() method is called when an unhandled exception is encountered during receiving

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                // Expects for Receive()
                SetBeginMessageReceiveExpectations(testMessageByteArray.Length);
                Expect.Once.On(mockNetworkStream).Method("Read").Will(Throw.Exception(new AccessViolationException("Mock AccessViolationException.")));
                Expect.Once.On(mockMetricLogger).Method("CancelBegin").With(IsMetric.Equal(new MessageReceiveTime()));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteReceiver.Connect();
                testTcpRemoteReceiver.Receive();
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveReconnectMetricsTest()
        {
            // Tests receiving a message, where a new pending connection is detected and accepted after reading just the start delimiter byte from the initial connection
            //   Ensures the correct order of metric logging in this case, especially that a corresponding End() is called for each Begin()

            byte[] shortMessageByteArray = new byte[1];
            Array.Copy(testMessageByteArray, 0, shortMessageByteArray, 0, 1);

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                // Set expectations for reading partial message
                SetBeginMessageReceiveExpectations(shortMessageByteArray.Length);
                Expect.Once.On(mockNetworkStream).Method("Read").With(new byte[shortMessageByteArray.Length], 0, shortMessageByteArray.Length).Will(new SetNamedParameterAction("buffer", shortMessageByteArray), Return.Value(shortMessageByteArray.Length));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(true));
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new MessageReceiveTime()));
                // Set expectations for reconnect
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(true));
                SetReconnectExpectations();
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new TcpRemoteReceiverReconnected()));
                // Set expectations for reading whole message
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new MessageReceiveTime()));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockNetworkStream).Method("Read").With(new byte[testMessageByteArray.Length], 0, testMessageByteArray.Length).Will(new SetNamedParameterAction("buffer", testMessageByteArray), Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockNetworkStream).Method("WriteByte").With((byte)6);
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new MessageReceiveTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new MessageReceived()));
                Expect.Once.On(mockMetricLogger).Method("Add").With(IsAmountMetric.Equal(new ReceivedMessageSize(16)));
            }

            testTcpRemoteReceiver.Connect();
            string receivedMessage = testTcpRemoteReceiver.Receive();
            mocks.VerifyAllExpectationsHaveBeenMet();
        }


        [Test]
        public void ReceiveReconnectExceptionMetricsTest()
        {
            // Tests that if an exception occurs when receiving, subsequent failure to reconnect will call method CancelBegin() when handling the exception

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new MessageReceiveTime()));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Throw.Exception(new System.IO.IOException("Mock IOException.")));
                // Below expects simulate throwing an unhandled exception when attempting to reconnect
                Expect.Once.On(mockTcpListener).GetProperty("Active").Will(Return.Value(true));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(true));
                Expect.Once.On(mockTcpClient).Method("Close").WithNoArguments();
                Expect.Once.On(mockTcpClient).Method("Dispose").WithNoArguments();
                Expect.Once.On(mockTcpListener).Method("AcceptTcpClient").Will(Throw.Exception(new AccessViolationException("Mock AccessViolationException.")));
                Expect.Once.On(mockMetricLogger).Method("CancelBegin").With(IsMetric.Equal(new MessageReceiveTime()));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteReceiver.Connect();
                testTcpRemoteReceiver.Receive();
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveReconnectRereceieveExceptionMetricsTest()
        {
            // Tests that if an exception occurs when receiving and causes a reconnect, a subsequent failure to receive will call method CancelBegin() when handling the exception

            byte[] shortMessageByteArray = new byte[1];
            Array.Copy(testMessageByteArray, 0, shortMessageByteArray, 0, 1);

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new MessageReceiveTime()));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Throw.Exception(new System.IO.IOException("Mock IOException.")));
                SetReconnectExpectations();
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new TcpRemoteReceiverReconnected()));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockNetworkStream).Method("Read").With(new byte[testMessageByteArray.Length], 0, testMessageByteArray.Length).Will(Throw.Exception(new AccessViolationException("Mock AccessViolationException.")));
                Expect.Once.On(mockMetricLogger).Method("CancelBegin").With(IsMetric.Equal(new MessageReceiveTime()));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteReceiver.Connect();
                testTcpRemoteReceiver.Receive();
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveDuplicateSequenceNumberMetricsTest()
        {
            // Tests that a message received with the same sequence number as the previous message is ignored
            //   Ensures the correct order of metric logging in this case, especially that a corresponding End() is called for each Begin()

            byte[] secondMessageByteArray = new byte[testMessageByteArray.Length];
            Array.Copy(testMessageByteArray, secondMessageByteArray, testMessageByteArray.Length);
            // Update the sequence number in the second message
            byte[] updatedSequenceNumber = BitConverter.GetBytes(124);
            if (BitConverter.IsLittleEndian == false)
            {
                Array.Reverse(updatedSequenceNumber);
            }
            Array.Copy(updatedSequenceNumber, 0, secondMessageByteArray, 1, updatedSequenceNumber.Length);
            // Update the body of the second message to be <Data>XYZ</Data>
            secondMessageByteArray[19] = 0x58;
            secondMessageByteArray[20] = 0x59;
            secondMessageByteArray[21] = 0x5A;

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                // Set expectations for receiving first message
                SetBeginMessageReceiveExpectations(testMessageByteArray.Length);
                Expect.Once.On(mockNetworkStream).Method("Read").With(new byte[testMessageByteArray.Length], 0, testMessageByteArray.Length).Will(new SetNamedParameterAction("buffer", testMessageByteArray), Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockNetworkStream).Method("WriteByte").With((byte)6);
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new MessageReceiveTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new MessageReceived()));
                Expect.Once.On(mockMetricLogger).Method("Add").With(IsAmountMetric.Equal(new ReceivedMessageSize(16)));
                // Set expectations for receiving duplicate message
                SetBeginMessageReceiveExpectations(testMessageByteArray.Length);
                Expect.Once.On(mockNetworkStream).Method("Read").With(new byte[testMessageByteArray.Length], 0, testMessageByteArray.Length).Will(new SetNamedParameterAction("buffer", testMessageByteArray), Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockNetworkStream).Method("WriteByte").With((byte)6);
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new TcpRemoteReceiverDuplicateSequenceNumber()));
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new MessageReceiveTime()));
                // Set expectations for receiving next message
                SetBeginMessageReceiveExpectations(secondMessageByteArray.Length);
                Expect.Once.On(mockNetworkStream).Method("Read").With(new byte[secondMessageByteArray.Length], 0, secondMessageByteArray.Length).Will(new SetNamedParameterAction("buffer", secondMessageByteArray), Return.Value(secondMessageByteArray.Length));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockNetworkStream).Method("WriteByte").With((byte)6);
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new MessageReceiveTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new MessageReceived()));
                Expect.Once.On(mockMetricLogger).Method("Add").With(IsAmountMetric.Equal(new ReceivedMessageSize(16)));
            }

            testTcpRemoteReceiver.Connect();
            testTcpRemoteReceiver.Receive();
            testTcpRemoteReceiver.Receive();
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveExceptionReconnectMetricsTest()
        {
            // Tests receiving a message, where an IO exception occurs causing reconnect and re-receive
            //   Ensures the correct order of metric logging in this case, especially that a corresponding End() is called for each Begin()

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                // Set expectations for socket exception
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new MessageReceiveTime()));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Throw.Exception(new System.IO.IOException("Mock IOException.")));
                SetReconnectExpectations();
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new TcpRemoteReceiverReconnected()));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockNetworkStream).Method("Read").With(new byte[testMessageByteArray.Length], 0, testMessageByteArray.Length).Will(new SetNamedParameterAction("buffer", testMessageByteArray), Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockNetworkStream).Method("WriteByte").With((byte)6);
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new MessageReceiveTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new MessageReceived()));
                Expect.Once.On(mockMetricLogger).Method("Add").With(IsAmountMetric.Equal(new ReceivedMessageSize(16)));
            }

            testTcpRemoteReceiver.Connect();
            testTcpRemoteReceiver.Receive();
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveAvailableDataExceptionMetricsTest()
        {
            // Tests where a socket exception occurs whilst checking the underlying client for available data, afterwhich the class reconnects and receives a message
            //   Ensures the correct order of metric logging in this case, especially that a corresponding End() is called for each Begin()

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                // Set expectations for socket exception when checking available data
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Throw.Exception(new System.Net.Sockets.SocketException(1)));
                SetReconnectExpectations();
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new TcpRemoteReceiverReconnected()));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new MessageReceiveTime()));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockNetworkStream).Method("Read").With(new byte[testMessageByteArray.Length], 0, testMessageByteArray.Length).Will(new SetNamedParameterAction("buffer", testMessageByteArray), Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockNetworkStream).Method("WriteByte").With((byte)6);
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new MessageReceiveTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new MessageReceived()));
                Expect.Once.On(mockMetricLogger).Method("Add").With(IsAmountMetric.Equal(new ReceivedMessageSize(16)));
            }

            testTcpRemoteReceiver.Connect();
            testTcpRemoteReceiver.Receive();
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveAfterNoDataAvailableMetricsTest()
        {
            // Tests where the loop in the Receive() method iterates through once with no data being available before the message is received
            
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                // Expects for Receive() where no data is available
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(0));
                // Expects for Receive() where data is available
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new MessageReceiveTime()));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockNetworkStream).Method("Read").Will(new SetNamedParameterAction("buffer", testMessageByteArray), Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockTcpListener).Method("Pending").Will(Return.Value(false));
                Expect.Once.On(mockNetworkStream).Method("WriteByte");
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new MessageReceiveTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new MessageReceived()));
                Expect.Once.On(mockMetricLogger).Method("Add").With(IsAmountMetric.Equal(new ReceivedMessageSize(16)));
            }

            testTcpRemoteReceiver.Connect();
            testTcpRemoteReceiver.Receive();
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        //******************************************************************************
        //
        // Method: SetConnectExpectations
        //
        //******************************************************************************
        /// <summary>
        /// Sets mock expectations for the Connect() method.
        /// </summary>
        private void SetConnectExpectations()
        {
            Expect.Once.On(mockTcpListener).GetProperty("Active").Will(Return.Value(false));
            Expect.Once.On(mockTcpListener).Method("Start").With(1);
            Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(true));
            Expect.Once.On(mockTcpListener).Method("AcceptTcpClient").WithNoArguments().Will(Return.Value(mockTcpClient));
        }

        //******************************************************************************
        //
        // Method: SetBeginMessageReceiveExpectations
        //
        //******************************************************************************
        /// <summary>
        /// Sets mock expectations for the start of the Receive() method.
        /// </summary>
        /// <param name="receiveMessageSize">The number of bytes in the received message.</param>
        private void SetBeginMessageReceiveExpectations(int receiveMessageSize)
        {
            Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
            Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(receiveMessageSize));
            Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new MessageReceiveTime()));
            Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
            Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
            Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(receiveMessageSize));
        }

        //******************************************************************************
        //
        // Method: SetReconnectExpectations
        //
        //******************************************************************************
        /// <summary>
        /// Sets mock expectations for reconnecting after an exception.
        /// </summary>
        private void SetReconnectExpectations()
        {
            Expect.Once.On(mockTcpListener).GetProperty("Active").Will(Return.Value(true));
            Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(true));
            Expect.Once.On(mockTcpClient).Method("Close").WithNoArguments();
            Expect.Once.On(mockTcpClient).Method("Dispose").WithNoArguments();
            Expect.Once.On(mockTcpListener).Method("AcceptTcpClient").WithNoArguments().Will(Return.Value(mockTcpClient));
        }
    }
}

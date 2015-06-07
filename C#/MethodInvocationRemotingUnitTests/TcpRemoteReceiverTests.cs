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
using NMock2.Actions;
using OperatingSystemAbstraction;
using ApplicationLogging;
using ApplicationMetrics;
using MethodInvocationRemoting;

namespace MethodInvocationRemotingUnitTests
{
    //******************************************************************************
    //
    // Class: TcpRemoteSenderTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for class MethodInvocationRemoting.TcpRemoteSender.
    /// </summary>
    [TestFixture]
    public class TcpRemoteReceiverTests
    {
        private Mockery mocks;
        private ITcpListener mockTcpListener;
        private ITcpClient mockTcpClient;
        private INetworkStream mockNetworkStream;
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
            testTcpRemoteReceiver = new TcpRemoteReceiver(testPort, 3, 10, 20, new ConsoleApplicationLogger(LogLevel.Warning, '|', "  "), new NullMetricLogger(), mockTcpListener);
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

        #region Invalid Argument Exception Tests

        [Test]
        public void InvalidConnectRetryCountArgument()
        {
            ArgumentOutOfRangeException e = Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                testTcpRemoteReceiver = new TcpRemoteReceiver(testPort, -1, 1000, 200, new ConsoleApplicationLogger(LogLevel.Warning, '|', "  "), new NullMetricLogger(), mockTcpListener);
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Argument 'connectRetryCount' must be greater than or equal to 0."));
            Assert.AreEqual("connectRetryCount", e.ParamName);
        }

        [Test]
        public void InvalidConnectRetryIntervalArgument()
        {
            ArgumentOutOfRangeException e = Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                testTcpRemoteReceiver = new TcpRemoteReceiver(testPort, 10, -1, 200, new ConsoleApplicationLogger(LogLevel.Warning, '|', "  "), new NullMetricLogger(), mockTcpListener);
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Argument 'connectRetryInterval' must be greater than or equal to 0."));
            Assert.AreEqual("connectRetryInterval", e.ParamName);
        }

        [Test]
        public void InvalidReceiveRetryIntervalArgument()
        {
            ArgumentOutOfRangeException e = Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                testTcpRemoteReceiver = new TcpRemoteReceiver(testPort, 10, 20, -1, new ConsoleApplicationLogger(LogLevel.Warning, '|', "  "), new NullMetricLogger(), mockTcpListener);
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Argument 'receiveRetryInterval' must be greater than or equal to 0."));
            Assert.AreEqual("receiveRetryInterval", e.ParamName);
        }

        #endregion

        #region Connect() and Disconnect() Method Tests

        [Test]
        public void ConnectWhenDisposed()
        {
            using (mocks.Ordered)
            {
                SetDisposeExpectations();
            }

            testTcpRemoteReceiver.Dispose();
            ObjectDisposedException e = Assert.Throws<ObjectDisposedException>(delegate
            {
                testTcpRemoteReceiver.Connect();
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void DisconnectWhenDisposed()
        {
            using (mocks.Ordered)
            {
                SetDisposeExpectations();
            }

            testTcpRemoteReceiver.Dispose();
            ObjectDisposedException e = Assert.Throws<ObjectDisposedException>(delegate
            {
                testTcpRemoteReceiver.Disconnect();
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ConnectStartListenerFailure()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockTcpListener).GetProperty("Active").Will(Return.Value(false));
                Expect.Once.On(mockTcpListener).Method("Start").With(1).Will(Throw.Exception(new System.Net.Sockets.SocketException(1)));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteReceiver.Connect();
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to start TcpListener whilst connecting."));
            Assert.AreEqual(1, ((System.Net.Sockets.SocketException)e.InnerException).ErrorCode);
        }

        [Test]
        public void ConnectFailureAfterRetryForNoPendingConnection()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockTcpListener).GetProperty("Active").Will(Return.Value(false));
                Expect.Once.On(mockTcpListener).Method("Start").With(1);
                for (int i = 1; i <= 4; i = i + 1)
                {
                    Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                }
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteReceiver.Connect();
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to receive connection on port " + testPort + " after 4 attempts."));
        }

        [Test]
        public void ConnectFailureAfterRetryForSocketException()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockTcpListener).GetProperty("Active").Will(Return.Value(false));
                Expect.Once.On(mockTcpListener).Method("Start").With(1);
                for (int i = 1; i <= 4; i = i + 1)
                {
                    Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(true));
                    Expect.Once.On(mockTcpListener).Method("AcceptTcpClient").WithNoArguments().Will(Throw.Exception(new System.Net.Sockets.SocketException(1)));
                }
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteReceiver.Connect();
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to receive connection on port " + testPort + " after 4 attempts."));
        }

        [Test]
        public void ConnectAfterRetrySuccessTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockTcpListener).GetProperty("Active").Will(Return.Value(false));
                Expect.Once.On(mockTcpListener).Method("Start").With(1);
                for (int i = 1; i <= 2; i = i + 1)
                {
                    Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                }
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(true));
                Expect.Once.On(mockTcpListener).Method("AcceptTcpClient").WithNoArguments().Will(Return.Value(mockTcpClient));
            }

            testTcpRemoteReceiver.Connect();
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ConnectSuccessTest()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
            }

            testTcpRemoteReceiver.Connect();
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ConnectWhenAlreadyConnected()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
            }

            testTcpRemoteReceiver.Connect();
            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteReceiver.Connect();
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Connection has already been established."));
        }

        [Test]
        public void DisconnectSuccessTest()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockTcpClient).Method("Close").WithNoArguments();
                Expect.Once.On(mockTcpClient).Method("Dispose").WithNoArguments();
                Expect.Once.On(mockTcpListener).Method("Stop").WithNoArguments();
            }

            testTcpRemoteReceiver.Connect();
            testTcpRemoteReceiver.Disconnect();
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        #endregion

        #region Receive() Method Tests

        [Test]
        public void ReceiveWhenDisposed()
        {
            using (mocks.Ordered)
            {
                SetDisposeExpectations();
            }

            testTcpRemoteReceiver.Dispose();
            ObjectDisposedException e = Assert.Throws<ObjectDisposedException>(delegate
            {
                testTcpRemoteReceiver.Receive();
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveWhenNotConnected()
        {
            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteReceiver.Receive();
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Connection on TCP socket has not been established."));
        }

        [Test]
        public void CancelReceiveWhenDisposed()
        {
            using (mocks.Ordered)
            {
                SetDisposeExpectations();
            }

            testTcpRemoteReceiver.Dispose();
            ObjectDisposedException e = Assert.Throws<ObjectDisposedException>(delegate
            {
                testTcpRemoteReceiver.CancelReceive();
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void CancelReceiveWhenNotConnected()
        {
            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteReceiver.CancelReceive();
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Connection on TCP socket has not been established."));
        }

        [Test]
        public void ReceiveIncorrectStartMessageDelimiter()
        {
            testMessageByteArray[0] = 0x01;

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                SetBeginMessageReceiveExpectations(testMessageByteArray.Length);
                Expect.Once.On(mockNetworkStream).Method("Read").With(new byte[testMessageByteArray.Length], 0, testMessageByteArray.Length).Will(new SetNamedParameterAction("buffer", testMessageByteArray), Return.Value(testMessageByteArray.Length));
            }

            testTcpRemoteReceiver.Connect();
            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteReceiver.Receive();
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Error receiving message.  Unhandled exception while attempting to receive and acknowledge message."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("First byte of received message was expected to be 2, but was 1."));
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveIncorrectEndMessageDelimiter()
        {
            testMessageByteArray[testMessageByteArray.Length - 1] = 0x41;

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                SetBeginMessageReceiveExpectations(testMessageByteArray.Length);
                Expect.Once.On(mockNetworkStream).Method("Read").With(new byte[testMessageByteArray.Length], 0, testMessageByteArray.Length).Will(new SetNamedParameterAction("buffer", testMessageByteArray), Return.Value(testMessageByteArray.Length));
            }

            testTcpRemoteReceiver.Connect();
            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteReceiver.Receive();
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Error receiving message.  Unhandled exception while attempting to receive and acknowledge message."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("Last byte of received message was expected to be 3, but was 65."));
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveExtraBytesAfterEndMessageDelimiter()
        {
            Array.Resize<byte>(ref testMessageByteArray, testMessageByteArray.Length + 1);
            testMessageByteArray[testMessageByteArray.Length - 1] = 0x41;

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                SetBeginMessageReceiveExpectations(testMessageByteArray.Length);
                Expect.Once.On(mockNetworkStream).Method("Read").With(new byte[testMessageByteArray.Length], 0, testMessageByteArray.Length).Will(new SetNamedParameterAction("buffer", testMessageByteArray), Return.Value(testMessageByteArray.Length));
            }

            testTcpRemoteReceiver.Connect();
            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteReceiver.Receive();
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Error receiving message.  Unhandled exception while attempting to receive and acknowledge message."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("Surplus data encountered after message delimiter character, starting with 65."));
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveSuccessTest()
        {
            // Tests receiving a message where the whole message is read from the underlying client in a single Read() method call
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                SetBeginMessageReceiveExpectations(testMessageByteArray.Length);
                Expect.Once.On(mockNetworkStream).Method("Read").With(new byte[testMessageByteArray.Length], 0, testMessageByteArray.Length).Will(new SetNamedParameterAction("buffer", testMessageByteArray), Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockNetworkStream).Method("WriteByte").With((byte)6);
            }

            testTcpRemoteReceiver.Connect();
            string receivedMessage = testTcpRemoteReceiver.Receive();
            Assert.AreEqual("<Data>ABC</Data>", receivedMessage);
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveByteByByteSuccessTest()
        {
            // Tests receiving a message where the message is read from the underlying client one byte at a time from multiple Read() method calls
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(1));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                foreach (byte currentByte in testMessageByteArray)
                {
                    Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                    Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(1));
                    Expect.Once.On(mockNetworkStream).Method("Read").With(new byte[1], 0, 1).Will(new SetNamedParameterAction("buffer", new byte[] { currentByte }), Return.Value(1));
                }
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockNetworkStream).Method("WriteByte").With((byte)6);
            }

            testTcpRemoteReceiver.Connect();
            string receivedMessage = testTcpRemoteReceiver.Receive();
            Assert.AreEqual("<Data>ABC</Data>", receivedMessage);
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveReconnectImmediateSuccessTest()
        {
            // Tests receiving a message, where a new pending connection is detected and accepted before any data has been read from the initial connection
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(true));
                // Set expectations for reconnect
                SetReconnectExpectations();
                // Set expectations for reading message
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                SetReceiveExpectations(testMessageByteArray, false);
            }

            testTcpRemoteReceiver.Connect();
            string receivedMessage = testTcpRemoteReceiver.Receive();
            Assert.AreEqual("<Data>ABC</Data>", receivedMessage);
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveReconnectAfterReadingStartDelimiterSuccessTest()
        {
            // Tests receiving a message, where a new pending connection is detected and accepted after reading just the start delimiter byte from the initial connection
            byte[] shortMessageByteArray = new byte[1];
            Array.Copy(testMessageByteArray, 0, shortMessageByteArray, 0, 1);

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                // Set expectations for reading partial message
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(shortMessageByteArray.Length));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(shortMessageByteArray.Length));
                Expect.Once.On(mockNetworkStream).Method("Read").With(new byte[shortMessageByteArray.Length], 0, shortMessageByteArray.Length).Will(new SetNamedParameterAction("buffer", shortMessageByteArray), Return.Value(shortMessageByteArray.Length));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(true));
                // Set expectations for reconnect
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(true));
                SetReconnectExpectations();
                // Set expectations for reading whole message
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                SetReceiveExpectations(testMessageByteArray, false);
            }

            testTcpRemoteReceiver.Connect();
            string receivedMessage = testTcpRemoteReceiver.Receive();
            Assert.AreEqual("<Data>ABC</Data>", receivedMessage);
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveReconnectAfterReadingSequenceNumberSuccessTest()
        {
            // Tests receiving a message, where a new pending connection is detected and accepted after reading up to just after the message sequence number from the initial connection
            byte[] shortMessageByteArray = new byte[5];
            Array.Copy(testMessageByteArray, 0, shortMessageByteArray, 0, 5);

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                // Set expectations for reading partial message
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(shortMessageByteArray.Length));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(shortMessageByteArray.Length));
                Expect.Once.On(mockNetworkStream).Method("Read").With(new byte[shortMessageByteArray.Length], 0, shortMessageByteArray.Length).Will(new SetNamedParameterAction("buffer", shortMessageByteArray), Return.Value(shortMessageByteArray.Length));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(true));
                // Set expectations for reconnect
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(true));
                SetReconnectExpectations();
                // Set expectations for reading whole message
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                SetReceiveExpectations(testMessageByteArray, false);
            }

            testTcpRemoteReceiver.Connect();
            string receivedMessage = testTcpRemoteReceiver.Receive();
            Assert.AreEqual("<Data>ABC</Data>", receivedMessage);
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveReconnectAfterReadingSizeHeaderSuccessTest()
        {
            // Tests receiving a message, where a new pending connection is detected and accepted after reading up to just after the message size header from the initial connection
            byte[] shortMessageByteArray = new byte[14];
            Array.Copy(testMessageByteArray, 0, shortMessageByteArray, 0, 14);

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                // Set expectations for reading partial message
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(shortMessageByteArray.Length));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(shortMessageByteArray.Length));
                Expect.Once.On(mockNetworkStream).Method("Read").With(new byte[shortMessageByteArray.Length], 0, shortMessageByteArray.Length).Will(new SetNamedParameterAction("buffer", shortMessageByteArray), Return.Value(shortMessageByteArray.Length));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(true));
                // Set expectations for reconnect
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(true));
                SetReconnectExpectations();
                // Set expectations for reading whole message
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                SetReceiveExpectations(testMessageByteArray, false);
            }

            testTcpRemoteReceiver.Connect();
            string receivedMessage = testTcpRemoteReceiver.Receive();
            Assert.AreEqual("<Data>ABC</Data>", receivedMessage);
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveReconnectAfterReadingBodySuccessTest()
        {
            // Tests receiving a message, where a new pending connection is detected and accepted after reading up to just after the message body from the initial connection
            byte[] shortMessageByteArray = new byte[29];
            Array.Copy(testMessageByteArray, 0, shortMessageByteArray, 0, 29);

            using (mocks.Ordered)
            {
                SetConnectExpectations();
                // Set expectations for reading partial message
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(shortMessageByteArray.Length));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(shortMessageByteArray.Length));
                Expect.Once.On(mockNetworkStream).Method("Read").With(new byte[shortMessageByteArray.Length], 0, shortMessageByteArray.Length).Will(new SetNamedParameterAction("buffer", shortMessageByteArray), Return.Value(shortMessageByteArray.Length));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(true));
                // Set expectations for reconnect
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(true));
                SetReconnectExpectations();
                // Set expectations for reading whole message
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                SetReceiveExpectations(testMessageByteArray, false);
            }

            testTcpRemoteReceiver.Connect();
            string receivedMessage = testTcpRemoteReceiver.Receive();
            Assert.AreEqual("<Data>ABC</Data>", receivedMessage);
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveIOExceptionRereceiveSuccessTest()
        {
            // Tests receiving a message, where an IO exception occurs causing reconnect and re-receive
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                // Set expectations for socket exception
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Throw.Exception(new System.IO.IOException("Mock IOException.")));
                SetReconnectExpectations();
                SetReceiveExpectations(testMessageByteArray, false);
            }

            testTcpRemoteReceiver.Connect();
            string receivedMessage = testTcpRemoteReceiver.Receive();
            Assert.AreEqual("<Data>ABC</Data>", receivedMessage);
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveUnhandledException()
        {
            // Tests receiving a message, where an unhandled exception occurs
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                // Set expectations for access violation exception
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockNetworkStream).Method("Read").With(new byte[testMessageByteArray.Length], 0, testMessageByteArray.Length).Will(Throw.Exception(new AccessViolationException("Mock AccessViolationException.")));
            }
            
            testTcpRemoteReceiver.Connect();
            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteReceiver.Receive();
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Error receiving message.  Unhandled exception while attempting to receive and acknowledge message."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("Mock AccessViolationException."));
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveAvailableDataSocketExceptionReconnectSuccessTest()
        {
            // Tests where a socket exception occurs whilst checking the underlying client for available data, afterwhich the class reconnects and receives a message
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                // Set expectations for socket exception when checking available data
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Throw.Exception(new System.Net.Sockets.SocketException(1)));
                SetReconnectExpectations();
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(testMessageByteArray.Length));
                SetReceiveExpectations(testMessageByteArray, false);
            }

            testTcpRemoteReceiver.Connect();
            string receivedMessage = testTcpRemoteReceiver.Receive();
            Assert.AreEqual("<Data>ABC</Data>", receivedMessage);
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveAvailableDataUnhandledException()
        {
            // Tests where an unhandled exception occurs whilst checking the underlying client for available data
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                // Set expectations for access violation exception when checking available data
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Throw.Exception(new AccessViolationException("Mock AccessViolationException.")));
            }

            testTcpRemoteReceiver.Connect();
            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteReceiver.Receive();
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Error receiving message.  Unhandled exception while checking for available data."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("Mock AccessViolationException."));
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveDuplicateSequenceNumberRereceiveSuccessTest()
        {
            // Tests that a message received with the same sequence number as the previous message is ignored
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
                // Set expectations for receiving duplicate message
                SetBeginMessageReceiveExpectations(testMessageByteArray.Length);
                Expect.Once.On(mockNetworkStream).Method("Read").With(new byte[testMessageByteArray.Length], 0, testMessageByteArray.Length).Will(new SetNamedParameterAction("buffer", testMessageByteArray), Return.Value(testMessageByteArray.Length));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockNetworkStream).Method("WriteByte").With((byte)6);
                // Set expectations for receiving next message
                SetBeginMessageReceiveExpectations(secondMessageByteArray.Length);
                Expect.Once.On(mockNetworkStream).Method("Read").With(new byte[secondMessageByteArray.Length], 0, secondMessageByteArray.Length).Will(new SetNamedParameterAction("buffer", secondMessageByteArray), Return.Value(secondMessageByteArray.Length));
                Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
                Expect.Once.On(mockNetworkStream).Method("WriteByte").With((byte)6);
            }

            testTcpRemoteReceiver.Connect();
            string receivedMessage = testTcpRemoteReceiver.Receive();
            Assert.AreEqual("<Data>ABC</Data>", receivedMessage);
            receivedMessage = testTcpRemoteReceiver.Receive();
            Assert.AreEqual("<Data>XYZ</Data>", receivedMessage);
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        #endregion

        #region Private Methods

        //******************************************************************************
        //
        // Method: SetDisposeExpectations
        //
        //******************************************************************************
        /// <summary>
        /// Sets mock expectations for the Dispose() method.
        /// </summary>
        private void SetDisposeExpectations()
        {
            Expect.Once.On(mockTcpListener).Method("Dispose").WithNoArguments();
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

        //******************************************************************************
        //
        // Method: SetReceiveExpectations
        //
        //******************************************************************************
        /// <summary>
        /// Sets mock expectations for receiving a complete message within one read operation.
        /// </summary>
        /// <param name="receiveMessage">The message bytes to receive.</param>
        /// <param name="pendingConnectionAfterRead">Whether or not to simulate a pending connection in the while loop conditions after reading the message.</param>
        private void SetReceiveExpectations(byte[] receiveMessage, bool pendingConnectionAfterRead)
        {
            Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
            Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(false));
            Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(receiveMessage.Length));
            Expect.Once.On(mockNetworkStream).Method("Read").With(new byte[receiveMessage.Length], 0, receiveMessage.Length).Will(new SetNamedParameterAction("buffer", receiveMessage), Return.Value(receiveMessage.Length));
            Expect.Once.On(mockTcpListener).Method("Pending").WithNoArguments().Will(Return.Value(pendingConnectionAfterRead));
            Expect.Once.On(mockNetworkStream).Method("WriteByte").With((byte)6);
        }

        #endregion
    }
}

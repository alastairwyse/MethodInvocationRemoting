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
    public class TcpRemoteSenderTests
    {
        private Mockery mocks;
        private ITcpClient mockTcpClient;
        private INetworkStream mockNetworkStream;
        private TcpRemoteSender testTcpRemoteSender;
        private System.Net.IPAddress testIpAddress;
        private int testPort = 55000;
        private string testMessage;
        private byte[] testMessageByteArray;
        private byte[] testMessageSequenceNumber;
        private byte[] testMessageSizeHeader;
        private byte[] testEncodedMessageByteArray;

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockTcpClient = mocks.NewMock<ITcpClient>();
            mockNetworkStream = mocks.NewMock<INetworkStream>();
            testIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            testTcpRemoteSender = new TcpRemoteSender(testIpAddress.ToString(), testPort, 3, 10, 25, 10, new ConsoleApplicationLogger(LogLevel.Warning, '|', "  "), new NullMetricLogger(), mockTcpClient);
            testMessageByteArray = new byte[] { 0x3c, 0x41, 0x02, 0x42, 0x03, 0x43, 0x3e };  // Equivalent to '<A[ASCII message start]B[ASCII message end]C>'
            testMessageSequenceNumber = new byte[] { 1, 0, 0, 0 };  // Sequence number 1 (first sequence number sent after instantiating the class), encoded as a little endian
            testMessageSizeHeader = new byte[] { 7, 0, 0, 0, 0, 0, 0, 0 };  // Size of the above test message, encoded as a little endian long
            testMessage = System.Text.Encoding.ASCII.GetString(testMessageByteArray);
            testEncodedMessageByteArray = new byte[testMessageSequenceNumber.Length + testMessageSizeHeader.Length + testMessageByteArray.Length + 2];  // Holds bytes of the complete encoded message including sequence number, size header, body, and delimiting characters
            testEncodedMessageByteArray[0] = 0x02;
            Array.Copy(testMessageSequenceNumber, 0, testEncodedMessageByteArray, 1, testMessageSequenceNumber.Length);
            Array.Copy(testMessageSizeHeader, 0, testEncodedMessageByteArray, 1 + testMessageSequenceNumber.Length, testMessageSizeHeader.Length);
            Array.Copy(testMessageByteArray, 0, testEncodedMessageByteArray, 1 + testMessageSequenceNumber.Length + testMessageSizeHeader.Length, testMessageByteArray.Length);
            testEncodedMessageByteArray[testEncodedMessageByteArray.Length - 1] = 0x03;
        }

        [Test]
        public void InvalidConnectRetryCountArgument()
        {
            ArgumentOutOfRangeException e = Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                testTcpRemoteSender = new TcpRemoteSender(testIpAddress.ToString(), testPort, -1, 1000, 60000, 100, new ConsoleApplicationLogger(LogLevel.Warning, '|', "  "), new NullMetricLogger(), mockTcpClient);
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Argument 'connectRetryCount' must be greater than or equal to 0."));
            Assert.AreEqual("connectRetryCount", e.ParamName);
        }

        [Test]
        public void InvalidConnectRetryIntervalArgument()
        {
            ArgumentOutOfRangeException e = Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                testTcpRemoteSender = new TcpRemoteSender(testIpAddress.ToString(), testPort, 10, -1, 60000, 100, new ConsoleApplicationLogger(LogLevel.Warning, '|', "  "), new NullMetricLogger(), mockTcpClient);
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Argument 'connectRetryInterval' must be greater than or equal to 0."));
            Assert.AreEqual("connectRetryInterval", e.ParamName);
        }

        [Test]
        public void InvalidAcknowledgementReceiveTimeoutArgument()
        {
            ArgumentOutOfRangeException e = Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                testTcpRemoteSender = new TcpRemoteSender(testIpAddress.ToString(), testPort, 100, 10, -1, 100, new ConsoleApplicationLogger(LogLevel.Warning, '|', "  "), new NullMetricLogger(), mockTcpClient);
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Argument 'acknowledgementReceiveTimeout' must be greater than or equal to 0."));
            Assert.AreEqual("acknowledgementReceiveTimeout", e.ParamName);
        }

        [Test]
        public void InvalidAcknowledgementReceiveRetryIntervalArgument()
        {
            ArgumentOutOfRangeException e = Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                testTcpRemoteSender = new TcpRemoteSender(testIpAddress.ToString(), testPort, 100, 10, 60000, -1, new ConsoleApplicationLogger(LogLevel.Warning, '|', "  "), new NullMetricLogger(), mockTcpClient);
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Argument 'acknowledgementReceiveRetryInterval' must be greater than or equal to 0."));
            Assert.AreEqual("acknowledgementReceiveRetryInterval", e.ParamName);
        }

        [Test]
        public void ConnectWhenDisposed()
        {
            using (mocks.Ordered)
            {
                SetDisposeExpectations();
            }

            testTcpRemoteSender.Dispose();
            ObjectDisposedException e = Assert.Throws<ObjectDisposedException>(delegate
            {
                testTcpRemoteSender.Connect();
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

            testTcpRemoteSender.Dispose();
            ObjectDisposedException e = Assert.Throws<ObjectDisposedException>(delegate
            {
                testTcpRemoteSender.Disconnect();
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ConnectException()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(false));
                Expect.Once.On(mockTcpClient).Method("Connect").With(new object[] { testIpAddress, testPort }).Will(Throw.Exception(new Exception("Mock Connection Failure")));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteSender.Connect();
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Error connecting to 127.0.0.1:55000."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("Mock Connection Failure"));
        }

        [Test]
        public void ConnectFailureAfterRetry()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(false));
                for (int i = 1; i <= 4; i = i + 1)
                {
                    Expect.Once.On(mockTcpClient).Method("Connect").With(new object[] { testIpAddress, testPort }).Will(Throw.Exception(new System.Net.Sockets.SocketException()));
                }
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(false));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteSender.Connect();
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to connect to " + testIpAddress.ToString() + ":" + testPort + " after 4 attempts."));
        }

        [Test]
        public void ConnectAfterRetrySuccessTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(false));
                for (int i = 1; i <= 2; i = i + 1)
                {
                    Expect.Once.On(mockTcpClient).Method("Connect").With(new object[] { testIpAddress, testPort }).Will(Throw.Exception(new System.Net.Sockets.SocketException()));
                }
                Expect.Once.On(mockTcpClient).Method("Connect").With(new object[] { testIpAddress, testPort });
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
            }

            testTcpRemoteSender.Connect();
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ConnectSuccessTest()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
            }

            testTcpRemoteSender.Connect();
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ConnectWhenAlreadyConnected()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
            }

            testTcpRemoteSender.Connect();
            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteSender.Connect();
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Connection to TCP socket has already been established."));
        }

        [Test]
        public void DisconnectSuccessTest()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
                Expect.Once.On(mockTcpClient).Method("Close").WithNoArguments();
            }

            testTcpRemoteSender.Connect();
            testTcpRemoteSender.Disconnect();
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SendWhenDisposed()
        {
            using (mocks.Ordered)
            {
                SetDisposeExpectations();
            }

            testTcpRemoteSender.Dispose();
            ObjectDisposedException e = Assert.Throws<ObjectDisposedException>(delegate
            {
                testTcpRemoteSender.Send(testMessage);
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SendWhenNotConnected()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(false));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteSender.Send(testMessage);
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Connection to TCP socket has not been established."));
        }

        [Test]
        public void SendSuccessTest()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockNetworkStream).Method("Write").With(new object[] { testEncodedMessageByteArray, 0, testEncodedMessageByteArray.Length });
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(1));
                Expect.Once.On(mockNetworkStream).Method("ReadByte").WithNoArguments().Will(Return.Value(6));
            }

            testTcpRemoteSender.Connect();
            testTcpRemoteSender.Send(testMessage);
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SendUnhandledException()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockNetworkStream).Method("Write").With(new object[] { testEncodedMessageByteArray, 0, testEncodedMessageByteArray.Length }).Will(Throw.Exception(new AccessViolationException("Mock AccessViolationException.")));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteSender.Connect();
                testTcpRemoteSender.Send(testMessage);
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Error sending message.  Unhandled exception while sending message."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("Mock AccessViolationException."));
        }

        [Test]
        public void SendIOExceptionAndStillConnected()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockNetworkStream).Method("Write").With(new object[] { testEncodedMessageByteArray, 0, testEncodedMessageByteArray.Length }).Will(Throw.Exception(new System.IO.IOException("Mock IOException")));
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteSender.Connect();
                testTcpRemoteSender.Send(testMessage);
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Error sending message.  IOException occurred when sending message, but client is reporting that it is still connected."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("Mock IOException"));
        }

        [Test]
        public void SendIOExceptionResendSuccessTest()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockNetworkStream).Method("Write").With(new object[] { testEncodedMessageByteArray, 0, testEncodedMessageByteArray.Length }).Will(Throw.Exception(new System.IO.IOException("Mock IOException")));
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(false));
                // Expectations for attempt to reconnect
                Expect.Once.On(mockTcpClient).Method("Close").WithNoArguments();
                Expect.Once.On(mockTcpClient).Method("Connect").With(new object[] { testIpAddress, testPort });
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
                // Expectations for resend
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockNetworkStream).Method("Write").With(new object[] { testEncodedMessageByteArray, 0, testEncodedMessageByteArray.Length });
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(1));
                Expect.Once.On(mockNetworkStream).Method("ReadByte").WithNoArguments().Will(Return.Value(6));
            }

            testTcpRemoteSender.Connect();
            testTcpRemoteSender.Send(testMessage);
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SendIOExceptionResendException()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockNetworkStream).Method("Write").With(new object[] { testEncodedMessageByteArray, 0, testEncodedMessageByteArray.Length }).Will(Throw.Exception(new System.IO.IOException("Mock IOException")));
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(false));
                // Expectations for attempt to reconnect
                Expect.Once.On(mockTcpClient).Method("Close").WithNoArguments();
                Expect.Once.On(mockTcpClient).Method("Connect").With(new object[] { testIpAddress, testPort });
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
                // Expectations for resend
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockNetworkStream).Method("Write").With(new object[] { testEncodedMessageByteArray, 0, testEncodedMessageByteArray.Length }).Will(Throw.Exception(new System.IO.IOException("Mock IOException 2")));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteSender.Connect();
                testTcpRemoteSender.Send(testMessage);
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Error sending message.  Failed to send message after reconnecting."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("Mock IOException 2"));
        }

        [Test]
        public void SendAcknowledgementExtraBytes()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockNetworkStream).Method("Write").With(new object[] { testEncodedMessageByteArray, 0, testEncodedMessageByteArray.Length });
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(2));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteSender.Connect();
                testTcpRemoteSender.Send(testMessage);
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Error sending message.  Unhandled exception while sending message."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("Surplus data encountered when receiving acknowledgement.  Expected 1 byte, but received 2."));
        }

        [Test]
        public void SendInvalidAcknowledgementByte()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockNetworkStream).Method("Write").With(new object[] { testEncodedMessageByteArray, 0, testEncodedMessageByteArray.Length });
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(1));
                Expect.Once.On(mockNetworkStream).Method("ReadByte").WithNoArguments().Will(Return.Value(4));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteSender.Connect();
                testTcpRemoteSender.Send(testMessage);
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Error sending message.  Unhandled exception while sending message."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("Acknowledgement byte was expected to be 6, but was 4."));
        }

        [Test]
        public void SendAcknowledgementNotReceivedReconnectSuccessTest()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockNetworkStream).Method("Write").With(new object[] { testEncodedMessageByteArray, 0, testEncodedMessageByteArray.Length });
                Expect.Between(1,3).On(mockTcpClient).GetProperty("Available").Will(Return.Value(0));
                // Expectations for attempt to reconnect
                Expect.Once.On(mockTcpClient).Method("Close").WithNoArguments();
                Expect.Once.On(mockTcpClient).Method("Connect").With(new object[] { testIpAddress, testPort });
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
                // Expectations for resend
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockNetworkStream).Method("Write").With(new object[] { testEncodedMessageByteArray, 0, testEncodedMessageByteArray.Length });
                Expect.Once.On(mockTcpClient).GetProperty("Available").Will(Return.Value(1));
                Expect.Once.On(mockNetworkStream).Method("ReadByte").WithNoArguments().Will(Return.Value(6));
            }

            testTcpRemoteSender.Connect();
            testTcpRemoteSender.Send(testMessage);
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SendAcknowledgementNotReceivedReconnectAcknowledgementNotReceived()
        {
            using (mocks.Ordered)
            {
                SetConnectExpectations();
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockNetworkStream).Method("Write").With(new object[] { testEncodedMessageByteArray, 0, testEncodedMessageByteArray.Length });
                Expect.Between(1, 3).On(mockTcpClient).GetProperty("Available").Will(Return.Value(0));
                // Expectations for attempt to reconnect
                Expect.Once.On(mockTcpClient).Method("Close").WithNoArguments();
                Expect.Once.On(mockTcpClient).Method("Connect").With(new object[] { testIpAddress, testPort });
                Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
                // Expectations for resend
                Expect.Once.On(mockTcpClient).Method("GetStream").WithNoArguments().Will(Return.Value(mockNetworkStream));
                Expect.Once.On(mockNetworkStream).Method("Write").With(new object[] { testEncodedMessageByteArray, 0, testEncodedMessageByteArray.Length });
                Expect.Between(1, 3).On(mockTcpClient).GetProperty("Available").Will(Return.Value(0));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testTcpRemoteSender.Connect();
                testTcpRemoteSender.Send(testMessage);
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Error sending message.  Failed to send message after reconnecting."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("Failed to receive message acknowledgement within timeout period of 25 milliseconds."));
        }

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
            Expect.Once.On(mockTcpClient).Method("Dispose").WithNoArguments();
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
            Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(false));
            Expect.Once.On(mockTcpClient).Method("Connect").With(new object[] { testIpAddress, testPort });
            Expect.Once.On(mockTcpClient).GetProperty("Connected").Will(Return.Value(true));
        }
    }
}

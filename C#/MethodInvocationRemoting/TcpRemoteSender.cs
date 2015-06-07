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

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using OperatingSystemAbstraction;
using ApplicationLogging;
using ApplicationMetrics;
using MethodInvocationRemotingMetrics;

namespace MethodInvocationRemoting
{
    //******************************************************************************
    //
    // Class: TcpRemoteSender
    //
    //******************************************************************************
    /// <summary>
    /// Sends messages to a remote location via a TCP socket connection.
    /// </summary>
    public class TcpRemoteSender : IRemoteSender, IDisposable
    {
        private System.Net.IPAddress ipAddress;
        private int port;
        private int connectRetryCount;
        private int connectRetryInterval;
        private int acknowledgementReceiveTimeout;
        private int acknowledgementReceiveRetryInterval;
        private ITcpClient client;
        private int messageSequenceNumber;
        private IApplicationLogger logger;
        private LoggingUtilities loggingUtilities;
        private MetricsUtilities metricsUtilities;
        /// <summary>Indicates whether the object has been disposed.</summary>
        protected bool disposed;
        /// <summary>The string encoding to use when sending a message.</summary>
        protected Encoding stringEncoding = Encoding.UTF8;
        /// <summary>The byte which denotes the start of the message when sending.</summary>
        protected byte messageStartDelimiter = 0x02;
        /// <summary>The byte which denotes the end of the message when sending.</summary>
        protected byte messageEndDelimiter = 0x03;
        /// <summary>The byte which is expected to be received back from the TcpRemoteReceiver to acknowledge receipt of the message.</summary>
        protected byte messageAcknowledgementByte = 0x06;

        //------------------------------------------------------------------------------
        //
        // Method: TcpRemoteSender (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.TcpRemoteSender class.
        /// </summary>
        /// <param name="ipAddress">The remote IP address to connect to.</param>
        /// <param name="port">The remote port to connect to.</param>
        /// <param name="connectRetryCount">The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteReceiver.</param>
        /// <param name="connectRetryInterval">The interval between retries to connect or reconnect in milliseconds.</param>
        /// <param name="acknowledgementReceiveTimeout">The maximum time to wait for an acknowledgement of a message in milliseconds.</param>
        /// <param name="acknowledgementReceiveRetryInterval">The time between retries to check for an acknowledgement in milliseconds.</param>
        public TcpRemoteSender(System.Net.IPAddress ipAddress, int port, int connectRetryCount, int connectRetryInterval, int acknowledgementReceiveTimeout, int acknowledgementReceiveRetryInterval)
        {
            this.ipAddress = ipAddress;
            this.port = port;

            if (connectRetryCount >= 0)
            {
                this.connectRetryCount = connectRetryCount;
            }
            else
            {
                throw new ArgumentOutOfRangeException("connectRetryCount", "Argument 'connectRetryCount' must be greater than or equal to 0.");
            }

            if (connectRetryInterval >= 0)
            {
                this.connectRetryInterval = connectRetryInterval;
            }
            else
            {
                throw new ArgumentOutOfRangeException("connectRetryInterval", "Argument 'connectRetryInterval' must be greater than or equal to 0.");
            }

            if (acknowledgementReceiveTimeout >= 0)
            {
                this.acknowledgementReceiveTimeout = acknowledgementReceiveTimeout;
            }
            else
            {
                throw new ArgumentOutOfRangeException("acknowledgementReceiveTimeout", "Argument 'acknowledgementReceiveTimeout' must be greater than or equal to 0.");
            }

            if(acknowledgementReceiveRetryInterval >= 0)
            {
                this.acknowledgementReceiveRetryInterval = acknowledgementReceiveRetryInterval;
            }
            else
            {
                throw new ArgumentOutOfRangeException("acknowledgementReceiveRetryInterval", "Argument 'acknowledgementReceiveRetryInterval' must be greater than or equal to 0.");
            }

            logger = new ConsoleApplicationLogger(LogLevel.Information, '|', "  ");
            loggingUtilities = new LoggingUtilities(logger);
            metricsUtilities = new MetricsUtilities(new NullMetricLogger());
            client = new TcpClient();

            messageSequenceNumber = 1;
            disposed = false;
        }

        //------------------------------------------------------------------------------
        //
        // Method: TcpRemoteSender (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.TcpRemoteSender class.
        /// </summary>
        /// <param name="ipAddress">The remote IP address to connect to.</param>
        /// <param name="port">The remote port to connect to.</param>
        /// <param name="connectRetryCount">The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteReceiver.</param>
        /// <param name="connectRetryInterval">The interval between retries to connect or reconnect in milliseconds.</param>
        /// <param name="acknowledgementReceiveTimeout">The maximum time to wait for an acknowledgement of a message in milliseconds.</param>
        /// <param name="acknowledgementReceiveRetryInterval">The time between retries to check for an acknowledgement in milliseconds.</param>
        public TcpRemoteSender(string ipAddress, int port, int connectRetryCount, int connectRetryInterval, int acknowledgementReceiveTimeout, int acknowledgementReceiveRetryInterval)
            : this(System.Net.IPAddress.Parse("0.0.0.0"), port, connectRetryCount, connectRetryInterval, acknowledgementReceiveTimeout, acknowledgementReceiveRetryInterval)
        {
            this.ipAddress = System.Net.IPAddress.Parse(ipAddress);
        }

        //------------------------------------------------------------------------------
        //
        // Method: TcpRemoteSender (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.TcpRemoteSender class.
        /// </summary>
        /// <param name="ipAddress">The remote IP address to connect to.</param>
        /// <param name="port">The remote port to connect to.</param>
        /// <param name="connectRetryCount">The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteReceiver.</param>
        /// <param name="connectRetryInterval">The interval between retries to connect or reconnect in milliseconds.</param>
        /// <param name="acknowledgementReceiveTimeout">The maximum time to wait for an acknowledgement of a message in milliseconds.</param>
        /// <param name="acknowledgementReceiveRetryInterval">The time between retries to check for an acknowledgement in milliseconds.</param>
        /// <param name="logger">The logger to write log events to.</param>
        public TcpRemoteSender(System.Net.IPAddress ipAddress, int port, int connectRetryCount, int connectRetryInterval, int acknowledgementReceiveTimeout, int acknowledgementReceiveRetryInterval, IApplicationLogger logger)
            : this(ipAddress, port, connectRetryCount, connectRetryInterval, acknowledgementReceiveTimeout, acknowledgementReceiveRetryInterval)
        {
            this.logger = logger;
            loggingUtilities = new LoggingUtilities(logger);
        }

        //------------------------------------------------------------------------------
        //
        // Method: TcpRemoteSender (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.TcpRemoteSender class.
        /// </summary>
        /// <param name="ipAddress">The remote IP address to connect to.</param>
        /// <param name="port">The remote port to connect to.</param>
        /// <param name="connectRetryCount">The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteReceiver.</param>
        /// <param name="connectRetryInterval">The interval between retries to connect or reconnect in milliseconds.</param>
        /// <param name="acknowledgementReceiveTimeout">The maximum time to wait for an acknowledgement of a message in milliseconds.</param>
        /// <param name="acknowledgementReceiveRetryInterval">The time between retries to check for an acknowledgement in milliseconds.</param>
        /// <param name="metricLogger">The metric logger to write metric and instrumentation events to.</param>
        public TcpRemoteSender(System.Net.IPAddress ipAddress, int port, int connectRetryCount, int connectRetryInterval, int acknowledgementReceiveTimeout, int acknowledgementReceiveRetryInterval, IMetricLogger metricLogger)
            : this(ipAddress, port, connectRetryCount, connectRetryInterval, acknowledgementReceiveTimeout, acknowledgementReceiveRetryInterval)
        {
            metricsUtilities = new MetricsUtilities(metricLogger);
        }

        //------------------------------------------------------------------------------
        //
        // Method: TcpRemoteSender (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.TcpRemoteSender class.
        /// </summary>
        /// <param name="ipAddress">The remote IP address to connect to.</param>
        /// <param name="port">The remote port to connect to.</param>
        /// <param name="connectRetryCount">The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteReceiver.</param>
        /// <param name="connectRetryInterval">The interval between retries to connect or reconnect in milliseconds.</param>
        /// <param name="acknowledgementReceiveTimeout">The maximum time to wait for an acknowledgement of a message in milliseconds.</param>
        /// <param name="acknowledgementReceiveRetryInterval">The time between retries to check for an acknowledgement in milliseconds.</param>
        /// <param name="logger">The logger to write log events to.</param>
        /// <param name="metricLogger">The metric logger to write metric and instrumentation events to.</param>
        public TcpRemoteSender(System.Net.IPAddress ipAddress, int port, int connectRetryCount, int connectRetryInterval, int acknowledgementReceiveTimeout, int acknowledgementReceiveRetryInterval, IApplicationLogger logger, IMetricLogger metricLogger)
            : this(ipAddress, port, connectRetryCount, connectRetryInterval, acknowledgementReceiveTimeout, acknowledgementReceiveRetryInterval)
        {
            this.logger = logger;
            loggingUtilities = new LoggingUtilities(logger);
            metricsUtilities = new MetricsUtilities(metricLogger);
        }

        //------------------------------------------------------------------------------
        //
        // Method: TcpRemoteSender (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.TcpRemoteSender class.
        /// </summary>
        /// <param name="ipAddress">The remote IP address to connect to.</param>
        /// <param name="port">The remote port to connect to.</param>
        /// <param name="connectRetryCount">The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteReceiver.</param>
        /// <param name="connectRetryInterval">The interval between retries to connect or reconnect in milliseconds.</param>
        /// <param name="acknowledgementReceiveTimeout">The maximum time to wait for an acknowledgement of a message in milliseconds.</param>
        /// <param name="acknowledgementReceiveRetryInterval">The time between retries to check for an acknowledgement in milliseconds.</param>
        /// <param name="logger">The logger to write log events to.</param>
        public TcpRemoteSender(string ipAddress, int port, int connectRetryCount, int connectRetryInterval, int acknowledgementReceiveTimeout, int acknowledgementReceiveRetryInterval, IApplicationLogger logger)
            : this(System.Net.IPAddress.Parse("0.0.0.0"), port, connectRetryCount, connectRetryInterval, acknowledgementReceiveTimeout, acknowledgementReceiveRetryInterval)
        {
            this.ipAddress = System.Net.IPAddress.Parse(ipAddress);
            this.logger = logger;
            loggingUtilities = new LoggingUtilities(logger);
        }

        //------------------------------------------------------------------------------
        //
        // Method: TcpRemoteSender (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.TcpRemoteSender class.
        /// </summary>
        /// <param name="ipAddress">The remote IP address to connect to.</param>
        /// <param name="port">The remote port to connect to.</param>
        /// <param name="connectRetryCount">The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteReceiver.</param>
        /// <param name="connectRetryInterval">The interval between retries to connect or reconnect in milliseconds.</param>
        /// <param name="acknowledgementReceiveTimeout">The maximum time to wait for an acknowledgement of a message in milliseconds.</param>
        /// <param name="acknowledgementReceiveRetryInterval">The time between retries to check for an acknowledgement in milliseconds.</param>
        /// <param name="metricLogger">The metric logger to write metric and instrumentation events to.</param>
        public TcpRemoteSender(string ipAddress, int port, int connectRetryCount, int connectRetryInterval, int acknowledgementReceiveTimeout, int acknowledgementReceiveRetryInterval, IMetricLogger metricLogger)
            : this(System.Net.IPAddress.Parse("0.0.0.0"), port, connectRetryCount, connectRetryInterval, acknowledgementReceiveTimeout, acknowledgementReceiveRetryInterval)
        {
            this.ipAddress = System.Net.IPAddress.Parse(ipAddress);
            metricsUtilities = new MetricsUtilities(metricLogger);
        }

        //------------------------------------------------------------------------------
        //
        // Method: TcpRemoteSender (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.TcpRemoteSender class.
        /// </summary>
        /// <param name="ipAddress">The remote IP address to connect to.</param>
        /// <param name="port">The remote port to connect to.</param>
        /// <param name="connectRetryCount">The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteReceiver.</param>
        /// <param name="connectRetryInterval">The interval between retries to connect or reconnect in milliseconds.</param>
        /// <param name="acknowledgementReceiveTimeout">The maximum time to wait for an acknowledgement of a message in milliseconds.</param>
        /// <param name="acknowledgementReceiveRetryInterval">The time between retries to check for an acknowledgement in milliseconds.</param>
        /// <param name="logger">The logger to write log events to.</param>
        /// <param name="metricLogger">The metric logger to write metric and instrumentation events to.</param>
        public TcpRemoteSender(string ipAddress, int port, int connectRetryCount, int connectRetryInterval, int acknowledgementReceiveTimeout, int acknowledgementReceiveRetryInterval, IApplicationLogger logger, IMetricLogger metricLogger)
            : this(System.Net.IPAddress.Parse("0.0.0.0"), port, connectRetryCount, connectRetryInterval, acknowledgementReceiveTimeout, acknowledgementReceiveRetryInterval)
        {
            this.ipAddress = System.Net.IPAddress.Parse(ipAddress);
            this.logger = logger;
            loggingUtilities = new LoggingUtilities(logger);
            metricsUtilities = new MetricsUtilities(metricLogger);
        }

        //------------------------------------------------------------------------------
        //
        // Method: TcpRemoteSender (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.TcpRemoteSender class.  Note this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
        /// </summary>
        /// <param name="ipAddress">The remote IP address to connect to.</param>
        /// <param name="port">The remote port to connect to.</param>
        /// <param name="connectRetryCount">The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteReceiver.</param>
        /// <param name="connectRetryInterval">The interval between retries to connect or reconnect in milliseconds.</param>
        /// <param name="acknowledgementReceiveTimeout">The maximum time to wait for an acknowledgement of a message in milliseconds.</param>
        /// <param name="acknowledgementReceiveRetryInterval">The time between retries to check for an acknowledgement in milliseconds.</param>
        /// <param name="logger">The logger to write log events to.</param>
        /// <param name="metricLogger">The metric logger to write metric and instrumentation events to.</param>
        /// <param name="client">A test (mock) TCP client.</param>
        public TcpRemoteSender(string ipAddress, int port, int connectRetryCount, int connectRetryInterval, int acknowledgementReceiveTimeout, int acknowledgementReceiveRetryInterval, IApplicationLogger logger, IMetricLogger metricLogger, ITcpClient client)
            : this(ipAddress, port, connectRetryCount, connectRetryInterval, acknowledgementReceiveTimeout, acknowledgementReceiveRetryInterval, logger, metricLogger)
        {
            this.client.Dispose();
            this.client = client;
        }

        //------------------------------------------------------------------------------
        //
        // Method: Connect
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Connects to the configured IP address and port.
        /// </summary>
        public void Connect()
        {
            CheckNotDisposed();
            if (client.Connected == true)
            {
                throw new Exception("Connection to TCP socket has already been established.");
            }
            AttemptConnect();
        }

        //------------------------------------------------------------------------------
        //
        // Method: Disconnect
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Disconnects from the configured IP address and port.
        /// </summary>
        public void Disconnect()
        {
            CheckNotDisposed();
            if (client.Connected == true)
            {
                client.Close();
                loggingUtilities.Log(this, LogLevel.Information, "Disconnected.");
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IRemoteSender.Send(System.String)"]/*'/>
        public void Send(string message)
        {
            metricsUtilities.Begin(new MessageSendTime());

            CheckNotDisposed();
            CheckConnected();

            try
            {
                EncodeAndSend(message);
            }
            catch (Exception e)
            {
                HandleExceptionAndResend(e, message);
            }

            IncrementMessageSequenceNumber();

            metricsUtilities.End(new MessageSendTime());
            metricsUtilities.Increment(new MessageSent());
            loggingUtilities.Log(this, LogLevel.Information, "Message sent and acknowledged.");
        }

        //------------------------------------------------------------------------------
        //
        // Method: AttemptConnect
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Attempts to connect to the specified IP address and port, and retries for the specified number of times if the attempt is unsuccessful.
        /// </summary>
        private void AttemptConnect()
        {
            int connectAttempt = 0;

            while (connectAttempt <= connectRetryCount)
            {
                try
                {
                    client.Connect(ipAddress, port);
                    logger.Log(this, LogLevel.Information, "Connected to " + ipAddress.ToString() + ":" + port + ".");
                    break;
                }
                catch (System.Net.Sockets.SocketException socketException)
                {
                    logger.Log(this, LogLevel.Error, "SocketException with error code " + socketException.ErrorCode + " occurred whilst trying to connect to " + ipAddress.ToString() + ":" + port + ".", socketException);
                    if (connectRetryInterval > 0)
                    {
                        Thread.Sleep(connectRetryInterval);
                    }
                }
                catch (System.Security.SecurityException securityException)
                {
                    logger.Log(this, LogLevel.Error, "SecurityException occurred whilst trying to connect to " + ipAddress.ToString() + ":" + port + ".", securityException);
                    if (connectRetryInterval > 0)
                    {
                        Thread.Sleep(connectRetryInterval);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Error connecting to " + ipAddress.ToString() + ":" + port +".", e);
                }

                connectAttempt = connectAttempt + 1;
            }

            if (client.Connected == false)
            {
                throw new Exception("Failed to connect to " + ipAddress.ToString() + ":" + port + " after " + connectAttempt + " attempts.");
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: CheckConnected
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Throws an exception if a connection has not been established.
        /// </summary>
        private void CheckConnected()
        {
            if (client.Connected == false)
            {
                throw new Exception("Connection to TCP socket has not been established.");
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: EncodeAndSend
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Adds delimiter characters and header information to the specified message and sends it.
        /// </summary>
        /// <param name="message">The message to send.</param>
        private void EncodeAndSend(string message)
        {
            byte[] messageByteArray = stringEncoding.GetBytes(message);

            // Create a 4 byte message sequence number, and encode as little endian
            byte[] messageSequenceNumberByteArray = BitConverter.GetBytes(messageSequenceNumber);
            EncodeAsLittleEndian(messageSequenceNumberByteArray);

            // Create 8 bytes containing the length of the message body, and encode as little endian
            byte[] messageSizeHeader = BitConverter.GetBytes(messageByteArray.LongLength);
            EncodeAsLittleEndian(messageSizeHeader);

            // Encode the message
            byte[] encodedMessageByteArray = new byte[messageSequenceNumberByteArray.Length + messageSizeHeader.Length + messageByteArray.Length + 2];
            encodedMessageByteArray[0] = messageStartDelimiter;
            Array.Copy(messageSequenceNumberByteArray, 0, encodedMessageByteArray, 1, messageSequenceNumberByteArray.Length);
            Array.Copy(messageSizeHeader, 0, encodedMessageByteArray, 1 + messageSequenceNumberByteArray.Length, messageSizeHeader.Length);
            Array.Copy(messageByteArray, 0, encodedMessageByteArray, 1 + messageSequenceNumberByteArray.Length + messageSizeHeader.Length, messageByteArray.Length);
            encodedMessageByteArray[encodedMessageByteArray.Length - 1] = messageEndDelimiter;

            // Send the message
            INetworkStream networkStream = client.GetStream();
            networkStream.Write(encodedMessageByteArray, 0, encodedMessageByteArray.Length);
            // As per http://msdn.microsoft.com/en-us/library/system.net.sockets.networkstream.flush%28v=vs.80%29.aspx the Flush() method has no effect on NetworkStream objects, and hence is not called.
            WaitForMessageAcknowledgement(networkStream);
        }

        //------------------------------------------------------------------------------
        //
        // Method: HandleExceptionAndResend
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Handles an exception that occurred when attempting to send a message, before reconnecting and re-sending.
        /// </summary>
        /// <param name="sendException">The exception that occurred when attempting to send the message.</param>
        /// <param name="message">The message to send.</param>
        private void HandleExceptionAndResend(Exception sendException, string message)
        {
            /*
             * In testing of this class, the only exhibited exception for probable real-world network issues (i.e. disconnecting network cable), was the System.IO.IOException.
             * However, the documentation states that numerous exceptions can potentially be thrown by the NetworkStream and TcpClient classes.  Hence the below code block handles all theoretically potential exceptions.
             * These exceptions, and the classes which can cause them are listed below...
             * TcpClient.Available
             *   ObjectDisposedException
             *   SocketException
             * TcpClient.GetStream()
             *   InvalidOperationException
             *   ObjectDisposedException
             * NetworkStream.ReadByte()
             *   IOException
             *   ObjectDisposedException
             * NetworkStream.Write(byte[] buffer, int offset, int size)
             *   IOException
             *   ObjectDisposedException
             */
            try
            {
                if (sendException is System.IO.IOException)
                {
                    logger.Log(this, LogLevel.Error, sendException.GetType().Name + " occurred whilst attempting to send message.", sendException);

                    // If the TCP client is still connected, the situation cannot be handled so re-throw the exception
                    if (client.Connected == true)
                    {
                        throw new Exception("Error sending message.  IOException occurred when sending message, but client is reporting that it is still connected.", sendException);
                    }

                    logger.Log(this, LogLevel.Warning, "Disconnected from TCP socket.");
                }
                else if ((sendException is MessageAcknowledgementTimeoutException) ||
                          (sendException is System.Net.Sockets.SocketException) ||
                          (sendException is ObjectDisposedException) ||
                          (sendException is InvalidOperationException))
                {
                    logger.Log(this, LogLevel.Error, sendException.GetType().Name + " occurred whilst attempting to send message.", sendException);
                }
                else
                {
                    throw new Exception("Error sending message.  Unhandled exception while sending message.", sendException);
                }

                logger.Log(this, LogLevel.Warning, "Attempting to reconnect to TCP socket.");

                client.Close();
                AttemptConnect();
                metricsUtilities.Increment(new TcpRemoteSenderReconnected());
                try
                {
                    EncodeAndSend(message);
                }
                catch (Exception e)
                {
                    throw new Exception("Error sending message.  Failed to send message after reconnecting.", e);
                }
            }
            catch (Exception e)
            {
                metricsUtilities.CancelBegin(new MessageSendTime());
                throw e;
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: WaitForMessageAcknowledgement
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Checks for a message acknowledgement on the specified network stream, and throws an exception if the acknowledgement is not received before the specified timeout period.
        /// </summary>
        /// <param name="networkStream">The network stream to attempt to read the acknowledgement from.</param>
        private void WaitForMessageAcknowledgement(INetworkStream networkStream)
        {
            bool acknowledgementReceived = false;
            Stopwatch acknowledgementTimer = new Stopwatch();
            acknowledgementTimer.Start();
            while ((acknowledgementTimer.ElapsedMilliseconds <= acknowledgementReceiveTimeout) && (acknowledgementReceived == false))
            {
                int availableData = client.Available;
                if (availableData > 0)
                {
                    if (availableData > 1)
                    {
                        throw new Exception("Surplus data encountered when receiving acknowledgement.  Expected 1 byte, but received " + availableData.ToString() + ".");
                    }

                    int acknowledgementByte = networkStream.ReadByte();
                    if (acknowledgementByte != (int)messageAcknowledgementByte)
                    {
                        throw new Exception("Acknowledgement byte was expected to be " + messageAcknowledgementByte.ToString() + ", but was " + acknowledgementByte.ToString() + ".");
                    }
                    else
                    {
                        acknowledgementReceived = true;
                    }
                }

                if (acknowledgementReceiveRetryInterval > 0)
                {
                    Thread.Sleep(acknowledgementReceiveRetryInterval);
                }
            }

            if (acknowledgementReceived == false)
            {
                throw new MessageAcknowledgementTimeoutException("Failed to receive message acknowledgement within timeout period of " + acknowledgementReceiveTimeout.ToString() + " milliseconds.");
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: EncodeAsLittleEndian
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Encodes the inputted byte array as little endian.
        /// </summary>
        /// <param name="inputByteArray">The byte array to encode.</param>
        private void EncodeAsLittleEndian(byte[] inputByteArray) 
        {
            if (BitConverter.IsLittleEndian == false)
            {
                Array.Reverse(inputByteArray);
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: IncrementMessageSequenceNumber
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Increments the internal message sequence number.
        /// </summary>
        private void IncrementMessageSequenceNumber()
        {
            if (messageSequenceNumber == int.MaxValue)
            {
                messageSequenceNumber = 0;
            }
            else
            {
                messageSequenceNumber = messageSequenceNumber + 1;
            }
        }

        #region Finalize / Dispose Methods

        /// <summary>
        /// Releases the unmanaged resources used by the TcpRemoteSender.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #pragma warning disable 1591
        ~TcpRemoteSender()
        {
            Dispose(false);
        }
        #pragma warning restore 1591

        //------------------------------------------------------------------------------
        //
        // Method: Dispose
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Provides a method to free unmanaged resources used by this class.
        /// </summary>
        /// <param name="disposing">Whether the method is being called as part of an explicit Dispose routine, and hence whether managed resources should also be freed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                    client.Dispose();
                    ipAddress = null;
                }
                // Free your own state (unmanaged objects).

                // Set large fields to null.

                disposed = true;
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: CheckNotDisposed
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Throws an exception if the disposed property is true.
        /// </summary>
        private void CheckNotDisposed()
        {
            if (disposed == true)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        #endregion
    }
}

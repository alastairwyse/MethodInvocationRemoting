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
using System.Threading;
using OperatingSystemAbstraction;
using ApplicationLogging;

namespace MethodInvocationRemoting
{
    /// <summary>
    /// Represents the state of parsing a message received by the TcpRemoteReceiver class.
    /// </summary>
    enum MessageParseState
    {
        /// <summary>No data has been parsed.</summary>
        StartOfMessage,
        /// <summary>Only the start delimiter has been parsed.  The sequence number is currently being parsed.</summary>
        ReadStartDelimiter,
        /// <summary>The start delimiter and sequence number have been parsed.  The message size header is currently being parsed.</summary>
        ReadSequenceNumber,
        /// <summary>The start delimiter, sequence number, and message size header have been parsed.  The message body is currently being parsed.</summary>
        ReadSizeHeader,
        /// <summary>The started delimiter, sequence number, message size header, and message body have been parsed.  The end delimiter is being parsed.</summary>
        ReadMessageBody,
        /// <summary>The complete message has been parsed.</summary>
        ReadCompleteMessage
    };

    //******************************************************************************
    //
    // Class: TcpRemoteReceiver
    //
    //******************************************************************************
    /// <summary>
    /// Receives messages from a remote location via a TCP socket connection.
    /// </summary>
    public class TcpRemoteReceiver : IRemoteReceiver, IDisposable
    {
        private int port;
        private int connectRetryCount;
        private int connectRetryInterval;
        private int receiveRetryInterval;
        private ITcpListener listener;
        private ITcpClient client;
        private bool connected;
        private volatile bool cancelRequest;
        private volatile bool waitingForRetry = false;
        private int lastMessageSequenceNumber;
        private IApplicationLogger logger;
        private LoggingUtilities loggingUtilities;
        protected bool disposed;
        /// <summary>The string encoding to expect when receiving a message.</summary>
        protected Encoding stringEncoding = Encoding.UTF8;
        /// <summary>The byte which denotes the start of the message.</summary>
        protected byte messageStartDelimiter = 0x02;
        /// <summary>The byte which denotes the end of the message.</summary>
        protected byte messageEndDelimiter = 0x03;
        /// <summary>The byte used to send back to the TcpRemoteSender to acknowledge receipt of the message.</summary>
        protected byte messageAcknowledgementByte = 0x06;

        //******************************************************************************
        //
        // Method: TcpRemoteReceiver (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.TcpRemoteReceiver class.
        /// </summary>
        /// <param name="port">The port to listen for incoming connections on.</param>
        /// <param name="connectRetryCount">The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteSender.</param>
        /// <param name="connectRetryInterval">The interval between retries to connect or reconnect in milliseconds.</param>
        /// <param name="receiveRetryInterval">The time to wait between attempts to receive a message in milliseconds.</param>
        public TcpRemoteReceiver(int port, int connectRetryCount, int connectRetryInterval, int receiveRetryInterval)
        {
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

            if (receiveRetryInterval >= 0)
            {
                this.receiveRetryInterval = receiveRetryInterval;
            }
            else
            {
                throw new ArgumentOutOfRangeException("receiveRetryInterval", "Argument 'receiveRetryInterval' must be greater than or equal to 0.");
            }

            listener = new TcpListener(System.Net.IPAddress.Any, port);
            logger = new ConsoleApplicationLogger(LogLevel.Information, '|', "  ");
            loggingUtilities = new LoggingUtilities(logger);

            lastMessageSequenceNumber = 0;
            connected = false;
            disposed = false;
        }

        //******************************************************************************
        //
        // Method: TcpRemoteReceiver (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.TcpRemoteReceiver class.
        /// </summary>
        /// <param name="port">The port to listen for incoming connections on.</param>
        /// <param name="connectRetryCount">The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteSender.</param>
        /// <param name="connectRetryInterval">The interval between retries to connect or reconnect in milliseconds.</param>
        /// <param name="receiveRetryInterval">The time to wait between attempts to receive a message in milliseconds.</param>
        /// <param name="logger">The logger to write log events to.</param>
        public TcpRemoteReceiver(int port, int connectRetryCount, int connectRetryInterval, int receiveRetryInterval, IApplicationLogger logger)
            : this(port, connectRetryCount, connectRetryInterval, receiveRetryInterval)
        {
            this.logger = logger;
            loggingUtilities = new LoggingUtilities(logger);
        }

        //******************************************************************************
        //
        // Method: TcpRemoteReceiver (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.TcpRemoteReceiver class.  Note this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
        /// </summary>
        /// <param name="port">The port to listen for incoming connections on.</param>
        /// <param name="connectRetryCount">The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteSender.</param>
        /// <param name="connectRetryInterval">The interval between retries to connect or reconnect in milliseconds.</param>
        /// <param name="receiveRetryInterval">The time to wait between attempts to receive a message in milliseconds.</param>
        /// <param name="logger">The logger to write log events to.</param>
        /// <param name="listener">A test (mock) TCP listener.</param>
        public TcpRemoteReceiver(int port, int connectRetryCount, int connectRetryInterval, int receiveRetryInterval, IApplicationLogger logger, ITcpListener listener) 
            : this(port, connectRetryCount, connectRetryInterval, receiveRetryInterval, logger)
        {
            this.listener.Dispose();
            this.listener = listener;
        }

        //******************************************************************************
        //
        // Method: Connect
        //
        //******************************************************************************
        /// <summary>
        /// Listens for and accepts an incoming connection on the configured TCP port.
        /// </summary>
        public void Connect()
        {
            CheckNotDisposed();
            if (connected == true)
            {
                throw new Exception("Connection has already been established.");
            }
            AttemptConnect();
        }

        //******************************************************************************
        //
        // Method: Disconnect
        //
        //******************************************************************************
        /// <summary>
        /// Disconnects a connected client and stops listening on the configured TCP port.
        /// </summary>
        public void Disconnect()
        {
            CheckNotDisposed();
            try
            {
                client.Close();
                DisposeClient();
                listener.Stop();
                connected = false;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to disconnect listener.", e);
            }

            loggingUtilities.Log(this, LogLevel.Information, "Disconnected.");
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IRemoteReceiver.Receive"]/*'/>
        public string Receive()
        {
            cancelRequest = false;
            CheckNotDisposed();
            CheckConnected();
            int messageSequenceNumber = -1;
            string returnMessage = "";

            while (cancelRequest == false)
            {
                // Check if there are any pending connections which would indicate the TcpRemoteSender has encountered an error and reconnected
                if (listener.Pending() == true)
                {
                    logger.Log(this, LogLevel.Warning, "New connection detected.  Attempting reconnect.");
                    AttemptConnect();
                }

                // Check if any data has been received from the TcpRemoteSender, and handle and retry if an exception occurs
                int availableData = 0;
                try
                {
                    availableData = client.Available;
                }
                catch(Exception e)
                {
                    availableData = HandleExceptionAndCheckAvailableData(e);
                }

                // If data has been received, attempt to read and parse it, and handle and retry if an exception occurs
                if (availableData != 0)
                {
                    MessageParseState parseState = MessageParseState.StartOfMessage;
                    Queue<byte> messageBytes = null;  // Holds the bytes which form the body of the message received

                    try
                    {
                        messageBytes = SetupAndReadMessage(ref parseState, ref messageSequenceNumber);
                    }
                    catch (Exception e)
                    {
                        messageBytes = HandleExceptionAndRereadMessage(e, ref parseState, ref messageSequenceNumber);
                    }

                    // If the complete message has been read, break out of the current while loop
                    //   If the complete message was not read it would have been caused by a cancel request, or by a pending connection which is handled outside this block
                    if ((cancelRequest == false) && (parseState == MessageParseState.ReadCompleteMessage))
                    {
                        // If the sequence number of the message is the same as the last received message, then discard the message
                        //   This situation can be caused by the connection breaking before the sender received the last acknowledgment
                        if (messageSequenceNumber != lastMessageSequenceNumber)
                        {
                            lastMessageSequenceNumber = messageSequenceNumber;
                            returnMessage = stringEncoding.GetString(messageBytes.ToArray());
                            loggingUtilities.LogMessageReceived(this, returnMessage);
                            break;
                        }
                        else
                        {
                            logger.Log(this, LogLevel.Warning, "Duplicate message with sequence number " + messageSequenceNumber + " received.  Message discarded.");
                            // Reset variables
                            messageSequenceNumber = -1;
                            returnMessage = "";
                        }
                    }
                }

                waitingForRetry = true;
                if (receiveRetryInterval > 0)
                {
                    Thread.Sleep(receiveRetryInterval);
                }
                waitingForRetry = false;
            }

            return returnMessage;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IRemoteReceiver.CancelReceive"]/*'/>
        public void CancelReceive()
        {
            CheckNotDisposed();
            CheckConnected();
            cancelRequest = true;
            while (waitingForRetry == true);

            loggingUtilities.Log(this, LogLevel.Information, "Receive operation cancelled.");
        }

        #region Private Methods

        //******************************************************************************
        //
        // Method: AttemptConnect
        //
        //******************************************************************************
        /// <summary>
        /// Attempts to accept an incoming connection, and retries for the specified number of times if the attempt is unsuccessful.
        /// </summary>
        private void AttemptConnect()
        {
            int connectAttempt = 0;

            if (listener.Active == false)
            {
                try
                {
                    listener.Start(1);
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to start TcpListener whilst connecting.", e);
                }

                connected = false;
            }

            while (connectAttempt <= connectRetryCount)
            {
                if (listener.Pending() == true)
                {
                    if (client != null)
                    {
                        client.Close();
                    }
                    try
                    {
                        DisposeClient();
                        client = listener.AcceptTcpClient();
                        connected = true;
                        logger.Log(this, LogLevel.Information, "Connection received on port " + port + ".");
                        break;
                    }
                    catch (System.Net.Sockets.SocketException socketException)
                    {
                        logger.Log(this, LogLevel.Error, "SocketException with error code " + socketException.ErrorCode + " occurred whilst attempting to receive connection on port " + port + ".", socketException);
                        if (connectRetryInterval > 0)
                        {
                            Thread.Sleep(connectRetryInterval);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error attempting to receive connection on port " + port + ".", e);
                    }
                }
                else
                {
                    logger.Log(this, LogLevel.Warning, "No pending connection requests on port " + port + ".");
                    if (connectRetryInterval > 0)
                    {
                        Thread.Sleep(connectRetryInterval);
                    }
                }

                connectAttempt = connectAttempt + 1;
            }

            if (connected == false)
            {
                throw new Exception("Failed to receive connection on port " + port + " after " + connectAttempt + " attempts.");
            }
        }

        //******************************************************************************
        //
        // Method: SetupAndReadMessage
        //
        //******************************************************************************
        /// <summary>
        /// Sets up variables and objects, and calls routines to read a message.
        /// </summary>
        /// <param name="parseState">The current state of parsing the message.</param>
        /// <param name="messageSequenceNumber">Populated with the sequence number of the received message.</param>
        /// <returns>The bytes of the message that were read.</returns>
        /// <remarks>In the usual case this routine will return the entire message.  It will only return a partial message if a cancel request occurs, or a pending connection is detected during reading.</remarks>
        private Queue<byte> SetupAndReadMessage(ref MessageParseState parseState, ref int messageSequenceNumber)
        {
            byte[] messageSequenceNumberBytes = new byte[4];  // Holds the bytes of the message sequence number
            int messageSequenceNumberCurrentPosition = 0;     // The current read position within the message sequence number
            byte[] messageSizeHeaderBytes = new byte[8];      // Holds the bytes of the message size header
            int messageSizeHeaderCurrentPosition = 0;         // The current read position within the message size header 
            long messageSize = -1;                            // The size of the message body
            Queue<byte> messageBytes = new Queue<byte>();     // Holds the bytes of the message body
            INetworkStream networkStream = client.GetStream();

            // Continue to read until a complete message has been received, unless a cancel request has been received or there is a pending connection (i.e. TcpRemoteSender has reconnected due to an error)
            while ((cancelRequest == false) && (listener.Pending() == false) && (parseState != MessageParseState.ReadCompleteMessage))
            {
                ReadAndParseMessageData(networkStream, ref parseState, messageSequenceNumberBytes, ref messageSequenceNumberCurrentPosition, ref messageSequenceNumber, messageSizeHeaderBytes, ref messageSizeHeaderCurrentPosition, ref messageSize, messageBytes);
            }

            // If a complete message has been received, send back the acknowledgement byte
            if ((cancelRequest == false) && (parseState == MessageParseState.ReadCompleteMessage))
            {
                networkStream.WriteByte(messageAcknowledgementByte);
            }

            return messageBytes;
        }

        //******************************************************************************
        //
        // Method: HandleExceptionAndCheckAvailableData
        //
        //******************************************************************************
        /// <summary>
        /// Handles an exception that occurred when attempting to get the amount of data availble from the network, and retries this operation.
        /// </summary>
        /// <param name="availableDataException">The exception that occurred when attempting to get the available data.</param>
        /// <returns>The number of bytes available from the network.</returns>
        private int HandleExceptionAndCheckAvailableData(Exception availableDataException)
        {
            if ((availableDataException is System.Net.Sockets.SocketException) || (availableDataException is ObjectDisposedException))
            {
                logger.Log(this, LogLevel.Error, availableDataException.GetType().Name + " occurred whilst attempting to get available data.", availableDataException);
            }
            else
            {
                throw new Exception("Error receiving message.  Unhandled exception while checking for available data.", availableDataException);
            }

            int availableData = 0;
            AttemptConnect();
            try
            {
                availableData = client.Available;
            }
            catch (Exception e)
            {
                throw new Exception("Error receiving message.  Failed to check available data after reconnecting.", e);
            }

            return availableData;
        }

        //******************************************************************************
        //
        // Method: HandleExceptionAndRereadMessage
        //
        //******************************************************************************
        /// <summary>
        /// Handles an exception that occurred when attempting to read a message, before re-establishing the connection and repeating the read operation.
        /// </summary>
        /// <param name="readException">The exception that occurred when attempting to read the message.</param>
        /// <param name="parseState">The current state of parsing the message.</param>
        /// <param name="messageSequenceNumber">Populated with the seqence number of the received message.</param>
        /// <returns>The bytes of the message that were read.</returns>
        private Queue<byte> HandleExceptionAndRereadMessage(Exception readException, ref MessageParseState parseState, ref int messageSequenceNumber)
        {
            /*
             * In testing of this class, the only exhibited exception for probable real-world network issues (i.e. disconnecting network cable), was the System.IO.IOException.
             * However, the documentation states that numerous exceptions can potentially be thrown by the NetworkStream and TcpClient classes.  Hence the below code block handles all theoretically potential exceptions.
             * These exceptions, and the methods which can cause them are listed below...
             * TcpClient.Available
             *   ObjectDisposedException
             *   SocketException
             * TcpClient.GetStream()
             *   InvalidOperationException
             *   ObjectDisposedException
             * NetworkStream.Read(byte[] buffer, int offset, int size)
             *   IOException
             *   ObjectDisposedException
             * NetworkStream.WriteByte()
             *   IOException
             *   NotSupportedException
             *   ObjectDisposedException
             */

            if (readException is System.IO.IOException)
            {
                logger.Log(this, LogLevel.Error, "IOException occurred whilst attempting to receive and acknowledge message.", readException);
            }
            else if ( (readException is System.Net.Sockets.SocketException) ||
                      (readException is ObjectDisposedException) ||
                      (readException is InvalidOperationException) ||
                      (readException is NotSupportedException) )
            {
                logger.Log(this, LogLevel.Error, readException.GetType().Name + " occurred whilst attempting to receive and acknowledge message.", readException);
            }
            else
            {
                throw new Exception("Error receiving message.  Unhandled exception while attempting to receive and acknowledge message.", readException);
            }

            logger.Log(this, LogLevel.Warning, "Attempting to reconnect to and re-receive.");

            Queue<byte> messageBytes = new Queue<byte>();
            AttemptConnect();
            parseState = MessageParseState.StartOfMessage;
            try
            {
                messageBytes = SetupAndReadMessage(ref parseState, ref messageSequenceNumber);
            }
            catch (Exception e)
            {
                throw new Exception("Error receiving message.  Failed to read message after reconnecting.", e);
            }

            return messageBytes;
        }

        //******************************************************************************
        //
        // Method: ReadAndParseMessageData
        //
        //******************************************************************************
        /// <summary>
        /// Reads bytes from the inputted network stream and parses them, storing the results in the inputted parameters.
        /// </summary>
        /// <param name="networkStream">The network stream to read data from.</param>
        /// <param name="parseState">Represents the current state of parsing.</param>
        /// <param name="messageSequenceNumberBytes">The bytes containing the message sequence number.</param>
        /// <param name="messageSequenceNumberCurrentPosition">The current read position within the message sequence number bytes.</param>
        /// <param name="messageSequenceNumber">The message sequence number.</param>
        /// <param name="messageSizeHeaderBytes">The header bytes containing the message size.</param>
        /// <param name="messageSizeHeaderCurrentPosition">The current read position within the message size header bytes.</param>
        /// <param name="messageSize">The size of the message body.</param>
        /// <param name="messageBytes">The bytes containing the message body.</param>
        private void ReadAndParseMessageData(INetworkStream networkStream, ref MessageParseState parseState, byte[] messageSequenceNumberBytes, ref int messageSequenceNumberCurrentPosition, ref int messageSequenceNumber, byte[] messageSizeHeaderBytes, ref int messageSizeHeaderCurrentPosition, ref long messageSize, Queue<byte> messageBytes)
        {
            int availableBytes = client.Available;

            if (availableBytes > 0)
            {
                byte[] tempBuffer = new byte[availableBytes];  // Temporary buffer to store bytes read from the network before parsing
                networkStream.Read(ref tempBuffer, 0, availableBytes);

                // Iterate through the bytes in the buffer, advancing the parse state as successive sections (i.e. start delimiter, sequence number, size header, body, etc...) are read
                for (int i = 0; i < tempBuffer.Length; i = i + 1)
                {
                    switch (parseState)
                    {
                        case MessageParseState.StartOfMessage:
                            if (tempBuffer[i] != messageStartDelimiter)
                            {
                                throw new Exception("First byte of received message was expected to be " + messageStartDelimiter.ToString() + ", but was " + tempBuffer[i].ToString() + ".");
                            }
                            else
                            {
                                parseState = MessageParseState.ReadStartDelimiter;
                            }
                            break;

                        case MessageParseState.ReadStartDelimiter:
                            messageSequenceNumberBytes[messageSequenceNumberCurrentPosition] = tempBuffer[i];
                            messageSequenceNumberCurrentPosition++;
                            // If 4 bytes have been read into the sequence number byte array, then set the sequence number, and advance to the next parse state
                            if (messageSequenceNumberCurrentPosition == 4)
                            {
                                // Decode as little endian
                                if (BitConverter.IsLittleEndian == false)
                                {
                                    Array.Reverse(messageSequenceNumberBytes);
                                }
                                messageSequenceNumber = BitConverter.ToInt32(messageSequenceNumberBytes, 0);
                                parseState = MessageParseState.ReadSequenceNumber;
                            }
                            break;

                        case MessageParseState.ReadSequenceNumber:
                            messageSizeHeaderBytes[messageSizeHeaderCurrentPosition] = tempBuffer[i];
                            messageSizeHeaderCurrentPosition++;
                            // If 8 bytes have been read into the message size header byte array, then set the message size, and advance to the next parse state
                            if (messageSizeHeaderCurrentPosition == 8)
                            {
                                // Decode as little endian
                                if (BitConverter.IsLittleEndian == false)
                                {
                                    Array.Reverse(messageSizeHeaderBytes);
                                }
                                messageSize = BitConverter.ToInt64(messageSizeHeaderBytes, 0);
                                parseState = MessageParseState.ReadSizeHeader;
                            }
                            break;

                        case MessageParseState.ReadSizeHeader:
                            messageBytes.Enqueue(tempBuffer[i]);
                            // If the number of bytes read matches the size specified in the header, advance to the next parse state
                            if (messageBytes.Count == messageSize)
                            {
                                parseState = MessageParseState.ReadMessageBody;
                            }
                            break;

                        case MessageParseState.ReadMessageBody:
                            if (tempBuffer[i] != messageEndDelimiter)
                            {
                                throw new Exception("Last byte of received message was expected to be " + messageEndDelimiter.ToString() + ", but was " + tempBuffer[i].ToString() + ".");
                            }
                            else
                            {
                                parseState = MessageParseState.ReadCompleteMessage;
                            }
                            break;

                        case MessageParseState.ReadCompleteMessage:
                            throw new Exception("Surplus data encountered after message delimiter character, starting with " + tempBuffer[i].ToString() + ".");
                    }
                }
            }
        }

        //******************************************************************************
        //
        // Method: DisposeClient
        //
        //******************************************************************************
        /// <summary>
        /// Calls the Dispose() method on the client, if it has been initialised.
        /// </summary>
        private void DisposeClient()
        {
            if (client != null)
            {
                client.Dispose();
            }
        }

        //******************************************************************************
        //
        // Method: CheckConnected
        //
        //******************************************************************************
        /// <summary>
        /// Throws an exception if a connection has not been established.
        /// </summary>
        private void CheckConnected()
        {
            if (connected == false)
            {
                throw new Exception("Connection on TCP socket has not been established.");
            }
        }

        #endregion

        #region Finalize / Dispose Methods

        /// <summary>
        /// Releases the unmanaged resources used by the TcpRemoteReceiver.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TcpRemoteReceiver()
        {
            Dispose(false);
        }

        //******************************************************************************
        //
        // Method: Dispose
        //
        //******************************************************************************
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
                }
                // Free your own state (unmanaged objects).
                DisposeClient();
                if (listener != null)
                {
                    listener.Dispose();
                }

                // Set large fields to null.

                connected = false;
                disposed = true;
            }
        }

        //******************************************************************************
        //
        // Method: CheckNotDisposed
        //
        //******************************************************************************
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

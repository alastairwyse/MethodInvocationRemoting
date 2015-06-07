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

package net.alastairwyse.methodinvocationremoting;

import java.net.*;
import java.io.*;
import java.nio.*;
import net.alastairwyse.operatingsystemabstraction.*;
import net.alastairwyse.applicationlogging.*;
import net.alastairwyse.applicationmetrics.*;
import net.alastairwyse.methodinvocationremotingmetrics.*;

/**
 * Represents the state of parsing a message received by the TcpRemoteReceiver class.
 * @author Alastair Wyse
 */
enum MessageParseState {
    /** No data has been parsed. */
    StartOfMessage,
    /** Only the start delimiter has been parsed.  The sequence number is currently being parsed. */
    ReadStartDelimiter,
    /** The start delimiter and sequence number have been parsed.  The message size header is currently being parsed. */
    ReadSequenceNumber, 
    /** The start delimiter, sequence number, and message size header have been parsed.  The message body is currently being parsed. */
    ReadSizeHeader,
    /** The started delimiter, sequence number, message size header, and message body have been parsed.  The end delimiter is being parsed. */
    ReadMessageBody,
    /** The complete message has been parsed. */
    ReadCompleteMessage
}

/**
 * Receives messages from a remote location via a TCP socket connection.
 * @author Alastair Wyse
 */
public class TcpRemoteReceiver implements IRemoteReceiver, AutoCloseable {

    private int port;
    private int connectRetryCount;
    private int connectRetryInterval;
    private int receiveRetryInterval;
    private int socketReadBufferSize;
    private IServerSocketChannel serverSocketChannel;
    private ISocketChannel socketChannel;
    private ISocketChannel pendingSocketChannel;  // Used to temporarily store any socket channel from a pending inbound connection 
    private boolean connected;
    private volatile boolean cancelRequest;
    private volatile boolean waitingForRetry = false;
    private int lastMessageSequenceNumber;
    private IApplicationLogger logger;
    private LoggingUtilities loggingUtilities;
    private IMetricLogger metricLogger;
    /** The string encoding to expect when receiving a message. */
    protected String stringEncodingCharset = "UTF-8";
    /** The byte which denotes the start of the message. */
    protected byte messageStartDelimiter = 0x02;
    /** The byte which denotes the end of the message. */
    protected byte messageEndDelimiter = 0x03;
    /** The byte used to send back to the TcpRemoteSender to acknowledge receipt of the message. */
    protected byte messageAcknowledgementByte = 0x06;

    /**
     * Initialises a new instance of the TcpRemoteReceiver class.
     * @param port                  The port to listen for incoming connections on.
     * @param connectRetryCount     The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteSender.
     * @param connectRetryInterval  The interval between retries to connect or reconnect in milliseconds.
     * @param receiveRetryInterval  The time to wait between attempts to receive a message in milliseconds.
     * @param socketReadBufferSize  The number of bytes to read from the socket in each read operation.
     */
    public TcpRemoteReceiver(int port, int connectRetryCount, int connectRetryInterval, int receiveRetryInterval, int socketReadBufferSize) {
        this.port = port;

        if (connectRetryCount >= 0) {
            this.connectRetryCount = connectRetryCount;
        }
        else {
            throw new IllegalArgumentException("Argument 'connectRetryCount' must be greater than or equal to 0.");
        }

        if (connectRetryInterval >= 0) {
            this.connectRetryInterval = connectRetryInterval;
        }
        else {
            throw new IllegalArgumentException("Argument 'connectRetryInterval' must be greater than or equal to 0.");
        }

        if (receiveRetryInterval >= 0) {
            this.receiveRetryInterval = receiveRetryInterval;
        }
        else {
            throw new IllegalArgumentException("Argument 'receiveRetryInterval' must be greater than or equal to 0.");
        }

        if (socketReadBufferSize > 0) {
            this.socketReadBufferSize = socketReadBufferSize;
        }
        else {
            throw new IllegalArgumentException("Argument 'socketReadBufferSize' must be greater than 0.");
        }

        serverSocketChannel = new ServerSocketChannel();
        logger = new ConsoleApplicationLogger(LogLevel.Information, '|', "  ");
        loggingUtilities = new LoggingUtilities(logger);
        metricLogger = new NullMetricLogger();
        
        lastMessageSequenceNumber = 0;
        connected = false;
        pendingSocketChannel = null;
    }
    
    /**
     * Initialises a new instance of the TcpRemoteReceiver class.
     * @param port                  The port to listen for incoming connections on.
     * @param connectRetryCount     The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteSender.
     * @param connectRetryInterval  The interval between retries to connect or reconnect in milliseconds.
     * @param receiveRetryInterval  The time to wait between attempts to receive a message in milliseconds.
     * @param socketReadBufferSize  The number of bytes to read from the socket in each read operation.
     * @param logger                The logger to write log events to.
     */
    public TcpRemoteReceiver(int port, int connectRetryCount, int connectRetryInterval, int receiveRetryInterval, int socketReadBufferSize, IApplicationLogger logger) {
        this(port, connectRetryCount, connectRetryInterval, receiveRetryInterval, socketReadBufferSize);
        this.logger = logger;
        loggingUtilities = new LoggingUtilities(logger);
    }
    
    /**
     * Initialises a new instance of the TcpRemoteReceiver class.
     * @param port                  The port to listen for incoming connections on.
     * @param connectRetryCount     The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteSender.
     * @param connectRetryInterval  The interval between retries to connect or reconnect in milliseconds.
     * @param receiveRetryInterval  The time to wait between attempts to receive a message in milliseconds.
     * @param socketReadBufferSize  The number of bytes to read from the socket in each read operation.
     * @param metricLogger          The metric logger to write metric and instrumentation events to.
     */
    public TcpRemoteReceiver(int port, int connectRetryCount, int connectRetryInterval, int receiveRetryInterval, int socketReadBufferSize, IMetricLogger metricLogger) {
        this(port, connectRetryCount, connectRetryInterval, receiveRetryInterval, socketReadBufferSize);
        this.metricLogger = metricLogger;
    }
    
    /**
     * Initialises a new instance of the TcpRemoteReceiver class.
     * @param port                  The port to listen for incoming connections on.
     * @param connectRetryCount     The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteSender.
     * @param connectRetryInterval  The interval between retries to connect or reconnect in milliseconds.
     * @param receiveRetryInterval  The time to wait between attempts to receive a message in milliseconds.
     * @param socketReadBufferSize  The number of bytes to read from the socket in each read operation.
     * @param logger                The logger to write log events to.
     * @param metricLogger          The metric logger to write metric and instrumentation events to.
     */
    public TcpRemoteReceiver(int port, int connectRetryCount, int connectRetryInterval, int receiveRetryInterval, int socketReadBufferSize, IApplicationLogger logger, IMetricLogger metricLogger) {
        this(port, connectRetryCount, connectRetryInterval, receiveRetryInterval, socketReadBufferSize);
        this.logger = logger;
        loggingUtilities = new LoggingUtilities(logger);
        this.metricLogger = metricLogger;
    }
    
    /**
     * Initialises a new instance of the TcpRemoteReceiver class.  
     * <b>Note</b> this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
     * @param port                  The port to listen for incoming connections on.
     * @param connectRetryCount     The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteSender.
     * @param connectRetryInterval  The interval between retries to connect or reconnect in milliseconds.
     * @param receiveRetryInterval  The time to wait between attempts to receive a message in milliseconds.
     * @param socketReadBufferSize  The number of bytes to read from the socket in each read operation.
     * @param logger                The logger to write log events to.
     * @param metricLogger          The metric logger to write metric and instrumentation events to.
     * @param serverSocketChannel   A test (mock) server socket channel.
     */
    public TcpRemoteReceiver(int port, int connectRetryCount, int connectRetryInterval, int receiveRetryInterval, int socketReadBufferSize, IApplicationLogger logger, IMetricLogger metricLogger, IServerSocketChannel serverSocketChannel) {
        this(port, connectRetryCount, connectRetryInterval, receiveRetryInterval, socketReadBufferSize, logger);
        this.serverSocketChannel = serverSocketChannel;
        this.metricLogger = metricLogger;
    }
    
    /**
     * Listens for and accepts an incoming connection on the configured TCP port.
     * @throws Exception  if an error occurs while attempting to connect.
     */
    public void Connect() throws Exception {
        if (connected == true)
        {
            throw new Exception("Connection has already been established.");
        }
        AttemptConnect();
    }
    
    /**
     * Disconnects a connected client and stops listening on the configured TCP port.
     * @throws Exception  if an error occurs while attempting to disconnect.
     */
    public void Disconnect() throws Exception {
        try {
            if(socketChannel != null) {
                socketChannel.close();
            }
            serverSocketChannel.close();
            connected = false;
        }
        catch(Exception e) {
            throw new Exception("Failed to disconnect listener.", e);
        }
        
        /* //[BEGIN_LOGGING]
        logger.Log(this, LogLevel.Information, "Disconnected.");
        //[END_LOGGING] */
    }
    
    @Override
    public void close() throws Exception {
        Disconnect();
    }
    
    @Override
    public String Receive() throws Exception {
        cancelRequest = false;
        CheckConnected();
        int messageSequenceNumber = -1;
        String returnMessage = "";
        
        while (cancelRequest == false) {
            // Check if there are any pending connections which would indicate the TcpRemoteSender has encountered an error and reconnected
            if (PendingConnectionExists() == true) {
                logger.Log(this, LogLevel.Warning, "New connection detected.  Attempting reconnect.");
                AttemptConnect();
                /* //[BEGIN_METRICS]
                metricLogger.Increment(new TcpRemoteReceiverReconnected());
                //[END_METRICS] */
            }
            
            // Check if any data has been received from the socket channel, and handle and retry if an exception occurs
            ByteBuffer initialReceivedBytes = ByteBuffer.allocate(socketReadBufferSize);
            int receivedDataCount = 0;
            try {
                receivedDataCount = socketChannel.read(initialReceivedBytes);
            }
            catch (Exception e) {
                receivedDataCount = HandleExceptionAndReadReceivedData(initialReceivedBytes, e);
            }
        
            // If data has been received, attempt parse it and read and parse any remaining data, and handle and retry if an exception occurs
            if (receivedDataCount > 0){
                /* //[BEGIN_METRICS]
                metricLogger.Begin(new MessageReceiveTime());
                //[END_METRICS] */
                
                MessageParseState parseState = MessageParseState.StartOfMessage;
                ByteBuffer messageBytes = null;  // Holds the bytes which form the body of the message received
                SetupAndReadMessageParameters methodParameters = new SetupAndReadMessageParameters(parseState, messageSequenceNumber);
                
                try {
                    messageBytes = SetupAndReadMessage(initialReceivedBytes, methodParameters);
                }
                catch (Exception e) {
                    messageBytes = HandleExceptionAndRereadMessage(e, methodParameters);
                }
                
                // Copy primitive parameters back to their original variables
                parseState = methodParameters.parseState;
                messageSequenceNumber = methodParameters.messageSequenceNumber;
                
                // If the complete message has been read, break out of the current while loop
                //   If the complete message was not read it would have been caused by a cancel request, or by a pending connection which is handled outside this block
                if ((cancelRequest == false) && (parseState == MessageParseState.ReadCompleteMessage)) {
                    // If the sequence number of the message is the same as the last received message, then discard the message
                    //   This situation can be caused by the connection breaking before the sender received the last acknowledgment
                    if (messageSequenceNumber != lastMessageSequenceNumber) {
                        lastMessageSequenceNumber = messageSequenceNumber;
                        returnMessage = new String(messageBytes.array(), stringEncodingCharset);
                        
                        /* //[BEGIN_METRICS]
                        metricLogger.End(new MessageReceiveTime());
                        metricLogger.Increment(new MessageReceived());
                        metricLogger.Add(new ReceivedMessageSize(returnMessage.length()));
                        //[END_METRICS] */
                        /* //[BEGIN_LOGGING]
                        loggingUtilities.LogMessageReceived(this, returnMessage);
                        //[END_LOGGING] */
                        break;
                    }
                    else {
                        /* //[BEGIN_METRICS]
                        metricLogger.Increment(new TcpRemoteReceiverDuplicateSequenceNumber());
                        //[END_METRICS] */
                        logger.Log(this, LogLevel.Warning, "Duplicate message with sequence number " + messageSequenceNumber + " received.  Message discarded.");
                        // Reset variables
                        messageSequenceNumber = -1;
                        returnMessage = "";
                    }
                }
                /* //[BEGIN_METRICS]
                metricLogger.End(new MessageReceiveTime());
                //[END_METRICS] */
            }

            waitingForRetry = true;
            if (receiveRetryInterval > 0){
                Thread.sleep(receiveRetryInterval);
            }
            waitingForRetry = false;
        }
        
        return returnMessage;
    }

    @Override
    public void CancelReceive() {
        cancelRequest = true;
        while (waitingForRetry == true);
        
        /* //[BEGIN_LOGGING]
        try {
            logger.Log(this, LogLevel.Information, "Receive operation cancelled.");
        }
        catch(Exception e) {
        }
        //[END_LOGGING] */
    }
    
    /**
     * Throws an exception if a connection has not been established.
     * @throws Exception  if a connection has not been established.
     */
    private void CheckConnected() throws Exception {
        if (connected == false) {
            throw new Exception("Connection on TCP socket has not been established.");
        }
    }

    /**
     * Attempts to accept an incoming connection, and retries for the specified number of times if the attempt is unsuccessful.
     */
    private void AttemptConnect() throws Exception {
        int connectAttempt = 0;
        
        if (socketChannel != null) {
            socketChannel.close();
        }
        socketChannel = null;

        if (serverSocketChannel.isOpen() == false) {
            try {
                serverSocketChannel.open();
                serverSocketChannel.bind(new InetSocketAddress(port), 1);
                serverSocketChannel.configureBlocking(false);
            }
            catch (Exception e) {
                throw new Exception("Failed to open ServerSocketChannel whilst connecting.", e);
            }
            
            connected = false;
        }
   
        while (connectAttempt <= connectRetryCount) {
            try {
                if (PendingConnectionExists() == true) {
                    socketChannel = pendingSocketChannel;
                    pendingSocketChannel = null;
                    socketChannel.configureBlocking(false);
                    connected = true;
                    logger.Log(this, LogLevel.Information, "Connection received on port " + port + ".");
                    break;
                }
                else {
                    logger.Log(this, LogLevel.Warning, "No pending connection requests on port " + port + ".");
                    if (connectRetryInterval > 0) {
                        Thread.sleep(connectRetryInterval);
                    }
                }
            }
            catch (IOException ioException) {
                logger.Log(this, LogLevel.Error, ioException.getClass().getSimpleName() + " occurred whilst trying to to receive connection on port " + port + ".", ioException);
                if (connectRetryInterval > 0) {
                    Thread.sleep(connectRetryInterval);
                }
            }
            catch (Exception e) {
                throw new Exception("Error attempting to receive connection on port " + port + ".", e);
            }

            connectAttempt = connectAttempt + 1;
        }

        if (connected == false) {
            throw new Exception("Failed to receive connection on port " + port + " after " + connectAttempt + " attempts.");
        }
    }

    /**
     * Returns true if either a pending connection was already set on member pendingSocketChannel, or if a pending connection was found after calling ServerSocketChannel.accept().
     * @return              Whether there is currently a pending TCP connection.
     * @throws IOException  if an error occurred when attempting to accept the connection.
     */
    private Boolean PendingConnectionExists() throws IOException {
        if (pendingSocketChannel != null) {
            return true;
        }
        pendingSocketChannel = serverSocketChannel.accept();
        if (pendingSocketChannel == null) {
            return false;
        }
        else {
            return true;
        }
    }
    
    /**
     * Handles an exception that occurred when attempting to read data from the socket channel, and retries this operation.
     * @param receivedDataBuffer  The buffer to write the received data to.
     * @param readException       The exception that occurred when attempting to read data from the socket channel.
     * @return                    The number of bytes read from the socket channel.
     * @throws Exception          if an unhandled error occurred when attempting to read the data.
     */
    private int HandleExceptionAndReadReceivedData(ByteBuffer receivedDataBuffer, Exception readException) throws Exception {
        if ((readException instanceof java.nio.channels.AsynchronousCloseException) || (readException instanceof java.nio.channels.ClosedByInterruptException)) {
            throw new Exception("Error receiving message.  Unhandled exception while checking for available data.", readException);
        }
        else {
            logger.Log(this, LogLevel.Error, readException.getClass().getSimpleName() + " occurred whilst checking for available data.", readException);
        }
        
        int receivedDataCount = 0;
        AttemptConnect();
        /* //[BEGIN_METRICS]
        metricLogger.Increment(new TcpRemoteReceiverReconnected());
        //[END_METRICS] */
        try {
            receivedDataCount = socketChannel.read(receivedDataBuffer);
        }
        catch (Exception e) {
            throw new Exception("Error receiving message.  Failed to check available data after reconnecting.", e);
        }

        return receivedDataCount;
    }
    
    /**
     * Sets up variables and objects, and calls routines to read a message or the remainder of a message.
     * @param initialReceivedBytes  Bytes that were read from the network when initially checking for a received message.
     * @param methodParameters      Container holding the primitive type parameters passed to the method.  These are wrapped in a container object to allow pass by reference.
     * @return                      Buffer containing the bytes of the message body that were read.
     * @throws Exception            if an unhandled error occurred.
     */
    private ByteBuffer SetupAndReadMessage(ByteBuffer initialReceivedBytes, SetupAndReadMessageParameters methodParameters) throws Exception {
        ByteBuffer tempBuffer;                            // Temporary buffer to store bytes read from the network before parsing
        int parseStartIndex = 0;                          // The position in the temporary buffer to begin parsing from
        int parseLength;                                  // The number of bytes to parse from the temporary buffer
        byte[] messageSequenceNumberBytes = new byte[4];  // Holds the bytes of the message sequence number
        byte[] messageSizeHeaderBytes = new byte[8];      // Holds the bytes of the message size header
        ByteBuffer messageBytes = null;                   // Holds the bytes of the message body
        ParseMessageDataParameters parseMessageDataParameters = new ParseMessageDataParameters(methodParameters.parseState, 0, methodParameters.messageSequenceNumber, 0, messageBytes);
        
        // Parse the bytes in parameter initialReceivedBytes
        if (initialReceivedBytes != null) {
            ParseMessageData(initialReceivedBytes.array(), 0, initialReceivedBytes.position(), messageSequenceNumberBytes, messageSizeHeaderBytes, parseMessageDataParameters);

            // If the initial buffer is full, create a new one
            if (initialReceivedBytes.hasRemaining() == false) {
                tempBuffer = ByteBuffer.allocate(socketReadBufferSize);
                parseStartIndex = 0;
                /* //[BEGIN_METRICS]
                metricLogger.Increment(new TcpRemoteReceiverReadBufferCreated());
                //[END_METRICS] */
            }
            // Otherwise set the initially read ByteBuffer to member tempBuffer, and update the parse start index to the correct position
            else {
                tempBuffer = initialReceivedBytes;
                parseStartIndex = tempBuffer.position();
            }
            
            // Copy primitive parameters back to the parameter container
            methodParameters.parseState = parseMessageDataParameters.parseState;
        }
        else {
            tempBuffer = ByteBuffer.allocate(socketReadBufferSize);
        }
        
        // Continue to read until a complete message has been received, unless a cancel request has been received or there is a pending connection (i.e. TcpRemoteSender has reconnected due to an error)
        while ((cancelRequest == false) && (PendingConnectionExists() == false) && (methodParameters.parseState != MessageParseState.ReadCompleteMessage)) { 
            parseLength = socketChannel.read(tempBuffer);
            ParseMessageData(tempBuffer.array(), parseStartIndex, parseLength, messageSequenceNumberBytes, messageSizeHeaderBytes, parseMessageDataParameters);
            
            // If the temporary buffer is full, create a new one
            if (tempBuffer.hasRemaining() == false) {
                tempBuffer = ByteBuffer.allocate(socketReadBufferSize);
                parseStartIndex = 0;
                /* //[BEGIN_METRICS]
                metricLogger.Increment(new TcpRemoteReceiverReadBufferCreated());
                //[END_METRICS] */
            }
            // Otherwise update the parse start index to the correct position
            else {
                parseStartIndex = parseStartIndex + parseLength;
            }
            
            // Copy primitive parameters back to the parameter container
            methodParameters.parseState = parseMessageDataParameters.parseState;
        }
        
        // If a complete message has been received, send back the acknowledgement byte
        if ((cancelRequest == false) && (methodParameters.parseState == MessageParseState.ReadCompleteMessage)) {
            socketChannel.write(ByteBuffer.wrap(new byte[] { messageAcknowledgementByte }));
        }
        
        // Copy primitive parameters back to the parameter container
        methodParameters.messageSequenceNumber = parseMessageDataParameters.messageSequenceNumber;
        
        return parseMessageDataParameters.messageBodyBytes;
    }
    
    /**
     * Parses a subset of message bytes specified by the inputted byte array, start index, and length.  Results of parsing are stored in the remaining parameters.
     * @param parseBytes                  The portion of the message to parse.
     * @param startIndex                  The index of parameter 'messageBytes' to begin parsing at.
     * @param parseLength                 The number of bytes to parse.
     * @param messageSequenceNumberBytes  The bytes containing the message sequence number.
     * @param messageSizeHeaderBytes      The header bytes containing the message size.
     * @param methodParameters            Container holding the primitive type parameters passed to the method.  These are wrapped in a container object to allow pass by reference.
     * @throws Exception                  if an unhandled error occurred.
     */
    private void ParseMessageData(byte[] parseBytes, int startIndex, int parseLength, byte[] messageSequenceNumberBytes, byte[] messageSizeHeaderBytes, ParseMessageDataParameters methodParameters) throws Exception {
        for (int i = startIndex; i < (startIndex + parseLength); i++) {
            switch (methodParameters.parseState) {
                case StartOfMessage: 
                    if (parseBytes[i] != messageStartDelimiter) {
                        throw new Exception("First byte of received message was expected to be " + messageStartDelimiter + ", but was " + parseBytes[i] + ".");
                    }
                    else {
                        methodParameters.parseState = MessageParseState.ReadStartDelimiter;
                    }
                    break;
                    
                case ReadStartDelimiter:
                    messageSequenceNumberBytes[methodParameters.messageSequenceNumberCurrentPosition] = parseBytes[i];
                    methodParameters.messageSequenceNumberCurrentPosition++;
                    // If 4 bytes have been read into the sequence number byte array, then set the sequence number, and advance to the next parse state
                    if (methodParameters.messageSequenceNumberCurrentPosition == 4) {
                        // Decode as little endian
                        ByteBuffer tempBuffer = ByteBuffer.wrap(messageSequenceNumberBytes);
                        tempBuffer.order(ByteOrder.LITTLE_ENDIAN);
                        methodParameters.messageSequenceNumber = tempBuffer.getInt();
                        methodParameters.parseState = MessageParseState.ReadSequenceNumber;
                    }
                    break;
                    
                case ReadSequenceNumber:
                    messageSizeHeaderBytes[methodParameters.messageSizeHeaderCurrentPosition] = parseBytes[i];
                    methodParameters.messageSizeHeaderCurrentPosition++;
                    // If 8 bytes have been read into the message size header byte array, then set the message size, and advance to the next parse state
                    if (methodParameters.messageSizeHeaderCurrentPosition == 8) {
                        // Decode as little endian
                        ByteBuffer tempBuffer = ByteBuffer.wrap(messageSizeHeaderBytes);
                        tempBuffer.order(ByteOrder.LITTLE_ENDIAN);
                        // Initialise the message body byte array
                        // TODO: Note that casting a long to an int like this could result in a negative number which will cause the allocate() method to throw an IllegalArgumentException
                        methodParameters.messageBodyBytes = ByteBuffer.allocate((int)tempBuffer.getLong());
                        methodParameters.parseState = MessageParseState.ReadSizeHeader;
                    }
                    break;
                    
                case ReadSizeHeader:
                    methodParameters.messageBodyBytes.put(parseBytes[i]);
                    // If there is no space remaining in the message body buffer, advance to the next parse state
                    if (methodParameters.messageBodyBytes.hasRemaining() == false) {
                        methodParameters.parseState = MessageParseState.ReadMessageBody;
                    }
                    break;
                    
                case ReadMessageBody:
                    if (parseBytes[i] != messageEndDelimiter) {
                        throw new Exception("Last byte of received message was expected to be " + messageEndDelimiter + ", but was " + parseBytes[i] + ".");
                    }
                    else {
                        methodParameters.parseState = MessageParseState.ReadCompleteMessage;
                    }
                    break;

                case ReadCompleteMessage:
                    throw new Exception("Surplus data encountered after message delimiter character, starting with " + parseBytes[i] + ".");
            }
        }
    }
    
    /**
     * Handles an exception that occurred when attempting to read a message, before re-establishing the connection and repeating the read operation.
     * @param readException     The exception that occurred when attempting to read the message.
     * @param methodParameters  Container holding the primitive type parameters passed to the method. These are wrapped in a container object to allow pass by reference.
     * @return                  Buffer containing the bytes of the message body that were read.
     * @throws Exception        if an unhandled error occurred.
     */
    private ByteBuffer HandleExceptionAndRereadMessage(Exception readException, SetupAndReadMessageParameters methodParameters) throws Exception {
        /*
         * All likely real-world network issues caught by this method derive from java.io.IOException.
         * The only exception to this are the AsynchronousCloseException and ClosedByInterruptExceptions, which would result from concurrent threads accessing the class, which it is not designed to handle.
         * A full list of the methods which could cause an exception, and the possible exceptions for each method appears below...
         * ServerSocketChannel.accept()
         *   ClosedChannelException
         *   AsynchronousCloseException
         *   ClosedByInterruptException
         *   NotYetBoundException
         *   SecurityException
         *   IOException
         * SocketChannel.read(ByteBuffer dst)
         *   NotYetConnectedException
         *   ClosedChannelException
         *   AsynchronousCloseException
         *   ClosedByInterruptException
         *   IOException
         * SocketChannel.write(ByteBuffer src)
         *   NotYetConnectedException
         *   ClosedChannelException
         *   AsynchronousCloseException
         *   ClosedByInterruptException
         *   IOException
         */
        ByteBuffer messageBytes = null; 
        
        try {
            // This 'if' block is required, even though the action contained in it is the same as for the 'else' block, due to AsynchronousCloseException and ClosedByInterruptException deriving from IOException
            if ((readException instanceof java.nio.channels.AsynchronousCloseException) || (readException instanceof java.nio.channels.ClosedByInterruptException)) {
                throw new Exception("Error receiving message.  Unhandled exception while attempting to receive and acknowledge message.", readException);
            }
            else if (readException instanceof IOException) {
                logger.Log(this, LogLevel.Error, readException.getClass().getSimpleName() + " occurred whilst attempting to receive and acknowledge message.", readException);
            }
            else {
                throw new Exception("Error receiving message.  Unhandled exception while attempting to receive and acknowledge message.", readException);
            }
            
            logger.Log(this, LogLevel.Warning, "Attempting to reconnect to and re-receive.");
    
            AttemptConnect();
            /* //[BEGIN_METRICS]
            metricLogger.Increment(new TcpRemoteReceiverReconnected());
            //[END_METRICS] */
            methodParameters.parseState = MessageParseState.StartOfMessage;
            try {
                messageBytes = SetupAndReadMessage(null, methodParameters);
            }
            catch (Exception e) {
                throw new Exception("Error receiving message.  Failed to read message after reconnecting.", e);
            }
        }
        catch (Exception e) {
            /* //[BEGIN_METRICS]
            metricLogger.CancelBegin(new MessageReceiveTime());
            //[END_METRICS] */
            throw e;
        }
        
        return messageBytes;
    }

    /**
     * Reverses the order of the inputted array of bytes.
     * @param inputByteArray  The array of bytes to reverse.
     */
    private void ReverseByteArray(byte[] inputByteArray) {
        int lowerIndex = 0;
        int upperIndex = inputByteArray.length - 1;
        
        while((upperIndex - lowerIndex) > 0) {
            byte swapStorage = inputByteArray[lowerIndex];
            inputByteArray[lowerIndex] = inputByteArray[upperIndex];
            inputByteArray[upperIndex] = swapStorage;
            
            lowerIndex++;
            upperIndex--;
        }
    }
    
    /**
     * Container class to hold parameters passed to method SetupAndReadMessage.  Allows values in the class to be changed, to simulate passing by reference.
     * @author Alastair Wyse
     */
    private class SetupAndReadMessageParameters {
        
        /**
         * Initialises a new instance of the SetupAndReadMessageParameters class.
         * @param parseState             The current state of parsing the message.
         * @param messageSequenceNumber  The sequence number of the received message.
         */
        public SetupAndReadMessageParameters(MessageParseState parseState, int messageSequenceNumber) {
            this.parseState = parseState;
            this.messageSequenceNumber = messageSequenceNumber;
        }
        
        /**
         * The current state of parsing the message.
         */
        public MessageParseState parseState;
        
        /**
         * The sequence number of the received message.
         */
        public int messageSequenceNumber;
    }
    
    /**
     * Container class to hold parameters passed to method ParseMessageData.  Allows values in the class to be changed, to simulate passing by reference.
     * @author Alastair Wyse
     */
    private class ParseMessageDataParameters {
        
        /**
         * Initialises a new instance of the ParseMessageDataParameters class.
         * @param parseState                            The current state of parsing the message.
         * @param messageSequenceNumberCurrentPosition  The current read position within the message sequence number bytes.
         * @param messageSequenceNumber                 The message sequence number.
         * @param messageSizeHeaderCurrentPosition      The current read position within the message size header bytes.
         * @param messageBodyBytes                      The bytes containing the message body.
         */
        public ParseMessageDataParameters(MessageParseState parseState, int messageSequenceNumberCurrentPosition, int messageSequenceNumber, int messageSizeHeaderCurrentPosition, ByteBuffer messageBodyBytes) {
            this.parseState = parseState;
            this.messageSequenceNumberCurrentPosition = messageSequenceNumberCurrentPosition;
            this.messageSequenceNumber = messageSequenceNumber;
            this.messageSizeHeaderCurrentPosition = messageSizeHeaderCurrentPosition;
            this.messageBodyBytes = messageBodyBytes;
        }
        
        /**
         * The current state of parsing the message.
         */
        public MessageParseState parseState;
        
        /**
         * The current read position within the message sequence number bytes.
         */
        public int messageSequenceNumberCurrentPosition;
        
        /**
         * The message sequence number.
         */
        public int messageSequenceNumber;
        
        /**
         * The current read position within the message size header bytes.
         */
        public int messageSizeHeaderCurrentPosition;
        
        /**
         * The bytes containing the message body.
         */
        public ByteBuffer messageBodyBytes;
    }
}

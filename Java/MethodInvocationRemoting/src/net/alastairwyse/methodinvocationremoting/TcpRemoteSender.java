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

package net.alastairwyse.methodinvocationremoting;

import java.io.*;
import java.net.*;
import java.nio.*;
import net.alastairwyse.operatingsystemabstraction.*;
import net.alastairwyse.applicationlogging.*;
import net.alastairwyse.applicationmetrics.*;
import net.alastairwyse.methodinvocationremotingmetrics.*;

/**
 * Sends messages to a remote location via a TCP socket connection.
 * @author Alastair Wyse
 */
public class TcpRemoteSender implements IRemoteSender, AutoCloseable {

    private InetAddress ipAddress;
    private int port;
    private int connectRetryCount;
    private int connectRetryInterval;
    private int acknowledgementReceiveTimeout;
    private int acknowledgementReceiveRetryInterval;
    private ISocketChannel socketChannel;
    private int messageSequenceNumber;
    private IApplicationLogger logger;
    private IMetricLogger metricLogger;
    /** The string encoding to use when sending a message. */
    protected String stringEncodingCharset = "UTF-8";
    /** The byte which denotes the start of the message when sending. */
    protected byte messageStartDelimiter = 0x02;
    /** The byte which denotes the end of the message when sending. */
    protected byte messageEndDelimiter = 0x03;
    /** The byte which is expected to be received back from the TcpRemoteReceiver to acknowledge receipt of the message. */
    protected byte messageAcknowledgementByte = 0x06;
    
    /**
     * Initialises a new instance of the TcpRemoteSender class.
     * @param ipAddress                            The remote IP address to connect to.
     * @param port                                 The remote port to connect to.
     * @param connectRetryCount                    The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteReceiver.
     * @param connectRetryInterval                 The interval between retries to connect or reconnect in milliseconds.
     * @param acknowledgementReceiveTimeout        The maximum time to wait for an acknowledgement of a message in milliseconds.
     * @param acknowledgementReceiveRetryInterval  The time between retries to check for an acknowledgement in milliseconds.
     */
    public TcpRemoteSender(InetAddress ipAddress, int port, int connectRetryCount, int connectRetryInterval, int acknowledgementReceiveTimeout, int acknowledgementReceiveRetryInterval) {
        if (this.ipAddress == null) {
            this.ipAddress = ipAddress;
        }

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

        if (acknowledgementReceiveTimeout >= 0) {
            this.acknowledgementReceiveTimeout = acknowledgementReceiveTimeout;
        }
        else {
            throw new IllegalArgumentException("Argument 'acknowledgementReceiveTimeout' must be greater than or equal to 0.");
        }

        if(acknowledgementReceiveRetryInterval >= 0) {
            this.acknowledgementReceiveRetryInterval = acknowledgementReceiveRetryInterval;
        }
        else {
            throw new IllegalArgumentException("Argument 'acknowledgementReceiveRetryInterval' must be greater than or equal to 0.");
        }
        
        logger = new ConsoleApplicationLogger(LogLevel.Information, '|', "  ");
        metricLogger = new NullMetricLogger();
        socketChannel = new SocketChannel();

        messageSequenceNumber = 1;
    }
    
    /**
     * Initialises a new instance of the TcpRemoteSender class.
     * @param ipAddress                            The remote IP address to connect to.
     * @param port                                 The remote port to connect to.
     * @param connectRetryCount                    The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteReceiver.
     * @param connectRetryInterval                 The interval between retries to connect or reconnect in milliseconds.
     * @param acknowledgementReceiveTimeout        The maximum time to wait for an acknowledgement of a message in milliseconds.
     * @param acknowledgementReceiveRetryInterval  The time between retries to check for an acknowledgement in milliseconds.
     * @throws UnknownHostException                If the specified IP address could not be resolved.
     */
    public TcpRemoteSender(String ipAddress, int port, int connectRetryCount, int connectRetryInterval, int acknowledgementReceiveTimeout, int acknowledgementReceiveRetryInterval) throws UnknownHostException {
        this(InetAddress.getByName(ipAddress), port, connectRetryCount, connectRetryInterval, acknowledgementReceiveTimeout, acknowledgementReceiveRetryInterval);
    }
    
    /**
     * Initialises a new instance of the TcpRemoteSender class.
     * @param ipAddress                            The remote IP address to connect to.
     * @param port                                 The remote port to connect to.
     * @param connectRetryCount                    The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteReceiver.
     * @param connectRetryInterval                 The interval between retries to connect or reconnect in milliseconds.
     * @param acknowledgementReceiveTimeout        The maximum time to wait for an acknowledgement of a message in milliseconds.
     * @param acknowledgementReceiveRetryInterval  The time between retries to check for an acknowledgement in milliseconds.
     * @param logger                               The logger to write log events to.
     */
    public TcpRemoteSender(InetAddress ipAddress, int port, int connectRetryCount, int connectRetryInterval, int acknowledgementReceiveTimeout, int acknowledgementReceiveRetryInterval, IApplicationLogger logger) {
        this(ipAddress, port, connectRetryCount, connectRetryInterval, acknowledgementReceiveTimeout, acknowledgementReceiveRetryInterval);
        this.logger = logger;
    }
    
    /**
     * Initialises a new instance of the TcpRemoteSender class.
     * @param ipAddress                            The remote IP address to connect to.
     * @param port                                 The remote port to connect to.
     * @param connectRetryCount                    The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteReceiver.
     * @param connectRetryInterval                 The interval between retries to connect or reconnect in milliseconds.
     * @param acknowledgementReceiveTimeout        The maximum time to wait for an acknowledgement of a message in milliseconds.
     * @param acknowledgementReceiveRetryInterval  The time between retries to check for an acknowledgement in milliseconds.
     * @param metricLogger                         The metric logger to write metric and instrumentation events to.
     */
    public TcpRemoteSender(InetAddress ipAddress, int port, int connectRetryCount, int connectRetryInterval, int acknowledgementReceiveTimeout, int acknowledgementReceiveRetryInterval, IMetricLogger metricLogger) {
        this(ipAddress, port, connectRetryCount, connectRetryInterval, acknowledgementReceiveTimeout, acknowledgementReceiveRetryInterval);
        this.metricLogger = metricLogger;
    }
    
    /**
     * Initialises a new instance of the TcpRemoteSender class.
     * @param ipAddress                            The remote IP address to connect to.
     * @param port                                 The remote port to connect to.
     * @param connectRetryCount                    The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteReceiver.
     * @param connectRetryInterval                 The interval between retries to connect or reconnect in milliseconds.
     * @param acknowledgementReceiveTimeout        The maximum time to wait for an acknowledgement of a message in milliseconds.
     * @param acknowledgementReceiveRetryInterval  The time between retries to check for an acknowledgement in milliseconds.
     * @param logger                               The logger to write log events to.
     * @param metricLogger                         The metric logger to write metric and instrumentation events to.
     */
    public TcpRemoteSender(InetAddress ipAddress, int port, int connectRetryCount, int connectRetryInterval, int acknowledgementReceiveTimeout, int acknowledgementReceiveRetryInterval, IApplicationLogger logger, IMetricLogger metricLogger) {
        this(ipAddress, port, connectRetryCount, connectRetryInterval, acknowledgementReceiveTimeout, acknowledgementReceiveRetryInterval);
        this.logger = logger;
        this.metricLogger = metricLogger;
    }
    
    /**
     * Initialises a new instance of the TcpRemoteSender class.
     * @param ipAddress                            The remote IP address to connect to.
     * @param port                                 The remote port to connect to.
     * @param connectRetryCount                    The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteReceiver.
     * @param connectRetryInterval                 The interval between retries to connect or reconnect in milliseconds.
     * @param acknowledgementReceiveTimeout        The maximum time to wait for an acknowledgement of a message in milliseconds.
     * @param acknowledgementReceiveRetryInterval  The time between retries to check for an acknowledgement in milliseconds.
     * @param logger                               The logger to write log events to.
     * @throws UnknownHostException                If the specified IP address could not be resolved.
     */
    public TcpRemoteSender(String ipAddress, int port, int connectRetryCount, int connectRetryInterval, int acknowledgementReceiveTimeout, int acknowledgementReceiveRetryInterval, IApplicationLogger logger) throws UnknownHostException {
        this(ipAddress, port, connectRetryCount, connectRetryInterval, acknowledgementReceiveTimeout, acknowledgementReceiveRetryInterval);
        this.logger = logger;
    }
    
    /**
     * Initialises a new instance of the TcpRemoteSender class.
     * @param ipAddress                            The remote IP address to connect to.
     * @param port                                 The remote port to connect to.
     * @param connectRetryCount                    The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteReceiver.
     * @param connectRetryInterval                 The interval between retries to connect or reconnect in milliseconds.
     * @param acknowledgementReceiveTimeout        The maximum time to wait for an acknowledgement of a message in milliseconds.
     * @param acknowledgementReceiveRetryInterval  The time between retries to check for an acknowledgement in milliseconds.
     * @param metricLogger                         The metric logger to write metric and instrumentation events to.
     * @throws UnknownHostException                If the specified IP address could not be resolved.
     */
    public TcpRemoteSender(String ipAddress, int port, int connectRetryCount, int connectRetryInterval, int acknowledgementReceiveTimeout, int acknowledgementReceiveRetryInterval, IMetricLogger metricLogger) throws UnknownHostException {
        this(ipAddress, port, connectRetryCount, connectRetryInterval, acknowledgementReceiveTimeout, acknowledgementReceiveRetryInterval);
        this.metricLogger = metricLogger;
    }
    
    /**
     * Initialises a new instance of the TcpRemoteSender class.
     * @param ipAddress                            The remote IP address to connect to.
     * @param port                                 The remote port to connect to.
     * @param connectRetryCount                    The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteReceiver.
     * @param connectRetryInterval                 The interval between retries to connect or reconnect in milliseconds.
     * @param acknowledgementReceiveTimeout        The maximum time to wait for an acknowledgement of a message in milliseconds.
     * @param acknowledgementReceiveRetryInterval  The time between retries to check for an acknowledgement in milliseconds.
     * @param logger                               The logger to write log events to.
     * @param metricLogger                         The metric logger to write metric and instrumentation events to.
     * @throws UnknownHostException                If the specified IP address could not be resolved.
     */
    public TcpRemoteSender(String ipAddress, int port, int connectRetryCount, int connectRetryInterval, int acknowledgementReceiveTimeout, int acknowledgementReceiveRetryInterval, IApplicationLogger logger, IMetricLogger metricLogger) throws UnknownHostException {
        this(ipAddress, port, connectRetryCount, connectRetryInterval, acknowledgementReceiveTimeout, acknowledgementReceiveRetryInterval);
        this.logger = logger;
        this.metricLogger = metricLogger;
    }
    
    /**
     * Initialises a new instance of the TcpRemoteSender class.  
     * <b>Note</b> this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
     * @param ipAddress                            The remote IP address to connect to.
     * @param port                                 The remote port to connect to.
     * @param connectRetryCount                    The number of times to retry when initially connecting, or attempting to reconnect to a TcpRemoteReceiver.
     * @param connectRetryInterval                 The interval between retries to connect or reconnect in milliseconds.
     * @param acknowledgementReceiveTimeout        The maximum time to wait for an acknowledgement of a message in milliseconds.
     * @param acknowledgementReceiveRetryInterval  The time between retries to check for an acknowledgement in milliseconds.
     * @param logger                               The logger to write log events to.
     * @param metricLogger                         The metric logger to write metric and instrumentation events to.
     * @param socketChannel                        A test (mock) socket channel.
     * @throws UnknownHostException                If the specified IP address could not be resolved.
     */
    public TcpRemoteSender(String ipAddress, int port, int connectRetryCount, int connectRetryInterval, int acknowledgementReceiveTimeout, int acknowledgementReceiveRetryInterval, IApplicationLogger logger, IMetricLogger metricLogger, ISocketChannel socketChannel) throws UnknownHostException {
        this(InetAddress.getByName(ipAddress), port, connectRetryCount, connectRetryInterval, acknowledgementReceiveTimeout, acknowledgementReceiveRetryInterval, logger, metricLogger);
        this.socketChannel = socketChannel;
    }
    
    /**
     * Connects to the configured IP address and port.
     * @throws Exception  If an error occurs while attempting to connect.
     */
    public void Connect() throws Exception {
        if(socketChannel.isConnected() == true) {
            throw new Exception("Connection to TCP socket has already been established.");
        }
        AttemptConnect();
    }
    
    /**
     * Disconnects from the configured IP address and port.
     * @throws IOException  If an error occurs while attempting to disconnect.
     * @throws Exception    If an error occurs while logging the disconnect operation.
     */
    public void Disconnect() throws IOException, Exception {
        if((socketChannel != null) && (socketChannel.isConnected() == true)) {
            socketChannel.close();
            
            /* //[BEGIN_LOGGING]
            logger.Log(this, LogLevel.Information, "Disconnected.");
            //[END_LOGGING] */
        }
    }

    @Override
    public void close() throws IOException, Exception {
        Disconnect();
    }
    
    @Override
    public void Send(String message) throws Exception {
        /* //[BEGIN_METRICS]
        metricLogger.Begin(new MessageSendTime());
        //[END_METRICS] */
        
        if (socketChannel.isConnected() == false) {
            throw new Exception("Connection to TCP socket has not been established.");
        }

        try {
            EncodeAndSend(message);
        }
        catch (Exception e) {
            HandleExceptionAndResend(e, message);
        }
        
        IncrementMessageSequenceNumber();
        
        /* //[BEGIN_METRICS]
        metricLogger.End(new MessageSendTime());
        metricLogger.Increment(new MessageSent());
        //[END_METRICS] */
        /* //[BEGIN_LOGGING]
        logger.Log(this, LogLevel.Information, "Message sent and acknowledged.");
        //[END_LOGGING] */
    }

    /**
     * Attempts to connect to the specified IP address and port, and retries for the specified number of times if the attempt is unsuccessful.
     */
    private void AttemptConnect() throws Exception {
        int connectAttempt = 0;

        while (connectAttempt <= connectRetryCount) {
            try {
                socketChannel.open();
                socketChannel.connect(new InetSocketAddress(ipAddress, port));
                logger.Log(this, LogLevel.Information, "Connected to " + ipAddress.toString() + ":" + port + ".");
                break;
            }
            catch (IOException ioException) {
                logger.Log(this, LogLevel.Error, ioException.getClass().getSimpleName() + " occurred whilst trying to connect to " + ipAddress.toString() + ":" + port + ".", ioException);
                if (connectRetryInterval > 0) {
                    Thread.sleep(connectRetryInterval);
                }
            }
            catch (Exception e) {
                throw new Exception("Error connecting to " + ipAddress.toString() + ":" + port +".", e);
            }
            
            connectAttempt = connectAttempt + 1;
        }
        
        if (socketChannel.isConnected() == false)
        {
            throw new Exception("Failed to connect to " + ipAddress.toString() + ":" + port + " after " + connectAttempt + " attempts.");
        }
    }
    
    /**
     * Adds delimiter characters and header information to the specified message and sends it.
     * @param message  The message to send.
     */
    private void EncodeAndSend(String message) throws Exception {
        byte[] messageByteArray = message.getBytes(stringEncodingCharset);
        
        // Create a 4 byte message sequence number, and encode as little endian
        ByteBuffer messageSequenceNumberByteBuffer = ByteBuffer.allocate(4);
        messageSequenceNumberByteBuffer.order(ByteOrder.LITTLE_ENDIAN);
        messageSequenceNumberByteBuffer.putInt(messageSequenceNumber);
        byte[] messageSequenceNumber = messageSequenceNumberByteBuffer.array();
        
        // Create 8 bytes containing the length of the message body, and encode as little endian
        ByteBuffer messageSizeHeaderByteBuffer = ByteBuffer.allocate(8);
        messageSizeHeaderByteBuffer.order(ByteOrder.LITTLE_ENDIAN);
        messageSizeHeaderByteBuffer.putLong((long)messageByteArray.length);
        byte[] messageSizeHeader = messageSizeHeaderByteBuffer.array();
        
        // Encode the message
        ByteBuffer encodedMessage = ByteBuffer.allocate(messageSequenceNumber.length + messageSizeHeader.length + messageByteArray.length + 2);
        encodedMessage.put(messageStartDelimiter);
        encodedMessage.put(messageSequenceNumber);
        encodedMessage.put(messageSizeHeader);
        encodedMessage.put(messageByteArray);
        encodedMessage.put(messageEndDelimiter);
        encodedMessage.flip();

        // Send the message
        socketChannel.write(encodedMessage);
        WaitForMessageAcknowledgement();
    }
    
    /**
     * Checks for a message acknowledgement on the underlying socket channel, and throws an exception if the acknowledgement is not received before the specified timeout period.
     */
    private void WaitForMessageAcknowledgement() throws Exception {
        // Set the socket channel to non-blocking mode
        socketChannel.configureBlocking(false);
        
        boolean acknowledgementReceived = false;
        long waitStartTime = System.currentTimeMillis();
        ByteBuffer acknowledgementBuffer = ByteBuffer.allocate(1);
        
        try {
            while((System.currentTimeMillis() - waitStartTime <= acknowledgementReceiveTimeout) && (acknowledgementReceived == false)) {
                int numBytesRead = socketChannel.read(acknowledgementBuffer);
                
                if(numBytesRead == 1) {
                    acknowledgementBuffer.flip();
                    byte byteRead = acknowledgementBuffer.get();
                    if(byteRead != messageAcknowledgementByte) {
                        throw new Exception("Acknowledgement byte was expected to be " + messageAcknowledgementByte + ", but was " + byteRead + ".");
                    }
                    else{
                        acknowledgementReceived = true;
                    }
                }
                
                if (acknowledgementReceiveRetryInterval > 0)
                {
                    Thread.sleep(acknowledgementReceiveRetryInterval);
                }
            }
        }
        catch (Exception e) {
            throw(e);
        }
        // Set the socket channel back to blocking regardless of whether the read of the acknowledgement is successful.
        finally {
            socketChannel.configureBlocking(true);
        }
        
        if (acknowledgementReceived == false)
        {
            throw new MessageAcknowledgementTimeoutException("Failed to receive message acknowledgement within timeout period of " + acknowledgementReceiveTimeout + " milliseconds.");
        }
    }
    
    /**
     * Handles an exception that occurred when attempting to send a message, before reconnecting and re-sending.
     * @param sendException  The exception that occurred when attempting to send the message.
     * @param message        The message to send.
     */
    private void HandleExceptionAndResend(Exception sendException, String message) throws Exception {
        if (sendException instanceof IOException) {
            logger.Log(this, LogLevel.Error, sendException.getClass().getSimpleName() + " occurred whilst attempting to send message.", sendException);
            logger.Log(this, LogLevel.Warning, "Disconnected from TCP socket.");
        }
        else if ((sendException instanceof java.nio.channels.ClosedChannelException) || (sendException instanceof MessageAcknowledgementTimeoutException)) {
            logger.Log(this, LogLevel.Error, sendException.getClass().getSimpleName() + " occurred whilst attempting to send message.", sendException);
        }
        else {
            throw new Exception("Error sending message.  Unhandled exception while sending message.", sendException);
        }

        logger.Log(this, LogLevel.Warning, "Attempting to reconnect to TCP socket.");

        socketChannel.close();
        AttemptConnect();
        /* //[BEGIN_METRICS]
        metricLogger.Increment(new TcpRemoteSenderReconnected());
        //[END_METRICS] */
        try
        {
            EncodeAndSend(message);
        }
        catch (Exception e)
        {
            throw new Exception("Error sending message.  Failed to send message after reconnecting.", e);
        }
    }

    /**
     * Increments the internal message sequence number.
     */
    private void IncrementMessageSequenceNumber() {
        if (messageSequenceNumber == Integer.MAX_VALUE) {
            messageSequenceNumber = 0;
        }
        else {
            messageSequenceNumber = messageSequenceNumber + 1;
        }
    }
}

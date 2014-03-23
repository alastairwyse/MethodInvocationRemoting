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

import javax.jms.*;
import net.alastairwyse.applicationlogging.*;

/**
 * Receives messages from a remote location via Apache ActiveMQ.
 * @author Alastair Wyse
 */
public class ActiveMqRemoteReceiver extends ActiveMqRemoteConnectionBase implements IRemoteReceiver {

    private MessageConsumer consumer;
    private int connectLoopTimeout;
    private volatile boolean cancelRequest = false;
    private volatile boolean waitingForMessage = false;
    private IApplicationLogger logger;
    private LoggingUtilities loggingUtilities;
    
    /**
     * Initializes a new instance of the ActiveMqRemoteReceiver class.
     * @param connectUriName      The uniform resource identifier of the ActiveMQ broker to connect to.
     * @param queueName           The name of the queue to connect to.
     * @param messageFilter       The filter to apply to the queue.  Allows multiple remote senders and receivers to use the same queue by each applying their own unique filter.
     * @param connectLoopTimeout  The time to wait for a message before retrying in milliseconds.
     */
    public ActiveMqRemoteReceiver(String connectUriName, String queueName, String messageFilter, int connectLoopTimeout) {
        super(connectUriName, queueName, messageFilter);
        this.connectLoopTimeout = connectLoopTimeout;
        logger = new ConsoleApplicationLogger(LogLevel.Information, '|', "  ");
        loggingUtilities = new LoggingUtilities(logger);
    }
    
    /**
     * Initializes a new instance of the ActiveMqRemoteReceiver class.
     * @param connectUriName      The uniform resource identifier of the ActiveMQ broker to connect to.
     * @param queueName           The name of the queue to connect to.
     * @param messageFilter       The filter to apply to the queue.  Allows multiple remote senders and receivers to use the same queue by each applying their own unique filter.
     * @param connectLoopTimeout  The time to wait for a message before retrying in milliseconds.
     * @param logger              The logger to write log events to.
     */
    public ActiveMqRemoteReceiver(String connectUriName, String queueName, String messageFilter, int connectLoopTimeout, IApplicationLogger logger) {
        this(connectUriName, queueName, messageFilter, connectLoopTimeout);
        this.logger = logger;
        loggingUtilities = new LoggingUtilities(logger);
    }
    
    /**
     * Initializes a new instance of the ActiveMqRemoteReceiver class.  
     * <b>Note</b> this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
     * @param connectUriName         The uniform resource identifier of the ActiveMQ broker to connect to.
     * @param queueName              The name of the queue to connect to.
     * @param messageFilter          The filter to apply to the queue.  Allows multiple remote senders and receivers to use the same queue by each applying their own unique filter.
     * @param connectLoopTimeout     The time to wait for a message before retrying in milliseconds.
     * @param logger                 The logger to write log events to.
     * @param testConnectionFactory  A test (mock) jms connection factory.
     * @param testConnection         A test (mock) jms connection.
     * @param testSession            A test (mock) jms session.
     * @param testDestination        A test (mock) jms destination.
     * @param testConsumer           A test (mock) jms message consumer.
     */
    public ActiveMqRemoteReceiver(String connectUriName, String queueName, String messageFilter, int connectLoopTimeout, IApplicationLogger logger, ConnectionFactory testConnectionFactory, Connection testConnection, Session testSession, Destination testDestination, MessageConsumer testConsumer) {
        super(connectUriName, queueName, messageFilter, testConnectionFactory, testConnection, testSession, testDestination);
        this.connectLoopTimeout = connectLoopTimeout;
        consumer = testConsumer;
        this.logger = logger;
        loggingUtilities = new LoggingUtilities(logger);
    }

    @Override
    public void Connect() throws Exception {
        super.Connect();
        try {
            if (testConstructor == false) {
                consumer = session.createConsumer(destination, filterIdentifier + " = '" + messageFilter + "'");
            }
        }
        catch (Exception e) {
            throw new Exception("Error creating message consumer.", e);
        }
        
        /* //[BEGIN_LOGGING]
        logger.Log(this, LogLevel.Information, "Connected to URI: '" + connectUriName + "', Queue: '" + queueName + "'.");
        //[END_LOGGING] */
    }

    @Override
    public void Disconnect() throws Exception {
        if(connected == true) {
            try {
                consumer.close();
            }
            catch (JMSException e) {
                throw new Exception("Error disconnecting from message queue.", e);
            }
            super.Disconnect();
            
            /* //[BEGIN_LOGGING]
            logger.Log(this, LogLevel.Information, "Disconnected.");
            //[END_LOGGING] */
        }
    }

    @Override
    public String Receive() throws Exception {
        String returnMessage = "";
        cancelRequest = false;
        
        CheckConnectionOpen();
        try {
            Message receivedMessage;
            while (cancelRequest == false)
            {
                waitingForMessage = true;
                receivedMessage = consumer.receive(connectLoopTimeout);
                waitingForMessage = false;
                if (receivedMessage != null)
                {
                    if(receivedMessage instanceof TextMessage) {
                        TextMessage receivedTextMessage = (TextMessage) receivedMessage;
                        returnMessage = receivedTextMessage.getText();
                        /* //[BEGIN_LOGGING]
                        loggingUtilities.LogMessageReceived(this, returnMessage);
                        //[END_LOGGING] */
                        break;
                    }
                    else {
                        throw new Exception("Received message was not of type javax.jms.TextMessage.");
                    }
                }
            }
        }
        catch (Exception e) {
            throw new Exception("Error receiving message.", e);
        }
        finally {
            waitingForMessage = false;
        }
        
        return returnMessage;
    }

    @Override
    public void CancelReceive() {
        cancelRequest = true;
        while (waitingForMessage == true);
        
        /* //[BEGIN_LOGGING]
        try {
            logger.Log(this, LogLevel.Information, "Receive operation cancelled.");
        }
        catch(Exception e) {
        }
        //[END_LOGGING] */
    }
}

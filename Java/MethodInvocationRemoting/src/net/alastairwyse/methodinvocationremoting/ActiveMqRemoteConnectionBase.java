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
import org.apache.activemq.ActiveMQConnectionFactory;

/**
 * Provides common connection functionality for classes connecting to an Apache ActiveMQ message broker.
 * @author Alastair Wyse
 */
public abstract class ActiveMqRemoteConnectionBase implements AutoCloseable {

    /** Used in the properties of a message to identify the message filter. */
    protected final String filterIdentifier = "Filter";

    /** Uniform resource identifier of the ActiveMQ broker to connect to. */
    protected String connectUriName;
    /** The name of the queue to connect to. */
    protected String queueName;
    /** The value of the message filter. */
    protected String messageFilter;
    /** The connection factory to use when connecting the ActiveMQ broker. */
    protected ConnectionFactory connectionFactory;
    /** The connection to use when connecting the ActiveMQ broker. */
    protected Connection connection;
    /** The session to use when connecting the ActiveMQ broker. */
    protected Session session;
    /** The destination to use when connecting the ActiveMQ broker. */
    protected Destination destination;
    /** Indicates whether the object is currently connected to the ActiveMQ broker. */
    protected boolean connected;
    /** Indicates that the object was instantiated using the test constructor. */
    protected boolean testConstructor;
    
    /**
     * @return        Indicates whether the object is currently connected to a remote queue.
     */
    public boolean getConnected() {
        return connected;
    }
    
    /**
     * Initializes a new instance of the ActiveMqRemoteConnectionBase class.
     * @param connectUriName  The uniform resource identifier of the ActiveMQ broker to connect to.
     * @param queueName       The name of the queue to connect to.
     * @param messageFilter   The filter to apply to the queue.  Allows multiple remote senders and receivers to use the same queue by each applying their own unique filter.
     */
    protected ActiveMqRemoteConnectionBase(String connectUriName, String queueName, String messageFilter) {
        connected = false;
        testConstructor = false;

        this.connectUriName = connectUriName;
        this.queueName = queueName;
        this.messageFilter = messageFilter;
    }
    
    /**
     * Initializes a new instance of the ActiveMqRemoteConnectionBase class.  
     * <b>Note</b> this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
     * @param connectUriName         The uniform resource identifier of the ActiveMQ broker to connect to.
     * @param queueName              The name of the queue to connect to.
     * @param messageFilter          The filter to apply to the queue.  Allows multiple remote senders and receivers to use the same queue by each applying their own unique filter.
     * @param testConnectionFactory  A test (mock) jms connection factory.
     * @param testConnection         A test (mock) jms connection.
     * @param testSession            A test (mock) jms session.
     * @param testDestination        A test (mock) jms destination.
     */
    protected ActiveMqRemoteConnectionBase(String connectUriName, String queueName, String messageFilter, ConnectionFactory testConnectionFactory, Connection testConnection, Session testSession, Destination testDestination) {
        this(connectUriName, queueName, messageFilter);
        testConstructor = true;
        connectionFactory = testConnectionFactory;
        connection = testConnection;
        session = testSession;
        destination = testDestination;
    }
    
    /**
     * Connects to the message queue.
     * @throws Exception  If a connection to the message queue is already open, or an error occurs when attempting to connect to the queue.
     */
    public void Connect() throws Exception {
        if(connected == true) {
            throw new Exception("Connection to message queue has already been opened.");
        }
        
        try {
            if(testConstructor == false) {
                connectionFactory = new ActiveMQConnectionFactory(connectUriName);
                connection = connectionFactory.createConnection();
                session = connection.createSession(false, Session.AUTO_ACKNOWLEDGE);
                destination = session.createQueue(queueName);
            }
            connection.start();
            connected = true;
        }
        catch (Exception e) {
            throw new Exception("Error connecting to message queue.", e);
        }
    }
    
    /**
     * Disconnects from the message queue.
     * @throws Exception  if an error occurs when attempting to disconnect from the queue.
     */
    public void Disconnect() throws Exception {
        if(connected == true) {
            try {
                session.close();
                connection.stop();
                connection.close();
            }
            catch (JMSException e) {
                throw new Exception("Error disconnecting from message queue.", e);
            }
            connected =  false;
        }
    }
    
    @Override
    public void close() throws Exception {
        Disconnect();
    }
    
    /**
     * Throws an exception if the object is currently not in a connected state.
     * @throws  Exception  if the object is currently not in a connected state.
     */
    protected void CheckConnectionOpen() throws Exception {
        if(connected == false) {
            throw new Exception("Connection to message queue is not open.");
        }
    }
}
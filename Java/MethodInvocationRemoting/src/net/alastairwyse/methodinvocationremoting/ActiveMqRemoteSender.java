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

/**
 * Sends messages to a remote location via Apache ActiveMQ.
 * @author Alastair Wyse
 */
public class ActiveMqRemoteSender extends ActiveMqRemoteConnectionBase implements IRemoteSender {

    private MessageProducer producer;
    
    /**
     * Initializes a new instance of the methodinvocationremoting.ActiveMqRemoteSender class.
     * @param connectUriName  The uniform resource identifier of the ActiveMQ broker to connect to.
     * @param queueName       The name of the queue to connect to.
     * @param messageFilter   The filter to apply to the queue.  Allows multiple remote senders and receivers to use the same queue by each applying their own unique filter.
     */
    public ActiveMqRemoteSender(String connectUriName, String queueName, String messageFilter) {
        super(connectUriName, queueName, messageFilter);
    }
    
    /**
     * Initializes a new instance of the methodinvocationremoting.ActiveMqRemoteSender class.  
     * <b>Note</b> this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
     * @param connectUriName         The uniform resource identifier of the ActiveMQ broker to connect to.
     * @param queueName              The name of the queue to connect to.
     * @param messageFilter          The filter to apply to the queue.  Allows multiple remote senders and receivers to use the same queue by each applying their own unique filter.
     * @param testConnectionFactory  A test (mock) jms connection factory.
     * @param testConnection         A test (mock) jms connection.
     * @param testSession            A test (mock) jms session.
     * @param testDestination        A test (mock) jms destination.
     * @param testProducer           A test (mock) jms message producer.
     */
    public ActiveMqRemoteSender(String connectUriName, String queueName, String messageFilter, ConnectionFactory testConnectionFactory, Connection testConnection, Session testSession, Destination testDestination, MessageProducer testProducer) {
        super(connectUriName, queueName, messageFilter, testConnectionFactory, testConnection, testSession, testDestination);
        producer = testProducer;
    }
    
    @Override
    public void Connect() throws Exception {
        super.Connect();
        try {
            if (testConstructor == false) {
                producer = session.createProducer(destination);
            }
        }
        catch (Exception e) {
            throw new Exception("Error creating message producer.", e);
        }
    }

    @Override
    public void Disconnect() throws Exception {
        if(connected == true) {
            try {
                producer.close();
            }
            catch (JMSException e) {
                throw new Exception("Error disconnecting from message queue.", e);
            }
            super.Disconnect();
        }
    }

    @Override
    public void Send(String message) throws Exception {
        CheckConnectionOpen();
        try {
            TextMessage textMessage = session.createTextMessage(message);
            textMessage.setStringProperty(filterIdentifier, messageFilter);
            producer.send(textMessage);
        }
        catch (Exception e) {
            throw new Exception("Error sending message.", e);
        }
    }
}

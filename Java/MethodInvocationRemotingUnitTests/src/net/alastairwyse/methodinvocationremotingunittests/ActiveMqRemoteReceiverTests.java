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

package net.alastairwyse.methodinvocationremotingunittests;

import static org.junit.Assert.*;

import org.junit.Before;
import org.junit.Test;
import javax.jms.*;

import static org.mockito.Mockito.*;
import net.alastairwyse.methodinvocationremoting.*;

/**
 * Unit tests for class methodinvocationremoting.ActiveMqRemoteReceiver.
 * @author Alastair Wyse
 */
public class ActiveMqRemoteReceiverTests {

    private ConnectionFactory mockConnectionFactory;
    private Connection mockConnection;
    private Session mockSession;
    private Destination mockDestination;
    private MessageConsumer mockConsumer;
    private ActiveMqRemoteReceiver testActiveMqRemoteReceiver;
    
    private final String connectUriName = "tcp://localhost:61616";
    private final String queueName = "TestQueueName";
    private final String messageFilter = "TestMessageFilter";
    
    @Before
    public void setUp() throws Exception {
        mockConnectionFactory = mock(ConnectionFactory.class);
        mockConnection = mock(Connection.class);
        mockSession = mock(Session.class);
        mockDestination = mock(Destination.class);
        mockConsumer = mock(MessageConsumer.class);
        testActiveMqRemoteReceiver = new ActiveMqRemoteReceiver(connectUriName, queueName, messageFilter, 1000, mockConnectionFactory, mockConnection, mockSession, mockDestination, mockConsumer);
    }
    
    @Test
    public void DisconnnectNotAttemptedWhenNotConnected() throws Exception {
        testActiveMqRemoteReceiver.Disconnect();
        
        verifyZeroInteractions(mockConsumer);
        assertEquals(false, testActiveMqRemoteReceiver.getConnected());
    }
    
    @Test
    public void DisconnectSuccessTest() throws JMSException {
        try {
            testActiveMqRemoteReceiver.Connect();
            testActiveMqRemoteReceiver.Disconnect();
        }
        catch (Exception e) {
            fail("Unexpected exception thrown.");
        }
        verify(mockConsumer).close();
        // Note other mock interactions are tested in base class tests 
        assertEquals(false, testActiveMqRemoteReceiver.getConnected());
    }
    
    @Test
    public void ReceiveConnectionClosed() {
        try {
            testActiveMqRemoteReceiver.Receive();
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            verifyZeroInteractions(mockConnectionFactory, mockConnection, mockSession, mockDestination, mockConsumer);
            assertTrue(e.getMessage().contains("Connection to message queue is not open."));
        }
    }
    
    @Test
    public void ReceiveException() throws Exception {
        when(mockConsumer.receive(1000)).thenThrow(new JMSException("Mock Receive Failure"));
        try {
            testActiveMqRemoteReceiver.Connect();
            testActiveMqRemoteReceiver.Receive();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockConnection).start();
            verify(mockConsumer).receive(1000);
            verifyNoMoreInteractions(mockConnection);
            verifyNoMoreInteractions(mockConsumer);
            verifyZeroInteractions(mockConnectionFactory, mockSession, mockDestination);
            assertTrue(e.getMessage().contains("Error receiving message."));
        }
    }
    
    @Test
    public void ReceiveNonTextMessage() throws Exception {
        ObjectMessage mockObjectMessage = mock(ObjectMessage.class);
        
        when(mockConsumer.receive(1000)).thenReturn(mockObjectMessage);
        try {
            testActiveMqRemoteReceiver.Connect();
            testActiveMqRemoteReceiver.Receive();
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            verify(mockConnection).start();
            verify(mockConsumer).receive(1000);
            verifyNoMoreInteractions(mockConnection);
            verifyNoMoreInteractions(mockConsumer);
            verifyZeroInteractions(mockConnectionFactory, mockSession, mockDestination);
            assertTrue(e.getCause().getMessage().contains("Received message was not of type javax.jms.TextMessage."));
        }
    }
    
    @Test
    public void ReceiveSuccessTest() throws Exception {
        String testMessage = "<TestMessage>Test message content</TestMessage>";
        String receivedMessage = "";
        TextMessage mockTextMessage = mock(TextMessage.class);
        
        when(mockConsumer.receive(1000)).thenReturn(mockTextMessage);
        doReturn(testMessage).when(mockTextMessage).getText();
        try {
            testActiveMqRemoteReceiver.Connect();
            receivedMessage = testActiveMqRemoteReceiver.Receive();
        }
        catch(Exception e) {
            fail("Unexpected exception thrown.");
        }
        verify(mockConnection).start();
        verify(mockConsumer).receive(1000);
        verify(mockTextMessage).getText();
        verifyNoMoreInteractions(mockConnection);
        verifyNoMoreInteractions(mockConsumer);
        verifyNoMoreInteractions(mockTextMessage);
        verifyZeroInteractions(mockConnectionFactory, mockSession, mockDestination);
        assertEquals(testMessage, receivedMessage);
    }
}

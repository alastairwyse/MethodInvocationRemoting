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
 * Unit tests for class methodinvocationremoting.ActiveMqRemoteSender.
 * @author Alastair Wyse
 */
public class ActiveMqRemoteSenderTests {

    private ConnectionFactory mockConnectionFactory;
    private Connection mockConnection;
    private Session mockSession;
    private Destination mockDestination;
    private MessageProducer mockProducer;
    private ActiveMqRemoteSender testActiveMqRemoteSender;
    
    private final String filterIdentifier = "Filter";
    private final String connectUriName = "tcp://localhost:61616";
    private final String queueName = "TestQueueName";
    private final String messageFilter = "TestMessageFilter";
    
    @Before
    public void setUp() throws Exception {
        mockConnectionFactory = mock(ConnectionFactory.class);
        mockConnection = mock(Connection.class);
        mockSession = mock(Session.class);
        mockDestination = mock(Destination.class);
        mockProducer = mock(MessageProducer.class);
        testActiveMqRemoteSender = new ActiveMqRemoteSender(connectUriName, queueName, messageFilter, mockConnectionFactory, mockConnection, mockSession, mockDestination, mockProducer);
    }
    
    @Test
    public void DisconnnectNotAttemptedWhenNotConnected() throws Exception {
        testActiveMqRemoteSender.Disconnect();
        
        verifyZeroInteractions(mockProducer);
        assertEquals(false, testActiveMqRemoteSender.getConnected());
    }
    
    @Test
    public void DisconnectSuccessTest() throws JMSException {
        try {
            testActiveMqRemoteSender.Connect();
            testActiveMqRemoteSender.Disconnect();
        }
        catch (Exception e) {
            fail("Unexpected exception thrown.");
        }
        verify(mockProducer).close();
        // Note other mock interactions are tested in base class tests 
        assertEquals(false, testActiveMqRemoteSender.getConnected());
    }
    
    @Test
    public void SendConnectionClosed() {
        try {
            testActiveMqRemoteSender.Send("<TestMessage>Test message content</TestMessage>");
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            verifyZeroInteractions(mockConnectionFactory, mockConnection, mockSession, mockDestination, mockProducer);
            assertTrue(e.getMessage().contains("Connection to message queue is not open."));
        }
    }
    
    @Test
    public void SendException() throws JMSException {
        final String testMessage = "<TestMessage>Test message content</TestMessage>";
        TextMessage mockTextMessage = mock(TextMessage.class);
        
        when(mockSession.createTextMessage(testMessage)).thenReturn(mockTextMessage);
        doThrow(new JMSException("Mock Send Failure")).when(mockProducer).send(mockTextMessage);
        try {
            testActiveMqRemoteSender.Connect();
            testActiveMqRemoteSender.Send(testMessage);
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            verify(mockConnection).start();
            verify(mockSession).createTextMessage(testMessage);
            verify(mockTextMessage).setStringProperty(filterIdentifier, messageFilter);
            verifyNoMoreInteractions(mockSession);
            verifyNoMoreInteractions(mockTextMessage);
            verifyZeroInteractions(mockConnectionFactory, mockDestination);
            assertTrue(e.getMessage().contains("Error sending message."));
        }
    }

    @Test
    public void SendSuccessTest() throws Exception {
        final String testMessage = "<TestMessage>Test message content</TestMessage>";
        TextMessage mockTextMessage = mock(TextMessage.class);
        
        when(mockSession.createTextMessage(testMessage)).thenReturn(mockTextMessage);
        try {
            testActiveMqRemoteSender.Connect();
            testActiveMqRemoteSender.Send(testMessage);
        }
        catch(Exception e) {
            fail("Unexpected exception thrown.");
        }
        verify(mockConnection).start();
        verify(mockSession).createTextMessage(testMessage);
        verify(mockTextMessage).setStringProperty(filterIdentifier, messageFilter);
        verify(mockProducer).send(mockTextMessage);
        verifyNoMoreInteractions(mockSession);
        verifyNoMoreInteractions(mockTextMessage);
        verifyNoMoreInteractions(mockProducer);
        verifyZeroInteractions(mockConnectionFactory, mockDestination);
    }
}

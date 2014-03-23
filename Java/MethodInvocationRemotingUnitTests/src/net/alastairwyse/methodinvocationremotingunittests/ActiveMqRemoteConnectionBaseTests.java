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
import net.alastairwyse.applicationlogging.*;

/**
 * Unit tests for class methodinvocationremoting.ActiveMqRemoteConnectionBase.
 * As ActiveMqRemoteConnectionBase is an abstract base class, functionality will be tested through derived class ActiveMqRemoteSender.
 * @author Alastair Wyse
 */
public class ActiveMqRemoteConnectionBaseTests {

    private ConnectionFactory mockConnectionFactory;
    private Connection mockConnection;
    private Session mockSession;
    private Destination mockDestination;
    private MessageProducer mockProducer;
    private ActiveMqRemoteSender testActiveMqRemoteSender;
    
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
        testActiveMqRemoteSender = new ActiveMqRemoteSender(connectUriName, queueName, messageFilter, new ConsoleApplicationLogger(LogLevel.Warning, '|', "  "), mockConnectionFactory, mockConnection, mockSession, mockDestination, mockProducer);
    }
    
    @Test 
    public void ConnectException() throws JMSException {
        doThrow(new JMSException("Mock Connection Failure")).when(mockConnection).start();
        try  {
            testActiveMqRemoteSender.Connect();
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            CheckNoAdditionalMockInteractions();
            assertTrue(e.getMessage().contains("Error connecting to message queue."));
        }
    }
    
    @Test
    public void ConnectWhenAlreadyConnected() throws JMSException {
        try  {
            testActiveMqRemoteSender.Connect();
            testActiveMqRemoteSender.Connect();
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            verify(mockConnection).start();
            verifyNoMoreInteractions(mockConnection);
            CheckNoAdditionalMockInteractions();
            assertTrue(e.getMessage().contains("Connection to message queue has already been opened."));
        }
    }
    
    @Test
    public void ConnectSuccessTest() throws Exception {
        assertEquals(false, testActiveMqRemoteSender.getConnected());

        testActiveMqRemoteSender.Connect();

        verify(mockConnection).start();
        verifyNoMoreInteractions(mockConnection);
        CheckNoAdditionalMockInteractions();
        assertEquals(true, testActiveMqRemoteSender.getConnected());
    }
    
    @Test
    public void DisconnnectNotAttemptedWhenNotConnected() throws Exception {
        testActiveMqRemoteSender.Disconnect();
        
        verifyZeroInteractions(mockConnection);
        verifyZeroInteractions(mockSession);
        assertEquals(false, testActiveMqRemoteSender.getConnected());
    }
    
    @Test
    public void DisconnnectSuccessTest() throws Exception {
        testActiveMqRemoteSender.Connect();
        testActiveMqRemoteSender.Disconnect();

        verify(mockConnection).start();
        verify(mockConnection).stop();
        verify(mockConnection).close();
        verify(mockSession).close();
        verifyNoMoreInteractions(mockConnection);
        verifyNoMoreInteractions(mockSession);
        CheckNoAdditionalMockInteractions();
        assertEquals(false, testActiveMqRemoteSender.getConnected());
    }
    
    /**
     * Checks that there were no interactions with mocks mockConnectionFactory, mockSession, or mockDestination.
     */
    private void CheckNoAdditionalMockInteractions() {
        verifyZeroInteractions(mockConnectionFactory, mockSession, mockDestination);
    }
}

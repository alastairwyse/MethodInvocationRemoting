/*
 * Copyright 2014 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
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

package net.alastairwyse.methodinvocationremotingloggingtests;

import org.junit.Before;
import org.junit.Test;
import javax.jms.*;
import static org.mockito.Mockito.*;
import net.alastairwyse.methodinvocationremoting.*;
import net.alastairwyse.applicationlogging.*;

/**
 * Unit tests for the logging functionality in class methodinvocationremoting.ActiveMqRemoteSender.
 * @author Alastair Wyse
 */
public class ActiveMqRemoteSenderLoggingTests {

    private ConnectionFactory mockConnectionFactory;
    private Connection mockConnection;
    private Session mockSession;
    private Destination mockDestination;
    private MessageProducer mockProducer;
    private IApplicationLogger mockApplicationLogger;
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
        mockApplicationLogger = mock(IApplicationLogger.class);
        testActiveMqRemoteSender = new ActiveMqRemoteSender(connectUriName, queueName, messageFilter, mockApplicationLogger, mockConnectionFactory, mockConnection, mockSession, mockDestination, mockProducer);
    }
    
    @Test
    public void ConnectLoggingTest() throws Exception {
        testActiveMqRemoteSender.Connect();
        
        verify(mockApplicationLogger).Log(testActiveMqRemoteSender, LogLevel.Information, "Connected to URI: '" + connectUriName + "', Queue: '" + queueName + "'.");
        verifyNoMoreInteractions(mockApplicationLogger);
    }
    
    @Test
    public void DisconnectLoggingTest() throws Exception {
        testActiveMqRemoteSender.Connect();
        testActiveMqRemoteSender.Disconnect();
        
        verify(mockApplicationLogger).Log(testActiveMqRemoteSender, LogLevel.Information, "Disconnected.");
    }
    
    @Test
    public void SendLoggingTest() throws Exception {
        final String testMessage = "<TestMessage>Test message content</TestMessage>";
        TextMessage mockTextMessage = mock(TextMessage.class);
        
        when(mockSession.createTextMessage(testMessage)).thenReturn(mockTextMessage);
        
        testActiveMqRemoteSender.Connect();
        testActiveMqRemoteSender.Send(testMessage);
        
        verify(mockApplicationLogger).Log(testActiveMqRemoteSender, LogLevel.Information, "Message sent.");
    }
}

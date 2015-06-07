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

package net.alastairwyse.methodinvocationremotingmetricstests;

import org.junit.Before;
import org.junit.Test;

import javax.jms.*;

import static org.junit.Assert.fail;
import static org.mockito.Mockito.*;
import net.alastairwyse.applicationlogging.*;
import net.alastairwyse.applicationmetrics.*;
import net.alastairwyse.methodinvocationremoting.*;
import net.alastairwyse.methodinvocationremotingmetrics.*;

/**
 * Unit tests for the metrics logging functionality in class methodinvocationremoting.ActiveMqRemoteSender.
 * @author Alastair Wyse
 */
public class ActiveMqRemoteSenderMetricsTests {

    private ConnectionFactory mockConnectionFactory;
    private Connection mockConnection;
    private Session mockSession;
    private Destination mockDestination;
    private MessageProducer mockProducer;
    private IMetricLogger mockMetricLogger;
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
        mockMetricLogger = mock(IMetricLogger.class);
        testActiveMqRemoteSender = new ActiveMqRemoteSender(connectUriName, queueName, messageFilter, new ConsoleApplicationLogger(LogLevel.Critical, '|', "  "), mockMetricLogger, mockConnectionFactory, mockConnection, mockSession, mockDestination, mockProducer);
    }
    
    @Test
    public void SendMetricsTest() throws Exception {
        final String testMessage = "<TestMessage>Test message content</TestMessage>";
        TextMessage mockTextMessage = mock(TextMessage.class);
        
        when(mockSession.createTextMessage(testMessage)).thenReturn(mockTextMessage);
        
        testActiveMqRemoteSender.Connect();
        testActiveMqRemoteSender.Send(testMessage);
        
        verify(mockMetricLogger).Begin(isA(MessageSendTime.class));
        verify(mockMetricLogger).End(isA(MessageSendTime.class));
        verify(mockMetricLogger).Increment(isA(MessageSent.class));
        verifyNoMoreInteractions(mockMetricLogger);
    }
    
    @Test
    public void SendExceptionMetricsTest() throws Exception {
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
            verify(mockMetricLogger).Begin(isA(MessageSendTime.class));
            verify(mockMetricLogger).CancelBegin(isA(MessageSendTime.class));
            verifyNoMoreInteractions(mockMetricLogger);
        }
    }
}
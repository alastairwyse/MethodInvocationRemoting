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
import net.alastairwyse.applicationlogging.*;
import net.alastairwyse.applicationmetrics.*;
import net.alastairwyse.methodinvocationremoting.*;

/**
 * Unit tests for the logging functionality in class methodinvocationremoting.ActiveMqRemoteReceiver.
 * @author Alastair Wyse
 */
public class ActiveMqRemoteReceiverLoggingTests {

    private ConnectionFactory mockConnectionFactory;
    private Connection mockConnection;
    private Session mockSession;
    private Destination mockDestination;
    private MessageConsumer mockConsumer;
    private IApplicationLogger mockApplicationLogger;
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
        mockApplicationLogger = mock(IApplicationLogger.class);
        testActiveMqRemoteReceiver = new ActiveMqRemoteReceiver(connectUriName, queueName, messageFilter, 1000, mockApplicationLogger, new NullMetricLogger(), mockConnectionFactory, mockConnection, mockSession, mockDestination, mockConsumer);
    }
    
    @Test
    public void ConnectLoggingTest() throws Exception {
        testActiveMqRemoteReceiver.Connect();
        
        verify(mockApplicationLogger).Log(testActiveMqRemoteReceiver, LogLevel.Information, "Connected to URI: '" + connectUriName + "', Queue: '" + queueName + "'.");
        verifyNoMoreInteractions(mockApplicationLogger);
    }
    
    @Test
    public void DisconnectLoggingTest() throws Exception {
        testActiveMqRemoteReceiver.Connect();
        testActiveMqRemoteReceiver.Disconnect();
        
        verify(mockApplicationLogger).Log(testActiveMqRemoteReceiver, LogLevel.Information, "Disconnected.");
    }
    
    @Test
    public void ReceiveLoggingTest() throws Exception {
        String receivedMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><Data>abc</Data></Parameter><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter /><Parameter><DataType>double</DataType><Data>4.5678899999999999e+002</Data></Parameter></Parameters><ReturnType /></MethodInvocation>";
        String smallMessage = "<TestMessage>Test message content</TestMessage>";
        TextMessage mockTextMessage = mock(TextMessage.class);
        
        when(mockConsumer.receive(1000)).thenReturn(mockTextMessage);
        when(mockTextMessage.getText())
            .thenReturn(receivedMessage)
            .thenReturn(smallMessage);
        
        testActiveMqRemoteReceiver.Connect();
        testActiveMqRemoteReceiver.Receive();
        testActiveMqRemoteReceiver.Receive();
        
        verify(mockApplicationLogger).Log(testActiveMqRemoteReceiver, LogLevel.Information, "Received message '<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataT' (truncated).");
        verify(mockApplicationLogger).Log(testActiveMqRemoteReceiver, LogLevel.Debug, "Complete message content: '" + receivedMessage + "'.");
        verify(mockApplicationLogger).Log(testActiveMqRemoteReceiver, LogLevel.Information, "Received message '" + smallMessage + "'.");
        verify(mockApplicationLogger).Log(testActiveMqRemoteReceiver, LogLevel.Debug, "Complete message content: '" + smallMessage + "'.");
    }
    
    @Test
    public void CancelReceiveLoggingTests() throws Exception {
        testActiveMqRemoteReceiver.CancelReceive();
        
        verify(mockApplicationLogger).Log(testActiveMqRemoteReceiver, LogLevel.Information, "Receive operation cancelled.");
        verifyNoMoreInteractions(mockApplicationLogger);
    }
}

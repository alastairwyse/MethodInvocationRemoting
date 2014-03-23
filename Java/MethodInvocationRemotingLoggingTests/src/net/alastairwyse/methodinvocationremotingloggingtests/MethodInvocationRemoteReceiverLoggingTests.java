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
import static org.mockito.Mockito.*;
import net.alastairwyse.methodinvocationremoting.*;
import net.alastairwyse.applicationlogging.*;

/**
 * Unit tests for the logging functionality in class methodinvocationremoting.MethodInvocationRemoteReceiver.
 * @author Alastair Wyse
 */
public class MethodInvocationRemoteReceiverLoggingTests {

    private IMethodInvocationSerializer mockMethodInvocationSerializer;
    private IRemoteSender mockRemoteSender;
    private IRemoteReceiver mockRemoteReceiver;
    private IApplicationLogger mockApplicationLogger;
    private IMethodInvocationReceivedEventHandler mockMethodInvocationReceivedEventHandler;
    private MethodInvocationRemoteReceiver testMethodInvocationRemoteReceiver;
    private String testSerializedReturnValue;
    private String testVoidReturnValue;
    private String testSerializedMethodInvocation;
    
    @Before
    public void setUp() throws Exception {
        mockMethodInvocationSerializer = mock(IMethodInvocationSerializer.class);
        mockRemoteSender = mock(IRemoteSender.class);
        mockRemoteReceiver = mock(IRemoteReceiver.class);
        mockApplicationLogger = mock(IApplicationLogger.class);
        mockMethodInvocationReceivedEventHandler = mock(IMethodInvocationReceivedEventHandler.class);
        testMethodInvocationRemoteReceiver = new MethodInvocationRemoteReceiver(mockMethodInvocationSerializer, mockRemoteSender, mockRemoteReceiver, mockApplicationLogger);
        testMethodInvocationRemoteReceiver.setReceivedEventHandler(mockMethodInvocationReceivedEventHandler);
        testSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><string>TestReturnValue</string></ReturnValue>";
        testVoidReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnType>void</ReturnType>";
        testSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><string>ABC</string></Parameter><Parameter><DataType>integer</DataType><int>12345</int></Parameter><Parameter><DataType>boolean</DataType><boolean>true</boolean></Parameter></Parameters><ReturnType>string</ReturnType></MethodInvocation>";
    }
    
    @Test
    public void ReceiveLoggingTest() throws Exception {
        when(mockRemoteReceiver.Receive())
            .thenReturn(testSerializedMethodInvocation)
            .thenReturn("");
        when(mockMethodInvocationSerializer.Deserialize(testSerializedMethodInvocation)).thenReturn(new MethodInvocation("TestMethod"));

        testMethodInvocationRemoteReceiver.Receive();
        // Need to pause so that receiveLoopThread has time to iterate before CancelReceive() is sent.
        //   Unfortunately this is still not a deterministic way to test, but best that can be done given that the Receive() spawns off a new thread.
        Thread.sleep(50);
        testMethodInvocationRemoteReceiver.CancelReceive();

        verify(mockApplicationLogger).Log(testMethodInvocationRemoteReceiver, LogLevel.Information, "Received method invocation 'TestMethod'.");
        verify(mockApplicationLogger).Log(testMethodInvocationRemoteReceiver, LogLevel.Information, "Receive operation cancelled.");
        verifyNoMoreInteractions(mockApplicationLogger);
    }
    
    @Test
    public void SendReturnValueLoggingTest() throws Exception {
        String testReturnValue = "TestReturnValue";
        
        when(mockMethodInvocationSerializer.SerializeReturnValue(testReturnValue)).thenReturn(testSerializedReturnValue);
        
        testMethodInvocationRemoteReceiver.SendReturnValue(testReturnValue);
        
        verify(mockApplicationLogger).Log(testMethodInvocationRemoteReceiver, LogLevel.Information, "Sent return value.");
        verifyNoMoreInteractions(mockApplicationLogger);
    }
    
    @Test
    public void SendVoidReturnValueLoggingTest() throws Exception {
        when(mockMethodInvocationSerializer.getVoidReturnValue()).thenReturn(testVoidReturnValue);
        
        testMethodInvocationRemoteReceiver.SendVoidReturn();
        
        verify(mockApplicationLogger).Log(testMethodInvocationRemoteReceiver, LogLevel.Information, "Sent void return value.");
        verifyNoMoreInteractions(mockApplicationLogger);
    }
    
    @Test
    public void CancelReceiveLoggingTest() throws Exception {
        when(mockRemoteReceiver.Receive()).thenReturn("");
        
        testMethodInvocationRemoteReceiver.Receive();
        testMethodInvocationRemoteReceiver.CancelReceive();
        
        verify(mockApplicationLogger).Log(testMethodInvocationRemoteReceiver, LogLevel.Information, "Receive operation cancelled.");
        verifyNoMoreInteractions(mockApplicationLogger);
    }
}

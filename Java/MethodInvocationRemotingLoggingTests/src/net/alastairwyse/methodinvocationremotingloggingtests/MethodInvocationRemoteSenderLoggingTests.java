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
 * Unit tests for the logging functionality in class methodinvocationremoting.MethodInvocationRemoteSender.
 * @author Alastair Wyse
 */
public class MethodInvocationRemoteSenderLoggingTests {
    
    private IMethodInvocationSerializer mockMethodInvocationSerializer;
    private IRemoteSender mockRemoteSender;
    private IRemoteReceiver mockRemoteReceiver;
    private IApplicationLogger mockApplicationLogger;
    private MethodInvocationRemoteSender testMethodInvocationRemoteSender;
    private IMethodInvocation testMethodInvocation;
    private String testSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><string>ABC</string></Parameter><Parameter><DataType>integer</DataType><int>12345</int></Parameter><Parameter><DataType>boolean</DataType><boolean>true</boolean></Parameter></Parameters><ReturnType>string</ReturnType></MethodInvocation>";
    private String testSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><DataType>string</DataType><string>Return Data</string>";
    private String testVoidSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><void/>";
    
    @Before
    public void setUp() throws Exception {
        mockMethodInvocationSerializer = mock(IMethodInvocationSerializer.class);
        mockRemoteSender = mock(IRemoteSender.class);
        mockRemoteReceiver = mock(IRemoteReceiver.class);
        mockApplicationLogger = mock(IApplicationLogger.class);
        testMethodInvocationRemoteSender = new MethodInvocationRemoteSender(mockMethodInvocationSerializer, mockRemoteSender, mockRemoteReceiver, mockApplicationLogger);
        testMethodInvocation = new MethodInvocation("TestMethod", String.class);
    }
    
    @Test
    public void InvokeMethodLoggingTest() throws Exception {
        when(mockMethodInvocationSerializer.Serialize(testMethodInvocation)).thenReturn(testSerializedMethodInvocation);
        when(mockRemoteReceiver.Receive()).thenReturn(testSerializedReturnValue);
        when(mockMethodInvocationSerializer.DeserializeReturnValue(testSerializedReturnValue)).thenReturn("Return Data");
        
        testMethodInvocationRemoteSender.InvokeMethod(testMethodInvocation);
        
        verify(mockApplicationLogger).Log(testMethodInvocationRemoteSender, LogLevel.Information, "Invoked method 'TestMethod'.");
        verifyNoMoreInteractions(mockApplicationLogger);
    }
    
    @Test
    public void InvokeVoidMethodLoggingTest() throws Exception {
        testMethodInvocation = new MethodInvocation("TestMethod");
        
        when(mockMethodInvocationSerializer.Serialize(testMethodInvocation)).thenReturn(testSerializedMethodInvocation);
        when(mockRemoteReceiver.Receive()).thenReturn(testVoidSerializedReturnValue);
        when(mockMethodInvocationSerializer.getVoidReturnValue()).thenReturn(testVoidSerializedReturnValue);
        
        testMethodInvocationRemoteSender.InvokeVoidMethod(testMethodInvocation);
        
        verify(mockApplicationLogger).Log(testMethodInvocationRemoteSender, LogLevel.Information, "Invoked void method 'TestMethod'.");
        verifyNoMoreInteractions(mockApplicationLogger);
    }
}

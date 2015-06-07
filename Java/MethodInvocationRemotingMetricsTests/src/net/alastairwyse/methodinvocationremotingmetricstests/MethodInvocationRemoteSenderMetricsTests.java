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

import static org.junit.Assert.fail;
import static org.mockito.Mockito.*;
import net.alastairwyse.applicationmetrics.*;
import net.alastairwyse.methodinvocationremoting.*;
import net.alastairwyse.methodinvocationremotingmetrics.*;

/**
 * Unit tests for the metrics logging functionality in class methodinvocationremoting.MethodInvocationRemoteSender.
 * @author Alastair Wyse
 */
public class MethodInvocationRemoteSenderMetricsTests {

    private IMethodInvocationSerializer mockMethodInvocationSerializer;
    private IRemoteSender mockRemoteSender;
    private IRemoteReceiver mockRemoteReceiver;
    private IMetricLogger mockMetricLogger;
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
        mockMetricLogger = mock(IMetricLogger.class);
        testMethodInvocationRemoteSender = new MethodInvocationRemoteSender(mockMethodInvocationSerializer, mockRemoteSender, mockRemoteReceiver, mockMetricLogger);
        testMethodInvocation = new MethodInvocation("TestMethod", String.class);
    }
    
    @Test
    public void InvokeMethodMetricsTest() throws Exception {
        when(mockMethodInvocationSerializer.Serialize(testMethodInvocation)).thenReturn(testSerializedMethodInvocation);
        when(mockRemoteReceiver.Receive()).thenReturn(testSerializedReturnValue);
        when(mockMethodInvocationSerializer.DeserializeReturnValue(testSerializedReturnValue)).thenReturn("Return Data");
        
        testMethodInvocationRemoteSender.InvokeMethod(testMethodInvocation);
        
        verify(mockMetricLogger).Begin(isA(RemoteMethodSendTime.class));
        verify(mockMetricLogger).End(isA(RemoteMethodSendTime.class));
        verify(mockMetricLogger).Increment(isA(RemoteMethodSent.class));
        verifyNoMoreInteractions(mockMetricLogger);
    }
    
    @Test
    public void InvokeMethodVoidMethodInvocationExceptionMetricsTest() throws Exception {
        try {
            testMethodInvocationRemoteSender.InvokeMethod(new MethodInvocation("TestMethod"));
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            verify(mockMetricLogger).Begin(isA(RemoteMethodSendTime.class));
            verify(mockMetricLogger).CancelBegin(isA(RemoteMethodSendTime.class));
            verifyNoMoreInteractions(mockMetricLogger);
        }
    }
    
    @Test
    public void InvokeMethodSerializationExceptionMetricsTest() throws Exception {
        doThrow(new SerializationException("Mock Serialization Failure")).when(mockMethodInvocationSerializer).Serialize(testMethodInvocation);
        try {
            testMethodInvocationRemoteSender.InvokeMethod(testMethodInvocation);
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            verify(mockMetricLogger).Begin(isA(RemoteMethodSendTime.class));
            verify(mockMetricLogger).CancelBegin(isA(RemoteMethodSendTime.class));
            verifyNoMoreInteractions(mockMetricLogger);
        }
    }
    
    @Test
    public void InvokeMethodReturnValueDeserializationExceptionMetricsTest() throws Exception {
        when(mockMethodInvocationSerializer.Serialize(testMethodInvocation)).thenReturn(testSerializedMethodInvocation);
        when(mockRemoteReceiver.Receive()).thenReturn(testSerializedReturnValue);
        doThrow(new DeserializationException("Mock Deerialization Failure")).when(mockMethodInvocationSerializer).DeserializeReturnValue(testSerializedReturnValue);
        try {
            testMethodInvocationRemoteSender.InvokeMethod(testMethodInvocation);
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            verify(mockMetricLogger).Begin(isA(RemoteMethodSendTime.class));
            verify(mockMetricLogger).CancelBegin(isA(RemoteMethodSendTime.class));
            verifyNoMoreInteractions(mockMetricLogger);
        }
    }
    
    @Test
    public void InvokeVoidMethodMetricsTest() throws Exception {
        testMethodInvocation = new MethodInvocation("TestMethod");
        
        when(mockMethodInvocationSerializer.Serialize(testMethodInvocation)).thenReturn(testSerializedMethodInvocation);
        when(mockRemoteReceiver.Receive()).thenReturn(testVoidSerializedReturnValue);
        when(mockMethodInvocationSerializer.getVoidReturnValue()).thenReturn(testVoidSerializedReturnValue);
        
        testMethodInvocationRemoteSender.InvokeVoidMethod(testMethodInvocation);
        
        verify(mockMetricLogger).Begin(isA(RemoteMethodSendTime.class));
        verify(mockMetricLogger).End(isA(RemoteMethodSendTime.class));
        verify(mockMetricLogger).Increment(isA(RemoteMethodSent.class));
        verifyNoMoreInteractions(mockMetricLogger);
    }
    
    @Test
    public void InvokeVoidMethodNonVoidMethodInvocationExceptionMetricsTest() throws Exception {
        try {
            testMethodInvocationRemoteSender.InvokeVoidMethod(testMethodInvocation);
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            verify(mockMetricLogger).Begin(isA(RemoteMethodSendTime.class));
            verify(mockMetricLogger).CancelBegin(isA(RemoteMethodSendTime.class));
            verifyNoMoreInteractions(mockMetricLogger);
        }
    }
    
    @Test
    public void InvokeVoidMethodSerializationExceptionMetricsTest() throws Exception {
        testMethodInvocation = new MethodInvocation("TestMethod");
        
        doThrow(new SerializationException("Mock Serialization Failure")).when(mockMethodInvocationSerializer).Serialize(testMethodInvocation);
        try {
            testMethodInvocationRemoteSender.InvokeVoidMethod(testMethodInvocation);
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            verify(mockMetricLogger).Begin(isA(RemoteMethodSendTime.class));
            verify(mockMetricLogger).CancelBegin(isA(RemoteMethodSendTime.class));
            verifyNoMoreInteractions(mockMetricLogger);
        }
    }
    
    @Test
    public void InvokeVoidMethodNonVoidReturnValueExceptionMetricsTest() throws Exception {
        testMethodInvocation = new MethodInvocation("TestMethod");
        
        when(mockMethodInvocationSerializer.Serialize(testMethodInvocation)).thenReturn(testSerializedMethodInvocation);
        when(mockRemoteReceiver.Receive()).thenReturn(testVoidSerializedReturnValue);
        when(mockMethodInvocationSerializer.getVoidReturnValue()).thenReturn(testSerializedReturnValue);
        
        try {
            testMethodInvocationRemoteSender.InvokeVoidMethod(testMethodInvocation);
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            verify(mockMetricLogger).Begin(isA(RemoteMethodSendTime.class));
            verify(mockMetricLogger).CancelBegin(isA(RemoteMethodSendTime.class));
            verifyNoMoreInteractions(mockMetricLogger);
        }
    }
}

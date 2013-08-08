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
import static org.mockito.Mockito.*;
import net.alastairwyse.methodinvocationremoting.*;

/**
 * Unit tests for class methodinvocationremoting.MethodInvocationRemoteSender.
 * @author Alastair Wyse
 */
public class MethodInvocationRemoteSenderTests {
    
    private IMethodInvocationSerializer mockMethodInvocationSerializer;
    private IRemoteSender mockRemoteSender;
    private IRemoteReceiver mockRemoteReceiver;
    private IMethodInvocationRemoteSender testMethodInvocationRemoteSender;
    private IMethodInvocation testMethodInvocation;
    private String testSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><string>ABC</string></Parameter><Parameter><DataType>integer</DataType><int>12345</int></Parameter><Parameter><DataType>boolean</DataType><boolean>true</boolean></Parameter></Parameters><ReturnType>string</ReturnType></MethodInvocation>";
    private String testSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><DataType>string</DataType><string>Return Data</string>";
    private String testReturnValue = "Return Data";
    private IMethodInvocation testVoidMethodInvocation;
    private String testVoidSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><string>ABC</string></Parameter><Parameter><DataType>integer</DataType><int>12345</int></Parameter><Parameter><DataType>boolean</DataType><boolean>true</boolean></Parameter></Parameters><ReturnType/></MethodInvocation>";
    private String testVoidSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><void/>";
    
    @Before
    public void setUp() throws Exception {
        mockMethodInvocationSerializer = mock(IMethodInvocationSerializer.class);
        mockRemoteSender = mock(IRemoteSender.class);
        mockRemoteReceiver = mock(IRemoteReceiver.class);
        testMethodInvocationRemoteSender = new MethodInvocationRemoteSender(mockMethodInvocationSerializer, mockRemoteSender, mockRemoteReceiver);
        Object[] testMethodInvocationParameters = new Object[3];
        testMethodInvocationParameters[0] = "ABC";
        testMethodInvocationParameters[1] = 12345;
        testMethodInvocationParameters[2] = true;
        testMethodInvocation = new MethodInvocation("TestMethod", testMethodInvocationParameters, String.class);
        testVoidMethodInvocation = new MethodInvocation("TestMethod", testMethodInvocationParameters);
    }
    
    @Test
    public void InvokeMethodVoidMethodInvocation() {
        try {
            testMethodInvocationRemoteSender.InvokeMethod(testVoidMethodInvocation);
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            assertTrue(e instanceof IllegalArgumentException);
            assertTrue(e.getMessage().contains("Method invocation cannot have a void return type."));
        }
    }
    
    @Test
    public void InvokeMethodException() throws SerializationException, Exception {
        when(mockMethodInvocationSerializer.Serialize(testMethodInvocation)).thenReturn(testSerializedMethodInvocation);
        doThrow(new Exception("Mock Receive Failure")).when(mockRemoteReceiver).Receive();
        try {
            testMethodInvocationRemoteSender.InvokeMethod(testMethodInvocation);
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            verify(mockMethodInvocationSerializer).Serialize(testMethodInvocation);
            verify(mockRemoteSender).Send(testSerializedMethodInvocation);
            verifyNoMoreInteractions(mockMethodInvocationSerializer);
            verifyNoMoreInteractions(mockRemoteSender);
            assertTrue(e.getMessage().contains("Failed to invoke method."));
        }
    }
    
    @Test
    public void InvokeMethodReturnValueDeserializationException() throws DeserializationException, Exception {
        when(mockMethodInvocationSerializer.Serialize(testMethodInvocation)).thenReturn(testSerializedMethodInvocation);
        doReturn(testSerializedReturnValue).when(mockRemoteReceiver).Receive();
        when(mockMethodInvocationSerializer.DeserializeReturnValue(testSerializedReturnValue)).thenThrow(new DeserializationException("Mock Deserialize Failure."));
        try {
            testMethodInvocationRemoteSender.InvokeMethod(testMethodInvocation);
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            verify(mockMethodInvocationSerializer).Serialize(testMethodInvocation);
            verify(mockRemoteSender).Send(testSerializedMethodInvocation);
            verify(mockRemoteReceiver).Receive();
            verifyNoMoreInteractions(mockRemoteSender);
            verifyNoMoreInteractions(mockRemoteReceiver);
            assertTrue(e.getMessage().contains("Failed to deserialize return value."));
        }
    }
    
    @Test
    public void InvokeMethodSuccessTests() throws Exception {
        String returnValue = "";
        
        when(mockMethodInvocationSerializer.Serialize(testMethodInvocation)).thenReturn(testSerializedMethodInvocation);
        doReturn(testSerializedReturnValue).when(mockRemoteReceiver).Receive();
        when(mockMethodInvocationSerializer.DeserializeReturnValue(testSerializedReturnValue)).thenReturn(testReturnValue);
        try {
            returnValue = (String)testMethodInvocationRemoteSender.InvokeMethod(testMethodInvocation);
        }
        catch(Exception e) {
            fail("Unexpected exception thrown.");
        }
        verify(mockMethodInvocationSerializer).Serialize(testMethodInvocation);
        verify(mockRemoteSender).Send(testSerializedMethodInvocation);
        verify(mockRemoteReceiver).Receive();
        verify(mockMethodInvocationSerializer).DeserializeReturnValue(testSerializedReturnValue);
        verifyNoMoreInteractions(mockRemoteSender);
        verifyNoMoreInteractions(mockRemoteReceiver);
        verifyNoMoreInteractions(mockMethodInvocationSerializer);
        assertEquals(testReturnValue, returnValue);
    }
    
    @Test
    public void InvokeVoidMethodNonVoidMethodInvocation() {
        try {
            testMethodInvocationRemoteSender.InvokeVoidMethod(testMethodInvocation);
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            assertTrue(e instanceof IllegalArgumentException);
            assertTrue(e.getMessage().contains("Method invocation must have a void return type."));
        }
    }
    
    @Test
    public void InvokeVoidMethodException() throws Exception {
        when(mockMethodInvocationSerializer.Serialize(testVoidMethodInvocation)).thenReturn(testVoidSerializedMethodInvocation);
        doThrow(new Exception("Mock Receive Failure")).when(mockRemoteReceiver).Receive();
        try {
            testMethodInvocationRemoteSender.InvokeVoidMethod(testVoidMethodInvocation);
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            verify(mockMethodInvocationSerializer).Serialize(testVoidMethodInvocation);
            verify(mockRemoteSender).Send(testVoidSerializedMethodInvocation);
            verifyNoMoreInteractions(mockMethodInvocationSerializer);
            verifyNoMoreInteractions(mockRemoteSender);
            assertTrue(e.getMessage().contains("Failed to invoke method."));
        }
    }
    
    @Test
    public void InvokeVoidMethodNonVoidReturnType() throws Exception {
        when(mockMethodInvocationSerializer.Serialize(testVoidMethodInvocation)).thenReturn(testVoidSerializedMethodInvocation);
        doReturn(testSerializedReturnValue).when(mockRemoteReceiver).Receive();
        doReturn(testVoidSerializedReturnValue).when(mockMethodInvocationSerializer).getVoidReturnValue();
        try {
            testMethodInvocationRemoteSender.InvokeVoidMethod(testVoidMethodInvocation);
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            verify(mockMethodInvocationSerializer).Serialize(testVoidMethodInvocation);
            verify(mockRemoteSender).Send(testVoidSerializedMethodInvocation);
            verify(mockRemoteReceiver).Receive();
            verify(mockMethodInvocationSerializer).getVoidReturnValue();
            verifyNoMoreInteractions(mockMethodInvocationSerializer);
            verifyNoMoreInteractions(mockRemoteSender);
            verifyNoMoreInteractions(mockRemoteReceiver);
            assertTrue(e.getMessage().contains("Invocation of void method returned non-void."));
        }
    }
    
    @Test
    public void InvokeVoidMethodSuccessTests() throws Exception {
        when(mockMethodInvocationSerializer.Serialize(testVoidMethodInvocation)).thenReturn(testVoidSerializedMethodInvocation);
        doReturn(testVoidSerializedReturnValue).when(mockRemoteReceiver).Receive();
        doReturn(testVoidSerializedReturnValue).when(mockMethodInvocationSerializer).getVoidReturnValue();
        try {
            testMethodInvocationRemoteSender.InvokeVoidMethod(testVoidMethodInvocation);
        }
        catch(Exception e) {
            fail("Unexpected exception thrown.");
        }
        verify(mockMethodInvocationSerializer).Serialize(testVoidMethodInvocation);
        verify(mockRemoteSender).Send(testVoidSerializedMethodInvocation);
        verify(mockRemoteReceiver).Receive();
        verify(mockMethodInvocationSerializer).getVoidReturnValue();
        verifyNoMoreInteractions(mockMethodInvocationSerializer);
        verifyNoMoreInteractions(mockRemoteSender);
        verifyNoMoreInteractions(mockRemoteReceiver);
    }
}

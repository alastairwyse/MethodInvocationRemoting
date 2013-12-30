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
 * Unit tests for class methodinvocationremoting.MethodInvocationRemoteReceiver.
 * @author Alastair Wyse
 */
public class MethodInvocationRemoteReceiverTests {

    private IMethodInvocationSerializer mockMethodInvocationSerializer;
    private IRemoteSender mockRemoteSender;
    private IRemoteReceiver mockRemoteReceiver;
    private IMethodInvocationReceivedEventHandler mockMethodInvocationReceivedEventHandler;
    private IMethodInvocationRemoteReceiver testMethodInvocationRemoteReceiver;
    private MethodInvocation testMethodInvocation;
    private String testSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><string>ABC</string></Parameter><Parameter><DataType>integer</DataType><int>12345</int></Parameter><Parameter><DataType>boolean</DataType><boolean>true</boolean></Parameter></Parameters><ReturnType>string</ReturnType></MethodInvocation>";
    
    @Before
    public void setUp() throws Exception {
        mockMethodInvocationSerializer = mock(IMethodInvocationSerializer.class);
        mockRemoteSender = mock(IRemoteSender.class);
        mockRemoteReceiver = mock(IRemoteReceiver.class);
        mockMethodInvocationReceivedEventHandler = mock(IMethodInvocationReceivedEventHandler.class);
        testMethodInvocationRemoteReceiver = new MethodInvocationRemoteReceiver(mockMethodInvocationSerializer, mockRemoteSender, mockRemoteReceiver);
        testMethodInvocationRemoteReceiver.setReceivedEventHandler(mockMethodInvocationReceivedEventHandler);
        
        Object[] testMethodInvocationParameters = new Object[3];
        testMethodInvocationParameters[0] = "ABC";
        testMethodInvocationParameters[1] = 12345;
        testMethodInvocationParameters[2] = true;
        testMethodInvocation = new MethodInvocation("TestMethod", testMethodInvocationParameters, String.class);
    }
    
    @Test
    public void ReceiveEventHandlerNotSet() {
        testMethodInvocationRemoteReceiver = new MethodInvocationRemoteReceiver(mockMethodInvocationSerializer, mockRemoteSender, mockRemoteReceiver);
        try {
            testMethodInvocationRemoteReceiver.Receive();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verifyNoMoreInteractions(mockRemoteSender);
            verifyNoMoreInteractions(mockRemoteReceiver);
            verifyNoMoreInteractions(mockMethodInvocationSerializer);
            assertTrue(e.getMessage().contains("Member 'ReceivedEventHandler' has not been set."));
        }
    }
    
    @Test
    public void ReceiveSuccessException() throws Exception {
        MethodInvocationReceivedEventHandlerStub stubEventHandler = new MethodInvocationReceivedEventHandlerStub();
        
        doThrow(new Exception("Mock Receive Failure")).when(mockRemoteReceiver).Receive();
        
        testMethodInvocationRemoteReceiver.setReceivedEventHandler(stubEventHandler);
        testMethodInvocationRemoteReceiver.Receive();
        // Need to pause so that receiveLoopThread has time to iterate before CancelReceive() is sent.
        //   Unfortunately this is still not a deterministic way to test, but best that can be done given that the Receive() spawns off a new thread.
        Thread.sleep(50);
        testMethodInvocationRemoteReceiver.CancelReceive();

        verify(mockRemoteReceiver).Receive();
        verify(mockRemoteReceiver).CancelReceive();
        verifyNoMoreInteractions(mockRemoteSender);
        verifyNoMoreInteractions(mockRemoteReceiver);
        verifyNoMoreInteractions(mockMethodInvocationSerializer);
        assertTrue(stubEventHandler.getE().getMessage().contains("Failed to invoke method."));
    }
    
    @Test
    public void ReceiveSuccessTests() throws Exception {
        doReturn(testSerializedMethodInvocation).when(mockRemoteReceiver).Receive();
        when(mockMethodInvocationSerializer.Deserialize(testSerializedMethodInvocation)).thenReturn(testMethodInvocation);

        testMethodInvocationRemoteReceiver.Receive();
        // Need to pause so that receiveLoopThread has time to iterate before CancelReceive() is sent.
        //   Unfortunately this is still not a deterministic way to test, but best that can be done given that the Receive() spawns off a new thread.
        Thread.sleep(50);
        testMethodInvocationRemoteReceiver.CancelReceive();

        verify(mockRemoteReceiver, atLeastOnce()).Receive();
        verify(mockMethodInvocationSerializer, atLeastOnce()).Deserialize(testSerializedMethodInvocation);
        verify(mockMethodInvocationReceivedEventHandler, atLeastOnce()).MethodInvocationReceived(testMethodInvocationRemoteReceiver, testMethodInvocation);
        verify(mockRemoteReceiver).CancelReceive();
        verifyNoMoreInteractions(mockRemoteSender);
        verifyNoMoreInteractions(mockRemoteReceiver);
        verifyNoMoreInteractions(mockMethodInvocationSerializer);
    }

    @Test
    public void SendReturnValueException() throws Exception {
        String testReturnValue = "TestReturnValue";
        String testSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><string>TestReturnValue</string></ReturnValue>";

        when(mockMethodInvocationSerializer.SerializeReturnValue(testReturnValue)).thenReturn(testSerializedReturnValue);
        doThrow(new Exception("Mock Send Failure")).when(mockRemoteSender).Send(testSerializedReturnValue);
        try {
            testMethodInvocationRemoteReceiver.SendReturnValue(testReturnValue);
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            verify(mockMethodInvocationSerializer).SerializeReturnValue(testReturnValue);
            verifyNoMoreInteractions(mockRemoteReceiver);
            verifyNoMoreInteractions(mockMethodInvocationSerializer);
            assertTrue(e.getMessage().contains("Failed to send return value."));
        }
    }
    
    @Test
    public void SendReturnValueSuccessTests() throws Exception {
        String testReturnValue = "TestReturnValue";
        String testSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><string>TestReturnValue</string></ReturnValue>";

        when(mockMethodInvocationSerializer.SerializeReturnValue(testReturnValue)).thenReturn(testSerializedReturnValue);

        testMethodInvocationRemoteReceiver.SendReturnValue(testReturnValue);

        verify(mockMethodInvocationSerializer).SerializeReturnValue(testReturnValue);
        verify(mockRemoteSender).Send(testSerializedReturnValue);
        verifyNoMoreInteractions(mockRemoteSender);
        verifyNoMoreInteractions(mockRemoteReceiver);
        verifyNoMoreInteractions(mockMethodInvocationSerializer);
    }
    
    @Test
    public void SendVoidReturnException() throws Exception {
        String testVoidReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnType>void</ReturnType>";
        
        doReturn(testVoidReturnValue).when(mockMethodInvocationSerializer).getVoidReturnValue();
        doThrow(new Exception("Mock Send Failure")).when(mockRemoteSender).Send(testVoidReturnValue);
        try {
            testMethodInvocationRemoteReceiver.SendVoidReturn();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockMethodInvocationSerializer).getVoidReturnValue();
            verifyNoMoreInteractions(mockRemoteReceiver);
            verifyNoMoreInteractions(mockMethodInvocationSerializer);
            assertTrue(e.getMessage().contains("Failed to send void return value."));
        }
    }
    
    @Test
    public void SendVoidReturnSuccessTests() throws Exception {
        String testVoidReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnType>void</ReturnType>";
        
        doReturn(testVoidReturnValue).when(mockMethodInvocationSerializer).getVoidReturnValue();

        testMethodInvocationRemoteReceiver.SendVoidReturn();

        verify(mockMethodInvocationSerializer).getVoidReturnValue();
        verify(mockRemoteSender).Send(testVoidReturnValue);
        verifyNoMoreInteractions(mockRemoteSender);
        verifyNoMoreInteractions(mockRemoteReceiver);
        verifyNoMoreInteractions(mockMethodInvocationSerializer);
    }
    
    private class MethodInvocationReceivedEventHandlerStub implements IMethodInvocationReceivedEventHandler {

        private Exception e;

        public Exception getE() {
            return e;
        }
        
        @Override
        public void MethodInvocationReceived(IMethodInvocationRemoteReceiver source, IMethodInvocation receivedMethodInvocation) {
            // Not used for the purpose of these tests
        }

        @Override
        public void MethodInvocationReceiveException(IMethodInvocationRemoteReceiver source, Exception e) {
            this.e = e;
        }
    }
}

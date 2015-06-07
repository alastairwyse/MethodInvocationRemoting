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
 * Unit tests for the metrics logging functionality in class methodinvocationremoting.MethodInvocationRemoteReceiver.
 * @author Alastair Wyse
 */
public class MethodInvocationRemoteReceiverMetricsTests {

    private IMethodInvocationSerializer mockMethodInvocationSerializer;
    private IRemoteSender mockRemoteSender;
    private IRemoteReceiver mockRemoteReceiver;
    private IMetricLogger mockMetricLogger;
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
        mockMetricLogger = mock(IMetricLogger.class);
        mockMethodInvocationReceivedEventHandler = mock(IMethodInvocationReceivedEventHandler.class);
        testMethodInvocationRemoteReceiver = new MethodInvocationRemoteReceiver(mockMethodInvocationSerializer, mockRemoteSender, mockRemoteReceiver, mockMetricLogger);
        testMethodInvocationRemoteReceiver.setReceivedEventHandler(mockMethodInvocationReceivedEventHandler);
        testSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><string>TestReturnValue</string></ReturnValue>";
        testVoidReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnType>void</ReturnType>";
        testSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><string>ABC</string></Parameter><Parameter><DataType>integer</DataType><int>12345</int></Parameter><Parameter><DataType>boolean</DataType><boolean>true</boolean></Parameter></Parameters><ReturnType>string</ReturnType></MethodInvocation>";
    }
    
    @Test
    public void ReceiveMetricsTest() throws Exception {
        when(mockRemoteReceiver.Receive())
            .thenReturn(testSerializedMethodInvocation)
            .thenReturn("");
        when(mockMethodInvocationSerializer.Deserialize(testSerializedMethodInvocation)).thenReturn(new MethodInvocation("TestMethod"));

        testMethodInvocationRemoteReceiver.Receive();
        // Need to pause so that receiveLoopThread has time to iterate before CancelReceive() is sent.
        //   Unfortunately this is still not a deterministic way to test, but best that can be done given that the Receive() spawns off a new thread.
        Thread.sleep(50);
        testMethodInvocationRemoteReceiver.CancelReceive();

        verify(mockMetricLogger).Begin(isA(RemoteMethodReceiveTime.class));
        verifyNoMoreInteractions(mockMetricLogger);
    }
    
    @Test
    public void SendReturnValueMetricsTest() throws Exception {
        String testReturnValue = "TestReturnValue";
        
        when(mockMethodInvocationSerializer.SerializeReturnValue(testReturnValue)).thenReturn(testSerializedReturnValue);
        
        testMethodInvocationRemoteReceiver.SendReturnValue(testReturnValue);
        
        verify(mockMetricLogger).End(isA(RemoteMethodReceiveTime.class));
        verify(mockMetricLogger).Increment(isA(RemoteMethodReceived.class));
        verifyNoMoreInteractions(mockMetricLogger);
    }
    
    @Test
    public void SendReturnValueExceptionMetricsTest() throws Exception {
        String testReturnValue = "TestReturnValue";
        String testSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><string>TestReturnValue</string></ReturnValue>";

        when(mockMethodInvocationSerializer.SerializeReturnValue(testReturnValue)).thenReturn(testSerializedReturnValue);
        doThrow(new Exception("Mock Send Failure")).when(mockRemoteSender).Send(testSerializedReturnValue);
        try {
            testMethodInvocationRemoteReceiver.SendReturnValue(testReturnValue);
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            verify(mockMetricLogger).CancelBegin(isA(RemoteMethodReceiveTime.class));
            verifyNoMoreInteractions(mockMetricLogger);
        }
    }
    
    @Test
    public void SendVoidReturnValueMetricsTest() throws Exception {
        when(mockMethodInvocationSerializer.getVoidReturnValue()).thenReturn(testVoidReturnValue);
        
        testMethodInvocationRemoteReceiver.SendVoidReturn();
        
        verify(mockMetricLogger).End(isA(RemoteMethodReceiveTime.class));
        verify(mockMetricLogger).Increment(isA(RemoteMethodReceived.class));
        verifyNoMoreInteractions(mockMetricLogger);
    }
    
    @Test
    public void SendVoidReturnValueExceptionMetricsTest() throws Exception {
        String testVoidReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnType>void</ReturnType>";
        
        doReturn(testVoidReturnValue).when(mockMethodInvocationSerializer).getVoidReturnValue();
        doThrow(new Exception("Mock Send Failure")).when(mockRemoteSender).Send(testVoidReturnValue);
        try {
            testMethodInvocationRemoteReceiver.SendVoidReturn();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockMetricLogger).CancelBegin(isA(RemoteMethodReceiveTime.class));
            verifyNoMoreInteractions(mockMetricLogger);
        }
    }
}

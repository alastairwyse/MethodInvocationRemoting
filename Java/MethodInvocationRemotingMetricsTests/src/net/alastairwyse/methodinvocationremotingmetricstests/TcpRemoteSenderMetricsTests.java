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

import java.net.InetAddress;
import java.net.InetSocketAddress;
import java.nio.*;
import java.io.*;

import static org.junit.Assert.fail;
import static org.mockito.Mockito.*;

import org.mockito.invocation.InvocationOnMock;
import org.mockito.stubbing.*;

import net.alastairwyse.operatingsystemabstraction.*;
import net.alastairwyse.applicationlogging.*;
import net.alastairwyse.applicationmetrics.*;
import net.alastairwyse.methodinvocationremoting.*;
import net.alastairwyse.methodinvocationremotingmetrics.*;

/**
 * Unit tests for the metrics logging functionality in class methodinvocationremoting.TcpRemoteSender.
 * @author Alastair Wyse
 */
public class TcpRemoteSenderMetricsTests {

    private ISocketChannel mockSocketChannel;
    private IMetricLogger mockMetricLogger;
    private TcpRemoteSender testTcpRemoteSender;
    private String testIpAddress = "127.0.0.1";
    private int testPort = 55000;
    private String testMessage;
    private byte[] testMessageByteArray;
    private byte[] testMessageSequenceNumber;
    private byte[] testMessageSizeHeader;
    private ByteBuffer testEncodedMessage;
    
    @Before
    public void setUp() throws Exception {
        mockSocketChannel = mock(ISocketChannel.class);
        mockMetricLogger = mock(IMetricLogger.class);
        testTcpRemoteSender = new TcpRemoteSender(testIpAddress, testPort, 3, 10, 25, 10, new ConsoleApplicationLogger(LogLevel.Critical, '|', "  "), mockMetricLogger, mockSocketChannel);
        // Setup test message
        testMessageByteArray = new byte[] { 0x3c, 0x41, 0x02, 0x42, 0x03, 0x43, 0x3e };  // Equivalent to '<A[ASCII message start]B[ASCII message end]C>'
        testMessageSequenceNumber = new byte[] { 1, 0, 0, 0 };  // Sequence number 1 (first sequence number sent after instantiating the class), encoded as a little endian
        testMessageSizeHeader = new byte[] { 7, 0, 0, 0, 0, 0, 0, 0 };  // Size of the above test message, encoded as a little endian long
        testMessage = new String(testMessageByteArray, "UTF-8");
        testEncodedMessage = ByteBuffer.allocate(testMessageSequenceNumber.length + testMessageSizeHeader.length + testMessageByteArray.length + 2);
        testEncodedMessage.put((byte)0x02);
        testEncodedMessage.put(testMessageSequenceNumber);
        testEncodedMessage.put(testMessageSizeHeader);
        testEncodedMessage.put(testMessageByteArray);
        testEncodedMessage.put((byte)0x03);
        testEncodedMessage.flip();
    }
    
    @Test
    public void SendMetricsTest() throws Exception {
        ByteBuffer acknowledgementBuffer = ByteBuffer.allocate(1);
        
        when(mockSocketChannel.isConnected()).thenReturn(true);
        // As the read method passes a reference to a ByteBuffer, and data is written to that ByteBuffer, to correctly mock this a custom Answer implementation is required.
        //   An instance of the AcknowledgementAnswer class is used.
        when(mockSocketChannel.read(acknowledgementBuffer)).thenAnswer(new AcknowledgementAnswer((byte)0x06, 1));
        
        testTcpRemoteSender.Send("<TestMessage>Test message content</TestMessage>");
        
        verify(mockMetricLogger).Begin(isA(MessageSendTime.class));
        verify(mockMetricLogger).End(isA(MessageSendTime.class));
        verify(mockMetricLogger).Increment(isA(MessageSent.class));
        verifyNoMoreInteractions(mockMetricLogger);
    }
    
    @Test
    public void SendExceptionMetricsTest() throws Exception {
        // Tests that the CancelBegin() method is called when an unhandled exception is encountered during sending
        
        when(mockSocketChannel.isConnected()).thenReturn(true);
        when(mockSocketChannel.write(testEncodedMessage)).thenThrow(new java.nio.channels.NotYetConnectedException());
    
        try {
            testTcpRemoteSender.Send(testMessage);
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockMetricLogger).Begin(isA(MessageSendTime.class));
            verify(mockMetricLogger).CancelBegin(isA(MessageSendTime.class));
            verifyNoMoreInteractions(mockMetricLogger);
        }
    }
    
    @Test
    public void SendReconnectMetricsTest() throws Exception {
        // See methodinvocationremotingunittests.setUp() for an explanation of the below variables
        byte[] testMessageByteArray = new byte[] { 0x3c, 0x41, 0x02, 0x42, 0x03, 0x43, 0x3e };
        byte[] testMessageSequenceNumber = new byte[] { 1, 0, 0, 0 };
        byte[] testMessageSizeHeader = new byte[] { 7, 0, 0, 0, 0, 0, 0, 0 };
        String testMessage = new String(testMessageByteArray, "UTF-8");
        ByteBuffer testEncodedMessage = ByteBuffer.allocate(testMessageSequenceNumber.length + testMessageSizeHeader.length + testMessageByteArray.length + 2);
        testEncodedMessage.put((byte)0x02);
        testEncodedMessage.put(testMessageSequenceNumber);
        testEncodedMessage.put(testMessageSizeHeader);
        testEncodedMessage.put(testMessageByteArray);
        testEncodedMessage.put((byte)0x03);
        testEncodedMessage.flip();
        ByteBuffer acknowledgementBuffer = ByteBuffer.allocate(1);
        
        when(mockSocketChannel.isConnected()).thenReturn(true);
        // Should throw exception on first call, and then return correctly on second (after reconnecting)
        when(mockSocketChannel.write(testEncodedMessage))
            .thenThrow(new IOException("Mock IOException."))
            .thenReturn(testEncodedMessage.array().length);
        when(mockSocketChannel.read(acknowledgementBuffer)).thenAnswer(new AcknowledgementAnswer((byte)0x06, 1));

        testTcpRemoteSender.Send(testMessage);

        verify(mockMetricLogger).Begin(isA(MessageSendTime.class));
        verify(mockMetricLogger).Increment(isA(TcpRemoteSenderReconnected.class));
        verify(mockMetricLogger).End(isA(MessageSendTime.class));
        verify(mockMetricLogger).Increment(isA(MessageSent.class));
        verifyNoMoreInteractions(mockMetricLogger);
    }
    
    @Test
    public void SendReconnectExceptionMetricsTest() throws Exception {
        // Tests that if an exception occurs when sending, subsequent failure to reconnect will call method CancelBegin() when handling the exception
        
        when(mockSocketChannel.isConnected()).thenReturn(true);
        // Simulate an exception on the attempt to reconnect
        when(mockSocketChannel.connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort))).thenThrow(new java.nio.channels.NotYetConnectedException());
        when(mockSocketChannel.write(testEncodedMessage)).thenThrow(new IOException("Mock IOException."));
    
        try {
            testTcpRemoteSender.Send(testMessage);
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockMetricLogger).Begin(isA(MessageSendTime.class));
            verify(mockMetricLogger).CancelBegin(isA(MessageSendTime.class));
            verifyNoMoreInteractions(mockMetricLogger);
        }
    }
    
    @Test
    public void SendReconnectResendExceptionMetricsTest() throws Exception {
        // Tests that if an exception occurs when sending and causes a reconnect, a subsequent failure to send will call method CancelBegin() when handling the exception
        
        when(mockSocketChannel.isConnected()).thenReturn(true);
        when(mockSocketChannel.connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort))).thenReturn(true);
        when(mockSocketChannel.write(testEncodedMessage)).thenThrow(new IOException("Mock IOException."));
    
        try {
            testTcpRemoteSender.Send(testMessage);
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockMetricLogger).Begin(isA(MessageSendTime.class));
            verify(mockMetricLogger).Increment(isA(TcpRemoteSenderReconnected.class));
            verify(mockMetricLogger).CancelBegin(isA(MessageSendTime.class));
            verifyNoMoreInteractions(mockMetricLogger);
        }
    }
    
    /**
     * Mock answer for the SocketChannel.Read() method, to simulate the reading of the message acknowledgement byte
     * @author Alastair Wyse
     */
    private class AcknowledgementAnswer implements Answer<Integer> {

        private byte byteToWrite;
        private Integer returnValue;
        
        /**
         * Initialises a new instance of the AcknowledgementAnswer class.
         * @param byteToWrite  The single byte to return when the Read() method is called.
         * @param returnValue  The value to return.  Should be 1 for the success case.
         */
        public AcknowledgementAnswer(byte byteToWrite, Integer returnValue) {
            this.byteToWrite = byteToWrite;
            this.returnValue = returnValue;
        }
        
        @Override
        public Integer answer(InvocationOnMock invocation) throws Throwable {
            ByteBuffer returnByteBuffer = (ByteBuffer)invocation.getArguments()[0];
            returnByteBuffer.put(byteToWrite);

            return returnValue;
        }
    }
}

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
import java.net.*;
import java.nio.*;
import java.io.*;
import static org.mockito.Mockito.*;
import org.mockito.invocation.InvocationOnMock;
import org.mockito.stubbing.*;
import net.alastairwyse.methodinvocationremoting.*;
import net.alastairwyse.operatingsystemabstraction.*;

/**
 * Unit tests for class methodinvocationremoting.TcpRemoteSender.
 * @author Alastair Wyse
 */
public class TcpRemoteSenderTests {

    private ISocketChannel mockSocketChannel;
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
        testTcpRemoteSender = new TcpRemoteSender(testIpAddress, testPort, 3, 10, 25, 10, mockSocketChannel);
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
    public void InvalidConnectRetryCountArgument() throws Exception {
        try {
            testTcpRemoteSender = new TcpRemoteSender(testIpAddress, testPort, -1, 1000, 60000, 100, mockSocketChannel);
            fail("Exception was not thrown.");
        }
        catch (IllegalArgumentException e) {
            assertTrue(e.getMessage().contains("Argument 'connectRetryCount' must be greater than or equal to 0."));
        }
    }
    
    @Test
    public void InvalidConnectRetryIntervalArgument() throws Exception {
        try {
            testTcpRemoteSender = new TcpRemoteSender(testIpAddress, testPort, 10, -1, 60000, 100, mockSocketChannel);
            fail("Exception was not thrown.");
        }
        catch (IllegalArgumentException e) {
            assertTrue(e.getMessage().contains("Argument 'connectRetryInterval' must be greater than or equal to 0."));
        }
    }
    
    @Test
    public void InvalidAcknowledgementReceiveTimeoutArgument() throws Exception {
        try {
            testTcpRemoteSender = new TcpRemoteSender(testIpAddress, testPort, 100, 10, -1, 100, mockSocketChannel);
            fail("Exception was not thrown.");
        }
        catch (IllegalArgumentException e) {
            assertTrue(e.getMessage().contains("Argument 'acknowledgementReceiveTimeout' must be greater than or equal to 0."));
        }
    }
    
    @Test
    public void InvalidAcknowledgementReceiveRetryIntervalArgument() throws Exception {
        try {
            testTcpRemoteSender = new TcpRemoteSender(testIpAddress, testPort, 100, 10, 60000, -1, mockSocketChannel);
            fail("Exception was not thrown.");
        }
        catch (IllegalArgumentException e) {
            assertTrue(e.getMessage().contains("Argument 'acknowledgementReceiveRetryInterval' must be greater than or equal to 0."));
        }
    }
    
    @Test
    public void ConnectWhenAlreadyConnected() {
        when(mockSocketChannel.isConnected()).thenReturn(true);
        
        try {
            testTcpRemoteSender.Connect();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockSocketChannel).isConnected();
            verifyNoMoreInteractions(mockSocketChannel);
            assertTrue(e.getMessage().contains("Connection to TCP socket has already been established."));
        }
    }
    
    @Test
    public void ConnectException() throws Exception {
        when(mockSocketChannel.isConnected()).thenReturn(false);
        doThrow(new SecurityException()).when(mockSocketChannel).connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort));
        
        try {
            testTcpRemoteSender.Connect();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockSocketChannel).isConnected();
            verify(mockSocketChannel).open();
            verify(mockSocketChannel).connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort));
            verifyNoMoreInteractions(mockSocketChannel);
            assertTrue(e.getMessage().contains("Error connecting to /" + testIpAddress.toString() + ":" + testPort +"."));
        }
    }
    
    @Test
    public void ConnectFailureAfterRetry() throws Exception {
        when(mockSocketChannel.isConnected()).thenReturn(false);
        doThrow(new ConnectException()).when(mockSocketChannel).connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort));
        
        try {
            testTcpRemoteSender.Connect();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockSocketChannel, times(2)).isConnected();
            verify(mockSocketChannel, times(4)).open();
            verify(mockSocketChannel, times(4)).connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort));
            verifyNoMoreInteractions(mockSocketChannel);
            assertTrue(e.getMessage().contains("Failed to connect to /" + testIpAddress.toString() + ":" + testPort +" after 4 attempts."));
        }
    }
    
    @Test
    public void ConnectAfterRetrySuccessTest() throws Exception {
        when(mockSocketChannel.isConnected())
            .thenReturn(false)
            .thenReturn(true);
        when(mockSocketChannel.connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort)))
            .thenThrow(new ConnectException())
            .thenReturn(true);

        testTcpRemoteSender.Connect();

        verify(mockSocketChannel, times(2)).isConnected();
        verify(mockSocketChannel, times(2)).open();
        verify(mockSocketChannel, times(2)).connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort));
        verifyNoMoreInteractions(mockSocketChannel);
    }
    
    @Test
    public void ConnectSuccessTest() throws Exception {
        when(mockSocketChannel.isConnected())
            .thenReturn(false)
            .thenReturn(true);
        when(mockSocketChannel.connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort))).thenReturn(true);

        testTcpRemoteSender.Connect();
        
        verify(mockSocketChannel, times(2)).isConnected();
        verify(mockSocketChannel).open();
        verify(mockSocketChannel).connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort));
        verifyNoMoreInteractions(mockSocketChannel);
    }
    
    @Test
    public void DisconnectSuccessTest() throws Exception {
        when(mockSocketChannel.isConnected()).thenReturn(true);
        
        testTcpRemoteSender.Disconnect();
        
        verify(mockSocketChannel).isConnected();
        verify(mockSocketChannel).close();
        verifyNoMoreInteractions(mockSocketChannel);
    }
    
    @Test
    public void SendWhenNotConnected() throws Exception {
        when(mockSocketChannel.isConnected()).thenReturn(false);
        
        try {
            testTcpRemoteSender.Send(testMessage);
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockSocketChannel).isConnected();
            verifyNoMoreInteractions(mockSocketChannel);
            assertTrue(e.getMessage().contains("Connection to TCP socket has not been established."));
        }
    }
    
    @Test
    public void SendSuccessTest() throws Exception {
        ByteBuffer acknowledgementBuffer = ByteBuffer.allocate(1);
        
        when(mockSocketChannel.isConnected())
            .thenReturn(false)
            .thenReturn(true)
            .thenReturn(true);
        when(mockSocketChannel.connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort))).thenReturn(true);
        // As the read method passes a reference to a ByteBuffer, and data is written to that ByteBuffer, to correctly mock this a custom Answer implementation is required.
        //   An instance of the AcknowledgementAnswer class is used.
        when(mockSocketChannel.read(acknowledgementBuffer)).thenAnswer(new AcknowledgementAnswer((byte)0x06, 1));
        
        testTcpRemoteSender.Connect();
        testTcpRemoteSender.Send(testMessage);
        
        verify(mockSocketChannel, times(3)).isConnected();
        verify(mockSocketChannel).open();
        verify(mockSocketChannel).connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort));
        verify(mockSocketChannel).write(testEncodedMessage);
        verify(mockSocketChannel).configureBlocking(false);
        // Using the acknowledgementBuffer member in the below verify statement causes an exception.  Hence any ByteBuffer class is specified.  Correct calling of the Read() method is checked by the above when statement in any case.
        verify(mockSocketChannel).read(any(ByteBuffer.class));
        verify(mockSocketChannel).configureBlocking(true);
        verifyNoMoreInteractions(mockSocketChannel);
    }
    
    @Test
    public void SendUnhandledException() throws Exception {
        when(mockSocketChannel.isConnected())
            .thenReturn(false)
            .thenReturn(true)
            .thenReturn(true);
        when(mockSocketChannel.connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort))).thenReturn(true);
        when(mockSocketChannel.write(testEncodedMessage)).thenThrow(new java.nio.channels.NotYetConnectedException());
        
        try {
            testTcpRemoteSender.Connect();
            testTcpRemoteSender.Send(testMessage);
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockSocketChannel, times(3)).isConnected();
            verify(mockSocketChannel).open();
            verify(mockSocketChannel).connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort));
            verify(mockSocketChannel).write(testEncodedMessage);
            verifyNoMoreInteractions(mockSocketChannel);
            assertTrue(e.getMessage().contains("Error sending message.  Unhandled exception while sending message."));
        }
    }
    
    @Test
    public void SendIOExceptionResendSuccessTest() throws Exception {
        ByteBuffer acknowledgementBuffer = ByteBuffer.allocate(1);
        
        when(mockSocketChannel.isConnected())
            .thenReturn(false)
            .thenReturn(true)
            .thenReturn(true)
            .thenReturn(true);
        when(mockSocketChannel.connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort))).thenReturn(true);
        // Should throw exception on first call, and then return correctly on second (after reconnecting)
        when(mockSocketChannel.write(testEncodedMessage))
        	.thenThrow(new IOException("Mock IOException."))
        	.thenReturn(testEncodedMessage.array().length);
        when(mockSocketChannel.read(acknowledgementBuffer)).thenAnswer(new AcknowledgementAnswer((byte)0x06, 1));

        testTcpRemoteSender.Connect();
        testTcpRemoteSender.Send(testMessage);

        verify(mockSocketChannel, times(4)).isConnected();
        verify(mockSocketChannel, times(2)).open();
        verify(mockSocketChannel, times(2)).connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort));
        verify(mockSocketChannel, times(2)).write(testEncodedMessage);
        verify(mockSocketChannel).close();
        verify(mockSocketChannel).configureBlocking(false);
        verify(mockSocketChannel).read(any(ByteBuffer.class));
        verify(mockSocketChannel).configureBlocking(true);
        verifyNoMoreInteractions(mockSocketChannel);
    }
    
    @Test
    public void SendIOExceptionResendException() throws Exception {
        when(mockSocketChannel.isConnected())
            .thenReturn(false)
            .thenReturn(true)
            .thenReturn(true)
            .thenReturn(true);
        when(mockSocketChannel.connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort))).thenReturn(true);
        when(mockSocketChannel.write(testEncodedMessage)).thenThrow(new IOException("Mock IOException."));

        try {
            testTcpRemoteSender.Connect();
            testTcpRemoteSender.Send(testMessage);
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockSocketChannel, times(4)).isConnected();
            verify(mockSocketChannel, times(2)).open();
            verify(mockSocketChannel, times(2)).connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort));
            verify(mockSocketChannel, times(2)).write(testEncodedMessage);
            verify(mockSocketChannel).close();
            verifyNoMoreInteractions(mockSocketChannel);
            assertTrue(e.getMessage().contains("Error sending message.  Failed to send message after reconnecting."));
        }
    }
    
    @Test
    public void SendInvalidAcknowledgementByte() throws Exception {
        ByteBuffer acknowledgementBuffer = ByteBuffer.allocate(1);
        
        when(mockSocketChannel.isConnected())
            .thenReturn(false)
            .thenReturn(true)
            .thenReturn(true);
        when(mockSocketChannel.connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort))).thenReturn(true);
        when(mockSocketChannel.read(acknowledgementBuffer)).thenAnswer(new AcknowledgementAnswer((byte)0x04, 1));
        
        try {
            testTcpRemoteSender.Connect();
            testTcpRemoteSender.Send(testMessage);
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockSocketChannel, times(3)).isConnected();
            verify(mockSocketChannel).open();
            verify(mockSocketChannel).connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort));
            verify(mockSocketChannel).write(testEncodedMessage);
            verify(mockSocketChannel).configureBlocking(false);
            verify(mockSocketChannel).read(any(ByteBuffer.class));
            verify(mockSocketChannel).configureBlocking(true);
            verifyNoMoreInteractions(mockSocketChannel);
            assertTrue(e.getMessage().contains("Error sending message.  Unhandled exception while sending message."));
            assertTrue(e.getCause().getMessage().contains("Acknowledgement byte was expected to be 6, but was 4."));
        }
    }
    
    @Test
    public void SendAcknowledgementNotReceivedReconnectSuccessTest() throws Exception {
        ByteBuffer acknowledgementBuffer = ByteBuffer.allocate(1);
        
        when(mockSocketChannel.isConnected())
            .thenReturn(false)
            .thenReturn(true)
            .thenReturn(true);
        when(mockSocketChannel.connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort))).thenReturn(true);
        // This part is non-deterministic.  Parameter 'acknowledgementReceiveTimeout' is set to 25ms, and 'acknowledgementReceiveRetryInterval' set to 10ms.  Hence usually there should be 3 calls to read() before an exception is thrown.
        //   However, if the test runs slowly it may be called less times, which will cause the test to fail
        when(mockSocketChannel.read(acknowledgementBuffer))
        	.thenReturn(0)
        	.thenReturn(0)
        	.thenReturn(0)
        	.thenAnswer(new AcknowledgementAnswer((byte)0x06, 1));

        testTcpRemoteSender.Connect();
        testTcpRemoteSender.Send(testMessage);

        verify(mockSocketChannel, times(4)).isConnected();
        verify(mockSocketChannel, times(2)).open();
        verify(mockSocketChannel, times(2)).connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort));
        verify(mockSocketChannel, times(2)).write(testEncodedMessage);
        verify(mockSocketChannel, times(2)).configureBlocking(false);
        verify(mockSocketChannel, times(4)).read(any(ByteBuffer.class));
        verify(mockSocketChannel, times(2)).configureBlocking(true);
        verify(mockSocketChannel).close();
        verifyNoMoreInteractions(mockSocketChannel);
    }
    
    @Test
    public void SendAcknowledgementNotReceivedReconnectAcknowledgementNotReceived() throws Exception {
        ByteBuffer acknowledgementBuffer = ByteBuffer.allocate(1);
        
        when(mockSocketChannel.isConnected())
            .thenReturn(false)
            .thenReturn(true)
            .thenReturn(true);
        when(mockSocketChannel.connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort))).thenReturn(true);
        when(mockSocketChannel.read(acknowledgementBuffer))
        	.thenReturn(0);

        try {
	        testTcpRemoteSender.Connect();
	        testTcpRemoteSender.Send(testMessage);
	        fail("Exception was not thrown.");
        }
        catch (Exception e) {
	        verify(mockSocketChannel, times(4)).isConnected();
	        verify(mockSocketChannel, times(2)).open();
	        verify(mockSocketChannel, times(2)).connect(new InetSocketAddress(InetAddress.getByName(testIpAddress), testPort));
	        verify(mockSocketChannel, times(2)).write(testEncodedMessage);
	        verify(mockSocketChannel, times(2)).configureBlocking(false);
	        // Read should be called a minimum of 2 times, once for the first send attempt, and once for the second
	        verify(mockSocketChannel, atLeast(2)).read(any(ByteBuffer.class));
	        verify(mockSocketChannel, times(2)).configureBlocking(true);
	        verify(mockSocketChannel).close();
	        verifyNoMoreInteractions(mockSocketChannel);
	        assertTrue(e.getMessage().contains("Error sending message.  Failed to send message after reconnecting."));
            assertTrue(e.getCause().getMessage().contains("Failed to receive message acknowledgement within timeout period of 25 milliseconds."));
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

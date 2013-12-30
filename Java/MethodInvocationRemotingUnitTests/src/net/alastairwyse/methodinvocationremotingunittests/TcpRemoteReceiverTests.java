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
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.Arrays;
import java.io.*;
import static org.mockito.Mockito.*;
import org.mockito.invocation.InvocationOnMock;
import org.mockito.stubbing.*;
import net.alastairwyse.methodinvocationremoting.*;
import net.alastairwyse.operatingsystemabstraction.*;

/**
 * Unit tests for class methodinvocationremoting.TcpRemoteReceiver.
 * @author Alastair Wyse
 */
public class TcpRemoteReceiverTests {
    private IServerSocketChannel mockServerSocketChannel;
    private ISocketChannel mockSocketChannel;
    private TcpRemoteReceiver testTcpRemoteReceiver;
    private int testPort = 55000;
    private int socketReadBufferSize = 1024;
    private ByteBuffer testMessageByteArray;
    private final String stringEncodingCharset = "UTF-8";

    @Before
    public void setUp() throws Exception {
        mockServerSocketChannel = mock(IServerSocketChannel.class);
        mockSocketChannel = mock(ISocketChannel.class);
        testTcpRemoteReceiver = new TcpRemoteReceiver(testPort, 3, 10, 20, socketReadBufferSize, mockServerSocketChannel);
        // Setup test message
        byte[] testMessageBody = "<Data>ABC</Data>".getBytes(stringEncodingCharset);
        byte[] testMessageSequenceNumber = ByteBuffer.allocate(4).order(ByteOrder.LITTLE_ENDIAN).putInt(123).array();
        byte[] testMessageSizeHeader = ByteBuffer.allocate(8).order(ByteOrder.LITTLE_ENDIAN).putLong(testMessageBody.length).array();
        testMessageByteArray = ByteBuffer.allocate(testMessageSequenceNumber.length + testMessageSizeHeader.length + testMessageBody.length + 2);
        testMessageByteArray.put((byte)0x02);  // Set the start delimiter
        testMessageByteArray.put(testMessageSequenceNumber);
        testMessageByteArray.put(testMessageSizeHeader);
        testMessageByteArray.put(testMessageBody);
        testMessageByteArray.put((byte)0x03);  // Set the end delimiter
        testMessageByteArray.flip();
    }
    
    @Test
    public void InvalidConnectRetryCountArgument() throws Exception {
        try {
            testTcpRemoteReceiver = new TcpRemoteReceiver(testPort, -1, 10, 20, 32767, mockServerSocketChannel);
            fail("Exception was not thrown.");
        }
        catch (IllegalArgumentException e) {
            assertTrue(e.getMessage().contains("Argument 'connectRetryCount' must be greater than or equal to 0."));
        }
    }
    
    @Test
    public void InvalidConnectRetryIntervalArgument() throws Exception {
        try {
            testTcpRemoteReceiver = new TcpRemoteReceiver(testPort, 3, -1, 20, 32767, mockServerSocketChannel);
            fail("Exception was not thrown.");
        }
        catch (IllegalArgumentException e) {
            assertTrue(e.getMessage().contains("Argument 'connectRetryInterval' must be greater than or equal to 0."));
        }
    }
    
    @Test
    public void InvalidReceiveRetryIntervalArgument() throws Exception {
        try {
            testTcpRemoteReceiver = new TcpRemoteReceiver(testPort, 3, 10, -1, 32767, mockServerSocketChannel);
            fail("Exception was not thrown.");
        }
        catch (IllegalArgumentException e) {
            assertTrue(e.getMessage().contains("Argument 'receiveRetryInterval' must be greater than or equal to 0."));
        }
    }
    
    @Test
    public void InvalidSocketReadBufferSizeArgument() throws Exception {
        try {
            testTcpRemoteReceiver = new TcpRemoteReceiver(testPort, 3, 10, 20, 0, mockServerSocketChannel);
            fail("Exception was not thrown.");
        }
        catch (IllegalArgumentException e) {
            assertTrue(e.getMessage().contains("Argument 'socketReadBufferSize' must be greater than 0."));
        }
    }
    
    @Test
    public void ConnectOpenServerSocketChannelFailure() throws Exception {
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        doThrow(new BindException("Mock BindException.")).when(mockServerSocketChannel).bind(new InetSocketAddress(testPort), 1);
        
        try {
            testTcpRemoteReceiver.Connect();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockServerSocketChannel).isOpen();
            verify(mockServerSocketChannel).open();
            verify(mockServerSocketChannel).bind(new InetSocketAddress(testPort), 1);
            verifyNoMoreInteractions(mockServerSocketChannel);
            assertTrue(e.getMessage().contains("Failed to open ServerSocketChannel whilst connecting."));
            assertTrue(e.getCause().getMessage().contains("Mock BindException."));
        }
    }
    
    @Test
    public void ConnectFailureAfterRetryForNoPendingConnection() throws Exception {
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept()).thenReturn(null);
        
        try {
            testTcpRemoteReceiver.Connect();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockServerSocketChannel).isOpen();
            verify(mockServerSocketChannel).open();
            verify(mockServerSocketChannel).bind(new InetSocketAddress(testPort), 1);
            verify(mockServerSocketChannel).configureBlocking(false);
            verify(mockServerSocketChannel, times(4)).accept();
            verifyNoMoreInteractions(mockServerSocketChannel);
            assertTrue(e.getMessage().contains("Failed to receive connection on port " + testPort + " after 4 attempts."));
        }
    }
    
    @Test
    public void ConnectFailureAfterRetryForIoException() throws Exception {
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept()).thenThrow(new IOException("Mock IOException."));
        
        try {
            testTcpRemoteReceiver.Connect();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockServerSocketChannel).isOpen();
            verify(mockServerSocketChannel).open();
            verify(mockServerSocketChannel).bind(new InetSocketAddress(testPort), 1);
            verify(mockServerSocketChannel).configureBlocking(false);
            verify(mockServerSocketChannel, times(4)).accept();
            verifyNoMoreInteractions(mockServerSocketChannel);
            assertTrue(e.getMessage().contains("Failed to receive connection on port " + testPort + " after 4 attempts."));
        }
    }
    
    @Test
    public void ConnectAfterRetrySuccessTest() throws Exception {
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept())
            .thenReturn(null)
            .thenReturn(null)
            .thenReturn(null)
            .thenReturn(mockSocketChannel);
        
        testTcpRemoteReceiver.Connect();
        
        verify(mockServerSocketChannel).isOpen();
        verify(mockServerSocketChannel).open();
        verify(mockServerSocketChannel).bind(new InetSocketAddress(testPort), 1);
        verify(mockServerSocketChannel).configureBlocking(false);
        verify(mockServerSocketChannel, times(4)).accept();
        verifyNoMoreInteractions(mockServerSocketChannel);
    }
    
    @Test
    public void ConnectSuccessTest() throws Exception {
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept()).thenReturn(mockSocketChannel);
    
        testTcpRemoteReceiver.Connect();
        
        verify(mockServerSocketChannel).isOpen();
        verify(mockServerSocketChannel).open();
        verify(mockServerSocketChannel).bind(new InetSocketAddress(testPort), 1);
        verify(mockServerSocketChannel).configureBlocking(false);
        verify(mockServerSocketChannel).accept();
        verifyNoMoreInteractions(mockServerSocketChannel);
    }
    
    @Test
    public void ConnectWhenAlreadyConnected() throws Exception {
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept()).thenReturn(mockSocketChannel);
        
        try {
            testTcpRemoteReceiver.Connect();
            testTcpRemoteReceiver.Connect();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockServerSocketChannel).isOpen();
            verify(mockServerSocketChannel).open();
            verify(mockServerSocketChannel).bind(new InetSocketAddress(testPort), 1);
            verify(mockServerSocketChannel).configureBlocking(false);
            verify(mockServerSocketChannel).accept();
            verifyNoMoreInteractions(mockServerSocketChannel);
            assertTrue(e.getMessage().contains("Connection has already been established."));
        }
    }
    
    @Test
    public void  DisconnectSuccessTest() throws Exception {
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept()).thenReturn(mockSocketChannel);
        
        testTcpRemoteReceiver.Connect();
        testTcpRemoteReceiver.Disconnect();
        
        // Verifications for Connect()
        verify(mockServerSocketChannel).isOpen();
        verify(mockServerSocketChannel).open();
        verify(mockServerSocketChannel).bind(new InetSocketAddress(testPort), 1);
        verify(mockServerSocketChannel).configureBlocking(false);
        verify(mockServerSocketChannel).accept();
        verify(mockSocketChannel).configureBlocking(false);
        // Verifications for Disconnect()
        verify(mockSocketChannel).close();
        verify(mockServerSocketChannel).close();
        verifyNoMoreInteractions(mockServerSocketChannel);
        verifyNoMoreInteractions(mockSocketChannel);
    }
    
    @Test
    public void ReceiveWhenNotConnected() {
        try {
            testTcpRemoteReceiver.Receive();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verifyNoMoreInteractions(mockServerSocketChannel);
            assertTrue(e.getMessage().contains("Connection on TCP socket has not been established."));
        }
    }

    @Test
    public void ReceiveIncorrectStartMessageDelimiter() throws Exception {
        ByteBuffer readBuffer = ByteBuffer.allocate(socketReadBufferSize);
        testMessageByteArray.array()[0] = 1;
        
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept())
            .thenReturn(mockSocketChannel)
            .thenReturn(null);
        when(mockSocketChannel.read(readBuffer)).thenAnswer(new ReadMethodAnswer(testMessageByteArray, testMessageByteArray.remaining()));
        
        try {
            testTcpRemoteReceiver.Connect();
            testTcpRemoteReceiver.Receive();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockServerSocketChannel).isOpen();
            verify(mockServerSocketChannel).open();
            verify(mockServerSocketChannel).bind(new InetSocketAddress(testPort), 1);
            verify(mockServerSocketChannel).configureBlocking(false);
            verify(mockServerSocketChannel, times(2)).accept();
            verify(mockSocketChannel).configureBlocking(false);
            // Using the readBuffer member in the below verify statement causes an exception.  Hence any ByteBuffer class is specified.  Correct calling of the Read() method is checked by the above when statement in any case.
            verify(mockSocketChannel).read(any(ByteBuffer.class));
            verifyNoMoreInteractions(mockServerSocketChannel);
            verifyNoMoreInteractions(mockSocketChannel);
            assertTrue(e.getMessage().contains("Error receiving message.  Unhandled exception while attempting to receive and acknowledge message."));
            assertTrue(e.getCause().getMessage().contains("First byte of received message was expected to be 2, but was 1."));
        }
    }
    
    @Test
    public void ReceiveIncorrectEndMessageDelimiter() throws Exception {
        ByteBuffer readBuffer = ByteBuffer.allocate(socketReadBufferSize);
        testMessageByteArray.array()[29] = 0x41;
        
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept())
            .thenReturn(mockSocketChannel)
            .thenReturn(null);
        when(mockSocketChannel.read(readBuffer)).thenAnswer(new ReadMethodAnswer(testMessageByteArray, testMessageByteArray.remaining()));
        
        try {
            testTcpRemoteReceiver.Connect();
            testTcpRemoteReceiver.Receive();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockServerSocketChannel).isOpen();
            verify(mockServerSocketChannel).open();
            verify(mockServerSocketChannel).bind(new InetSocketAddress(testPort), 1);
            verify(mockServerSocketChannel).configureBlocking(false);
            verify(mockServerSocketChannel, times(2)).accept();
            verify(mockSocketChannel).configureBlocking(false);
            // Using the readBuffer member in the below verify statement causes an exception.  Hence any ByteBuffer class is specified.  Correct calling of the Read() method is checked by the above when statement in any case.
            verify(mockSocketChannel).read(any(ByteBuffer.class));
            verifyNoMoreInteractions(mockServerSocketChannel);
            verifyNoMoreInteractions(mockSocketChannel);
            assertTrue(e.getMessage().contains("Error receiving message.  Unhandled exception while attempting to receive and acknowledge message."));
            assertTrue(e.getCause().getMessage().contains("Last byte of received message was expected to be 3, but was 65."));
        }
    }
    
    @Test
    public void ReceiveExtraBytesAfterEndMessageDelimiter() throws Exception {
        ByteBuffer readBuffer = ByteBuffer.allocate(socketReadBufferSize);
        ByteBuffer longTestMessageByteArray = ByteBuffer.allocate(testMessageByteArray.array().length + 1);
        longTestMessageByteArray.put(testMessageByteArray.array());
        longTestMessageByteArray.put((byte)0x41);
        longTestMessageByteArray.flip();
        
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept())
            .thenReturn(mockSocketChannel)
            .thenReturn(null);
        when(mockSocketChannel.read(readBuffer)).thenAnswer(new ReadMethodAnswer(longTestMessageByteArray, longTestMessageByteArray.remaining()));
        
        try {
            testTcpRemoteReceiver.Connect();
            testTcpRemoteReceiver.Receive();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockServerSocketChannel).isOpen();
            verify(mockServerSocketChannel).open();
            verify(mockServerSocketChannel).bind(new InetSocketAddress(testPort), 1);
            verify(mockServerSocketChannel).configureBlocking(false);
            verify(mockServerSocketChannel, times(2)).accept();
            verify(mockSocketChannel).configureBlocking(false);
            // Using the readBuffer member in the below verify statement causes an exception.  Hence any ByteBuffer class is specified.  Correct calling of the Read() method is checked by the above when statement in any case.
            verify(mockSocketChannel).read(any(ByteBuffer.class));
            verifyNoMoreInteractions(mockServerSocketChannel);
            verifyNoMoreInteractions(mockSocketChannel);
            assertTrue(e.getMessage().contains("Error receiving message.  Unhandled exception while attempting to receive and acknowledge message."));
            assertTrue(e.getCause().getMessage().contains("Surplus data encountered after message delimiter character, starting with 65."));
        }
    }
    
    @Test
    public void ReceiveSuccessTest() throws Exception {
        // Tests receiving a message where the whole message is read from the underlying socket channel in a single read() method call
        ByteBuffer readBuffer = ByteBuffer.allocate(socketReadBufferSize);
        
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept())
            .thenReturn(mockSocketChannel)
            .thenReturn(null);
        // As the read method passes a reference to a ByteBuffer, and data is written to that ByteBuffer, to correctly mock this a custom Answer implementation is required.
        //   An instance of the ReadMethodAnswer class is used.
        when(mockSocketChannel.read(readBuffer)).thenAnswer(new ReadMethodAnswer(testMessageByteArray, testMessageByteArray.remaining()));
        
        testTcpRemoteReceiver.Connect();
        String receivedMessage = testTcpRemoteReceiver.Receive();
        
        verify(mockServerSocketChannel).isOpen();
        verify(mockServerSocketChannel).open();
        verify(mockServerSocketChannel).bind(new InetSocketAddress(testPort), 1);
        verify(mockServerSocketChannel).configureBlocking(false);
        verify(mockServerSocketChannel, times(3)).accept();
        verify(mockSocketChannel).configureBlocking(false);
        // Using the readBuffer member in the below verify statement causes an exception.  Hence any ByteBuffer class is specified.  Correct calling of the Read() method is checked by the above when statement in any case.
        verify(mockSocketChannel).read(any(ByteBuffer.class));
        verify(mockSocketChannel).write(ByteBuffer.wrap(new byte[] { 6 }));
        verifyNoMoreInteractions(mockServerSocketChannel);
        verifyNoMoreInteractions(mockSocketChannel);
        assertEquals("<Data>ABC</Data>", receivedMessage);
    }
    
    @Test
    public void ReceiveByteByByteSuccessTest() throws Exception {
        // Tests receiving a message where the message is read from the underlying socket channel one byte at a time in multiple read() method calls
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept())
            .thenReturn(mockSocketChannel)
            .thenReturn(null);
        // The mockito when() method requires separate declarations for each distinct set of parameters passed to the method to be mocked
        //   In this case the read() method is passed a different ByteBuffer every time its called, so need to setup mockSocketChannel to handle all these cases
        //   In each case, the ByteBuffer passed to read() will be one byte bigger than the previous time, and the next single byte of the message should be added to it
        for (int i = 0; i < 30; i++) {
            when(mockSocketChannel.read(getByteBufferSubSet(testMessageByteArray, 0, i, socketReadBufferSize, false)))
                .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, i, 1, socketReadBufferSize, true), 1));
        }
        
        testTcpRemoteReceiver.Connect();
        String receivedMessage = testTcpRemoteReceiver.Receive();
        
        verify(mockServerSocketChannel).isOpen();
        verify(mockServerSocketChannel).open();
        verify(mockServerSocketChannel).bind(new InetSocketAddress(testPort), 1);
        verify(mockServerSocketChannel).configureBlocking(false);
        verify(mockServerSocketChannel, times(32)).accept();
        verify(mockSocketChannel).configureBlocking(false);
        verify(mockSocketChannel, times(30)).read(any(ByteBuffer.class));
        verify(mockSocketChannel).write(ByteBuffer.wrap(new byte[] { 6 }));
        verifyNoMoreInteractions(mockServerSocketChannel);
        verifyNoMoreInteractions(mockSocketChannel);
        assertEquals("<Data>ABC</Data>", receivedMessage);
    }
    
    @Test
    public void ReceiveSmallBufferSuccessTest() throws Exception {
        // Tests receiving a message where the buffer size is 1 byte
        
        // Setup a test message with no duplicate characters to make the test simpler (otherwise multiple return values must be defined for the same call the read... e.g. for both 'a' characters in the string <Data>)
        byte[] testMessageBody = "ABCDEF".getBytes(stringEncodingCharset);
        byte[] testMessageSequenceNumber = ByteBuffer.allocate(4).order(ByteOrder.LITTLE_ENDIAN).putInt(123).array();
        byte[] testMessageSizeHeader = ByteBuffer.allocate(8).order(ByteOrder.LITTLE_ENDIAN).putLong(testMessageBody.length).array();
        testMessageByteArray = ByteBuffer.allocate(testMessageSequenceNumber.length + testMessageSizeHeader.length + testMessageBody.length + 2);
        testMessageByteArray.put((byte)0x02);  // Set the start delimiter
        testMessageByteArray.put(testMessageSequenceNumber);
        testMessageByteArray.put(testMessageSizeHeader);
        testMessageByteArray.put(testMessageBody);
        testMessageByteArray.put((byte)0x03);  // Set the end delimiter
        testMessageByteArray.flip();
        
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept())
            .thenReturn(mockSocketChannel)
            .thenReturn(null);
        // The read method will be called 20 times (once for each byte of the test message), and each time an empty byte buffer with capacity of 1 will be passed
        //   Setup the mockSocketChannel to return the bytes of the test message one at a time in order
        when(mockSocketChannel.read(getByteBufferSubSet(testMessageByteArray, 0, 0, 1, false)))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 0, 1, 1, true), 1))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 1, 1, 1, true), 1))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 2, 1, 1, true), 1))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 3, 1, 1, true), 1))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 4, 1, 1, true), 1))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 5, 1, 1, true), 1))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 6, 1, 1, true), 1))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 7, 1, 1, true), 1))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 8, 1, 1, true), 1))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 9, 1, 1, true), 1))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 10, 1, 1, true), 1))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 11, 1, 1, true), 1))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 12, 1, 1, true), 1))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 13, 1, 1, true), 1))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 14, 1, 1, true), 1))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 15, 1, 1, true), 1))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 16, 1, 1, true), 1))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 17, 1, 1, true), 1))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 18, 1, 1, true), 1))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 19, 1, 1, true), 1));

        testTcpRemoteReceiver = new TcpRemoteReceiver(testPort, 3, 10, 20, 1, mockServerSocketChannel);
        testTcpRemoteReceiver.Connect();
        String receivedMessage = testTcpRemoteReceiver.Receive();
        
        verify(mockServerSocketChannel).isOpen();
        verify(mockServerSocketChannel).open();
        verify(mockServerSocketChannel).bind(new InetSocketAddress(testPort), 1);
        verify(mockServerSocketChannel).configureBlocking(false);
        verify(mockServerSocketChannel, times(22)).accept();
        verify(mockSocketChannel).configureBlocking(false);
        verify(mockSocketChannel, times(20)).read(any(ByteBuffer.class));
        verify(mockSocketChannel).write(ByteBuffer.wrap(new byte[] { 6 }));
        verifyNoMoreInteractions(mockServerSocketChannel);
        verifyNoMoreInteractions(mockSocketChannel);
        assertEquals("ABCDEF", receivedMessage);
    }
    
    @Test
    public void ReceiveMultipleReadsSuccessTest() throws Exception {
        // Tests receiving a message where the complete message is read in 3 calls to the SocketChannel.Read() method.
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept())
            .thenReturn(mockSocketChannel)
            .thenReturn(null);
        // As the read method passes a reference to a ByteBuffer, and data is written to that ByteBuffer, to correctly mock this a custom Answer implementation is required.
        //   An instance of the ReadMethodAnswer class is used.
        when(mockSocketChannel.read(getByteBufferSubSet(testMessageByteArray, 0, 0, socketReadBufferSize, false)))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 0, 10, socketReadBufferSize, true), 10));
        when(mockSocketChannel.read(getByteBufferSubSet(testMessageByteArray, 0, 10, socketReadBufferSize, false)))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 10, 10, socketReadBufferSize, true), 10));
        when(mockSocketChannel.read(getByteBufferSubSet(testMessageByteArray, 0, 20, socketReadBufferSize, false)))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 20, 10, socketReadBufferSize, true), 10));
        
        testTcpRemoteReceiver.Connect();
        String receivedMessage = testTcpRemoteReceiver.Receive();
        
        verify(mockServerSocketChannel).isOpen();
        verify(mockServerSocketChannel).open();
        verify(mockServerSocketChannel).bind(new InetSocketAddress(testPort), 1);
        verify(mockServerSocketChannel).configureBlocking(false);
        verify(mockServerSocketChannel, times(5)).accept();
        verify(mockSocketChannel).configureBlocking(false);
        verify(mockSocketChannel, times(3)).read(any(ByteBuffer.class));
        verify(mockSocketChannel).write(ByteBuffer.wrap(new byte[] { 6 }));
        verifyNoMoreInteractions(mockServerSocketChannel);
        verifyNoMoreInteractions(mockSocketChannel);
        assertEquals("<Data>ABC</Data>", receivedMessage);
    }
    
    @Test
    public void ReceiveReconnectImmediateSuccessTest() throws Exception {
        // Tests receiving a message, where a new pending connection is detected and accepted before any data has been read from the initial connection
        ByteBuffer readBuffer = ByteBuffer.allocate(socketReadBufferSize);
        
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept())
            .thenReturn(mockSocketChannel)
            // The below return simulates the new pending connection
            .thenReturn(mockSocketChannel)
            .thenReturn(null);
        // As the read method passes a reference to a ByteBuffer, and data is written to that ByteBuffer, to correctly mock this a custom Answer implementation is required.
        //   An instance of the ReadMethodAnswer class is used.
        when(mockSocketChannel.read(readBuffer))
            .thenAnswer(new ReadMethodAnswer(testMessageByteArray, testMessageByteArray.remaining()));
        
        testTcpRemoteReceiver.Connect();
        String receivedMessage = testTcpRemoteReceiver.Receive();
        
        verify(mockServerSocketChannel, times(2)).isOpen();
        verify(mockServerSocketChannel, times(2)).open();
        verify(mockServerSocketChannel, times(2)).bind(new InetSocketAddress(testPort), 1);
        verify(mockServerSocketChannel, times(2)).configureBlocking(false);
        verify(mockServerSocketChannel, times(3)).accept();
        verify(mockSocketChannel, times(2)).configureBlocking(false);
        verify(mockSocketChannel).read(any(ByteBuffer.class));
        verify(mockSocketChannel).close();
        verify(mockSocketChannel).write(ByteBuffer.wrap(new byte[] { 6 }));
        verifyNoMoreInteractions(mockServerSocketChannel);
        verifyNoMoreInteractions(mockSocketChannel);
        assertEquals("<Data>ABC</Data>", receivedMessage);
    }
    
    @Test
    public void ReceiveIOExceptionDuringInitialReadRereceiveSuccessTest() throws Exception {
        // Tests receiving a message, where an IO exception occurs during the initial read operation, afterwhich the class reconnects and receives successfully
        ByteBuffer readBuffer = ByteBuffer.allocate(socketReadBufferSize);
        
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept())
            .thenReturn(mockSocketChannel)
            .thenReturn(null)
            // The below return simulates the new pending connection
            .thenReturn(mockSocketChannel)
            .thenReturn(null);
        // In the first call to read() and IOException occurs.  In the second, the complete message is returned.
        when(mockSocketChannel.read(readBuffer))
            .thenThrow(new IOException("Mock IOException."))
            .thenAnswer(new ReadMethodAnswer(testMessageByteArray, testMessageByteArray.remaining()));
        
        testTcpRemoteReceiver.Connect();
        String receivedMessage = testTcpRemoteReceiver.Receive();
        
        verify(mockServerSocketChannel, times(2)).isOpen();
        verify(mockServerSocketChannel, times(2)).open();
        verify(mockServerSocketChannel, times(2)).bind(new InetSocketAddress(testPort), 1);
        verify(mockServerSocketChannel, times(2)).configureBlocking(false);
        verify(mockServerSocketChannel, times(4)).accept();
        verify(mockSocketChannel, times(2)).configureBlocking(false);
        verify(mockSocketChannel, times(2)).read(any(ByteBuffer.class));
        verify(mockSocketChannel).close();
        verify(mockSocketChannel).write(ByteBuffer.wrap(new byte[] { 6 }));
        verifyNoMoreInteractions(mockServerSocketChannel);
        verifyNoMoreInteractions(mockSocketChannel);
        assertEquals("<Data>ABC</Data>", receivedMessage);
    }

    @Test
    public void ReceiveIOExceptionDuringInitialReadUnhandledException() throws Exception {
        // Tests receiving a message, where an unhandled exception occurs during the initial read operation
        ByteBuffer readBuffer = ByteBuffer.allocate(socketReadBufferSize);
        
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept())
            .thenReturn(mockSocketChannel)
            .thenReturn(null);
        when(mockSocketChannel.read(readBuffer))
            .thenThrow(new java.nio.channels.AsynchronousCloseException());
        
        try {
            testTcpRemoteReceiver.Connect();
            testTcpRemoteReceiver.Receive();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockServerSocketChannel, times(1)).isOpen();
            verify(mockServerSocketChannel, times(1)).open();
            verify(mockServerSocketChannel, times(1)).bind(new InetSocketAddress(testPort), 1);
            verify(mockServerSocketChannel, times(1)).configureBlocking(false);
            verify(mockServerSocketChannel, times(2)).accept();
            verify(mockSocketChannel, times(1)).configureBlocking(false);
            verify(mockSocketChannel, times(1)).read(any(ByteBuffer.class));
            verifyNoMoreInteractions(mockServerSocketChannel);
            verifyNoMoreInteractions(mockSocketChannel);
            assertTrue(e.getMessage().contains("Error receiving message.  Unhandled exception while checking for available data."));
        }
    }
    
    @Test
    public void ReceiveIOExceptionDuringSubsequentReadRereceiveSuccessTest() throws Exception {
        // Tests receiving a message, where an IO exception occurs during a second read operation
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept())
            .thenReturn(mockSocketChannel)
            .thenReturn(null)
            .thenReturn(null)
            // The below return simulates the new pending connection
            .thenReturn(mockSocketChannel)
            .thenReturn(null);

        // Mock the 1st and 3rd calls to read(), first returning just 10 bytes of the message, and then returning the whole message (after reconnecting)
        when(mockSocketChannel.read(getByteBufferSubSet(testMessageByteArray, 0, 0, socketReadBufferSize, false)))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 0, 10, socketReadBufferSize, true), 10))
            .thenAnswer(new ReadMethodAnswer(testMessageByteArray, testMessageByteArray.remaining()));
        // Mock the 2nd call the read(), causing an exception
        when(mockSocketChannel.read(getByteBufferSubSet(testMessageByteArray, 0, 10, socketReadBufferSize, false)))
            .thenThrow(new IOException("Mock IOException."));

        testTcpRemoteReceiver.Connect();
        String receivedMessage = testTcpRemoteReceiver.Receive();
        
        verify(mockServerSocketChannel, times(2)).isOpen();
        verify(mockServerSocketChannel, times(2)).open();
        verify(mockServerSocketChannel, times(2)).bind(new InetSocketAddress(testPort), 1);
        verify(mockServerSocketChannel, times(2)).configureBlocking(false);
        verify(mockServerSocketChannel, times(6)).accept();
        verify(mockSocketChannel, times(2)).configureBlocking(false);
        verify(mockSocketChannel, times(3)).read(any(ByteBuffer.class));
        verify(mockSocketChannel).close();
        verify(mockSocketChannel).write(ByteBuffer.wrap(new byte[] { 6 }));
        verifyNoMoreInteractions(mockServerSocketChannel);
        verifyNoMoreInteractions(mockSocketChannel);
        assertEquals("<Data>ABC</Data>", receivedMessage);
    }

    @Test
    public void ReceiveReconnectAfterReadingSizeHeaderSuccessTest() throws Exception {
        // Tests receiving a message, where a new pending connection is detected and accepted after reading up to after the message size header from the initial connection
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept())
            .thenReturn(mockSocketChannel)
            .thenReturn(null)
            .thenReturn(null)
            // The below return simulates the new pending connection
            .thenReturn(mockSocketChannel)
            .thenReturn(null);
        // Mock the initial calls to read(), in both cases returning the first 10 bytes of the message
        when(mockSocketChannel.read(getByteBufferSubSet(testMessageByteArray, 0, 0, socketReadBufferSize, false)))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 0, 10, socketReadBufferSize, true), 10))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 0, 10, socketReadBufferSize, true), 10));
        // Mock the subsequent calls the read(), in the first case returning just the next 10 bytes of the message, and in the second case the entire remainder of the message
        when(mockSocketChannel.read(getByteBufferSubSet(testMessageByteArray, 0, 10, socketReadBufferSize, false)))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 10, 10, socketReadBufferSize, true), 10))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 10, 20, socketReadBufferSize, true), 20));
        
        testTcpRemoteReceiver.Connect();
        String receivedMessage = testTcpRemoteReceiver.Receive();
        
        verify(mockServerSocketChannel, times(2)).isOpen();
        verify(mockServerSocketChannel, times(2)).open();
        verify(mockServerSocketChannel, times(2)).bind(new InetSocketAddress(testPort), 1);
        verify(mockServerSocketChannel, times(2)).configureBlocking(false);
        verify(mockServerSocketChannel, times(6)).accept();
        verify(mockSocketChannel, times(2)).configureBlocking(false);
        verify(mockSocketChannel, times(4)).read(any(ByteBuffer.class));
        verify(mockSocketChannel).close();
        verify(mockSocketChannel).write(ByteBuffer.wrap(new byte[] { 6 }));
        verifyNoMoreInteractions(mockServerSocketChannel);
        verifyNoMoreInteractions(mockSocketChannel);
        assertEquals("<Data>ABC</Data>", receivedMessage);
    }

    @Test
    public void ReceiveUnhandledException() throws Exception {
        // Tests receiving a message, where an unhandled exception occurs in the method SetupAndReadMessage()
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept())
            .thenReturn(mockSocketChannel)
            .thenReturn(null);

        // Mock the 1st call to read(), returning just 10 bytes of the message
        when(mockSocketChannel.read(getByteBufferSubSet(testMessageByteArray, 0, 0, socketReadBufferSize, false)))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 0, 10, socketReadBufferSize, true), 10));
        // Mock the 2nd call the read(), causing an exception
        when(mockSocketChannel.read(getByteBufferSubSet(testMessageByteArray, 0, 10, socketReadBufferSize, false)))
            .thenThrow(new java.nio.channels.AsynchronousCloseException());

        try {
            testTcpRemoteReceiver.Connect();
            testTcpRemoteReceiver.Receive();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockServerSocketChannel, times(1)).isOpen();
            verify(mockServerSocketChannel, times(1)).open();
            verify(mockServerSocketChannel, times(1)).bind(new InetSocketAddress(testPort), 1);
            verify(mockServerSocketChannel, times(1)).configureBlocking(false);
            verify(mockServerSocketChannel, times(3)).accept();
            verify(mockSocketChannel, times(1)).configureBlocking(false);
            verify(mockSocketChannel, times(2)).read(any(ByteBuffer.class));
            verifyNoMoreInteractions(mockServerSocketChannel);
            verifyNoMoreInteractions(mockSocketChannel);
            assertTrue(e.getMessage().contains("Error receiving message.  Unhandled exception while attempting to receive and acknowledge message."));
        }
    }

    @Test
    public void ReceiveDuplicateSequenceNumberRereceiveSuccessTest() throws Exception {
        // Tests that a message received with the same sequence number as the previous message is ignored
        ByteBuffer readBuffer = ByteBuffer.allocate(socketReadBufferSize);
        byte[] secondMessageByteArray = new byte[testMessageByteArray.array().length];
        System.arraycopy(testMessageByteArray.array(), 0, secondMessageByteArray, 0, testMessageByteArray.array().length);
        // Update the sequence number in the second message
        secondMessageByteArray[1] = 124;
        // Update the body of the second message to be <Data>XYZ</Data>
        secondMessageByteArray[19] = 0x58;
        secondMessageByteArray[20] = 0x59;
        secondMessageByteArray[21] = 0x5A;
        
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept())
            .thenReturn(mockSocketChannel)
            .thenReturn(null);
        // Return 'testMessageByteArray' twice (with same sequence number), and then return 'secondMessageByteArray'
        when(mockSocketChannel.read(readBuffer))
            // ByteBuffer returned by the mockSocketChannel is altered as part of the read process, and hence can't be used twice.
            //   Hence return a copy of testMessageByteArray using method getByteBufferSubSet()
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 0, 30, socketReadBufferSize, true), 30))
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 0, 30, socketReadBufferSize, true), 30))
            .thenAnswer(new ReadMethodAnswer(ByteBuffer.wrap(secondMessageByteArray), secondMessageByteArray.length));
        
        testTcpRemoteReceiver.Connect();
        String receivedMessage = testTcpRemoteReceiver.Receive();
        String secondReceivedMessage = testTcpRemoteReceiver.Receive();
        
        verify(mockServerSocketChannel).isOpen();
        verify(mockServerSocketChannel).open();
        verify(mockServerSocketChannel).bind(new InetSocketAddress(testPort), 1);
        verify(mockServerSocketChannel).configureBlocking(false);
        verify(mockServerSocketChannel, times(7)).accept();
        verify(mockSocketChannel).configureBlocking(false);
        // Using the readBuffer member in the below verify statement causes an exception.  Hence any ByteBuffer class is specified.  Correct calling of the Read() method is checked by the above when statement in any case.
        verify(mockSocketChannel, times(3)).read(any(ByteBuffer.class));
        verify(mockSocketChannel, times(3)).write(ByteBuffer.wrap(new byte[] { 6 }));
        verifyNoMoreInteractions(mockServerSocketChannel);
        verifyNoMoreInteractions(mockSocketChannel);
        assertEquals("<Data>ABC</Data>", receivedMessage);
        assertEquals("<Data>XYZ</Data>", secondReceivedMessage);
    }
    
    /**
     * Takes a subset of the contents of a ByteBuffer, and returns it in a new ByteBuffer.
     * @param inputByteBuffer       The source ByteBuffer;
     * @param startIndex            The start index of the subset.
     * @param length                The length of the subset.
     * @param returnByteBufferSize  The allocated size of the returned ByteBuffer.
     * @param flipBuffer            Whether to flip the buffer before returning.  
     * @return                      The ByteBuffer containing the subset.
     */
    private ByteBuffer getByteBufferSubSet(ByteBuffer inputByteBuffer, int startIndex, int length, int returnByteBufferSize, boolean flipBuffer) {
        ByteBuffer returnByteBuffer = ByteBuffer.allocate(returnByteBufferSize);
        returnByteBuffer.put(Arrays.copyOfRange(inputByteBuffer.array(), startIndex, startIndex + length));
        if(flipBuffer == true) {
            returnByteBuffer.flip();
        }
        return returnByteBuffer;
    }

    /**
     * Mock answer for the SocketChannel.Read() method.
     * @author Alastair Wyse
     */
    private class ReadMethodAnswer implements Answer<Integer> {

        private ByteBuffer bytesToWrite;
        private Integer returnValue;

        /**
         * Initialises a new instance of the ReadMethodAnswer class.
         * @param bytesToWrite  The bytes to return when the Read() method is called.
         * @param returnValue   The value to return.
         */
        public ReadMethodAnswer(ByteBuffer bytesToWrite, Integer returnValue) {
            this.bytesToWrite = bytesToWrite;
            this.returnValue = returnValue;
        }
        
        @Override
        public Integer answer(InvocationOnMock invocation) throws Throwable {
            ByteBuffer returnByteBuffer = (ByteBuffer)invocation.getArguments()[0];
            returnByteBuffer.put(bytesToWrite);
            return returnValue;
        }
    }
}

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
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import static org.mockito.Mockito.*;
import org.mockito.invocation.InvocationOnMock;
import org.mockito.stubbing.*;
import net.alastairwyse.operatingsystemabstraction.*;
import net.alastairwyse.applicationlogging.*;
import net.alastairwyse.applicationmetrics.*;
import net.alastairwyse.methodinvocationremoting.*;

/**
 * Unit tests for the logging functionality in class methodinvocationremoting.TcpRemoteReceiver.
 * @author Alastair Wyse
 */
public class TcpRemoteReceiverLoggingTests {
    
    private IServerSocketChannel mockServerSocketChannel;
    private ISocketChannel mockSocketChannel;
    private IApplicationLogger mockApplicationLogger;
    private TcpRemoteReceiver testTcpRemoteReceiver;
    private int testPort = 55000;
    private int socketReadBufferSize = 1024;
    private final String stringEncodingCharset = "UTF-8";
    
    @Before
    public void setUp() throws Exception {
        mockServerSocketChannel = mock(IServerSocketChannel.class);
        mockSocketChannel = mock(ISocketChannel.class);
        mockApplicationLogger = mock(IApplicationLogger.class);
        testTcpRemoteReceiver = new TcpRemoteReceiver(testPort, 3, 10, 20, socketReadBufferSize, mockApplicationLogger, new NullMetricLogger(), mockServerSocketChannel);
    }
    
    @Test
    public void ConnectLoggingTest() throws Exception {
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept()).thenReturn(mockSocketChannel);
        
        testTcpRemoteReceiver.Connect();
        
        verify(mockApplicationLogger).Log(testTcpRemoteReceiver, LogLevel.Information, "Connection received on port " + testPort + ".");
        verifyNoMoreInteractions(mockApplicationLogger);
    }
    
    @Test
    public void DisconnectLoggingTest() throws Exception {
        testTcpRemoteReceiver.Disconnect();
        
        verify(mockApplicationLogger).Log(testTcpRemoteReceiver, LogLevel.Information, "Disconnected.");
        verifyNoMoreInteractions(mockApplicationLogger);
    }
    
    @Test
    public void ReceiveLoggingTest() throws Exception {
        ByteBuffer readBuffer = ByteBuffer.allocate(socketReadBufferSize);
        
        // Setup test messages
        byte[] testMessageBody = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><Data>abc</Data></Parameter><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter /><Parameter><DataType>double</DataType><Data>4.5678899999999999e+002</Data></Parameter></Parameters><ReturnType /></MethodInvocation>".getBytes(stringEncodingCharset);
        byte[] testMessageSequenceNumber = ByteBuffer.allocate(4).order(ByteOrder.LITTLE_ENDIAN).putInt(123).array();
        byte[] testMessageSizeHeader = ByteBuffer.allocate(8).order(ByteOrder.LITTLE_ENDIAN).putLong(testMessageBody.length).array();
        ByteBuffer testMessageByteArray;
        testMessageByteArray = ByteBuffer.allocate(testMessageSequenceNumber.length + testMessageSizeHeader.length + testMessageBody.length + 2);
        testMessageByteArray.put((byte)0x02);  // Set the start delimiter
        testMessageByteArray.put(testMessageSequenceNumber);
        testMessageByteArray.put(testMessageSizeHeader);
        testMessageByteArray.put(testMessageBody);
        testMessageByteArray.put((byte)0x03);  // Set the end delimiter
        testMessageByteArray.flip();
        
        byte[] testSmallMessageBody = "<TestMessage>Test message content</TestMessage>".getBytes(stringEncodingCharset);
        byte[] testSmallMessageSequenceNumber = ByteBuffer.allocate(4).order(ByteOrder.LITTLE_ENDIAN).putInt(124).array();
        byte[] testSmallMessageSizeHeader = ByteBuffer.allocate(8).order(ByteOrder.LITTLE_ENDIAN).putLong(testSmallMessageBody.length).array();
        ByteBuffer testSmallMessageByteArray;
        testSmallMessageByteArray = ByteBuffer.allocate(testSmallMessageSequenceNumber.length + testSmallMessageSizeHeader.length + testSmallMessageBody.length + 2);
        testSmallMessageByteArray.put((byte)0x02);  // Set the start delimiter
        testSmallMessageByteArray.put(testSmallMessageSequenceNumber);
        testSmallMessageByteArray.put(testSmallMessageSizeHeader);
        testSmallMessageByteArray.put(testSmallMessageBody);
        testSmallMessageByteArray.put((byte)0x03);  // Set the end delimiter
        testSmallMessageByteArray.flip();
        
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept())
            .thenReturn(mockSocketChannel)
            .thenReturn(null)
            .thenReturn(mockSocketChannel)
            .thenReturn(null);
        // As the read method passes a reference to a ByteBuffer, and data is written to that ByteBuffer, to correctly mock this a custom Answer implementation is required.
        //   An instance of the ReadMethodAnswer class is used.
        when(mockSocketChannel.read(readBuffer))
            .thenAnswer(new ReadMethodAnswer(testMessageByteArray, testMessageByteArray.remaining()))
            .thenAnswer(new ReadMethodAnswer(testSmallMessageByteArray, testSmallMessageByteArray.remaining()));
        
        testTcpRemoteReceiver.Connect();
        testTcpRemoteReceiver.Receive();
        testTcpRemoteReceiver.Receive();

        verify(mockApplicationLogger).Log(testTcpRemoteReceiver, LogLevel.Information, "Received message '<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataT' (truncated).");
        verify(mockApplicationLogger).Log(testTcpRemoteReceiver, LogLevel.Debug, "Complete message content: '<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><Data>abc</Data></Parameter><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter /><Parameter><DataType>double</DataType><Data>4.5678899999999999e+002</Data></Parameter></Parameters><ReturnType /></MethodInvocation>'.");
        verify(mockApplicationLogger).Log(testTcpRemoteReceiver, LogLevel.Information, "Received message '<TestMessage>Test message content</TestMessage>'.");
        verify(mockApplicationLogger).Log(testTcpRemoteReceiver, LogLevel.Debug, "Complete message content: '<TestMessage>Test message content</TestMessage>'.");
    }
    
    @Test
    public void CancelReceiveLoggingTests() throws Exception {
        testTcpRemoteReceiver.CancelReceive();
        
        verify(mockApplicationLogger).Log(testTcpRemoteReceiver, LogLevel.Information, "Receive operation cancelled.");
        verifyNoMoreInteractions(mockApplicationLogger);
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

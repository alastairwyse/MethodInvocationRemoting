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

package net.alastairwyse.methodinvocationremotingmetricstests;

import org.junit.Before;
import org.junit.Test;

import java.io.*;
import java.nio.*;
import java.util.*;

import static org.mockito.Mockito.*;

import org.mockito.invocation.InvocationOnMock;
import org.mockito.stubbing.*;

import net.alastairwyse.operatingsystemabstraction.*;
import net.alastairwyse.applicationlogging.*;
import net.alastairwyse.applicationmetrics.*;
import net.alastairwyse.methodinvocationremoting.*;
import net.alastairwyse.methodinvocationremotingmetrics.*;

/**
 * Unit tests for the metrics logging functionality in class methodinvocationremoting.TcpRemoteReceiver.
 * @author Alastair Wyse
 */
public class TcpRemoteReceiverMetricsTests {

    private IServerSocketChannel mockServerSocketChannel;
    private ISocketChannel mockSocketChannel;
    private IMetricLogger mockMetricLogger;
    private TcpRemoteReceiver testTcpRemoteReceiver;
    private int testPort = 55000;
    private int socketReadBufferSize = 1024;
    private ByteBuffer testMessageByteArray;
    private final String stringEncodingCharset = "UTF-8";
    
    @Before
    public void setUp() throws Exception {
        mockServerSocketChannel = mock(IServerSocketChannel.class);
        mockSocketChannel = mock(ISocketChannel.class);
        mockMetricLogger = mock(IMetricLogger.class);
        testTcpRemoteReceiver = new TcpRemoteReceiver(testPort, 3, 10, 20, socketReadBufferSize, new ConsoleApplicationLogger(LogLevel.Critical, '|', "  "), mockMetricLogger, mockServerSocketChannel);
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
    public void ReceiveMetricsTest() throws Exception {
        ByteBuffer readBuffer = ByteBuffer.allocate(socketReadBufferSize);
        
        // Setup test message
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
        
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept())
            .thenReturn(mockSocketChannel)
            .thenReturn(null);
        // As the read method passes a reference to a ByteBuffer, and data is written to that ByteBuffer, to correctly mock this a custom Answer implementation is required.
        //   An instance of the ReadMethodAnswer class is used.
        when(mockSocketChannel.read(readBuffer))
            .thenAnswer(new ReadMethodAnswer(testMessageByteArray, testMessageByteArray.remaining()));
        
        testTcpRemoteReceiver.Connect();
        testTcpRemoteReceiver.Receive();

        verify(mockMetricLogger).Begin(isA(MessageReceiveTime.class));
        verify(mockMetricLogger).End(isA(MessageReceiveTime.class));
        verify(mockMetricLogger).Increment(isA(MessageReceived.class));
        verify(mockMetricLogger).Add((argThat(new IsAmountMetric(new ReceivedMessageSize(381)))));
        verifyNoMoreInteractions(mockMetricLogger);
    }

    @Test
    public void ReceiveReconnectMetricsTest() throws Exception {
        // Tests receiving a message, where a new pending connection is detected and accepted after reading up to after the message size header from the initial connection
        //   Ensures the correct order of metric logging in this case, especially that a corresponding End() is called for each Begin()\
        
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
        testTcpRemoteReceiver.Receive();
        
        verify(mockMetricLogger, times(2)).Begin(isA(MessageReceiveTime.class));
        verify(mockMetricLogger, times(2)).End(isA(MessageReceiveTime.class));
        verify(mockMetricLogger).Increment(isA(TcpRemoteReceiverReconnected.class));
        verify(mockMetricLogger).Increment(isA(MessageReceived.class));
        verify(mockMetricLogger).Add((argThat(new IsAmountMetric(new ReceivedMessageSize(16)))));
        verifyNoMoreInteractions(mockMetricLogger);
    }
    
    @Test
    public void ReceiveDuplicateSequenceNumberMetricsTest() throws Exception {
        // Tests that a message received with the same sequence number as the previous message is ignored
        //   Ensures the correct order of metric logging in this case, especially that a corresponding End() is called for each Begin()
        
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
        testTcpRemoteReceiver.Receive();
        testTcpRemoteReceiver.Receive();
        
        verify(mockMetricLogger, times(3)).Begin(isA(MessageReceiveTime.class));
        verify(mockMetricLogger).Increment(isA(TcpRemoteReceiverDuplicateSequenceNumber.class));
        verify(mockMetricLogger, times(3)).End(isA(MessageReceiveTime.class));
        verify(mockMetricLogger, times(2)).Increment(isA(MessageReceived.class));
        verify(mockMetricLogger, times(2)).Add((argThat(new IsAmountMetric(new ReceivedMessageSize(16)))));
        verifyNoMoreInteractions(mockMetricLogger);
    }
    
    @Test
    public void ReceiveExceptionMetricsTest() throws Exception {
        // Tests receiving a message, where an IO exception occurs causing reconnect and re-receive
        //   Ensures the correct order of metric logging in this case, especially that a corresponding End() is called for each Begin()
        
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
        testTcpRemoteReceiver.Receive();
        
        verify(mockMetricLogger).Begin(isA(MessageReceiveTime.class));
        verify(mockMetricLogger).End(isA(MessageReceiveTime.class));
        verify(mockMetricLogger).Increment(isA(TcpRemoteReceiverReconnected.class));
        verify(mockMetricLogger).Increment(isA(MessageReceived.class));
        verify(mockMetricLogger).Add((argThat(new IsAmountMetric(new ReceivedMessageSize(16)))));
        verifyNoMoreInteractions(mockMetricLogger);
    }
    
    @Test
    public void ReceiveAvailableDataExceptionMetricsTest() throws Exception {
        // Tests receiving a message, where an IO exception occurs during the initial read operation, afterwhich the class reconnects and receives successfully
        //   Ensures the correct order of metric logging in this case, especially that a corresponding End() is called for each Begin()
        
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
        testTcpRemoteReceiver.Receive();
        
        verify(mockMetricLogger).Begin(isA(MessageReceiveTime.class));
        verify(mockMetricLogger).End(isA(MessageReceiveTime.class));
        verify(mockMetricLogger).Increment(isA(TcpRemoteReceiverReconnected.class));
        verify(mockMetricLogger).Increment(isA(MessageReceived.class));
        verify(mockMetricLogger).Add((argThat(new IsAmountMetric(new ReceivedMessageSize(16)))));
        verifyNoMoreInteractions(mockMetricLogger);
    }
    
    @Test
    public void ReceiveSmallBufferMetricsTest() throws Exception {
        // Tests receiving a message where the buffer size is 1 byte, and logging of the metric 'TcpRemoteReceiverReadBufferCreated'
        
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

        testTcpRemoteReceiver = new TcpRemoteReceiver(testPort, 3, 10, 20, 1, new ConsoleApplicationLogger(LogLevel.Warning, '|', "  "), mockMetricLogger, mockServerSocketChannel);
        testTcpRemoteReceiver.Connect();
        testTcpRemoteReceiver.Receive();
        
        verify(mockMetricLogger).Begin(isA(MessageReceiveTime.class));
        // Expect the 'TcpRemoteReceiverReadBufferCreated' metric event to be logged 20 times... once after parsing each byte of the message into the single byte buffer
        //   Note that a new buffer is also created after parsing the last byte of the message, even though this buffer is not subsequently read into
        verify(mockMetricLogger, times(20)).Increment(isA(TcpRemoteReceiverReadBufferCreated.class));
        verify(mockMetricLogger).End(isA(MessageReceiveTime.class));
        verify(mockMetricLogger).Increment(isA(MessageReceived.class));
        verify(mockMetricLogger).Add((argThat(new IsAmountMetric(new ReceivedMessageSize(6)))));
        verifyNoMoreInteractions(mockMetricLogger);
    }
    
    @Test
    public void ReceiveAfterNoDataAvailableMetricsTest() throws Exception {
        // Tests where the loop in the Receive() method iterates through once with no data being available before the message is received
        //   Ensures the correct order of metric logging in this case, especially that a corresponding End() is called for each Begin()
        
        ByteBuffer readBuffer = ByteBuffer.allocate(socketReadBufferSize);
        
        // Setup test message
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
        
        when(mockServerSocketChannel.isOpen()).thenReturn(false);
        when(mockServerSocketChannel.accept())
            .thenReturn(mockSocketChannel)
            .thenReturn(null);
        // As the read method passes a reference to a ByteBuffer, and data is written to that ByteBuffer, to correctly mock this a custom Answer implementation is required.
        //   An instance of the ReadMethodAnswer class is used.
        when(mockSocketChannel.read(readBuffer))
            // On the first time through the Receive() method loop return 0 bytes and an empty ByteBuffer...
            .thenAnswer(new ReadMethodAnswer(getByteBufferSubSet(testMessageByteArray, 0, 0, 0, true), 0))
            // ...then on the next iteration return the bytes of the message
            .thenAnswer(new ReadMethodAnswer(testMessageByteArray, testMessageByteArray.remaining()));
        
        testTcpRemoteReceiver.Connect();
        testTcpRemoteReceiver.Receive();

        verify(mockMetricLogger).Begin(isA(MessageReceiveTime.class));
        verify(mockMetricLogger).End(isA(MessageReceiveTime.class));
        verify(mockMetricLogger).Increment(isA(MessageReceived.class));
        verify(mockMetricLogger).Add((argThat(new IsAmountMetric(new ReceivedMessageSize(381)))));
        verifyNoMoreInteractions(mockMetricLogger);
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

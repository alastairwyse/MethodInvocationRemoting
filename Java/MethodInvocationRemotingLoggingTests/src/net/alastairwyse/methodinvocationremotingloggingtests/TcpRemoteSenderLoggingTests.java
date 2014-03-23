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
import java.nio.*;
import static org.mockito.Mockito.*;
import org.mockito.invocation.InvocationOnMock;
import org.mockito.stubbing.*;
import net.alastairwyse.methodinvocationremoting.*;
import net.alastairwyse.operatingsystemabstraction.*;
import net.alastairwyse.applicationlogging.*;

/**
 * Unit tests for the logging functionality in class methodinvocationremoting.TcpRemoteSender.
 * @author Alastair Wyse
 */
public class TcpRemoteSenderLoggingTests {

    private ISocketChannel mockSocketChannel;
    private IApplicationLogger mockApplicationLogger;
    private TcpRemoteSender testTcpRemoteSender;
    private String testIpAddress = "127.0.0.1";
    private int testPort = 55000;
    
    @Before
    public void setUp() throws Exception {
        mockSocketChannel = mock(ISocketChannel.class);
        mockApplicationLogger = mock(IApplicationLogger.class);
        testTcpRemoteSender = new TcpRemoteSender(testIpAddress, testPort, 3, 10, 25, 10, mockApplicationLogger, mockSocketChannel);
    }
    
    @Test
    public void ConnectLoggingTest() throws Exception {
        when(mockSocketChannel.isConnected())
            .thenReturn(false)
            .thenReturn(true);
        
        testTcpRemoteSender.Connect();
        
        verify(mockApplicationLogger).Log(testTcpRemoteSender, LogLevel.Information, "Connected to /" + testIpAddress.toString() + ":" + testPort + ".");
        verifyNoMoreInteractions(mockApplicationLogger);
    }
    
    @Test
    public void DisconnectLoggingTest() throws Exception {
        when(mockSocketChannel.isConnected()).thenReturn(true);
        
        testTcpRemoteSender.Disconnect();
        
        verify(mockApplicationLogger).Log(testTcpRemoteSender, LogLevel.Information, "Disconnected.");
        verifyNoMoreInteractions(mockApplicationLogger);
    }
    
    @Test
    public void SendLoggingTest() throws Exception {
        ByteBuffer acknowledgementBuffer = ByteBuffer.allocate(1);
        
        when(mockSocketChannel.isConnected()).thenReturn(true);
        // As the read method passes a reference to a ByteBuffer, and data is written to that ByteBuffer, to correctly mock this a custom Answer implementation is required.
        //   An instance of the AcknowledgementAnswer class is used.
        when(mockSocketChannel.read(acknowledgementBuffer)).thenAnswer(new AcknowledgementAnswer((byte)0x06, 1));
        
        testTcpRemoteSender.Send("<TestMessage>Test message content</TestMessage>");
        
        verify(mockApplicationLogger).Log(testTcpRemoteSender, LogLevel.Information, "Message sent and acknowledged.");
        verifyNoMoreInteractions(mockApplicationLogger);
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

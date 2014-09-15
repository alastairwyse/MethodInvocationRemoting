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
import static org.mockito.Mockito.*;
import net.alastairwyse.operatingsystemabstraction.*;
import net.alastairwyse.applicationlogging.*;
import net.alastairwyse.applicationmetrics.*;
import net.alastairwyse.methodinvocationremoting.*;

/**
 * Unit tests for the logging functionality in class methodinvocationremoting.FileRemoteReceiver.
 * @author Alastair Wyse
 */
public class FileRemoteReceiverLoggingTests {
    
    private IFile mockMessageFile;
    private IFileSystem mockFileSystem;
    private IApplicationLogger mockApplicationLogger;
    private FileRemoteReceiver testFileRemoteReceiver;
    private String messageFilePath = "C:\\Temp\\TestFilePath.txt";
    private String lockFilePath = "C:\\Temp\\TestFilePath.lck";
    
    @Before
    public void setUp() throws Exception {
        mockMessageFile = mock(IFile.class);
        mockFileSystem = mock(IFileSystem.class);
        mockApplicationLogger = mock(IApplicationLogger.class);
        testFileRemoteReceiver = new FileRemoteReceiver(messageFilePath, lockFilePath, 1000, mockApplicationLogger, new NullMetricLogger(), mockMessageFile, mockFileSystem);
    }
    
    @Test
    public void ReceiveLoggingTest() throws Exception {
        String receivedMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><Data>abc</Data></Parameter><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter /><Parameter><DataType>double</DataType><Data>4.5678899999999999e+002</Data></Parameter></Parameters><ReturnType /></MethodInvocation>";
        String smallMessage = "<TestMessage>Test message content</TestMessage>";
        
        when(mockFileSystem.CheckFileExists(messageFilePath)).thenReturn(true);
        when(mockFileSystem.CheckFileExists(lockFilePath)).thenReturn(false);
        when(mockMessageFile.ReadAll())
            .thenReturn(receivedMessage)
            .thenReturn(smallMessage);
        
        testFileRemoteReceiver.Receive();
        testFileRemoteReceiver.Receive();
        
        verify(mockApplicationLogger).Log(testFileRemoteReceiver, LogLevel.Information, "Received message '<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataT' (truncated).");
        verify(mockApplicationLogger).Log(testFileRemoteReceiver, LogLevel.Debug, "Complete message content: '" + receivedMessage + "'.");
        verify(mockApplicationLogger).Log(testFileRemoteReceiver, LogLevel.Information, "Received message '" + smallMessage + "'.");
        verify(mockApplicationLogger).Log(testFileRemoteReceiver, LogLevel.Debug, "Complete message content: '" + smallMessage + "'.");
        verifyNoMoreInteractions(mockApplicationLogger);
    }
    
    @Test
    public void CancelReceiveLoggingTest() throws Exception {
        testFileRemoteReceiver.CancelReceive();
        
        verify(mockApplicationLogger).Log(testFileRemoteReceiver, LogLevel.Information, "Receive operation cancelled.");
        verifyNoMoreInteractions(mockApplicationLogger);
    }
}

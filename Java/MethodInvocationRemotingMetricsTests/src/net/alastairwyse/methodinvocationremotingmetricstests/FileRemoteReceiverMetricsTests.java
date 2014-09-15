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
import static org.mockito.Mockito.*;
import net.alastairwyse.operatingsystemabstraction.*;
import net.alastairwyse.applicationlogging.*;
import net.alastairwyse.applicationmetrics.*;
import net.alastairwyse.methodinvocationremoting.*;
import net.alastairwyse.methodinvocationremotingmetrics.*;

/**
 * Unit tests for the metrics logging functionality in class methodinvocationremoting.FileRemoteReceiver.
 * @author Alastair Wyse
 */
public class FileRemoteReceiverMetricsTests {

    private IFile mockMessageFile;
    private IFileSystem mockFileSystem;
    private IMetricLogger mockMetricLogger;
    private FileRemoteReceiver testFileRemoteReceiver;
    private String messageFilePath = "C:\\Temp\\TestFilePath.txt";
    private String lockFilePath = "C:\\Temp\\TestFilePath.lck";
    
    @Before
    public void setUp() throws Exception {
        mockMessageFile = mock(IFile.class);
        mockFileSystem = mock(IFileSystem.class);
        mockMetricLogger = mock(IMetricLogger.class);
        testFileRemoteReceiver = new FileRemoteReceiver(messageFilePath, lockFilePath, 1000, new ConsoleApplicationLogger(LogLevel.Critical, '|', "  "), mockMetricLogger, mockMessageFile, mockFileSystem);
    }
    
    @Test
    public void ReceiveMetricsTest() throws Exception {
        String receivedMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><Data>abc</Data></Parameter><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter /><Parameter><DataType>double</DataType><Data>4.5678899999999999e+002</Data></Parameter></Parameters><ReturnType /></MethodInvocation>";
        
        when(mockFileSystem.CheckFileExists(messageFilePath)).thenReturn(true);
        when(mockFileSystem.CheckFileExists(lockFilePath)).thenReturn(false);
        when(mockMessageFile.ReadAll())
            .thenReturn(receivedMessage);
        
        testFileRemoteReceiver.Receive();
        
        verify(mockMetricLogger).Begin(isA(MessageReceiveTime.class));
        verify(mockMetricLogger).End(isA(MessageReceiveTime.class));
        verify(mockMetricLogger).Increment(isA(MessageReceived.class));
        verify(mockMetricLogger).Add((argThat(new IsAmountMetric(new ReceivedMessageSize(381)))));
        verifyNoMoreInteractions(mockMetricLogger);
    }
}

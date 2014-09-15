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
 * Unit tests for the metrics logging functionality in class methodinvocationremoting.FileRemoteSender.
 * @author Alastair Wyse
 */
public class FileRemoteSenderMetricsTests {

    private IFile mockMessageFile;
    private IFile mockLockFile;
    private IFileSystem mockFileSystem;
    private IMetricLogger mockMetricLogger;
    private FileRemoteSender testFileRemoteSender;
    
    @Before
    public void setUp() throws Exception {
        mockMessageFile = mock(IFile.class);
        mockLockFile = mock(IFile.class);
        mockFileSystem = mock(IFileSystem.class);
        mockMetricLogger = mock(IMetricLogger.class);
        testFileRemoteSender = new FileRemoteSender("C:\\Temp\\TestFilePath.txt", "C:\\Temp\\TestFilePath.lck", new ConsoleApplicationLogger(LogLevel.Critical, '|', "  "), mockMetricLogger, mockMessageFile, mockLockFile, mockFileSystem);
    }
    
    @Test
    public void SendMetricsTest() throws Exception {
        testFileRemoteSender.Send("<TestMessage>Test message content</TestMessage>");
        
        verify(mockMetricLogger).Begin(isA(MessageSendTime.class));
        verify(mockMetricLogger).End(isA(MessageSendTime.class));
        verify(mockMetricLogger).Increment(isA(MessageSent.class));
        verifyNoMoreInteractions(mockMetricLogger);
    }
}

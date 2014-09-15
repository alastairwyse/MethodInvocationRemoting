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
 * Unit tests for the logging functionality in class methodinvocationremoting.FileRemoteSender.
 * @author Alastair Wyse
 */
public class FileRemoteSenderLoggingTests {
    
    private IFile mockMessageFile;
    private IFile mockLockFile;
    private IFileSystem mockFileSystem;
    private IApplicationLogger mockApplicationLogger;
    private FileRemoteSender testFileRemoteSender;
    
    @Before
    public void setUp() throws Exception {
        mockMessageFile = mock(IFile.class);
        mockLockFile = mock(IFile.class);
        mockFileSystem = mock(IFileSystem.class);
        mockApplicationLogger = mock(IApplicationLogger.class);
        testFileRemoteSender = new FileRemoteSender("C:\\Temp\\TestFilePath.txt", "C:\\Temp\\TestFilePath.lck", mockApplicationLogger, new NullMetricLogger(), mockMessageFile, mockLockFile, mockFileSystem);
    }
    
    @Test
    public void SendLoggingTest() throws Exception {
        testFileRemoteSender.Send("<TestMessage>Test message content</TestMessage>");
        
        verify(mockApplicationLogger).Log(testFileRemoteSender, LogLevel.Information, "Message sent.");
        verifyNoMoreInteractions(mockApplicationLogger);
    }
}

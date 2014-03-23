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

package net.alastairwyse.applicationloggingunittests;

import org.junit.Before;
import org.junit.Test;
import static org.mockito.Mockito.*;
import net.alastairwyse.applicationlogging.*;
import net.alastairwyse.operatingsystemabstraction.*;

/**
 * Unit tests for class applicationlogging.FileApplicationLogger.
 * @author Alastair Wyse
 */
public class FileApplicationLoggerTests {
    private IFileWriter mockFileWriter;
    private FileApplicationLogger testFileApplicationLogger;
    
    @Before
    public void setUp() {
        mockFileWriter = mock(IFileWriter.class);
        testFileApplicationLogger = new FileApplicationLogger(LogLevel.Debug, ':', "  ", mockFileWriter);
    }
    
    @Test
    public void LogSuccessTests() throws Exception {
        String expectedStackTraceText = System.lineSeparator() + "  java.lang.Exception: Error connecting to /127.0.0.1:50000." + System.lineSeparator() + "  \tat net.alastairwyse.methodinvocationremoting.TcpRemoteSender.AttemptConnect(TcpRemoteSender.java:187)" + System.lineSeparator() + "  \tat net.alastairwyse.methodinvocationremoting.TcpRemoteSender.Connect(TcpRemoteSender.java:136)" + System.lineSeparator() + "  \tat net.alastairwyse.applicationloggingunittests.FileApplicationLoggerTests.LogSuccessTests(Test.java:135)" + System.lineSeparator();
        
        testFileApplicationLogger.Log(LogLevel.Critical, "Log Text 1.");
        testFileApplicationLogger.Log(123, LogLevel.Error, "Log Text 2.");
        testFileApplicationLogger.Log(LogLevel.Warning, "Log Text 3.", new CustomStackTraceException("Error connecting to /127.0.0.1:50000."));
        testFileApplicationLogger.Log(124, LogLevel.Information, "Log Text 4.", new CustomStackTraceException("Error connecting to /127.0.0.1:50000."));
        testFileApplicationLogger.Log(this, LogLevel.Critical, "Log Text 5.");
        testFileApplicationLogger.Log(this, 123, LogLevel.Error, "Log Text 6.");
        testFileApplicationLogger.Log(this, LogLevel.Warning, "Log Text 7.", new CustomStackTraceException("Error connecting to /127.0.0.1:50000."));
        testFileApplicationLogger.Log(this, 124, LogLevel.Information, "Log Text 8.", new CustomStackTraceException("Error connecting to /127.0.0.1:50000."));
        
        verify(mockFileWriter).write(contains(" CRITICAL : Log Text 1."));
        verify(mockFileWriter).write(contains(" Log Event Id = 123 : ERROR : Log Text 2."));
        verify(mockFileWriter).write(contains(" WARNING : Log Text 3." + expectedStackTraceText));
        verify(mockFileWriter).write(contains(" Log Event Id = 124 : Log Text 4." + expectedStackTraceText));
        verify(mockFileWriter).write(contains(" Source = FileApplicationLoggerTests : CRITICAL : Log Text 5."));
        verify(mockFileWriter).write(contains(" Source = FileApplicationLoggerTests : Log Event Id = 123 : ERROR : Log Text 6."));
        verify(mockFileWriter).write(contains(" Source = FileApplicationLoggerTests : WARNING : Log Text 7." + expectedStackTraceText));
        verify(mockFileWriter).write(contains(" Source = FileApplicationLoggerTests : Log Event Id = 124 : Log Text 8." + expectedStackTraceText));
        
        verify(mockFileWriter, times(8)).write(System.lineSeparator());
        verify(mockFileWriter, times(8)).flush();
        verifyNoMoreInteractions(mockFileWriter);
    }
    
    @Test
    public void LogBelowMinimumLogLevelNotLogged() throws Exception {
        testFileApplicationLogger = new FileApplicationLogger(LogLevel.Warning, ':', "  ", mockFileWriter);
        
        testFileApplicationLogger.Log(LogLevel.Information, "Log Text 1.");
        testFileApplicationLogger.Log(123, LogLevel.Information, "Log Text 2.");
        testFileApplicationLogger.Log(LogLevel.Information, "Log Text 3.", new Exception("Mocked Exception."));
        testFileApplicationLogger.Log(124, LogLevel.Information, "Log Text 4.", new Exception("Mocked Exception 2."));
        testFileApplicationLogger.Log(LogLevel.Debug, "Log Text 1.");
        testFileApplicationLogger.Log(123, LogLevel.Debug, "Log Text 2.");
        testFileApplicationLogger.Log(LogLevel.Debug, "Log Text 3.", new Exception("Mocked Exception."));
        testFileApplicationLogger.Log(124, LogLevel.Debug, "Log Text 4.", new Exception("Mocked Exception 2."));
        testFileApplicationLogger.Log(this, LogLevel.Information, "Log Text 5.");
        testFileApplicationLogger.Log(this, 123, LogLevel.Information, "Log Text 6.");
        testFileApplicationLogger.Log(this, LogLevel.Information, "Log Text 7.", new Exception("Mocked Exception 3."));
        testFileApplicationLogger.Log(this, 124, LogLevel.Information, "Log Text 8.", new Exception("Mocked Exception 4."));
        testFileApplicationLogger.Log(this, LogLevel.Debug, "Log Text 5.");
        testFileApplicationLogger.Log(this, 123, LogLevel.Debug, "Log Text 6.");
        testFileApplicationLogger.Log(this, LogLevel.Debug, "Log Text 7.", new Exception("Mocked Exception 3."));
        testFileApplicationLogger.Log(this, 124, LogLevel.Debug, "Log Text 8.", new Exception("Mocked Exception 4."));
        
        verifyZeroInteractions(mockFileWriter);
    }
    
    @Test
    public void CloseSuccessTest() throws Exception {
        testFileApplicationLogger.Close();
        
        verify(mockFileWriter).close();
        verifyNoMoreInteractions(mockFileWriter);
    }
}

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
import java.io.IOException;
import static org.mockito.Mockito.*;
import net.alastairwyse.methodinvocationremoting.*;
import net.alastairwyse.operatingsystemabstraction.*;

/**
 * Unit tests for class methodinvocationremoting.FileRemoteSender.
 * @author Alastair Wyse
 */
public class FileRemoteSenderTests {

    private IFile mockMessageFile;
    private IFile mockLockFile;
    private IFileSystem mockFileSystem;
    private FileRemoteSender testFileRemoteSender;
	
    private final String messageFilePath = "C:\\Temp\\TestFilePath.txt";
    private final String lockFilePath = "C:\\Temp\\TestFilePath.lck";
    private final String testMessage = "<TestMessage>Test message content</TestMessage>";
    
    @Before
    public void setUp() throws Exception {
    	mockMessageFile = mock(IFile.class);
    	mockLockFile = mock(IFile.class);
    	mockFileSystem = mock(IFileSystem.class);
    	testFileRemoteSender = new FileRemoteSender(messageFilePath, lockFilePath, mockMessageFile, mockLockFile, mockFileSystem);
    }
    
    @Test
    public void SendException() throws Exception {
    	doThrow(new IOException("Mock Write Failure")).when(mockMessageFile).WriteAll(testMessage);
    	
    	try {
    		testFileRemoteSender.Send(testMessage);
    		fail("Exception was not thrown.");
    	}
    	catch (Exception e) {
    		verify(mockLockFile).WriteAll("");
    		verify(mockMessageFile).WriteAll(testMessage);
        	verifyNoMoreInteractions(mockLockFile);
        	verifyNoMoreInteractions(mockMessageFile);
    		verifyZeroInteractions(mockFileSystem);
    		assertTrue(e.getMessage().contains("Error sending message."));
    	}
    }
    
    @Test
    public void SendSuccessTest() throws Exception {
    	testFileRemoteSender.Send(testMessage);

    	verify(mockLockFile).WriteAll("");
    	verify(mockMessageFile).WriteAll(testMessage);
    	verify(mockFileSystem).DeleteFile(lockFilePath);
    	verifyNoMoreInteractions(mockLockFile);
    	verifyNoMoreInteractions(mockMessageFile);
    	verifyNoMoreInteractions(mockFileSystem);
    }
}

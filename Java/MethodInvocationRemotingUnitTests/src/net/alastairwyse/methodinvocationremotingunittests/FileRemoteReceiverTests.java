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
import net.alastairwyse.applicationlogging.*;

/**
 * Unit tests for class methodinvocationremoting.FileRemoteReceiver.
 * @author Alastair Wyse
 */
public class FileRemoteReceiverTests {

    private IFile mockMessageFile;
    private IFileSystem mockFileSystem;
    private FileRemoteReceiver testFileRemoteReceiver;
    
    private final String messageFilePath = "C:\\Temp\\TestFilePath.txt";
    private final String lockFilePath = "C:\\Temp\\TestFilePath.lck";
    private final String testMessage = "<TestMessage>Test message content</TestMessage>";
    
    @Before
    public void setUp() throws Exception {
    	mockMessageFile = mock(IFile.class);
    	mockFileSystem = mock(IFileSystem.class);
    	testFileRemoteReceiver = new FileRemoteReceiver(messageFilePath, lockFilePath, 1000, new ConsoleApplicationLogger(LogLevel.Warning, '|', "  "), mockMessageFile, mockFileSystem);
    }
    
    @Test
    public void ReceiveException() throws Exception {
    	when(mockFileSystem.CheckFileExists(messageFilePath)).thenReturn(true);
    	when(mockFileSystem.CheckFileExists(lockFilePath)).thenReturn(false);
        doThrow(new IOException("Mock Read Failure")).when(mockMessageFile).ReadAll();
        
    	try {
    		testFileRemoteReceiver.Receive();
    		fail("Exception was not thrown.");
    	}
    	catch (Exception e) {
    		verify(mockFileSystem).CheckFileExists(messageFilePath);
    		verify(mockFileSystem).CheckFileExists(lockFilePath);
    		verify(mockMessageFile).ReadAll();
    		verifyNoMoreInteractions(mockFileSystem);
    		verifyNoMoreInteractions(mockMessageFile);
    		assertTrue(e.getMessage().contains("Error receiving message."));
    		assertTrue(e.getCause().getMessage().contains("Mock Read Failure"));
    	}
    }
	
    @Test
    public void ReceiveSuccessTests() throws Exception {
    	when(mockFileSystem.CheckFileExists(messageFilePath)).thenReturn(true);
    	when(mockFileSystem.CheckFileExists(lockFilePath)).thenReturn(false);
    	when(mockMessageFile.ReadAll()).thenReturn(testMessage);

    	String receivedMessage = testFileRemoteReceiver.Receive();

		verify(mockFileSystem).CheckFileExists(messageFilePath);
		verify(mockFileSystem).CheckFileExists(lockFilePath);
		verify(mockMessageFile).ReadAll();
		verify(mockFileSystem).DeleteFile(messageFilePath);
		verifyNoMoreInteractions(mockFileSystem);
		verifyNoMoreInteractions(mockMessageFile);
		assertEquals(testMessage, receivedMessage);
    }
}

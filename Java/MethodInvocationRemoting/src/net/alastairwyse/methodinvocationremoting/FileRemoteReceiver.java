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

package net.alastairwyse.methodinvocationremoting;

import net.alastairwyse.operatingsystemabstraction.*;
import net.alastairwyse.applicationlogging.*;

/**
 * Receives messages from a remote location via the file system.
 * @author Alastair Wyse
 */
public class FileRemoteReceiver implements IRemoteReceiver {

    private IFile messageFile;
    private IFileSystem fileSystem;
    private String messageFilePath;
    private String lockFilePath;
    private int readLoopTimeout;
    private volatile boolean waitingForTimeout = false;
    private volatile boolean cancelRequest;
    private IApplicationLogger logger;
    private LoggingUtilities loggingUtilities;
    
    /**
     * Initialises a new instance of the FileRemoteReceiver class.
     * @param messageFilePath  The full path of the file used to receive messages.
     * @param lockFilePath     The full path of the file used to indicate when the message file is locked for writing.
     * @param readLoopTimeout  The time to wait between attempts to read the file in milliseconds.
     */
    public FileRemoteReceiver(String messageFilePath, String lockFilePath, int readLoopTimeout) {
        if (messageFile == null)
        {
            messageFile = new File(messageFilePath);
        }
        if (fileSystem == null)
        {
            fileSystem = new FileSystem();
        }
        this.messageFilePath = messageFilePath;
        this.lockFilePath = lockFilePath;
        this.readLoopTimeout = readLoopTimeout;

        logger = new ConsoleApplicationLogger(LogLevel.Information, '|', "  ");
        loggingUtilities = new LoggingUtilities(logger);
    }
    
    /**
     * Initialises a new instance of the FileRemoteReceiver class.
     * @param messageFilePath  The full path of the file used to receive messages.
     * @param lockFilePath     The full path of the file used to indicate when the message file is locked for writing.
     * @param readLoopTimeout  The time to wait between attempts to read the file in milliseconds.
     * @param logger           The logger to write log events to.
     */
    public FileRemoteReceiver(String messageFilePath, String lockFilePath, int readLoopTimeout, IApplicationLogger logger) {
        this(messageFilePath, lockFilePath, readLoopTimeout);
        this.logger = logger;
        loggingUtilities = new LoggingUtilities(logger);
    }
        
    /**
     * Initialises a new instance of the FileRemoteReceiver class.
     * <b>Note</b> this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
     * @param messageFilePath  The full path of the file used to receive messages.
     * @param lockFilePath     The full path of the file used to indicate when the message file is locked for writing.
     * @param readLoopTimeout  The time to wait between attempts to read the file in milliseconds.
     * @param logger           The logger to write log events to.
     * @param messageFile      A test (mock) message file.
     * @param fileSystem       A test (mock) file system.
     */
    public FileRemoteReceiver(String messageFilePath, String lockFilePath, int readLoopTimeout, IApplicationLogger logger, IFile messageFile, IFileSystem fileSystem) {
        this(messageFilePath, lockFilePath, readLoopTimeout);
        this.messageFile = messageFile;
        this.fileSystem = fileSystem;
        this.logger = logger;
        loggingUtilities = new LoggingUtilities(logger);
    }
    
    @Override
    public String Receive() throws Exception {
        String returnMessage = "";
        cancelRequest = false;

        try {
            while (cancelRequest == false) {
                if (fileSystem.CheckFileExists(messageFilePath) == true) {
                    if (fileSystem.CheckFileExists(lockFilePath) == false) {
                        returnMessage = messageFile.ReadAll();
                        fileSystem.DeleteFile(messageFilePath);
                        /* //[BEGIN_LOGGING]
                        loggingUtilities.LogMessageReceived(this, returnMessage);
                        //[END_LOGGING] */
                        break;
                    }
                }
                else {
                    waitingForTimeout = true;
                    Thread.sleep(readLoopTimeout);
                    waitingForTimeout = false;
                }
            }
        }
        catch (Exception e) {
            throw new Exception("Error receiving message.", e);
        }

        return returnMessage;
    }

    @Override
    public void CancelReceive() {
        cancelRequest = true;
        while (waitingForTimeout == true) ;
        
        /* //[BEGIN_LOGGING]
        try {
            logger.Log(this, LogLevel.Information, "Receive operation cancelled.");
        }
        catch(Exception e) {
        }
        //[END_LOGGING] */
    }
}

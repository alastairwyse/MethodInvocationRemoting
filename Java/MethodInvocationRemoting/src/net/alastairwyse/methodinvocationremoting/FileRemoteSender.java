/*
 * Copyright 2015 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
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
import net.alastairwyse.applicationmetrics.*;
import net.alastairwyse.methodinvocationremotingmetrics.*;

/**
 * Sends messages to a remote location via the file system.
 * @author Alastair Wyse
 */
public class FileRemoteSender implements IRemoteSender {

    private IFile messageFile;
    private IFile lockFile;
    private IFileSystem fileSystem;
    private String lockFilePath;
    private IApplicationLogger logger;
    private IMetricLogger metricLogger;
    
    /**
     * Initialises a new instance of the FileRemoteSender class.
     * @param messageFilePath  The full path of the file used to send messages.
     * @param lockFilePath     The full path of the file used to indicate when the message file is locked for writing.
     */
    public FileRemoteSender(String messageFilePath, String lockFilePath) {
        if (messageFile == null) {
            messageFile = new File(messageFilePath);
        }
        if (lockFile == null) {
            lockFile = new File(lockFilePath);
        }
        if (fileSystem == null) {
            fileSystem = new FileSystem();
        }
        this.lockFilePath = lockFilePath;

        logger = new ConsoleApplicationLogger(LogLevel.Information, '|', "  ");
        metricLogger = new NullMetricLogger();
    }
    
    /**
     * Initialises a new instance of the FileRemoteSender class.
     * @param messageFilePath  The full path of the file used to send messages.
     * @param lockFilePath     The full path of the file used to indicate when the message file is locked for writing.
     * @param logger           The logger to write log events to.
     */
    public FileRemoteSender(String messageFilePath, String lockFilePath, IApplicationLogger logger) {
        this(messageFilePath, lockFilePath);
        this.logger = logger;
    }
    
    /**
     * Initialises a new instance of the FileRemoteSender class.
     * @param messageFilePath  The full path of the file used to send messages.
     * @param lockFilePath     The full path of the file used to indicate when the message file is locked for writing.
     * @param metricLogger     The metric logger to write metric and instrumentation events to.
     */
    public FileRemoteSender(String messageFilePath, String lockFilePath, IMetricLogger metricLogger) {
        this(messageFilePath, lockFilePath);
        this.metricLogger = metricLogger;
    }
    
    /**
     * Initialises a new instance of the FileRemoteSender class.
     * @param messageFilePath  The full path of the file used to send messages.
     * @param lockFilePath     The full path of the file used to indicate when the message file is locked for writing.
     * @param logger           The logger to write log events to.
     * @param metricLogger     The metric logger to write metric and instrumentation events to.
     */
    public FileRemoteSender(String messageFilePath, String lockFilePath, IApplicationLogger logger, IMetricLogger metricLogger) {
        this(messageFilePath, lockFilePath);
        this.logger = logger;
        this.metricLogger = metricLogger;
    }
    
    /**
     * Initialises a new instance of the FileRemoteSender class. 
     * <b>Note</b> this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
     * @param messageFilePath  The full path of the file used to send messages.
     * @param lockFilePath     The full path of the file used to indicate when the message file is locked for writing.
     * @param logger           The logger to write log events to.
     * @param metricLogger     The metric logger to write metric and instrumentation events to.
     * @param messageFile      A test (mock) message file.
     * @param lockFile         A test (mock) lock file.
     * @param fileSystem       A test (mock) file system.
     */
    public FileRemoteSender(String messageFilePath, String lockFilePath, IApplicationLogger logger, IMetricLogger metricLogger, IFile messageFile, IFile lockFile, IFileSystem fileSystem) {
        this(messageFilePath, lockFilePath);
        this.messageFile = messageFile;
        this.lockFile = lockFile;
        this.fileSystem = fileSystem;
        this.logger = logger;
        this.metricLogger = metricLogger;
    }
    
    @Override
    public void Send(String message) throws Exception {
        /* //[BEGIN_METRICS]
        metricLogger.Begin(new MessageSendTime());
        //[END_METRICS] */
        
        try {
            // Lock file is created before data is written to the message file
            //   The FileRemoteReceiver class checks for the absence of the lock file to prevent attempting to open the message file when it is partially written and causing an exception
            lockFile.WriteAll("");
            messageFile.WriteAll(message);
            fileSystem.DeleteFile(lockFilePath);
        }
        catch (Exception e) {
            /* //[BEGIN_METRICS]
            metricLogger.CancelBegin(new MessageSendTime());
            //[END_METRICS] */
            throw new Exception("Error sending message.", e);
        }

        /* //[BEGIN_METRICS]
        metricLogger.End(new MessageSendTime());
        metricLogger.Increment(new MessageSent());
        //[END_METRICS] */
        /* //[BEGIN_LOGGING]
        logger.Log(this, LogLevel.Information, "Message sent.");
        //[END_LOGGING] */
    }
}

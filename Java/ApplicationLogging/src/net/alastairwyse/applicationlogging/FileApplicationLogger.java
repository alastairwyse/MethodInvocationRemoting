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

package net.alastairwyse.applicationlogging;

import java.io.IOException;
import java.text.*;

import net.alastairwyse.operatingsystemabstraction.*;

/**
 * Writes application log events and information to a file.
 * @author Alastair Wyse
 */
public class FileApplicationLogger extends ApplicationLoggerBase implements IApplicationLogger, AutoCloseable {

    private IFileWriter fileWriter;
    
    /**
     * Initialises a new instance of the FileApplicationLogger class.
     * @param  minimumLogLevel     The minimum level of log entries to write to the console.  Log entries with a level of importance lower than this parameter will not be written.
     * @param  separatorCharacter  The character to use to separate fields (e.g. date/time stamp, log level, log text) in the log entry.
     * @param  indentString        The string to use for indentation (e.g. of an exception stack trace) in the log entry.
     * @param  filePath            The full path of the file to write the log entries to.
     * @throws IOException         If the file specified in the filePath parameter cannot be opened.
     */
    public FileApplicationLogger(LogLevel minimumLogLevel, char separatorCharacter, String indentString, String filePath) throws IOException {
        super(minimumLogLevel, separatorCharacter, indentString);
        fileWriter = new FileWriter(filePath, false);
    }
    
    /**
     * Initialises a new instance of the FileApplicationLogger class.
     * @param  minimumLogLevel     The minimum level of log entries to write to the console.  Log entries with a level of importance lower than this parameter will not be written.
     * @param  separatorCharacter  The character to use to separate fields (e.g. date/time stamp, log level, log text) in the log entry.
     * @param  indentString        The string to use for indentation (e.g. of an exception stack trace) in the log entry.
     * @param  dateTimeFormat      The date formatter to use to format dates and times in the resulting logging information.
     * @param  filePath            The full path of the file to write the log entries to.
     * @param  appendToFile        Whether to append to an existing log file (if it exists) or overwrite.  A value of true causes appending.
     * @throws IOException         If the file specified in the filePath parameter cannot be opened.
     */
    public FileApplicationLogger(LogLevel minimumLogLevel, char separatorCharacter, String indentString, SimpleDateFormat dateTimeFormat, String filePath, boolean appendToFile) throws IOException {
        super(minimumLogLevel, separatorCharacter, indentString, dateTimeFormat);
        fileWriter = new FileWriter(filePath, appendToFile);
    }
    
    /**
     * Initialises a new instance of the FileApplicationLogger class.
     * <b>Note</b> this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
     * @param minimumLogLevel     The minimum level of log entries to write to the console.  Log entries with a level of importance lower than this parameter will not be written.
     * @param separatorCharacter  The character to use to separate fields (e.g. date/time stamp, log level, log text) in the log entry.
     * @param indentString        The string to use for indentation (e.g. of an exception stack trace) in the log entry.
     * @param fileWriter          A test (mock) file writer.
     */
    public FileApplicationLogger(LogLevel minimumLogLevel, char separatorCharacter, String indentString, IFileWriter fileWriter) {
        super(minimumLogLevel, separatorCharacter, indentString);
        this.fileWriter = fileWriter;
    }
    
    @Override
    public void Log(LogLevel level, String text) throws IOException {
        // Typically this and the other Log() method overrides would check that the class was not closed and not disposed, so that an exception with a clear message could be throw in the case that either were true.
        //    However, in the interest of performance such checks are omitted.

        if (level.getValue() >= minimumLogLevel.getValue()) {
            fileWriter.write(CreateLogEntry(level, text).toString());
            fileWriter.write(System.lineSeparator());
            fileWriter.flush();
        }
    }

    @Override
    public void Log(Object source, LogLevel level, String text) throws IOException {
        if (level.getValue() >= minimumLogLevel.getValue()) {
            fileWriter.write(CreateLogEntry(source, level, text).toString());
            fileWriter.write(System.lineSeparator());
            fileWriter.flush();
        }
    }
    
    @Override
    public void Log(int eventIdentifier, LogLevel level, String text) throws IOException {
        if (level.getValue() >= minimumLogLevel.getValue()) {
            fileWriter.write(CreateLogEntry(eventIdentifier, level, text).toString());
            fileWriter.write(System.lineSeparator());
            fileWriter.flush();
        }
    }

    @Override
    public void Log(Object source, int eventIdentifier, LogLevel level, String text) throws IOException {
        if (level.getValue() >= minimumLogLevel.getValue()) {
            fileWriter.write(CreateLogEntry(source, eventIdentifier, level, text).toString());
            fileWriter.write(System.lineSeparator());
            fileWriter.flush();
        }
    }
    
    @Override
    public void Log(LogLevel level, String text, Exception sourceException) throws IOException {
        if (level.getValue() >= minimumLogLevel.getValue()) {
            fileWriter.write(CreateLogEntry(level, text, sourceException).toString());
            fileWriter.write(System.lineSeparator());
            fileWriter.flush();
        }
    }

    @Override
    public void Log(Object source, LogLevel level, String text, Exception sourceException) throws IOException {
        if (level.getValue() >= minimumLogLevel.getValue()) {
            fileWriter.write(CreateLogEntry(source, level, text, sourceException).toString());
            fileWriter.write(System.lineSeparator());
            fileWriter.flush();
        }
    }
    
    @Override
    public void Log(int eventIdentifier, LogLevel level, String text, Exception sourceException) throws IOException {
        if (level.getValue() >= minimumLogLevel.getValue()) {
            fileWriter.write(CreateLogEntry(eventIdentifier, level, text, sourceException).toString());
            fileWriter.write(System.lineSeparator());
            fileWriter.flush();
        }
    }

    @Override
    public void Log(Object source, int eventIdentifier, LogLevel level, String text, Exception sourceException) throws IOException {
        if (level.getValue() >= minimumLogLevel.getValue()) {
            fileWriter.write(CreateLogEntry(source, eventIdentifier, level, text, sourceException).toString());
            fileWriter.write(System.lineSeparator());
            fileWriter.flush();
        }
    }
    
    /**
     * Closes the log file.
     * @throws IOException
     */
    public void Close() throws IOException {
        fileWriter.close();
    }
    
    @Override
    public void close() throws Exception {
        Close();
    }
}

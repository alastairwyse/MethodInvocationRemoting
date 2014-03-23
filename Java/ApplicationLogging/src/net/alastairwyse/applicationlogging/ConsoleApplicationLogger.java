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

import java.text.*;

/**
 * Writes application log events and information to the standard output.
 * @author Alastair Wyse
 */
public class ConsoleApplicationLogger extends ApplicationLoggerBase implements IApplicationLogger {

    /**
     * Initialises a new instance of the ConsoleApplicationLogger class.
     * @param minimumLogLevel     The minimum level of log entries to write to the console.  Log entries with a level of importance lower than this parameter will not be written.
     * @param separatorCharacter  The character to use to separate fields (e.g. date/time stamp, log level, log text) in the log entry.
     * @param indentString        The string to use for indentation (e.g. of an exception stack trace) in the log entry.
     */
    public ConsoleApplicationLogger(LogLevel minimumLogLevel, char separatorCharacter, String indentString) {
        super(minimumLogLevel, separatorCharacter, indentString);
    }
    
    /**
     * Initialises a new instance of the ConsoleApplicationLogger class.
     * @param minimumLogLevel     The minimum level of log entries to write to the console.  Log entries with a level of importance lower than this parameter will not be written.
     * @param separatorCharacter  The character to use to separate fields (e.g. date/time stamp, log level, log text) in the log entry.
     * @param indentString        The string to use for indentation (e.g. of an exception stack trace) in the log entry.
     * @param dateTimeFormat      The date formatter to use to format dates and times in the resulting logging information.
     */
    public ConsoleApplicationLogger(LogLevel minimumLogLevel, char separatorCharacter, String indentString, SimpleDateFormat dateTimeFormat) {
        super(minimumLogLevel, separatorCharacter, indentString, dateTimeFormat);
    }
    
    @Override
    public void Log(LogLevel level, String text) {
        // Typically this and the other Log() method overrides would check that the class was not closed and not disposed, so that an exception with a clear message could be throw in the case that either were true.
        //    However, in the interest of performance such checks are omitted.

        if (level.getValue() >= minimumLogLevel.getValue()) {
            System.out.println(CreateLogEntry(level, text).toString());
        }
    }

    @Override
    public void Log(Object source, LogLevel level, String text) {
        if (level.getValue() >= minimumLogLevel.getValue()) {
            System.out.println(CreateLogEntry(source, level, text).toString());
        }
    }
    
    @Override
    public void Log(int eventIdentifier, LogLevel level, String text) {
        if (level.getValue() >= minimumLogLevel.getValue()) {
            System.out.println(CreateLogEntry(eventIdentifier, level, text).toString());
        }
    }

    @Override
    public void Log(Object source, int eventIdentifier, LogLevel level, String text) {
        if (level.getValue() >= minimumLogLevel.getValue()) {
            System.out.println(CreateLogEntry(source, eventIdentifier, level, text).toString());
        }
    }
    
    @Override
    public void Log(LogLevel level, String text, Exception sourceException) {
        if (level.getValue() >= minimumLogLevel.getValue()) {
            System.out.println(CreateLogEntry(level, text, sourceException).toString());
        }
    }

    @Override
    public void Log(Object source, LogLevel level, String text, Exception sourceException) {
        if (level.getValue() >= minimumLogLevel.getValue()) {
            System.out.println(CreateLogEntry(source, level, text, sourceException).toString());
        }
    }
    
    @Override
    public void Log(int eventIdentifier, LogLevel level, String text, Exception sourceException) {
        if (level.getValue() >= minimumLogLevel.getValue()) {
            System.out.println(CreateLogEntry(eventIdentifier, level, text, sourceException).toString());
        }
    }

    @Override
    public void Log(Object source, int eventIdentifier, LogLevel level, String text, Exception sourceException) {
        if (level.getValue() >= minimumLogLevel.getValue()) {
            System.out.println(CreateLogEntry(source, eventIdentifier, level, text, sourceException).toString());
        }
    }
}

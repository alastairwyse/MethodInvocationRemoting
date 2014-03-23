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

import java.io.*;
import java.text.*;
import java.util.*;

/**
 * Provides common functionality for application logger implementations.
 * @author Alastair Wyse
 */
public abstract class ApplicationLoggerBase {

    /** The minimum level of log entries to write to the console.  Log entries with a level of importance lower than this will not be written. */
    protected LogLevel minimumLogLevel;
    /** The character to use to separate fields (e.g. date/time stamp, log level, log text) in the log entry. */
    protected char separatorCharacter;
    /** The string to use for indentation (e.g. of an exception stack trace) in the log entry. */
    protected String indentString;
    /** The date formatter to use to format dates and times in the resulting logging information. */
    protected SimpleDateFormat dateTimeFormat;
    
    /**
     * Initialises a new instance of the ApplicationLoggerBase class.
     * @param minimumLogLevel     The minimum level of log entries to write to the console.  Log entries with a level of importance lower than this will not be written.
     * @param separatorCharacter  The character to use to separate fields (e.g. date/time stamp, log level, log text) in the log entry.
     * @param indentString        The string to use for indentation (e.g. of an exception stack trace) in the log entry.
     */
    protected ApplicationLoggerBase(LogLevel minimumLogLevel, char separatorCharacter, String indentString) {
        this.minimumLogLevel = minimumLogLevel;
        this.separatorCharacter = separatorCharacter;
        this.indentString = indentString;
        if (dateTimeFormat == null) {
            dateTimeFormat = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss.SSS");
        }
    }
    
    /**
     * Initialises a new instance of the ApplicationLoggerBase class.
     * @param minimumLogLevel     The minimum level of log entries to write to the console.  Log entries with a level of importance lower than this will not be written.
     * @param separatorCharacter  The character to use to separate fields (e.g. date/time stamp, log level, log text) in the log entry.
     * @param indentString        The string to use for indentation (e.g. of an exception stack trace) in the log entry.
     * @param dateTimeFormat      The date formatter to use to format dates and times in the resulting logging information.
     */
    protected ApplicationLoggerBase(LogLevel minimumLogLevel, char separatorCharacter, String indentString, SimpleDateFormat dateTimeFormat) {
        this(minimumLogLevel, separatorCharacter, indentString);
        this.dateTimeFormat = dateTimeFormat;
    }

    /**
     * Creates the text of a log entry.
     * @param level  The level of importance of the log event.
     * @param text   The details of the log event.
     * @return       A string builder containing the log entry.
     */
    protected StringBuilder CreateLogEntry(LogLevel level, String text) {
        StringBuilder stringBuilder = InitializeStringBuilder();
        WriteLogLevel(level, stringBuilder);
        stringBuilder.append(text);
        return stringBuilder;
    }

    /**
     * Creates the text of a log entry.
     * @param source  The exception which caused the log event.
     * @param level   The level of importance of the log event.
     * @param text    The details of the log event.
     * @return        A string builder containing the log entry.
     */
    protected StringBuilder CreateLogEntry(Object source, LogLevel level, String text) {
        StringBuilder stringBuilder = InitializeStringBuilder();
        WriteSource(source, stringBuilder);
        WriteLogLevel(level, stringBuilder);
        stringBuilder.append(text);
        return stringBuilder;
    }

    /**
     * Creates the text of a log entry.
     * @param eventIdentifier  An ID number which uniquely identifies the log event.
     * @param level            The level of importance of the log event.
     * @param text             The details of the log event.
     * @return                 A string builder containing the log entry.
     */
    protected StringBuilder CreateLogEntry(int eventIdentifier, LogLevel level, String text) {
        StringBuilder stringBuilder = InitializeStringBuilder();
        WriteEventIdentifier(eventIdentifier, stringBuilder);
        WriteLogLevel(level, stringBuilder);
        stringBuilder.append(text);
        return stringBuilder;
    }

    /**
     * Creates the text of a log entry.
     * @param source           The exception which caused the log event.
     * @param eventIdentifier  An ID number which uniquely identifies the log event.
     * @param level            The level of importance of the log event.
     * @param text             The details of the log event.
     * @return                 A string builder containing the log entry.
     */
    protected StringBuilder CreateLogEntry(Object source, int eventIdentifier, LogLevel level, String text) {
        StringBuilder stringBuilder = InitializeStringBuilder();
        WriteSource(source, stringBuilder);
        WriteEventIdentifier(eventIdentifier, stringBuilder);
        WriteLogLevel(level, stringBuilder);
        stringBuilder.append(text);
        return stringBuilder;
    }

    /**
     * Creates the text of a log entry.
     * @param level            The level of importance of the log event.
     * @param text             The details of the log event.
     * @param sourceException  The exception which caused the log event.
     * @return                 A string builder containing the log entry.
     */
    protected StringBuilder CreateLogEntry(LogLevel level, String text, Exception sourceException) {
        StringBuilder stringBuilder = InitializeStringBuilder();
        WriteLogLevel(level, stringBuilder);
        stringBuilder.append(text);
        WriteException(sourceException, stringBuilder);
        return stringBuilder;
    }

    /**
     * Creates the text of a log entry.
     * @param source           The exception which caused the log event.
     * @param level            The level of importance of the log event.
     * @param text             The details of the log event.
     * @param sourceException  The exception which caused the log event.
     * @return                 A string builder containing the log entry.
     */
    protected StringBuilder CreateLogEntry(Object source, LogLevel level, String text, Exception sourceException) {
        StringBuilder stringBuilder = InitializeStringBuilder();
        WriteSource(source, stringBuilder);
        WriteLogLevel(level, stringBuilder);
        stringBuilder.append(text);
        WriteException(sourceException, stringBuilder);
        return stringBuilder;
    }

    /**
     * Creates the text of a log entry.
     * @param eventIdentifier  An ID number which uniquely identifies the log event.
     * @param level            The level of importance of the log event.
     * @param text             The details of the log event.
     * @param sourceException  The exception which caused the log event.
     * @return                 A string builder containing the log entry.
     */
    protected StringBuilder CreateLogEntry(int eventIdentifier, LogLevel level, String text, Exception sourceException) {
        StringBuilder stringBuilder = InitializeStringBuilder();
        WriteEventIdentifier(eventIdentifier, stringBuilder);
        WriteLogLevel(level, stringBuilder);
        stringBuilder.append(text);
        WriteException(sourceException, stringBuilder);
        return stringBuilder;
    }

    /**
     * Creates the text of a log entry.
     * @param source           The exception which caused the log event.
     * @param eventIdentifier  An ID number which uniquely identifies the log event.
     * @param level            The level of importance of the log event.
     * @param text             The details of the log event.
     * @param sourceException  The exception which caused the log event.
     * @return                 A string builder containing the log entry.
     */
    protected StringBuilder CreateLogEntry(Object source, int eventIdentifier, LogLevel level, String text, Exception sourceException) {
        StringBuilder stringBuilder = InitializeStringBuilder();
        WriteSource(source, stringBuilder);
        WriteEventIdentifier(eventIdentifier, stringBuilder);
        WriteLogLevel(level, stringBuilder);
        stringBuilder.append(text);
        WriteException(sourceException, stringBuilder);
        return stringBuilder;
    }
    
    /**
     * Creates and returns a StringBuilder class, with the current timestamp written to it.
     * @return  The initialized string builder.
     */
    private StringBuilder InitializeStringBuilder() {
        StringBuilder returnStringBuilder = new StringBuilder();
        returnStringBuilder.append(dateTimeFormat.format(new Date()));
        returnStringBuilder.append(" ");
        returnStringBuilder.append(separatorCharacter);
        returnStringBuilder.append(" ");
        return returnStringBuilder;
    }

    /**
     * Writes the specified log source object to the log entry in the specified string builder.
     * @param source         The object which created the log entry.
     * @param stringBuilder  The string builder to write information about the source object to.
     */
    private void WriteSource(Object source, StringBuilder stringBuilder) {
        stringBuilder.append("Source = ");
        stringBuilder.append(source.getClass().getSimpleName());
        stringBuilder.append(" ");
        stringBuilder.append(separatorCharacter);
        stringBuilder.append(" ");
    }
    
    /**
     * Writes the specified log level to the log entry in the specified string builder.
     * @param level          The log level to write.
     * @param stringBuilder  The string builder to write the log level to.
     */
    private void WriteLogLevel(LogLevel level, StringBuilder stringBuilder){
        if (level.getValue() >= LogLevel.Warning.getValue()) {
            switch (level) {
                case Warning:
                    stringBuilder.append("WARNING ");
                    break;
                case Error:
                    stringBuilder.append("ERROR ");
                    break;
                case Critical:
                    stringBuilder.append("CRITICAL ");
                    break;
            }
            stringBuilder.append(separatorCharacter);
            stringBuilder.append(" ");
        }
    }

    /**
     * Writes the specified log event identifier to the log entry in the specified string builder.
     * @param eventIdentifier  The log event identifier to write.
     * @param stringBuilder    The string builder to write the log level to.
     */
    private void WriteEventIdentifier(int eventIdentifier, StringBuilder stringBuilder) {
        stringBuilder.append("Log Event Id = ");
        stringBuilder.append(eventIdentifier);
        stringBuilder.append(" ");
        stringBuilder.append(separatorCharacter);
        stringBuilder.append(" ");
    }

    /**
     * Writes details of the specified exception to the log entry in the specified string builder.
     * @param e              The exception to write the details of.
     * @param stringBuilder  The string builder to write the exception details to.
     */
    private void WriteException(Exception e, StringBuilder stringBuilder) {
        StringWriter tempStringWriter = new StringWriter();
        try(PrintWriter tempPrintWriter = new PrintWriter(tempStringWriter)) {
            e.printStackTrace(tempPrintWriter);
            String indentedException = tempStringWriter.toString().replaceAll(System.lineSeparator(), System.lineSeparator() + indentString);
            stringBuilder.append(System.lineSeparator());
            stringBuilder.append(indentString);
            stringBuilder.append(indentedException);
        }
    }
}

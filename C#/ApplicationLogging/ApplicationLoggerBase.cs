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

using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLogging
{
    //******************************************************************************
    //
    // Class: ApplicationLoggerBase
    //
    //******************************************************************************
    /// <summary>
    /// Provides common functionality for application logger implementations.
    /// </summary>
    public abstract class ApplicationLoggerBase
    {
        /// <summary>The minimum level of log entries to write to the console.  Log entries with a level of importance lower than this will not be written.</summary>
        protected LogLevel minimumLogLevel;
        /// <summary>The character to use to separate fields (e.g. date/time stamp, log level, log text) in the log entry.</summary>
        protected char separatorCharacter;
        /// <summary>The string to use for indentation (e.g. of an exception stack trace) in the log entry.</summary>
        protected string indentString;
        /// <summary>A format string to use to format dates and times in the resulting logging information.</summary>
        protected string dateTimeFormat;

        //******************************************************************************
        //
        // Method: ApplicationLoggerBase (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the ApplicationLogging.ApplicationLoggerBase class.
        /// </summary>
        /// <param name="minimumLogLevel">The minimum level of log entries to write to the console.  Log entries with a level of importance lower than this parameter will not be written.</param>
        /// <param name="separatorCharacter">The character to use to separate fields (e.g. date/time stamp, log level, log text) in the log entry.</param>
        /// <param name="indentString">The string to use for indentation (e.g. of an exception stack trace) in the log entry.</param>
        protected ApplicationLoggerBase(LogLevel minimumLogLevel, char separatorCharacter, string indentString)
        {
            this.minimumLogLevel = minimumLogLevel;
            this.separatorCharacter = separatorCharacter;
            this.indentString = indentString;
            if (dateTimeFormat == null)
            {
                dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
            }
        }

        //******************************************************************************
        //
        // Method: ApplicationLoggerBase (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the ApplicationLogging.ApplicationLoggerBase class.
        /// </summary>
        /// <param name="minimumLogLevel">The minimum level of log entries to write to the console.  Log entries with a level of importance lower than this parameter will not be written.</param>
        /// <param name="separatorCharacter">The character to use to separate fields (e.g. date/time stamp, log level, log text) in the log entry.</param>
        /// <param name="indentString">The string to use for indentation (e.g. of an exception stack trace) in the log entry.</param>
        /// <param name="dateTimeFormat">A format string to use to format dates and times in the resulting logging information.</param>
        protected ApplicationLoggerBase(LogLevel minimumLogLevel, char separatorCharacter, string indentString, string dateTimeFormat)
            : this(minimumLogLevel, separatorCharacter, indentString)
        {
            this.dateTimeFormat = dateTimeFormat;
        }

        //------------------------------------------------------------------------------
        //
        // Method: CreateLogEntry
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Creates the text of a log entry.
        /// </summary>
        /// <param name="level">The level of importance of the log event.</param>
        /// <param name="text">The details of the log event.</param>
        /// <returns>A string builder containing the log entry.</returns>
        protected virtual StringBuilder CreateLogEntry(LogLevel level, string text)
        {
            StringBuilder stringBuilder = InitializeStringBuilder();
            WriteLogLevel(level, stringBuilder);
            stringBuilder.Append(text);
            return stringBuilder;
        }

        //------------------------------------------------------------------------------
        //
        // Method: CreateLogEntry
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Creates the text of a log entry.
        /// </summary>
        /// <param name="source">The exception which caused the log event.</param>
        /// <param name="level">The level of importance of the log event.</param>
        /// <param name="text">The details of the log event.</param>
        /// <returns>A string builder containing the log entry.</returns>
        protected virtual StringBuilder CreateLogEntry(object source, LogLevel level, string text)
        {
            StringBuilder stringBuilder = InitializeStringBuilder();
            WriteSource(source, stringBuilder);
            WriteLogLevel(level, stringBuilder);
            stringBuilder.Append(text);
            return stringBuilder;
        }

        //------------------------------------------------------------------------------
        //
        // Method: CreateLogEntry
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Creates the text of a log entry.
        /// </summary>
        /// <param name="eventIdentifier">An ID number which uniquely identifies the log event.</param>
        /// <param name="level">The level of importance of the log event.</param>
        /// <param name="text">The details of the log event.</param>
        /// <returns>A string builder containing the log entry.</returns>
        protected virtual StringBuilder CreateLogEntry(int eventIdentifier, LogLevel level, string text)
        {
            StringBuilder stringBuilder = InitializeStringBuilder();
            WriteEventIdentifier(eventIdentifier, stringBuilder);
            WriteLogLevel(level, stringBuilder);
            stringBuilder.Append(text);
            return stringBuilder;
        }

        //------------------------------------------------------------------------------
        //
        // Method: CreateLogEntry
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Creates the text of a log entry.
        /// </summary>
        /// <param name="source">The exception which caused the log event.</param>
        /// <param name="eventIdentifier">An ID number which uniquely identifies the log event.</param>
        /// <param name="level">The level of importance of the log event.</param>
        /// <param name="text">The details of the log event.</param>
        /// <returns>A string builder containing the log entry.</returns>
        protected virtual StringBuilder CreateLogEntry(object source, int eventIdentifier, LogLevel level, string text)
        {
            StringBuilder stringBuilder = InitializeStringBuilder();
            WriteSource(source, stringBuilder);
            WriteEventIdentifier(eventIdentifier, stringBuilder);
            WriteLogLevel(level, stringBuilder);
            stringBuilder.Append(text);
            return stringBuilder;
        }

        //------------------------------------------------------------------------------
        //
        // Method: CreateLogEntry
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Creates the text of a log entry.
        /// </summary>
        /// <param name="level">The level of importance of the log event.</param>
        /// <param name="text">The details of the log event.</param>
        /// <param name="sourceException">The exception which caused the log event.</param>
        /// <returns>A string builder containing the log entry.</returns>
        protected virtual StringBuilder CreateLogEntry(LogLevel level, string text, Exception sourceException)
        {
            StringBuilder stringBuilder = InitializeStringBuilder();
            WriteLogLevel(level, stringBuilder);
            stringBuilder.Append(text);
            WriteException(sourceException, stringBuilder);
            return stringBuilder;
        }

        //------------------------------------------------------------------------------
        //
        // Method: CreateLogEntry
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Creates the text of a log entry.
        /// </summary>
        /// <param name="source">The exception which caused the log event.</param>
        /// <param name="level">The level of importance of the log event.</param>
        /// <param name="text">The details of the log event.</param>
        /// <param name="sourceException">The exception which caused the log event.</param>
        /// <returns>A string builder containing the log entry.</returns>
        protected virtual StringBuilder CreateLogEntry(object source, LogLevel level, string text, Exception sourceException)
        {
            StringBuilder stringBuilder = InitializeStringBuilder();
            WriteSource(source, stringBuilder);
            WriteLogLevel(level, stringBuilder);
            stringBuilder.Append(text);
            WriteException(sourceException, stringBuilder);
            return stringBuilder;
        }

        //------------------------------------------------------------------------------
        //
        // Method: CreateLogEntry
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Creates the text of a log entry.
        /// </summary>
        /// <param name="eventIdentifier">An ID number which uniquely identifies the log event.</param>
        /// <param name="level">The level of importance of the log event.</param>
        /// <param name="text">The details of the log event.</param>
        /// <param name="sourceException">The exception which caused the log event.</param>
        /// <returns>A string builder containing the log entry.</returns>
        protected virtual StringBuilder CreateLogEntry(int eventIdentifier, LogLevel level, string text, Exception sourceException)
        {
            StringBuilder stringBuilder = InitializeStringBuilder();
            WriteEventIdentifier(eventIdentifier, stringBuilder);
            WriteLogLevel(level, stringBuilder);
            stringBuilder.Append(text);
            WriteException(sourceException, stringBuilder);
            return stringBuilder;
        }

        //------------------------------------------------------------------------------
        //
        // Method: CreateLogEntry
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Creates the text of a log entry.
        /// </summary>
        /// <param name="source">The exception which caused the log event.</param>
        /// <param name="eventIdentifier">An ID number which uniquely identifies the log event.</param>
        /// <param name="level">The level of importance of the log event.</param>
        /// <param name="text">The details of the log event.</param>
        /// <param name="sourceException">The exception which caused the log event.</param>
        /// <returns>A string builder containing the log entry.</returns>
        protected virtual StringBuilder CreateLogEntry(object source, int eventIdentifier, LogLevel level, string text, Exception sourceException)
        {
            StringBuilder stringBuilder = InitializeStringBuilder();
            WriteSource(source, stringBuilder);
            WriteEventIdentifier(eventIdentifier, stringBuilder);
            WriteLogLevel(level, stringBuilder);
            stringBuilder.Append(text);
            WriteException(sourceException, stringBuilder);
            return stringBuilder;
        }

        //------------------------------------------------------------------------------
        //
        // Method: InitializeStringBuilder
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Creates and returns a StringBuilder class, with the current timestamp written to it.
        /// </summary>
        /// <returns>The initialized string builder.</returns>
        private StringBuilder InitializeStringBuilder()
        {
            StringBuilder returnStringBuilder = new StringBuilder();
            returnStringBuilder.Append(DateTime.Now.ToString(dateTimeFormat));
            returnStringBuilder.Append(" ");
            returnStringBuilder.Append(separatorCharacter);
            returnStringBuilder.Append(" ");
            return returnStringBuilder;
        }

        //------------------------------------------------------------------------------
        //
        // Method: WriteSource
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Writes the specified log source object to the log entry in the specified string builder.
        /// </summary>
        /// <param name="source">The object which created the log entry.</param>
        /// <param name="stringBuilder">The string builder to write information about the source object to.</param>
        private void WriteSource(object source, StringBuilder stringBuilder)
        {
            stringBuilder.Append("Source = ");
            stringBuilder.Append(source.GetType().Name);
            stringBuilder.Append(" ");
            stringBuilder.Append(separatorCharacter);
            stringBuilder.Append(" ");
        }

        //------------------------------------------------------------------------------
        //
        // Method: WriteLogLevel
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Writes the specified log level to the log entry in the specified string builder.
        /// </summary>
        /// <param name="level">The log level to write.</param>
        /// <param name="stringBuilder">The string builder to write the log level to.</param>
        private void WriteLogLevel(LogLevel level, StringBuilder stringBuilder)
        {
            if (level >= LogLevel.Warning)
            {
                switch (level)
                {
                    case LogLevel.Warning:
                        stringBuilder.Append("WARNING ");
                        break;
                    case LogLevel.Error:
                        stringBuilder.Append("ERROR ");
                        break;
                    case LogLevel.Critical:
                        stringBuilder.Append("CRITICAL ");
                        break;
                }
                stringBuilder.Append(separatorCharacter);
                stringBuilder.Append(" ");
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: WriteEventIdentifier
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Writes the specified log event identifier to the log entry in the specified string builder.
        /// </summary>
        /// <param name="eventIdentifier">The log event identifier to write.</param>
        /// <param name="stringBuilder">The string builder to write the log event identifier to.</param>
        private void WriteEventIdentifier(int eventIdentifier, StringBuilder stringBuilder)
        {
            stringBuilder.Append("Log Event Id = ");
            stringBuilder.Append(eventIdentifier);
            stringBuilder.Append(" ");
            stringBuilder.Append(separatorCharacter);
            stringBuilder.Append(" ");
        }

        //------------------------------------------------------------------------------
        //
        // Method: WriteException
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Writes details of the specified exception to the log entry in the specified string builder.
        /// </summary>
        /// <param name="e">The exception to write the details of.</param>
        /// <param name="stringBuilder">The string builder to write the exception details to.</param>
        private void WriteException(Exception e, StringBuilder stringBuilder)
        {
            StringBuilder exceptionDetails = new StringBuilder();
            exceptionDetails.Append(indentString);
            exceptionDetails.Append(e.GetType().FullName);
            exceptionDetails.Append(": ");
            exceptionDetails.AppendLine(e.Message);
            exceptionDetails.Append(e.StackTrace);
            exceptionDetails.Replace(System.Environment.NewLine, System.Environment.NewLine + indentString);
            stringBuilder.AppendLine();
            stringBuilder.Append(exceptionDetails.ToString());
        }
    }
}

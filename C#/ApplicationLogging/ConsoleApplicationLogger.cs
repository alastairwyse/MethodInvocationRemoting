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
    // Class: ConsoleApplicationLogger
    //
    //******************************************************************************
    /// <summary>
    /// Writes application log events and information to the console (standard output).
    /// </summary>
    public class ConsoleApplicationLogger : ApplicationLoggerBase, IApplicationLogger
    {
        //------------------------------------------------------------------------------
        //
        // Method: ConsoleApplicationLogger (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationLogging.ConsoleApplicationLogger class.
        /// </summary>
        /// <param name="minimumLogLevel">The minimum level of log entries to write to the console.  Log entries with a level of importance lower than this parameter will not be written.</param>
        /// <param name="separatorCharacter">The character to use to separate fields (e.g. date/time stamp, log level, log text) in the log entry.</param>
        /// <param name="indentString">The string to use for indentation (e.g. of an exception stack trace) in the log entry.</param>
        public ConsoleApplicationLogger(LogLevel minimumLogLevel, char separatorCharacter, string indentString)
            : base(minimumLogLevel, separatorCharacter, indentString)
        {
        }

        //------------------------------------------------------------------------------
        //
        // Method: ConsoleApplicationLogger (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationLogging.ConsoleApplicationLogger class.
        /// </summary>
        /// <param name="minimumLogLevel">The minimum level of log entries to write to the console.  Log entries with a level of importance lower than this parameter will not be written.</param>
        /// <param name="separatorCharacter">The character to use to separate fields (e.g. date/time stamp, log level, log text) in the log entry.</param>
        /// <param name="indentString">The string to use for indentation (e.g. of an exception stack trace) in the log entry.</param>
        /// <param name="dateTimeFormat">A format string to use to format dates and times in the resulting logging information.</param>
        public ConsoleApplicationLogger(LogLevel minimumLogLevel, char separatorCharacter, string indentString, string dateTimeFormat)
            : base(minimumLogLevel, separatorCharacter, indentString, dateTimeFormat)
        {
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(ApplicationLogging.LogLevel,System.String)"]/*'/>
        public void Log(LogLevel level, string text)
        {
            if (level >= minimumLogLevel)
            {
                Console.WriteLine(CreateLogEntry(level, text).ToString());
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(System.Object,ApplicationLogging.LogLevel,System.String)"]/*'/>
        public void Log(object source, LogLevel level, string text)
        {
            if (level >= minimumLogLevel)
            {
                Console.WriteLine(CreateLogEntry(source, level, text).ToString());
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(System.Int32,ApplicationLogging.LogLevel,System.String)"]/*'/>
        public void Log(int eventIdentifier, LogLevel level, string text)
        {
            if (level >= minimumLogLevel)
            {
                Console.WriteLine(CreateLogEntry(eventIdentifier, level, text).ToString());
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(System.Object,System.Int32,ApplicationLogging.LogLevel,System.String)"]/*'/>
        public void Log(object source, int eventIdentifier, LogLevel level, string text)
        {
            if (level >= minimumLogLevel)
            {
                Console.WriteLine(CreateLogEntry(source, eventIdentifier, level, text).ToString());
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(ApplicationLogging.LogLevel,System.String,System.Exception)"]/*'/>
        public void Log(LogLevel level, string text, Exception sourceException)
        {
            if (level >= minimumLogLevel)
            {
                Console.WriteLine(CreateLogEntry(level, text, sourceException).ToString());
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(System.Object,ApplicationLogging.LogLevel,System.String,System.Exception)"]/*'/>
        public void Log(object source, LogLevel level, string text, Exception sourceException)
        {
            if (level >= minimumLogLevel)
            {
                Console.WriteLine(CreateLogEntry(source, level, text, sourceException).ToString());
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(System.Int32,ApplicationLogging.LogLevel,System.String,System.Exception)"]/*'/>
        public void Log(int eventIdentifier, LogLevel level, string text, Exception sourceException)
        {
            if (level >= minimumLogLevel)
            {
                Console.WriteLine(CreateLogEntry(eventIdentifier, level, text, sourceException).ToString());
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(System.Object,System.Int32,ApplicationLogging.LogLevel,System.String,System.Exception)"]/*'/>
        public void Log(object source, int eventIdentifier, LogLevel level, string text, Exception sourceException)
        {
            if (level >= minimumLogLevel)
            {
                Console.WriteLine(CreateLogEntry(source, eventIdentifier, level, text, sourceException).ToString());
            }
        }
    }
}

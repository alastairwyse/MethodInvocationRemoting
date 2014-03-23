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
using OperatingSystemAbstraction;

namespace ApplicationLogging
{
    //******************************************************************************
    //
    // Class: FileApplicationLogger
    //
    //******************************************************************************
    /// <summary>
    /// Writes application log events and information to a file.
    /// </summary>
    public class FileApplicationLogger : ApplicationLoggerBase, IApplicationLogger, IDisposable
    {
        protected bool disposed;
        private IStreamWriter streamWriter;
        private Encoding fileEncoding = Encoding.UTF8;

        //------------------------------------------------------------------------------
        //
        // Method: FileApplicationLogger (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationLogging.FileApplicationLogger class.
        /// </summary>
        /// <param name="minimumLogLevel">The minimum level of log entries to write to the console.  Log entries with a level of importance lower than this parameter will not be written.</param>
        /// <param name="separatorCharacter">The character to use to separate fields (e.g. date/time stamp, log level, log text) in the log entry.</param>
        /// <param name="indentString">The string to use for indentation (e.g. of an exception stack trace) in the log entry.</param>
        /// <param name="filePath">The full path of the file to write the log entries to.</param>
        public FileApplicationLogger(LogLevel minimumLogLevel, char separatorCharacter, string indentString, string filePath)
            : base(minimumLogLevel, separatorCharacter, indentString)
        {
            streamWriter = new StreamWriter(filePath, false, fileEncoding);
        }

        //------------------------------------------------------------------------------
        //
        // Method: FileApplicationLogger (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationLogging.FileApplicationLogger class.
        /// </summary>
        /// <param name="minimumLogLevel">The minimum level of log entries to write to the console.  Log entries with a level of importance lower than this parameter will not be written.</param>
        /// <param name="separatorCharacter">The character to use to separate fields (e.g. date/time stamp, log level, log text) in the log entry.</param>
        /// <param name="indentString">The string to use for indentation (e.g. of an exception stack trace) in the log entry.</param>
        /// <param name="dateTimeFormat">A format string to use to format dates and times in the resulting logging information.</param>
        /// <param name="filePath">The full path of the file to write the log entries to.</param>
        /// <param name="appendToFile">Whether to append to an existing log file (if it exists) or overwrite.  A value of true causes appending.</param>
        /// <param name="fileEncoding">The character encoding to use in the log file.</param>
        public FileApplicationLogger(LogLevel minimumLogLevel, char separatorCharacter, string indentString, string dateTimeFormat, string filePath, bool appendToFile, Encoding fileEncoding)
            : base(minimumLogLevel, separatorCharacter, indentString, dateTimeFormat)
        {
            streamWriter = new StreamWriter(filePath, appendToFile, fileEncoding);
        }

        //------------------------------------------------------------------------------
        //
        // Method: FileApplicationLogger (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationLogging.FileApplicationLogger class.  Note this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
        /// </summary>
        /// <param name="minimumLogLevel">The minimum level of log entries to write to the console.  Log entries with a level of importance lower than this parameter will not be written.</param>
        /// <param name="separatorCharacter">The character to use to separate fields (e.g. date/time stamp, log level, log text) in the log entry.</param>
        /// <param name="indentString">The string to use for indentation (e.g. of an exception stack trace) in the log entry.</param>
        /// <param name="streamWriter">A test (mock) stream writer.</param>
        public FileApplicationLogger(LogLevel minimumLogLevel, char separatorCharacter, string indentString, IStreamWriter streamWriter)
            : base(minimumLogLevel, separatorCharacter, indentString)
        {
            this.streamWriter = streamWriter;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(ApplicationLogging.LogLevel,System.String)"]/*'/>
        public void Log(LogLevel level, string text)
        {
            // Typically this and the other Log() method overrides would check that the class was not closed and not disposed, so that an exception with a clear message could be throw in the case that either were true.
            //    However, in the interest of performance such checks are omitted.

            if (level >= minimumLogLevel)
            {
                streamWriter.WriteLine(CreateLogEntry(level, text).ToString());
                streamWriter.Flush();
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(System.Object,ApplicationLogging.LogLevel,System.String)"]/*'/>
        public void Log(object source, LogLevel level, string text)
        {
            if (level >= minimumLogLevel)
            {
                streamWriter.WriteLine(CreateLogEntry(source, level, text).ToString());
                streamWriter.Flush();
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(System.Int32,ApplicationLogging.LogLevel,System.String)"]/*'/>
        public void Log(int eventIdentifier, LogLevel level, string text)
        {
            if (level >= minimumLogLevel)
            {
                streamWriter.WriteLine(CreateLogEntry(eventIdentifier, level, text).ToString());
                streamWriter.Flush();
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(System.Object,System.Int32,ApplicationLogging.LogLevel,System.String)"]/*'/>
        public void Log(object source, int eventIdentifier, LogLevel level, string text)
        {
            if (level >= minimumLogLevel)
            {
                streamWriter.WriteLine(CreateLogEntry(source, eventIdentifier, level, text).ToString());
                streamWriter.Flush();
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(ApplicationLogging.LogLevel,System.String,System.Exception)"]/*'/>
        public void Log(LogLevel level, string text, Exception sourceException)
        {
            if (level >= minimumLogLevel)
            {
                streamWriter.WriteLine(CreateLogEntry(level, text, sourceException).ToString());
                streamWriter.Flush();
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(System.Object,ApplicationLogging.LogLevel,System.String,System.Exception)"]/*'/>
        public void Log(object source, LogLevel level, string text, Exception sourceException)
        {
            if (level >= minimumLogLevel)
            {
                streamWriter.WriteLine(CreateLogEntry(source, level, text, sourceException).ToString());
                streamWriter.Flush();
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(System.Int32,ApplicationLogging.LogLevel,System.String,System.Exception)"]/*'/>
        public void Log(int eventIdentifier, LogLevel level, string text, Exception sourceException)
        {
            if (level >= minimumLogLevel)
            {
                streamWriter.WriteLine(CreateLogEntry(eventIdentifier, level, text, sourceException).ToString());
                streamWriter.Flush();
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(System.Object,System.Int32,ApplicationLogging.LogLevel,System.String,System.Exception)"]/*'/>
        public void Log(object source, int eventIdentifier, LogLevel level, string text, Exception sourceException)
        {
            if (level >= minimumLogLevel)
            {
                streamWriter.WriteLine(CreateLogEntry(source, eventIdentifier, level, text, sourceException).ToString());
                streamWriter.Flush();
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: Close
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Closes the log file.
        /// </summary>
        public void Close()
        {
            streamWriter.Close();
        }
        
        #region Finalize / Dispose Methods

        /// <summary>
        /// Releases the unmanaged resources used by the FileApplicationLogger.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~FileApplicationLogger()
        {
            Dispose(false);
        }

        //******************************************************************************
        //
        // Method: Dispose
        //
        //******************************************************************************
        /// <summary>
        /// Provides a method to free unmanaged resources used by this class.
        /// </summary>
        /// <param name="disposing">Whether the method is being called as part of an explicit Dispose routine, and hence whether managed resources should also be freed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                    if (streamWriter != null)
                    {
                        streamWriter.Dispose();
                        streamWriter = null;
                    }
                }
                // Free your own state (unmanaged objects).
                
                // Set large fields to null.

                disposed = true;
            }
        }

        #endregion
    }
}

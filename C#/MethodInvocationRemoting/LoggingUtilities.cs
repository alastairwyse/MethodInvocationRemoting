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
using System.Diagnostics;
using ApplicationLogging;

namespace MethodInvocationRemoting
{
    //******************************************************************************
    //
    // Class: LoggingUtilities
    //
    //******************************************************************************
    /// <summary>
    /// Contains common methods used to write log events.
    /// </summary>
    class LoggingUtilities
    {
        private IApplicationLogger logger;
        private int stringLengthLimit;

        //******************************************************************************
        //
        // Method: LoggingUtilities (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.LoggingUtilities class.
        /// </summary>
        /// <param name="logger">The logger to write log events to.</param>
        public LoggingUtilities(IApplicationLogger logger)
        {
            stringLengthLimit = 120;
            this.logger = logger;
        }

        //******************************************************************************
        //
        // Method: Log
        //
        //******************************************************************************
        /// <summary>
        /// Writes a log entry if the logging preprocessor identifier is defined.
        /// </summary>
        /// <param name="source">The object which created the log event.</param>
        /// <param name="level">The level of importance of the log event.</param>
        /// <param name="text">The details of the log event.</param>
        [Conditional("LOGGING_ON")]
        public void Log(object source, LogLevel level, string text)
        {
            logger.Log(source, level, text);
        }

        //******************************************************************************
        //
        // Method: LogMessageReceived
        //
        //******************************************************************************
        /// <summary>
        /// Logs details of a received message if the logging preprocessor identifier is defined.
        /// </summary>
        /// <param name="source">The object which created the log event.</param>
        /// <param name="message">The received message.</param>
        [Conditional("LOGGING_ON")]
        public void LogMessageReceived(object source, string message)
        {
            LogLongString("Received message", "Complete message content:", source, message);
        }

        //******************************************************************************
        //
        // Method: LogParameter
        //
        //******************************************************************************
        /// <summary>
        /// Logs details of a serialized or deserialized parameter.
        /// </summary>
        /// <param name="source">The object which created the log event.</param>
        /// <param name="operation">The operation which was performed (should usually be set to 'Serialized' or 'Deserialized').</param>
        /// <param name="parameter">The parameter that was serialized or deserialized.</param>
        [Conditional("LOGGING_ON")]
        public void LogParameter(object source, string operation, object parameter)
        {
            if (parameter == null)
            {
                logger.Log(source, LogLevel.Debug, operation + " null parameter.");
            }
            else
            {
                logger.Log(source, LogLevel.Debug, operation + " parameter of type '" + parameter.GetType().FullName + "'.");
            }
        }

        //******************************************************************************
        //
        // Method: LogDeserializedReturnValue
        //
        //******************************************************************************
        /// <summary>
        /// Logs details of a deserialized return value.
        /// </summary>
        /// <param name="source">The object which created the log event.</param>
        /// <param name="returnValue">The return value.</param>
        [Conditional("LOGGING_ON")]
        public void LogDeserializedReturnValue(object source, object returnValue)
        {
            if (returnValue == null)
            {
                logger.Log(source, LogLevel.Information, "Deserialized string to null return value");
            }
            else
            {
                logger.Log(source, LogLevel.Information, "Deserialized string to return value of type '" + returnValue.GetType().FullName + "'.");
            }
        }

        //******************************************************************************
        //
        // Method: LogSerializedItem
        //
        //******************************************************************************
        /// <summary>
        /// Logs details of a serialized item (e.g. method invocation or return value) if the logging preprocessor identifier is defined.
        /// </summary>
        /// <param name="source">The object which created the log event.</param>
        /// <param name="serializedItem">The serialized item.</param>
        /// <param name="itemType">The type of the serialized item.</param>
        [Conditional("LOGGING_ON")]
        public void LogSerializedItem(object source, string serializedItem, string itemType)
        {
            LogLongString("Serialized " + itemType + " to string", "Complete string content:", source, serializedItem);
        }

        //******************************************************************************
        //
        // Method: LogDecompressedString
        //
        //******************************************************************************
        /// <summary>
        /// Logs details of a decompressed string if the logging preprocessor identifier is defined.
        /// </summary>
        /// <param name="source">The object which created the log event.</param>
        /// <param name="inputString">The decompressed string.</param>
        [Conditional("LOGGING_ON")]
        public void LogDecompressedString(object source, string inputString)
        {
            LogLongString("Created decompressed string", "Complete string content:", source, inputString);
        }

        //******************************************************************************
        //
        // Method: LogCompressedString
        //
        //******************************************************************************
        /// <summary>
        /// Logs details of a compressed string if the logging preprocessor identifier is defined.
        /// </summary>
        /// <param name="source">The object which created the log event.</param>
        /// <param name="inputString">The compressed string.</param>
        [Conditional("LOGGING_ON")]
        public void LogCompressedString(object source, string inputString)
        {
            LogLongString("Created compressed string", "Complete string content:", source, inputString);
        }

        //******************************************************************************
        //
        // Method: LogLongString
        //
        //******************************************************************************
        /// <summary>
        /// Logs details of a large string, and truncates the information level logging if longer than the definied limit.
        /// </summary>
        /// <param name="informationPrefix">The string to prefix to the information level log entry.</param>
        /// <param name="debugPrefix">The string to prefix to the debug level log entry.</param>
        /// <param name="source">The object which created the log event.</param>
        /// <param name="inputString">The large string to log.</param>
        private void LogLongString(string informationPrefix, string debugPrefix, object source, string inputString)
        {
            if (inputString.Length > stringLengthLimit)
            {
                logger.Log(source, LogLevel.Information, informationPrefix + " '" + inputString.Substring(0, stringLengthLimit) + "' (truncated).");
            }
            else
            {
                logger.Log(source, LogLevel.Information, informationPrefix + " '" + inputString + "'.");
            }
            logger.Log(source, LogLevel.Debug, debugPrefix + " '" + inputString + "'.");
        }
    }
}

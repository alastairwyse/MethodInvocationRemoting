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
using ApplicationLogging;
using log4net;

namespace ApplicationLogging.Adapters
{
    //******************************************************************************
    //
    // Class: ApplicationLoggingLog4NetAdapter
    //
    //******************************************************************************
    /// <summary>
    /// Adapts the ApplicationLogging.IApplicationLogger interface to an implementation of the log4net.ILog interface.
    /// </summary>
    /// <remarks>Note that the ILog interface in log4net does not provide a property similar to the ApplicationLogging eventIdentifier by default (i.e. an id number for the log event).  Hence the eventIdentifier is not supported (i.e. not passed to log4net) in this adapter class.  Similarly there is not explicit property for the object creating the log event on the ILog interface.  In log4net this is usually defined when constructing an implementation of the ILog interface (e.g. through the LogManager class), hence to follow the log4net pattern a separate ApplicationLoggingLog4NetAdapter should be provided to each object performing logging.</remarks>
    public class ApplicationLoggingLog4NetAdapter : IApplicationLogger
    {
        private ILog logger;

        //------------------------------------------------------------------------------
        //
        // Method: ApplicationLoggingLog4NetAdapter (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationLogging.Adapters.ApplicationLoggingLog4NetAdapter class.
        /// </summary>
        /// <param name="logger">The Ilog implementation used to log messages into the log4net framework.</param>
        public ApplicationLoggingLog4NetAdapter(ILog logger)
        {
            this.logger = logger;
        }

        #region IApplicationLogger Members

        public void Log(LogLevel level, string text)
        {
            PerformLogging(level, text, null);
        }

        public void Log(object source, LogLevel level, string text)
        {
            PerformLogging(level, text, null);
        }

        public void Log(int eventIdentifier, LogLevel level, string text)
        {
            PerformLogging(level, text, null);
        }

        public void Log(object source, int eventIdentifier, LogLevel level, string text)
        {
            PerformLogging(level, text, null);
        }

        public void Log(LogLevel level, string text, Exception sourceException)
        {
            PerformLogging(level, text, sourceException);
        }

        public void Log(object source, LogLevel level, string text, Exception sourceException)
        {
            PerformLogging(level, text, sourceException);
        }

        public void Log(int eventIdentifier, LogLevel level, string text, Exception sourceException)
        {
            PerformLogging(level, text, sourceException);
        }

        public void Log(object source, int eventIdentifier, LogLevel level, string text, Exception sourceException)
        {
            PerformLogging(level, text, sourceException);
        }

        #endregion

        //------------------------------------------------------------------------------
        //
        // Method: PerformLogging
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Calls the underlying log4net.ILog object to log a message.
        /// </summary>
        /// <remarks>In ApplicationLogging the level of a log event is passed to the Log() method, but in log4net different levels have their own method.  The main purpose of this method is to translate the level parameter received in the ApplicationLogging method call, into the appropriate log4net method call.</remarks>
        /// <param name="level">The level of importance of the log event.</param>
        /// <param name="text">The details of the log event.</param>
        /// <param name="sourceException">The exception which caused the log event.</param>
        private void PerformLogging(LogLevel level, string text, Exception sourceException)
        {
            switch (level)
            {
                case LogLevel.Critical:
                    if (sourceException == null)
                    {
                        logger.Fatal(text);
                    }
                    else
                    {
                        logger.Fatal(text, sourceException);
                    }
                    break;

                case LogLevel.Error:
                    if (sourceException == null)
                    {
                        logger.Error(text);
                    }
                    else
                    {
                        logger.Error(text, sourceException);
                    }
                    break;

                case LogLevel.Warning:
                    if (sourceException == null)
                    {
                        logger.Warn(text);
                    }
                    else
                    {
                        logger.Warn(text, sourceException);
                    }
                    break;

                case LogLevel.Information:
                    if (sourceException == null)
                    {
                        logger.Info(text);
                    }
                    else
                    {
                        logger.Info(text, sourceException);
                    }
                    break;

                case LogLevel.Debug:
                    if (sourceException == null)
                    {
                        logger.Debug(text);
                    }
                    else
                    {
                        logger.Debug(text, sourceException);
                    }
                    break;
            }
        }
    }
}

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

namespace ApplicationMetrics
{
    //******************************************************************************
    //
    // Class: FileMetricLogger
    //
    //******************************************************************************
    /// <summary>
    /// Writes metric and instrumentation events for an application to a file.
    /// </summary>
    public class FileMetricLogger : IMetricLogger, IDisposable
    {
        protected bool disposed;
        private FileMetricLoggerImplementation loggerImplementation;

        //------------------------------------------------------------------------------
        //
        // Method: FileMetricLogger (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.FileMetricLogger class.
        /// </summary>
        /// <param name="separatorCharacter">The character to use to separate fields (e.g. date/time stamp, metric name) in the file.</param>
        /// <param name="filePath">The full path of the file to write the metric events to.</param>
        /// <param name="dequeueOperationLoopInterval">The time to wait in between iterations of the worker thread which dequeues metric events and writes them to the file.</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        public FileMetricLogger(char separatorCharacter, string filePath, int dequeueOperationLoopInterval, bool intervalMetricChecking)
        {
            loggerImplementation = new FileMetricLoggerImplementation(separatorCharacter, filePath, dequeueOperationLoopInterval, intervalMetricChecking);
        }

        //------------------------------------------------------------------------------
        //
        // Method: FileMetricLogger (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.FileMetricLogger class.
        /// </summary>
        /// <param name="separatorCharacter">The character to use to separate fields (e.g. date/time stamp, metric name) in the file.</param>
        /// <param name="filePath">The full path of the file to write the metric events to.</param>
        /// <param name="dequeueOperationLoopInterval">The time to wait in between iterations of the worker thread which dequeues metric events and writes them to the file.</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        /// <param name="appendToFile">Whether to append to an existing file (if it exists) or overwrite.  A value of true causes appending.</param>
        /// <param name="fileEncoding">The character encoding to use in the file.</param>
        public FileMetricLogger(char separatorCharacter, string filePath, int dequeueOperationLoopInterval, bool intervalMetricChecking, bool appendToFile, Encoding fileEncoding)
        {
            loggerImplementation = new FileMetricLoggerImplementation(separatorCharacter, filePath, dequeueOperationLoopInterval, intervalMetricChecking, appendToFile, fileEncoding);
        }

        //------------------------------------------------------------------------------
        //
        // Method: FileMetricLogger (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.FileMetricLogger class.  Note this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
        /// </summary>
        /// <param name="separatorCharacter">The character to use to separate fields (e.g. date/time stamp, metric name) in the file.</param>
        /// <param name="dequeueOperationLoopInterval">The time to wait in between iterations of the worker thread which dequeues metric events and writes them to the file.</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        /// <param name="streamWriter">A test (mock) stream writer.</param>
        /// <param name="dateTime">A test (mock) DateTime object.</param>
        /// <param name="exceptionHandler">A test (mock) exception handler object.</param>
        public FileMetricLogger(char separatorCharacter, int dequeueOperationLoopInterval, bool intervalMetricChecking, IStreamWriter streamWriter, IDateTime dateTime, IExceptionHandler exceptionHandler)
        {
            loggerImplementation = new FileMetricLoggerImplementation(separatorCharacter, dequeueOperationLoopInterval, intervalMetricChecking, streamWriter, dateTime, exceptionHandler);
        }

        //------------------------------------------------------------------------------
        //
        // Method: Start
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Starts a worker thread which calls methods to dequeue metric events and write them to the file.
        /// </summary>
        public void Start()
        {
            loggerImplementation.Start();
        }

        //------------------------------------------------------------------------------
        //
        // Method: Stop
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Stops the worker thread.
        /// </summary>
        public void Stop()
        {
            loggerImplementation.Stop();
        }

        //------------------------------------------------------------------------------
        //
        // Method: Close
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Closes the metric log file.
        /// </summary>
        public void Close()
        {
            loggerImplementation.Close();
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricLogger.Increment(ApplicationMetrics.CountMetric)"]/*'/>
        public void Increment(CountMetric countMetric)
        {
            loggerImplementation.Increment(countMetric);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricLogger.Add(ApplicationMetrics.AmountMetric)"]/*'/>
        public void Add(AmountMetric amountMetric)
        {
            loggerImplementation.Add(amountMetric);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricLogger.Set(ApplicationMetrics.StatusMetric)"]/*'/>
        public void Set(StatusMetric statusMetric)
        {
            loggerImplementation.Set(statusMetric);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricLogger.Begin(ApplicationMetrics.IntervalMetric)"]/*'/>
        public void Begin(IntervalMetric intervalMetric)
        {
            loggerImplementation.Begin(intervalMetric);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricLogger.End(ApplicationMetrics.IntervalMetric)"]/*'/>
        public void End(IntervalMetric intervalMetric)
        {
            loggerImplementation.End(intervalMetric);
        }

        #region Finalize / Dispose Methods

        /// <summary>
        /// Releases the unmanaged resources used by the FileMetricLogger.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~FileMetricLogger()
        {
            Dispose(false);
        }

        //------------------------------------------------------------------------------
        //
        // Method: Dispose
        //
        //------------------------------------------------------------------------------
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
                }
                // Free your own state (unmanaged objects).
                if (loggerImplementation != null)
                {
                    loggerImplementation.Dispose();
                }
                // Set large fields to null.
                loggerImplementation = null;

                disposed = true;
            }
        }

        #endregion
    }
}

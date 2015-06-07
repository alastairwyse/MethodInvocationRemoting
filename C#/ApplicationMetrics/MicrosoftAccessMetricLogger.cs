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

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OperatingSystemAbstraction;

namespace ApplicationMetrics
{
    //******************************************************************************
    //
    // Class: MicrosoftAccessMetricLogger
    //
    //******************************************************************************
    /// <summary>
    /// Writes metric and instrumentation events for an application to a Microsoft Access database.
    /// </summary>
    public class MicrosoftAccessMetricLogger : IMetricLogger, IDisposable
    {
        /// <summary>Indicates whether the object has been disposed.</summary>
        protected bool disposed = false;
        private MicrosoftAccessMetricLoggerImplementation loggerImplementation;

        //------------------------------------------------------------------------------
        //
        // Method: MicrosoftAccessMetricLogger (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MicrosoftAccessMetricLogger class.
        /// </summary>
        /// <remarks>This constructor defaults to using the LoopingWorkerThreadBufferProcessor as the buffer processing strategy, and is maintained for backwards compatibility.</remarks>
        /// <param name="databaseFilePath">The full path to the Microsoft Access data file.</param>
        /// <param name="metricCategoryName">The name of the category which the metric events should be logged under in the database.</param>
        /// <param name="dequeueOperationLoopInterval">The time to wait (in milliseconds) between iterations of the worker thread which dequeues metric events and writes them to the Access database.</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        public MicrosoftAccessMetricLogger(string databaseFilePath, string metricCategoryName, int dequeueOperationLoopInterval, bool intervalMetricChecking)
        {
            loggerImplementation = new MicrosoftAccessMetricLoggerImplementation(databaseFilePath, metricCategoryName, new LoopingWorkerThreadBufferProcessor(dequeueOperationLoopInterval, false), intervalMetricChecking);
        }

        //------------------------------------------------------------------------------
        //
        // Method: MicrosoftAccessMetricLogger (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MicrosoftAccessMetricLogger class.
        /// </summary>
        /// <param name="databaseFilePath">The full path to the Microsoft Access data file.</param>
        /// <param name="metricCategoryName">The name of the category which the metric events should be logged under in the database.</param>
        /// <param name="bufferProcessingStrategy">Object which implements a processing strategy for the buffers (queues).</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        public MicrosoftAccessMetricLogger(string databaseFilePath, string metricCategoryName, IBufferProcessingStrategy bufferProcessingStrategy, bool intervalMetricChecking)
        {
            loggerImplementation = new MicrosoftAccessMetricLoggerImplementation(databaseFilePath, metricCategoryName, bufferProcessingStrategy, intervalMetricChecking);
        }

        //------------------------------------------------------------------------------
        //
        // Method: MicrosoftAccessMetricLogger (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MicrosoftAccessMetricLogger class.  Note this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
        /// </summary>
        /// <param name="databaseFilePath">The full path to the Microsoft Access data file.</param>
        /// <param name="metricCategoryName">The name of the category which the metric events should be logged under in the database.</param>
        /// <param name="bufferProcessingStrategy">Object which implements a processing strategy for the buffers (queues).</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        /// <param name="dbConnection">A test (mock) database connection object.</param>
        /// <param name="dbCommand">A test (mock) database command object.</param>
        /// <param name="dateTime">A test (mock) DateTime object.</param>
        /// <param name="exceptionHandler">A test (mock) exception handler object.</param>
        public MicrosoftAccessMetricLogger(string databaseFilePath, string metricCategoryName, IBufferProcessingStrategy bufferProcessingStrategy, bool intervalMetricChecking, IOleDbConnection dbConnection, IOleDbCommand dbCommand, IDateTime dateTime, IExceptionHandler exceptionHandler)
        {
            loggerImplementation = new MicrosoftAccessMetricLoggerImplementation(databaseFilePath, metricCategoryName, bufferProcessingStrategy, intervalMetricChecking, dbConnection, dbCommand, dateTime, exceptionHandler);
        }

        //------------------------------------------------------------------------------
        //
        // Method: Connect
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Connects to the configured database.
        /// </summary>
        public void Connect()
        {
            loggerImplementation.Connect();
        }

        //------------------------------------------------------------------------------
        //
        // Method: Disconnect
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Disconnects from the configured database.
        /// </summary>
        public void Disconnect()
        {
            loggerImplementation.Disconnect();
        }

        //------------------------------------------------------------------------------
        //
        // Method: Start
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Starts a worker thread which calls methods to dequeue metric events and write them to the database.
        /// </summary>
        /// <remarks>This method is maintained on this class for backwards compatibility, as it is now available on interface IBufferProcessingStrategy.</remarks>
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
        /// <remarks>This method is maintained on this class for backwards compatibility, as it is now available on interface IBufferProcessingStrategy.</remarks>
        public void Stop()
        {
            loggerImplementation.Stop();
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

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricLogger.CancelBegin(ApplicationMetrics.IntervalMetric)"]/*'/>
        public void CancelBegin(IntervalMetric intervalMetric)
        {
            loggerImplementation.CancelBegin(intervalMetric);
        }

        #region Finalize / Dispose Methods

        /// <summary>
        /// Releases the unmanaged resources used by the MicrosoftAccessMetricLogger.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #pragma warning disable 1591
        ~MicrosoftAccessMetricLogger()
        {
            Dispose(false);
        }
        #pragma warning restore 1591

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
                    if (loggerImplementation != null)
                    {
                        loggerImplementation.Dispose();
                        loggerImplementation = null;
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

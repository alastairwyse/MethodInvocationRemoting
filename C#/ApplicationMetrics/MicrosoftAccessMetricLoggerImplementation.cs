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
    // Class: MicrosoftAccessMetricLoggerImplementation
    //
    //******************************************************************************
    /// <summary>
    /// Writes metric and instrumentation events for an application to a Microsoft Access database.
    /// </summary>
    /// <remarks>This class provides underlying functionality for public class MicrosoftAccessMetricLogger.  MicrosoftAccessMetricLogger utilizes this class via composition rather than inheritance to allow MetricLoggerBuffer to remain private within the ApplicationMetrics namespace.</remarks>
    class MicrosoftAccessMetricLoggerImplementation : MetricLoggerBuffer, IDisposable
    {
        const string accessConnectionStringPrefix = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=";

        /// <summary>Indicates whether the object has been disposed.</summary>
        protected bool disposed = false;
        private string databaseFilePath;
        private string metricCategoryName;
        private IOleDbConnection dbConnection;
        private IOleDbCommand dbCommand;

        //------------------------------------------------------------------------------
        //
        // Method: MicrosoftAccessMetricLoggerImplementation (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MicrosoftAccessMetricLoggerImplementation class.
        /// </summary>
        /// <param name="databaseFilePath">The full path to the Microsoft Access data file.</param>
        /// <param name="metricCategoryName">The name of the category which the metric events should be logged under in the database.</param>
        /// <param name="bufferProcessingStrategy">Object which implements a processing strategy for the buffers (queues).</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        public MicrosoftAccessMetricLoggerImplementation(string databaseFilePath, string metricCategoryName, IBufferProcessingStrategy bufferProcessingStrategy, bool intervalMetricChecking)
            : base(bufferProcessingStrategy, intervalMetricChecking)
        {
            InitialisePrivateMembers(databaseFilePath, metricCategoryName);
            dbConnection = new OleDbConnection();
            // Need to cast the connection to an OleDbConnection in order to get the underlying connection
            //   Usually would avoid casting, but in this case it should be safe, as the member was just set to this object type on the previous line
            dbCommand = new OleDbCommand("", ((OleDbConnection)dbConnection).Connection);
        }

        //------------------------------------------------------------------------------
        //
        // Method: MicrosoftAccessMetricLoggerImplementation (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MicrosoftAccessMetricLoggerImplementation class.  Note this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
        /// </summary>
        /// <param name="databaseFilePath">The full path to the Microsoft Access data file.</param>
        /// <param name="metricCategoryName">The name of the category which the metric events should be logged under in the database.</param>
        /// <param name="bufferProcessingStrategy">Object which implements a processing strategy for the buffers (queues).</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        /// <param name="dbConnection">A test (mock) database connection object.</param>
        /// <param name="dbCommand">A test (mock) database command object.</param>
        /// <param name="dateTime">A test (mock) DateTime object.</param>
        /// <param name="exceptionHandler">A test (mock) exception handler object.</param>
        public MicrosoftAccessMetricLoggerImplementation(string databaseFilePath, string metricCategoryName, IBufferProcessingStrategy bufferProcessingStrategy, bool intervalMetricChecking, IOleDbConnection dbConnection, IOleDbCommand dbCommand, IDateTime dateTime, IExceptionHandler exceptionHandler)
            : base(bufferProcessingStrategy, intervalMetricChecking, dateTime, exceptionHandler)
        {
            InitialisePrivateMembers(databaseFilePath, metricCategoryName);
            this.dbConnection = dbConnection;
            this.dbCommand = dbCommand;
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
            if (CheckConnected() == true)
            {
                throw new Exception("Connection to database has already been established.");
            }

            dbConnection.ConnectionString = accessConnectionStringPrefix + databaseFilePath;

            try
            {
                dbConnection.Open();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to connect to database at path '" + databaseFilePath + "'.", e);
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: Disconnect
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Disconnects from the database.
        /// </summary>
        public void Disconnect()
        {
            if (CheckConnected() == true)
            {
                try
                {
                    dbConnection.Close();
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to connect disconnect from database.", e);
                }
            }
        }

        #region Base Class Method Implementations

        //------------------------------------------------------------------------------
        //
        // Method: ProcessCountMetricEvent
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Writes a count metric event to the database.
        /// </summary>
        /// <param name="countMetricEvent">The count metric event to write.</param>
        protected override void ProcessCountMetricEvent(CountMetricEventInstance countMetricEvent)
        {
            StringBuilder sqlInsertStatement = new StringBuilder();
            sqlInsertStatement.Append("INSERT ");
            sqlInsertStatement.Append("INTO    CountMetricInstances ");
            sqlInsertStatement.Append("        ( CmetId, ");
            sqlInsertStatement.Append("          CtgrId, ");
            sqlInsertStatement.Append("          [Timestamp] ");
            sqlInsertStatement.Append("          ) ");
            sqlInsertStatement.Append("SELECT  Cmet.CmetId, ");
            sqlInsertStatement.Append("        Ctgr.CtgrId, ");
            sqlInsertStatement.Append("        '" + countMetricEvent.EventTime.ToString("yyyy-MM-dd HH:mm:ss") + "' ");
            sqlInsertStatement.Append("FROM    CountMetrics Cmet, ");
            sqlInsertStatement.Append("        Categories Ctgr ");
            sqlInsertStatement.Append("WHERE   Cmet.Name = '" + countMetricEvent.Metric.Name + "' ");
            sqlInsertStatement.Append("  AND   Ctgr.Name = '" + metricCategoryName + "';");

            dbCommand.CommandText = sqlInsertStatement.ToString();

            try
            {
                dbCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                exceptionHandler.Handle(new Exception("Failed to insert instance of count metric '" + countMetricEvent.Metric.Name + "'.", e));
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: ProcessAmountMetricEvent
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Writes an amount metric event to the database.
        /// </summary>
        /// <param name="amountMetricEvent">The amount metric event to write.</param>
        protected override void ProcessAmountMetricEvent(AmountMetricEventInstance amountMetricEvent)
        {
            StringBuilder sqlInsertStatement = new StringBuilder();
            sqlInsertStatement.Append("INSERT ");
            sqlInsertStatement.Append("INTO    AmountMetricInstances ");
            sqlInsertStatement.Append("        ( CtgrId, ");
            sqlInsertStatement.Append("          AmetId, ");
            sqlInsertStatement.Append("          Amount, ");
            sqlInsertStatement.Append("          [Timestamp] ");
            sqlInsertStatement.Append("          ) ");
            sqlInsertStatement.Append("SELECT  Ctgr.CtgrId, ");
            sqlInsertStatement.Append("        Amet.AmetId, ");
            sqlInsertStatement.Append("        " + amountMetricEvent.Metric.Amount.ToString() + ", ");
            sqlInsertStatement.Append("        '" + amountMetricEvent.EventTime.ToString("yyyy-MM-dd HH:mm:ss") + "' ");
            sqlInsertStatement.Append("FROM    AmountMetrics Amet, ");
            sqlInsertStatement.Append("        Categories Ctgr ");
            sqlInsertStatement.Append("WHERE   Amet.Name = '" + amountMetricEvent.Metric.Name + "' ");
            sqlInsertStatement.Append("  AND   Ctgr.Name = '" + metricCategoryName + "';");

            dbCommand.CommandText = sqlInsertStatement.ToString();

            try
            {
                dbCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                exceptionHandler.Handle(new Exception("Failed to insert instance of amount metric '" + amountMetricEvent.Metric.Name + "'.", e));
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: ProcessStatusMetricEvent
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Writes a status metric event to the database.
        /// </summary>
        /// <param name="statusMetricEvent">The status metric event to write.</param>
        protected override void ProcessStatusMetricEvent(StatusMetricEventInstance statusMetricEvent)
        {
            StringBuilder sqlInsertStatement = new StringBuilder();
            sqlInsertStatement.Append("INSERT ");
            sqlInsertStatement.Append("INTO    StatusMetricInstances ");
            sqlInsertStatement.Append("        ( CtgrId, ");
            sqlInsertStatement.Append("          SmetId, ");
            sqlInsertStatement.Append("          [Value], ");
            sqlInsertStatement.Append("          [Timestamp] ");
            sqlInsertStatement.Append("          ) ");
            sqlInsertStatement.Append("SELECT  Ctgr.CtgrId, ");
            sqlInsertStatement.Append("        Smet.SmetId, ");
            sqlInsertStatement.Append("        " + statusMetricEvent.Metric.Value.ToString() + ", ");
            sqlInsertStatement.Append("        '" + statusMetricEvent.EventTime.ToString("yyyy-MM-dd HH:mm:ss") + "' ");
            sqlInsertStatement.Append("FROM    StatusMetrics Smet, ");
            sqlInsertStatement.Append("        Categories Ctgr ");
            sqlInsertStatement.Append("WHERE   Smet.Name = '" + statusMetricEvent.Metric.Name + "' ");
            sqlInsertStatement.Append("  AND   Ctgr.Name = '" + metricCategoryName + "';");

            dbCommand.CommandText = sqlInsertStatement.ToString();

            try
            {
                dbCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                exceptionHandler.Handle(new Exception("Failed to insert instance of status metric '" + statusMetricEvent.Metric.Name + "'.", e));
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: ProcessIntervalMetricEvent
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Writes an interval metric event to the database.
        /// </summary>
        /// <param name="intervalMetricEvent">The interval metric event to write.</param>
        /// <param name="duration">The duration of the interval metric event in milliseconds.</param>
        protected override void ProcessIntervalMetricEvent(IntervalMetricEventInstance intervalMetricEvent, long duration)
        {
            StringBuilder sqlInsertStatement = new StringBuilder();
            sqlInsertStatement.Append("INSERT ");
            sqlInsertStatement.Append("INTO    IntervalMetricInstances ");
            sqlInsertStatement.Append("        ( CtgrId, ");
            sqlInsertStatement.Append("          ImetId, ");
            sqlInsertStatement.Append("          MilliSeconds, ");
            sqlInsertStatement.Append("          [Timestamp] ");
            sqlInsertStatement.Append("          ) ");
            sqlInsertStatement.Append("SELECT  Ctgr.CtgrId, ");
            sqlInsertStatement.Append("        Imet.ImetId, ");
            sqlInsertStatement.Append("        " + duration.ToString() + ", ");
            sqlInsertStatement.Append("        '" + intervalMetricEvent.EventTime.ToString("yyyy-MM-dd HH:mm:ss") + "' ");
            sqlInsertStatement.Append("FROM    IntervalMetrics Imet, ");
            sqlInsertStatement.Append("        Categories Ctgr ");
            sqlInsertStatement.Append("WHERE   Imet.Name = '" + intervalMetricEvent.Metric.Name + "' ");
            sqlInsertStatement.Append("  AND   Ctgr.Name = '" + metricCategoryName + "';");

            dbCommand.CommandText = sqlInsertStatement.ToString();

            try
            {
                dbCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                exceptionHandler.Handle(new Exception("Failed to insert instance of interval metric '" + intervalMetricEvent.Metric.Name + "'.", e));
            }
        }

        # endregion

        #region Private Methods

        //------------------------------------------------------------------------------
        //
        // Method: InitialisePrivateMembers
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises private members of the class.
        /// </summary>
        /// <param name="databaseFilePath">The full path to the Microsoft Access data file.</param>
        /// <param name="metricCategoryName">The name of the category which the metric events should be logged under in the database.</param>
        private void InitialisePrivateMembers(string databaseFilePath, string metricCategoryName)
        {
            this.databaseFilePath = databaseFilePath;
            if (metricCategoryName.Trim() != "")
            {
                this.metricCategoryName = metricCategoryName;
            }
            else
            {
                throw new ArgumentException("Argument 'metricCategoryName' cannot be blank.", "metricCategoryName");
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: CheckConnected
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Checks whether a connection to the database has been established.
        /// </summary>
        /// <returns>True if currently connected.</returns>
        private bool CheckConnected()
        {
            if ((dbConnection.State == System.Data.ConnectionState.Connecting) ||
                (dbConnection.State == System.Data.ConnectionState.Executing) ||
                (dbConnection.State == System.Data.ConnectionState.Fetching) ||
                (dbConnection.State == System.Data.ConnectionState.Open))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        # endregion

        #region Finalize / Dispose Methods

        /// <summary>
        /// Releases the unmanaged resources used by the MicrosoftAccessMetricLoggerImplementation.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #pragma warning disable 1591
        ~MicrosoftAccessMetricLoggerImplementation()
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
                    if (dbCommand != null)
                    {
                        dbCommand.Dispose();
                        dbCommand = null;
                    }
                    if (dbConnection != null)
                    {
                        dbConnection.Dispose();
                        dbConnection = null;
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

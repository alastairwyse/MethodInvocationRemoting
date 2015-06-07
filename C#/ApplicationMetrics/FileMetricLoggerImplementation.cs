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
using OperatingSystemAbstraction;

namespace ApplicationMetrics
{
    //******************************************************************************
    //
    // Class: FileMetricLoggerImplementation
    //
    //******************************************************************************
    /// <summary>
    /// Writes metric and instrumentation events for an application to a file.
    /// </summary>
    /// <remarks>This class provides underlying functionality for public class FileMetricLogger.  FileMetricLogger utilizes this class via composition rather than inheritance to allow MetricLoggerBuffer to remain private within the ApplicationMetrics namespace.</remarks>
    class FileMetricLoggerImplementation : MetricLoggerBuffer, IDisposable
    {
        /// <summary>Indicates whether the object has been disposed.</summary>
        protected bool disposed = false;
        private const string dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private char separatorCharacter;
        private IStreamWriter streamWriter;
        private Encoding fileEncoding = Encoding.UTF8;

        //------------------------------------------------------------------------------
        //
        // Method: FileMetricLoggerImplementation (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.FileMetricLoggerImplementation class.
        /// </summary>
        /// <param name="separatorCharacter">The character to use to separate fields (e.g. date/time stamp, metric name) in the file.</param>
        /// <param name="filePath">The full path of the file to write the metric events to.</param>
        /// <param name="bufferProcessingStrategy">Object which implements a processing strategy for the buffers (queues).</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        public FileMetricLoggerImplementation(char separatorCharacter, string filePath, IBufferProcessingStrategy bufferProcessingStrategy, bool intervalMetricChecking)
            : base(bufferProcessingStrategy, intervalMetricChecking)
        {
            this.separatorCharacter = separatorCharacter;
            streamWriter = new StreamWriter(filePath, false, fileEncoding);
        }

        //------------------------------------------------------------------------------
        //
        // Method: FileMetricLoggerImplementation (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.FileMetricLoggerImplementation class.
        /// </summary>
        /// <param name="separatorCharacter">The character to use to separate fields (e.g. date/time stamp, metric name) in the file.</param>
        /// <param name="filePath">The full path of the file to write the metric events to.</param>
        /// <param name="bufferProcessingStrategy">Object which implements a processing strategy for the buffers (queues).</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        /// <param name="appendToFile">Whether to append to an existing file (if it exists) or overwrite.  A value of true causes appending.</param>
        /// <param name="fileEncoding">The character encoding to use in the file.</param>
        public FileMetricLoggerImplementation(char separatorCharacter, string filePath, IBufferProcessingStrategy bufferProcessingStrategy, bool intervalMetricChecking, bool appendToFile, Encoding fileEncoding)
            : base(bufferProcessingStrategy, intervalMetricChecking)
        {
            this.separatorCharacter = separatorCharacter;
            streamWriter = new StreamWriter(filePath, intervalMetricChecking, fileEncoding);
        }

        //------------------------------------------------------------------------------
        //
        // Method: FileMetricLoggerImplementation (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.FileMetricLoggerImplementation class.  Note this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
        /// </summary>
        /// <param name="separatorCharacter">The character to use to separate fields (e.g. date/time stamp, metric name) in the file.</param>
        /// <param name="bufferProcessingStrategy">Object which implements a processing strategy for the buffers (queues).</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        /// <param name="streamWriter">A test (mock) stream writer.</param>
        /// <param name="dateTime">A test (mock) DateTime object.</param>
        /// <param name="exceptionHandler">A test (mock) exception handler object.</param>
        public FileMetricLoggerImplementation(char separatorCharacter, IBufferProcessingStrategy bufferProcessingStrategy, bool intervalMetricChecking, IStreamWriter streamWriter, IDateTime dateTime, IExceptionHandler exceptionHandler)
            : base(bufferProcessingStrategy, intervalMetricChecking, dateTime, exceptionHandler)
        {
            this.separatorCharacter = separatorCharacter;
            this.streamWriter = streamWriter;
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
            streamWriter.Close();
        }

        #region Base Class Method Implementations

        protected override void ProcessCountMetricEvent(CountMetricEventInstance countMetricEvent)
        {
            StringBuilder stringBuilder = InitializeStringBuilder(countMetricEvent.EventTime.ToLocalTime());
            stringBuilder.Append(countMetricEvent.Metric.Name);
            streamWriter.WriteLine(stringBuilder.ToString());
            streamWriter.Flush();
        }

        protected override void ProcessAmountMetricEvent(AmountMetricEventInstance amountMetricEvent)
        {
            StringBuilder stringBuilder = InitializeStringBuilder(amountMetricEvent.EventTime.ToLocalTime());
            stringBuilder.Append(amountMetricEvent.Metric.Name);
            AppendSeparatorCharacter(stringBuilder);
            stringBuilder.Append(amountMetricEvent.Metric.Amount);
            streamWriter.WriteLine(stringBuilder.ToString());
            streamWriter.Flush();
        }

        protected override void ProcessStatusMetricEvent(StatusMetricEventInstance statusMetricEvent)
        {
            StringBuilder stringBuilder = InitializeStringBuilder(statusMetricEvent.EventTime.ToLocalTime());
            stringBuilder.Append(statusMetricEvent.Metric.Name);
            AppendSeparatorCharacter(stringBuilder);
            stringBuilder.Append(statusMetricEvent.Metric.Value);
            streamWriter.WriteLine(stringBuilder.ToString());
            streamWriter.Flush();
        }

        protected override void ProcessIntervalMetricEvent(IntervalMetricEventInstance intervalMetricEvent, long duration)
        {
            StringBuilder stringBuilder = InitializeStringBuilder(intervalMetricEvent.EventTime.ToLocalTime());
            stringBuilder.Append(intervalMetricEvent.Metric.Name);
            AppendSeparatorCharacter(stringBuilder);
            stringBuilder.Append(duration);
            streamWriter.WriteLine(stringBuilder.ToString());
            streamWriter.Flush();
        }

        #endregion

        #region Private Methods

        //------------------------------------------------------------------------------
        //
        // Method: InitializeStringBuilder
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Creates and returns a StringBuilder class, with the specified timestamp written to it.
        /// </summary>
        /// <param name="timeStamp">The timestamp to write to the StringBuilder.</param>
        /// <returns>The initialized string builder.</returns>
        private StringBuilder InitializeStringBuilder(System.DateTime timeStamp)
        {
            StringBuilder returnStringBuilder = new StringBuilder();
            returnStringBuilder.Append(timeStamp.ToString(dateTimeFormat));
            AppendSeparatorCharacter(returnStringBuilder);
            return returnStringBuilder;
        }

        //------------------------------------------------------------------------------
        //
        // Method: AppendSeparatorCharacter
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Appends the separator character to a StringBuilder object.
        /// </summary>
        /// <param name="stringBuilder">The StringBuilder to append the separator character to.</param>
        private void AppendSeparatorCharacter(StringBuilder stringBuilder)
        {
            stringBuilder.Append(" ");
            stringBuilder.Append(separatorCharacter);
            stringBuilder.Append(" ");
        }

        #endregion

        #region Finalize / Dispose Methods

        /// <summary>
        /// Releases the unmanaged resources used by the FileMetricLoggerImplementation.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #pragma warning disable 1591
        ~FileMetricLoggerImplementation()
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

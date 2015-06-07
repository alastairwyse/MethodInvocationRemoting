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

namespace ApplicationMetrics
{
    //******************************************************************************
    //
    // Class: SizeLimitedBufferProcessor
    //
    //******************************************************************************
    /// <summary>
    /// Implements a buffer processing strategy for MetricLoggerBuffer classes, whereby when the total size of the buffers reaches a defined limit, a worker thread is signaled to process the buffers.
    /// </summary>
    public class SizeLimitedBufferProcessor : WorkerThreadBufferProcessorBase, IDisposable
    {
        private int bufferSizeLimit;
        private AutoResetEvent bufferProcessSignal;
        /// <summary>Indicates whether the object has been disposed.</summary>
        protected bool disposed;

        //------------------------------------------------------------------------------
        //
        // Method: SizeLimitedBufferProcessor (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.SizeLimitedBufferProcessor class.
        /// </summary>
        /// <param name="bufferSizeLimit">The total size of the buffers which when reached, triggers processing of the buffer contents.</param>
        public SizeLimitedBufferProcessor(int bufferSizeLimit)
            : base()
        {
            disposed = false;
            this.bufferSizeLimit = bufferSizeLimit;
            bufferProcessSignal = new AutoResetEvent(false);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.Start"]/*'/>
        public override void Start()
        {
            bufferProcessingWorkerThread = new Thread(delegate()
            {
                while (cancelRequest == false)
                {
                    bufferProcessSignal.WaitOne();
                    if (cancelRequest == false)
                    {
                        OnBufferProcessed(EventArgs.Empty);
                    }
                }
                if (TotalMetricEventsBufferred > 0 && processRemainingBufferredMetricsOnStop == true)
                {
                    OnBufferProcessed(EventArgs.Empty);
                }
            });

            base.Start();
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.Stop"]/*'/>
        public override void Stop()
        {
            cancelRequest = true;
            bufferProcessSignal.Set();
            if (bufferProcessingWorkerThread != null)
            {
                bufferProcessingWorkerThread.Join();
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.NotifyCountMetricEventBuffered"]/*'/>
        public override void NotifyCountMetricEventBuffered()
        {
            base.NotifyCountMetricEventBuffered();
            CheckBufferLimitReached();
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.NotifyAmountMetricEventBuffered"]/*'/>
        public override void NotifyAmountMetricEventBuffered()
        {
            base.NotifyAmountMetricEventBuffered();
            CheckBufferLimitReached();
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.NotifyStatusMetricEventBuffered"]/*'/>
        public override void NotifyStatusMetricEventBuffered()
        {
            base.NotifyStatusMetricEventBuffered();
            CheckBufferLimitReached();
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.NotifyIntervalMetricEventBuffered"]/*'/>
        public override void NotifyIntervalMetricEventBuffered()
        {
            base.NotifyIntervalMetricEventBuffered();
            CheckBufferLimitReached();
        }

        #region Private Methods

        //------------------------------------------------------------------------------
        //
        // Method: CheckBufferLimitReached
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Checks whether the size limit for the buffers has been reached, and if so, signals the worker thread to process the buffers.
        /// </summary>
        private void CheckBufferLimitReached()
        {
            if (TotalMetricEventsBufferred >= bufferSizeLimit)
            {
                bufferProcessSignal.Set();
            }
        }

        #endregion

        #region Finalize / Dispose Methods

        /// <summary>
        /// Releases the unmanaged resources used by the SizeLimitedBufferProcessor.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #pragma warning disable 1591
        ~SizeLimitedBufferProcessor()
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
                    Stop();
                    bufferProcessSignal.Close();
                }
                // Free your own state (unmanaged objects).

                // Set large fields to null.

                disposed = true;
            }
        }

        #endregion
    }
}

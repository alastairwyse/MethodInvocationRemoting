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
    // Class: WorkerThreadBufferProcessorBase
    //
    //******************************************************************************
    /// <summary>
    /// Provides common functionality for classes implementing interface IBufferProcessingStrategy, which use a worker thread to implement a buffer processing strategy.
    /// </summary>
    public abstract class WorkerThreadBufferProcessorBase : IBufferProcessingStrategy
    {
        /// <summary>The number of count metric events currently stored in the buffer.</summary>
        protected int countMetricEventsBuffered;
        /// <summary>The number of amount metric events currently stored in the buffer.</summary>
        protected int amountMetricEventsBuffered;
        /// <summary>The number of status metric events currently stored in the buffer.</summary>
        protected int statusMetricEventsBuffered;
        /// <summary>The number of interval metric events currently stored in the buffer.</summary>
        protected int intervalMetricEventsBuffered;
        /// <summary>Worker thread which implements the strategy to process the contents of the buffers.</summary>
        protected Thread bufferProcessingWorkerThread;
        /// <summary>Whether a stop/cancel request has been received.</summary>
        protected volatile bool cancelRequest;
        /// <summary>Whether any metric events remaining in the buffers when the Stop() method is called should be processed.</summary>
        protected volatile bool processRemainingBufferredMetricsOnStop;

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="E:ApplicationMetrics.IBufferProcessingStrategy.BufferProcessed"]/*'/>
        public event EventHandler BufferProcessed;

        /// <summary>
        /// The total number of metric events currently stored across all buffers.
        /// </summary>
        /// <remarks>Note that the counter members accessed in this property may be accessed by multiple threads (i.e. the worker thread in member bufferProcessingWorkerThread and the client code in the main thread).  This property should only be read from methods which have locks around the queues in the corresponding MetricLoggerBuffer class (e.g. overrides of the virtual 'Notify' methods defined in this class, which are called from the Add(), Set(), etc... methods in the MetricLoggerBuffer class).</remarks>
        protected virtual long TotalMetricEventsBufferred
        {
            get
            {
                return countMetricEventsBuffered + amountMetricEventsBuffered + statusMetricEventsBuffered + intervalMetricEventsBuffered;
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: WorkerThreadBufferProcessorBase (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.WorkerThreadBufferProcessorBase class.
        /// </summary>
        public WorkerThreadBufferProcessorBase()
        {
            countMetricEventsBuffered = 0;
            amountMetricEventsBuffered = 0;
            statusMetricEventsBuffered = 0;
            intervalMetricEventsBuffered = 0;
            processRemainingBufferredMetricsOnStop = true;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.Start"]/*'/>
        public virtual void Start()
        {
            cancelRequest = false;
            bufferProcessingWorkerThread.Name = "ApplicationMetrics.WorkerThreadBufferProcessorBase metric event buffer processing worker thread.";
            bufferProcessingWorkerThread.Start();
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.Stop"]/*'/>
        public virtual void Stop()
        {
            cancelRequest = true;
            if (bufferProcessingWorkerThread != null)
            {
                bufferProcessingWorkerThread.Join();
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.Stop(System.Boolean)"]/*'/>
        public virtual void Stop(bool processRemainingBufferedMetricEvents)
        {
            this.processRemainingBufferredMetricsOnStop = processRemainingBufferedMetricEvents;
            Stop();
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.NotifyCountMetricEventBuffered"]/*'/>
        public virtual void NotifyCountMetricEventBuffered()
        {
            countMetricEventsBuffered++;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.NotifyAmountMetricEventBuffered"]/*'/>
        public virtual void NotifyAmountMetricEventBuffered()
        {
            amountMetricEventsBuffered++;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.NotifyStatusMetricEventBuffered"]/*'/>
        public virtual void NotifyStatusMetricEventBuffered()
        {
            statusMetricEventsBuffered++;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.NotifyIntervalMetricEventBuffered"]/*'/>
        public virtual void NotifyIntervalMetricEventBuffered()
        {
            intervalMetricEventsBuffered++;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.NotifyCountMetricEventBufferCleared"]/*'/>
        public virtual void NotifyCountMetricEventBufferCleared()
        {
            countMetricEventsBuffered = 0;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.NotifyAmountMetricEventBufferCleared"]/*'/>
        public virtual void NotifyAmountMetricEventBufferCleared()
        {
            amountMetricEventsBuffered = 0;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.NotifyStatusMetricEventBufferCleared"]/*'/>
        public virtual void NotifyStatusMetricEventBufferCleared()
        {
            statusMetricEventsBuffered = 0;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.NotifyIntervalMetricEventBufferCleared"]/*'/>
        public virtual void NotifyIntervalMetricEventBufferCleared()
        {
            intervalMetricEventsBuffered = 0;
        }

        /// <summary>
        /// Raises the BufferProcessed event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnBufferProcessed(EventArgs e)
        {
            if (BufferProcessed != null)
            {
                BufferProcessed(this, e);
            }
        }
    }
}

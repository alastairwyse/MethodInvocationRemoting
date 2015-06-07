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
    // Class: LoopingWorkerThreadBufferProcessor
    //
    //******************************************************************************
    /// <summary>
    /// Implements a buffer processing strategy for MetricLoggerBuffer classes, using a worker thread which dequeues and processes buffered metric events at a regular interval.
    /// </summary>
    public class LoopingWorkerThreadBufferProcessor : WorkerThreadBufferProcessorBase
    {
        private int dequeueOperationLoopInterval;
        private bool unitTestMode;

        //------------------------------------------------------------------------------
        //
        // Method: LoopingWorkerThreadBufferProcessor (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.LoopingWorkerThreadBufferProcessor class.
        /// </summary>
        /// <param name="dequeueOperationLoopInterval">The time to wait (in milliseconds) between iterations of the worker thread which dequeues and processes metric events.</param>
        /// <param name="unitTestMode">Whether the class should run in unit test mode, whereby the worker thread is stopped after one iteration.</param>
        public LoopingWorkerThreadBufferProcessor(int dequeueOperationLoopInterval, bool unitTestMode) 
            : base()
        {
            if (dequeueOperationLoopInterval < 0)
            {
                throw new ArgumentOutOfRangeException("dequeueOperationLoopInterval", dequeueOperationLoopInterval, "Argument 'dequeueOperationLoopInterval' must be greater than or equal to 0.");
            }

            this.dequeueOperationLoopInterval = dequeueOperationLoopInterval;
            this.unitTestMode = unitTestMode;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.Start"]/*'/>
        public override void Start()
        {
            bufferProcessingWorkerThread = new Thread(delegate()
            {
                while (cancelRequest == false)
                {
                    OnBufferProcessed(EventArgs.Empty);
                    if (dequeueOperationLoopInterval > 0)
                    {
                        Thread.Sleep(dequeueOperationLoopInterval);
                    }
                    // If the code is being tested, allow only a single iteration of the loop
                    if (unitTestMode == true)
                    {
                        break;
                    }
                }
                if (TotalMetricEventsBufferred > 0 && processRemainingBufferredMetricsOnStop == true)
                {
                    OnBufferProcessed(EventArgs.Empty);
                }
            });

            base.Start();
        }
    }
}

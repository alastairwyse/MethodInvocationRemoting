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
using ApplicationMetrics;

namespace MethodInvocationRemoting
{
    //******************************************************************************
    //
    // Class: MetricsUtilities
    //
    //******************************************************************************
    /// <summary>
    /// Contains common methods used log application metric events.
    /// </summary>
    class MetricsUtilities
    {
        private IMetricLogger metricLogger;

        //------------------------------------------------------------------------------
        //
        // Method: MetricsUtilities (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MetricsUtilities class.
        /// </summary>
        /// <param name="metricLogger">The metric logger to write metric events to.</param>
        public MetricsUtilities(IMetricLogger metricLogger)
        {
            this.metricLogger = metricLogger;
        }

        //------------------------------------------------------------------------------
        //
        // Method: Increment
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Records a single instance of the specified count event.
        /// </summary>
        /// <param name="countMetric">The count metric that occurred.</param>
        [Conditional("METRICS_ON")]
        public void Increment(CountMetric countMetric)
        {
            metricLogger.Increment(countMetric);
        }

        //------------------------------------------------------------------------------
        //
        // Method: Add
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Records an instance of the specified amount metric event, and the associated amount.
        /// </summary>
        /// <param name="amountMetric">The amount metric that occurred.</param>
        [Conditional("METRICS_ON")]
        public void Add(AmountMetric amountMetric)
        {
            metricLogger.Add(amountMetric);
        }

        //------------------------------------------------------------------------------
        //
        // Method: Set
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Records an instance of the specified status metric event, and the associated value.
        /// </summary>
        /// <param name="statusMetric">The status metric that occurred.</param>
        [Conditional("METRICS_ON")]
        public void Set(StatusMetric statusMetric)
        {
            metricLogger.Set(statusMetric);
        }

        //------------------------------------------------------------------------------
        //
        // Method: Begin
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Records the starting of the specified interval event.
        /// </summary>
        /// <param name="intervalMetric">The interval metric that started.</param>
        [Conditional("METRICS_ON")]
        public void Begin(IntervalMetric intervalMetric)
        {
            metricLogger.Begin(intervalMetric);
        }

        //------------------------------------------------------------------------------
        //
        // Method: End
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Records the completion of the specified interval event.
        /// </summary>
        /// <param name="intervalMetric">The interval metric that completed.</param>
        [Conditional("METRICS_ON")]
        public void End(IntervalMetric intervalMetric)
        {
            metricLogger.End(intervalMetric);
        }
    }
}

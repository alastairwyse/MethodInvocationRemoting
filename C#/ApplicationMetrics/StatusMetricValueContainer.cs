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

namespace ApplicationMetrics
{
    //******************************************************************************
    //
    // Class: StatusMetricValueContainer
    //
    //******************************************************************************
    /// <summary>
    /// Container class which stores a status metric, and the value of the most recent instance of the metric.
    /// </summary>
    class StatusMetricValueContainer
    {
        private StatusMetric statusMetric;
        private long value;

        /// <summary>
        /// The status metric for which the most recent value is stored.
        /// </summary>
        public StatusMetric StatusMetric
        {
            get
            {
                return statusMetric;
            }
        }

        /// <summary>
        /// The value of the most recent instance of the metric.
        /// </summary>
        public long Value
        {
            get
            {
                return value;
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: StatusMetricValueContainer (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.StatusMetricValueContainer class.
        /// </summary>
        /// <param name="statusMetric">The status metric for which the most recent value stored.</param>
        public StatusMetricValueContainer(StatusMetric statusMetric)
        {
            this.statusMetric = statusMetric;
            value = 0;
        }

        //------------------------------------------------------------------------------
        //
        // Method: Set
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Sets the stored value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Set(long value)
        {
            this.value = value;
        }
    }
}

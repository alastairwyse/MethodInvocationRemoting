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
    // Class: MetricEventInstance
    //
    //******************************************************************************
    /// <summary>
    /// Base container class which stores information about the occurrence of a metric event.
    /// </summary>
    /// <typeparam name="T">The type of metric the event information should be stored for.</typeparam>
    abstract class MetricEventInstance<T> where T:MetricBase
    {
        /// <summary>The metric that occurred.</summary>
        protected T metric;
        /// <summary>The date and time the event occurred, expressed as UTC.</summary>
        protected DateTime eventTime;

        /// <summary>
        /// The metric that occurred.
        /// </summary>
        public T Metric
        {
            get
            {
                return metric;
            }
        }

        /// <summary>
        /// Returns the type of the metric that occurred.
        /// </summary>
        public Type MetricType
        {
            get
            {
                return metric.GetType();
            }
        }

        /// <summary>
        /// The date and time the event occurred, expressed as UTC.
        /// </summary>
        public DateTime EventTime
        {
            get
            {
                return eventTime;
            }
        }
    }
}

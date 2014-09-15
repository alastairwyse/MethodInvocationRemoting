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
    // Class: CountMetricEventInstance
    //
    //******************************************************************************
    /// <summary>
    /// Container class which stores information about the occurrence of a count metric event.
    /// </summary>
    class CountMetricEventInstance : MetricEventInstance<CountMetric>
    {
        //------------------------------------------------------------------------------
        //
        // Method: CountMetricEventInstance (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.CountMetricEventInstance class.
        /// </summary>
        /// <param name="countMetric">The metric which occurred.</param>
        /// <param name="eventTime">The date and time the metric event occurred, expressed as UTC.</param>
        public CountMetricEventInstance(CountMetric countMetric, DateTime eventTime)
        {
            base.metric = countMetric;
            base.eventTime = eventTime;
        }
    }
}

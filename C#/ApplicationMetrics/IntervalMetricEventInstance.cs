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
    /// <summary>
    /// Represents the time point of an instance of an interval metric event.
    /// </summary>
    enum IntervalMetricEventTimePoint
    {
        /// <summary>The start of the interval metric event.</summary>
        Start, 
        /// <summary>The completion of the interval metric event.</summary>
        End
    }

    //******************************************************************************
    //
    // Class: IntervalMetricEventInstance
    //
    //******************************************************************************
    /// <summary>
    /// Container class which stores information about the occurrence of an interval metric event.
    /// </summary>
    class IntervalMetricEventInstance : MetricEventInstance<IntervalMetric>
    {
        private IntervalMetricEventTimePoint timePoint;

        /// <summary>
        /// Whether the event represents the start or the end of the interval metric.
        /// </summary>
        public IntervalMetricEventTimePoint TimePoint
        {
            get
            {
                return timePoint;
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: IntervalMetricEventInstance (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.IntervalMetricEventInstance class.
        /// </summary>
        /// <param name="intervalMetric">The metric which occurred.</param>
        /// <param name="timePoint">Whether the event represents the start or the end of the interval metric.</param>
        /// <param name="eventTime">The date and time the metric event started, expressed as UTC.</param>
        public IntervalMetricEventInstance(IntervalMetric intervalMetric, IntervalMetricEventTimePoint timePoint, DateTime eventTime)
        {
            base.metric = intervalMetric;
            this.timePoint = timePoint;
            base.eventTime = eventTime;
        }
    }
}

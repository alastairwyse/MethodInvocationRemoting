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
    // Class: IntervalMetricTotalContainer
    //
    //******************************************************************************
    /// <summary>
    /// Container class which stores an interval metric, and the total interval of all instances of the metric.
    /// </summary>
    class IntervalMetricTotalContainer
    {
        private IntervalMetric intervalMetric;
        private long total;

        /// <summary>
        /// The interval metric for which the total is stored.
        /// </summary>
        public IntervalMetric IntervalMetric
        {
            get
            {
                return intervalMetric;
            }
        }

        /// <summary>
        /// The total interval of all instances of the metric.
        /// </summary>
        public long Total
        {
            get
            {
                return total;
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: IntervalMetricTotalContainer (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.IntervalMetricTotalContainer class.
        /// </summary>
        /// <param name="intervalMetric">The interval metric for which the total stored.</param>
        public IntervalMetricTotalContainer(IntervalMetric intervalMetric)
        {
            this.intervalMetric = intervalMetric;
            total = 0;
        }

        //------------------------------------------------------------------------------
        //
        // Method: Add
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Adds the specified amount to the stored total.
        /// </summary>
        /// <param name="amount">The amount to add.</param>
        public void Add(long amount)
        {
            if ((long.MaxValue - total) >= amount)
            {
                total = total + amount;
            }
        }
    }
}

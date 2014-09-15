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
    // Class: CountMetricTotalContainer
    //
    //******************************************************************************
    /// <summary>
    /// Container class which stores a count metric, and the total of the number of instances of the metric.
    /// </summary>
    class CountMetricTotalContainer
    {
        private CountMetric countMetric;
        private long totalCount;

        /// <summary>
        /// The count metric for which the total number of instances is stored.
        /// </summary>
        public CountMetric CountMetric
        {
            get
            {
                return countMetric;
            }
        }

        /// <summary>
        /// The total number of instances of the count metric.
        /// </summary>
        public long TotalCount
        {
            get
            {
                return totalCount;
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: CountMetricTotalContainer (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.CountMetricTotalContainer class.
        /// </summary>
        /// <param name="countMetric">The count metric for which the total number of instances is stored.</param>
        public CountMetricTotalContainer(CountMetric countMetric)
        {
            this.countMetric = countMetric;
            totalCount = 0;
        }

        //------------------------------------------------------------------------------
        //
        // Method: Increment
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Increments the total number of instances by 1.
        /// </summary>
        public void Increment()
        {
            if (totalCount != long.MaxValue)
            {
                totalCount++;
            }
        }
    }
}

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
    // Class: AmountMetricTotalContainer
    //
    //******************************************************************************
    /// <summary>
    /// Container class which stores an amount metric, and the total amount of all instances of the metric.
    /// </summary>
    class AmountMetricTotalContainer
    {
        private AmountMetric amountMetric;
        private long total;

        /// <summary>
        /// The amount metric for which the total is stored.
        /// </summary>
        public AmountMetric AmountMetric
        {
            get
            {
                return amountMetric;
            }
        }

        /// <summary>
        /// The total amount of all instances of the metric.
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
        // Method: AmountMetricTotalContainer (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.AmountMetricTotalContainer class.
        /// </summary>
        /// <param name="amountMetric">The amount metric for which the total stored.</param>
        public AmountMetricTotalContainer(AmountMetric amountMetric)
        {
            this.amountMetric = amountMetric;
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

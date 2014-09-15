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
    // Class: MetricAggregateContainer
    //
    //******************************************************************************
    /// <summary>
    /// Container class which stores definitions of aggregates of metrics.
    /// </summary>
    /// <typeparam name="TNumerator">The type of the numerator of the metric aggregate.</typeparam>
    /// <typeparam name="TDenominator">The type of the denominator of the metric aggregate.</typeparam>
    class MetricAggregateContainer<TNumerator, TDenominator> : MetricAggregateContainerBase<TNumerator>
    {
        /// <summary>The metric representing the denominator of the aggregate.</summary>
        protected TDenominator denominatorMetric;

        /// <summary>
        /// The type of the denominator of the metric aggregate.
        /// </summary>
        public Type DenominatorMetricType
        {
            get
            {
                return denominatorMetric.GetType();
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: MetricAggregateContainer (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MetricAggregateContainer class.
        /// </summary>
        /// <param name="numeratorMetric">The metric which is the numerator of the aggregate.</param>
        /// <param name="denominatorMetric">The metric which is the denominator of the aggregate.</param>
        /// <param name="name">The name of the metric aggregate.</param>
        /// <param name="description">A description of the metric aggregate, explaining what it measures and/or represents.</param>
        public MetricAggregateContainer(TNumerator numeratorMetric, TDenominator denominatorMetric, string name, string description)
            : base (numeratorMetric, name, description)
        {
            this.denominatorMetric = denominatorMetric;
        }
    }

    //******************************************************************************
    //
    // Class: MetricAggregateContainer
    //
    //******************************************************************************
    /// <summary>
    /// Container class which stores definitions of aggregates of metrics where the denominator of the metric is a unit of time.
    /// </summary>
    /// <typeparam name="TNumerator">The type of the numerator of the metric aggregate.</typeparam>
    class MetricAggregateContainer<TNumerator> : MetricAggregateContainerBase<TNumerator>
    {
        /// <summary>The time unit representing the denominator of the metric aggregate.</summary>
        protected TimeUnit timeUnit;

        /// <summary>
        /// The time unit representing the denominator of the metric aggregate.
        /// </summary>
        public TimeUnit DenominatorTimeUnit
        {
            get
            {
                return timeUnit;
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: MetricAggregateContainer (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MetricAggregateContainer class.
        /// </summary>
        /// <param name="numeratorMetric">The metric which is the numerator of the aggregate.</param>
        /// <param name="timeUnit">The time unit representing the denominator of the metric aggregate.</param>
        /// <param name="name">The name of the metric aggregate.</param>
        /// <param name="description">A description of the metric aggregate, explaining what it measures and/or represents.</param>
        public MetricAggregateContainer(TNumerator numeratorMetric, TimeUnit timeUnit, string name, string description)
            : base(numeratorMetric, name, description)
        {
            this.timeUnit = timeUnit;
        }
    }
}

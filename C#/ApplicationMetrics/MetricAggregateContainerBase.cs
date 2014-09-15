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
    // Class: MetricAggregateContainerBase
    //
    //******************************************************************************
    /// <summary>
    /// Base class for metric aggregate containers containing common properties.
    /// </summary>
    /// <typeparam name="TNumerator">The type representing the numerator of the aggregate.</typeparam>
    class MetricAggregateContainerBase<TNumerator> 
    {
        /// <summary>The metric representing the numerator of the aggregate.</summary>
        protected TNumerator numeratorMetric;
        /// <summary>The name of the metric aggregate.</summary>
        protected string name;
        /// <summary>A description of the metric aggregate, explaining what it measures and/or represents.</summary>
        protected string description;

        /// <summary>
        /// The type of the numerator of the metric aggregate.
        /// </summary>
        public Type NumeratorMetricType
        {
            get
            {
                return numeratorMetric.GetType();
            }
        }

        /// <summary>
        /// The name of the metric aggregate.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// A description of the metric aggregate, explaining what it measures and/or represents.
        /// </summary>
        public string Description
        {
            get
            {
                return description;
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: MetricAggregateContainerBase (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MetricAggregateContainerBase class.
        /// </summary>
        /// <param name="numeratorMetric">The metric which is the numerator of the aggregate.</param>
        /// <param name="name">The name of the metric aggregate.</param>
        /// <param name="description">A description of the metric aggregate, explaining what it measures and/or represents.</param>
        protected MetricAggregateContainerBase(TNumerator numeratorMetric, string name, string description)
        {
            this.numeratorMetric = numeratorMetric;

            if (name.Trim() != "")
            {
                this.name = name;
            }
            else
            {
                throw new ArgumentException("Argument 'name' cannot be blank.", "name");
            }

            if (description.Trim() != "")
            {
                this.description = description;
            }
            else
            {
                throw new ArgumentException("Argument 'description' cannot be blank.", "description");
            }
        }
    }
}

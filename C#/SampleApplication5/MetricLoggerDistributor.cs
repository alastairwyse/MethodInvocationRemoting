/*
 * Copyright 2013 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
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
using ApplicationMetrics;

namespace SampleApplication5
{
    //******************************************************************************
    //
    // Class: MetricLoggerDistributor
    //
    //******************************************************************************
    /// <summary>
    /// Distributes metric events to multiple implementations of interface IMetricLogger.
    /// </summary>
    class MetricLoggerDistributor : IMetricLogger
    {
        List<IMetricLogger> loggerList;

        //------------------------------------------------------------------------------
        //
        // Method: MetricLoggerDistributor (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the SampleApplication5.MetricLoggerDistributor class.
        /// </summary>
        public MetricLoggerDistributor()
        {
            loggerList = new List<IMetricLogger>();
        }

        //------------------------------------------------------------------------------
        //
        // Method: AddLogger
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Adds a IMetricLogger to the distribution list.
        /// </summary>
        /// <param name="logger">The logger to add.</param>
        public void AddLogger(IMetricLogger logger)
        {
            loggerList.Add(logger);
        }

        public void Increment(CountMetric countMetric)
        {
            foreach (IMetricLogger currentLogger in loggerList) 
            {
                currentLogger.Increment(countMetric);
            }
        }

        public void Add(AmountMetric amountMetric)
        {
            foreach (IMetricLogger currentLogger in loggerList)
            {
                currentLogger.Add(amountMetric);
            }
        }

        public void Set(StatusMetric statusMetric)
        {
            foreach (IMetricLogger currentLogger in loggerList)
            {
                currentLogger.Set(statusMetric);
            }
        }

        public void Begin(IntervalMetric intervalMetric)
        {
            foreach (IMetricLogger currentLogger in loggerList)
            {
                currentLogger.Begin(intervalMetric);
            }
        }

        public void End(IntervalMetric intervalMetric)
        {
            foreach (IMetricLogger currentLogger in loggerList)
            {
                currentLogger.End(intervalMetric);
            }
        }
    }
}

/*
 * Copyright 2015 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
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
using System.Threading;
using OperatingSystemAbstraction;

namespace ApplicationMetrics
{
    //******************************************************************************
    //
    // Class: MetricAggregateLogger
    //
    //******************************************************************************
    /// <summary>
    /// Base class which supports buffering and storing of metric events, and provides a base framework for classes which log aggregates of metric events.
    /// </summary>
    /// <remarks>Derived classes must implement methods which log defined metric aggregates (e.g. LogCountOverTimeUnitAggregate()).  These methods are called from a worker thread after dequeueing, totalling, and logging the base metric events.</remarks>
    abstract class MetricAggregateLogger : MetricLoggerStorer, IMetricAggregateLogger
    {
        // Containers for metric aggregates
        /// <summary>Container for aggregates which represent the number of occurrences of a count metric within the specified time unit</summary>
        protected List<MetricAggregateContainer<CountMetric>> countOverTimeUnitAggregateDefinitions;
        /// <summary>Container for aggregates which represent the total of an amount metric per instance of a count metric.</summary>
        protected List<MetricAggregateContainer<AmountMetric, CountMetric>> amountOverCountAggregateDefinitions;
        /// <summary>Container for aggregates which represent the total of an amount metric within the specified time unit.</summary>
        protected List<MetricAggregateContainer<AmountMetric>> amountOverTimeUnitAggregateDefinitions;
        /// <summary>Container for aggregates which represent the total of an amount metric divided by the total of another amount metric.</summary>
        protected List<MetricAggregateContainer<AmountMetric, AmountMetric>> amountOverAmountAggregateDefinitions;
        /// <summary>Container for aggregates which represent the total of an interval metric per instance of a count metric.</summary>
        protected List<MetricAggregateContainer<IntervalMetric, CountMetric>> intervalOverAmountAggregateDefinitions;
        /// <summary>Container for aggregates which represent an interval metric as a fraction of the total runtime of the logger.</summary>
        /// <remarks>Note that the TimeUnit member of the MetricAggregateContainer class is not used in this case.</remarks>
        protected List<MetricAggregateContainer<IntervalMetric>> intervalOverTotalRunTimeAggregateDefinitions;

        /// <summary>The time the Start() method was called.</summary>
        protected System.DateTime startTime;
        /// <summary>Object containing utility methods.</summary>
        protected ApplicationMetricsUtilities utilities;
        
        //------------------------------------------------------------------------------
        //
        // Method: MetricAggregateLogger (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MetricAggregateLogger class.
        /// </summary>
        /// <param name="bufferProcessingStrategy">Object which implements a processing strategy for the buffers (queues).</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        protected MetricAggregateLogger(IBufferProcessingStrategy bufferProcessingStrategy, bool intervalMetricChecking)
            : base(bufferProcessingStrategy, intervalMetricChecking)
        {
            InitialisePrivateMembers();
        }

        //------------------------------------------------------------------------------
        //
        // Method: MetricAggregateLogger (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MetricAggregateLogger class.  Note this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
        /// </summary>
        /// <param name="bufferProcessingStrategy">Object which implements a processing strategy for the buffers (queues).</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        /// <param name="dateTime">A test (mock) DateTime object.</param>
        /// <param name="exceptionHandler">A test (mock) exception handler object.</param>
        protected MetricAggregateLogger(IBufferProcessingStrategy bufferProcessingStrategy, bool intervalMetricChecking, IDateTime dateTime, IExceptionHandler exceptionHandler)
            : base(bufferProcessingStrategy, intervalMetricChecking, dateTime, exceptionHandler)
        {
            InitialisePrivateMembers();
        }

        //------------------------------------------------------------------------------
        //
        // Method: Start
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Starts the buffer processing (e.g. if the implementation of the buffer processing strategy uses a worker thread, this method starts the worker thread).
        /// </summary>
        /// <remarks>Although this method has been deprecated in base classes, in the case of MetricAggregateLogger this Start() method should be called (rather than the Start() on the IBufferProcessingStrategy implementation) as it performs additional initialization specific to MetricAggregateLogger.</remarks>
        public override void Start()
        {
            startTime = dateTime.UtcNow;
            base.Start();
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricAggregateLogger.DefineMetricAggregate(ApplicationMetrics.CountMetric,ApplicationMetrics.TimeUnit,System.String,System.String)"]/*'/>
        public virtual void DefineMetricAggregate(CountMetric countMetric, TimeUnit timeUnit, string name, string description)
        {
            CheckDuplicateAggregateName(name);
            countOverTimeUnitAggregateDefinitions.Add(new MetricAggregateContainer<CountMetric>(countMetric, timeUnit, name, description));
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricAggregateLogger.DefineMetricAggregate(ApplicationMetrics.AmountMetric,ApplicationMetrics.CountMetric,System.String,System.String)"]/*'/>
        public virtual void DefineMetricAggregate(AmountMetric amountMetric, CountMetric countMetric, string name, string description)
        {
            CheckDuplicateAggregateName(name);
            amountOverCountAggregateDefinitions.Add(new MetricAggregateContainer<AmountMetric, CountMetric>(amountMetric, countMetric, name, description));
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricAggregateLogger.DefineMetricAggregate(ApplicationMetrics.AmountMetric,ApplicationMetrics.TimeUnit,System.String,System.String)"]/*'/>
        public virtual void DefineMetricAggregate(AmountMetric amountMetric, TimeUnit timeUnit, string name, string description)
        {
            CheckDuplicateAggregateName(name);
            amountOverTimeUnitAggregateDefinitions.Add(new MetricAggregateContainer<AmountMetric>(amountMetric, timeUnit, name, description));
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricAggregateLogger.DefineMetricAggregate(ApplicationMetrics.AmountMetric,ApplicationMetrics.AmountMetric,System.String,System.String)"]/*'/>
        public virtual void DefineMetricAggregate(AmountMetric numeratorAmountMetric, AmountMetric denominatorAmountMetric, string name, string description)
        {
            CheckDuplicateAggregateName(name);
            amountOverAmountAggregateDefinitions.Add(new MetricAggregateContainer<AmountMetric, AmountMetric>(numeratorAmountMetric, denominatorAmountMetric, name, description));
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricAggregateLogger.DefineMetricAggregate(ApplicationMetrics.IntervalMetric,ApplicationMetrics.CountMetric,System.String,System.String)"]/*'/>
        public virtual void DefineMetricAggregate(IntervalMetric intervalMetric, CountMetric countMetric, string name, string description)
        {
            CheckDuplicateAggregateName(name);
            intervalOverAmountAggregateDefinitions.Add(new MetricAggregateContainer<IntervalMetric, CountMetric>(intervalMetric, countMetric, name, description));
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricAggregateLogger.DefineMetricAggregate(ApplicationMetrics.IntervalMetric,System.String,System.String)"]/*'/>
        public virtual void DefineMetricAggregate(IntervalMetric intervalMetric, string name, string description)
        {
            CheckDuplicateAggregateName(name);
            intervalOverTotalRunTimeAggregateDefinitions.Add(new MetricAggregateContainer<IntervalMetric>(intervalMetric, TimeUnit.Second, name, description));
        }

        #region Abstract Methods

        //------------------------------------------------------------------------------
        //
        // Method: LogCountOverTimeUnitAggregate
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs a metric aggregate representing the number of occurrences of a count metric within the specified time unit.
        /// </summary>
        /// <param name="metricAggregate">The metric aggregate to log.</param>
        /// <param name="totalInstances">The number of occurrences of the count metric.</param>
        /// <param name="totalElapsedTimeUnits">The total elapsed time units.</param>
        protected abstract void LogCountOverTimeUnitAggregate(MetricAggregateContainer<CountMetric> metricAggregate, long totalInstances, long totalElapsedTimeUnits);

        //------------------------------------------------------------------------------
        //
        // Method: LogAmountOverCountAggregate
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs a metric aggregate representing the total of an amount metric per occurrence of a count metric.
        /// </summary>
        /// <param name="metricAggregate">The metric aggregate to log.</param>
        /// <param name="totalAmount">The total of the amount metric.</param>
        /// <param name="totalInstances">The number of occurrences of the count metric.</param>
        protected abstract void LogAmountOverCountAggregate(MetricAggregateContainer<AmountMetric, CountMetric> metricAggregate, long totalAmount, long totalInstances);

        //------------------------------------------------------------------------------
        //
        // Method: LogAmountOverTimeUnitAggregate
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs a metric aggregate respresenting the total of an amount metric within the specified time unit.
        /// </summary>
        /// <param name="metricAggregate">The metric aggregate to log.</param>
        /// <param name="totalAmount">The total of the amount metric.</param>
        /// <param name="totalElapsedTimeUnits">The total elapsed time units.</param>
        protected abstract void LogAmountOverTimeUnitAggregate(MetricAggregateContainer<AmountMetric> metricAggregate, long totalAmount, long totalElapsedTimeUnits);

        //------------------------------------------------------------------------------
        //
        // Method: LogAmountOverAmountAggregate
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs a metric aggregate representing the total of an amount metric divided by the total of another amount metric.
        /// </summary>
        /// <param name="metricAggregate">The metric aggregate to log.</param>
        /// <param name="numeratorTotal">The total of the numerator amount metric.</param>
        /// <param name="denominatorTotal">The total of the denominator amount metric.</param>
        protected abstract void LogAmountOverAmountAggregate(MetricAggregateContainer<AmountMetric, AmountMetric> metricAggregate, long numeratorTotal, long denominatorTotal);

        //------------------------------------------------------------------------------
        //
        // Method: LogIntervalOverCountAggregate
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs a metric aggregate representing the total of an interval metric per occurrence of a count metric.
        /// </summary>
        /// <param name="metricAggregate">The metric aggregate to log.</param>
        /// <param name="totalInterval">The total of the interval metric.</param>
        /// <param name="totalInstances">The number of occurrences of the count metric.</param>
        protected abstract void LogIntervalOverCountAggregate(MetricAggregateContainer<IntervalMetric, CountMetric> metricAggregate, long totalInterval, long totalInstances);

        //------------------------------------------------------------------------------
        //
        // Method: LogIntervalOverTotalRunTimeAggregate
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs a metric aggregate representing the total of an interval metric as a fraction of the total runtime of the logger.
        /// </summary>
        /// <param name="metricAggregate">The metric aggregate to log.</param>
        /// <param name="totalInterval">The total of the interval metric.</param>
        /// <param name="totalRunTime">The total run time of the logger since starting in milliseconds.</param>
        protected abstract void LogIntervalOverTotalRunTimeAggregate(MetricAggregateContainer<IntervalMetric> metricAggregate, long totalInterval, long totalRunTime);

        #endregion

        #region Base Class Method Implementations

        //------------------------------------------------------------------------------
        //
        // Method: DequeueAndProcessMetricEvents
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Dequeues and logs metric events stored in the internal buffer, and logs any defined metric aggregates.
        /// </summary>
        protected override void DequeueAndProcessMetricEvents()
        {
            base.DequeueAndProcessMetricEvents();
            LogCountOverTimeUnitAggregates();
            LogAmountOverCountAggregates();
            LogAmountOverTimeUnitAggregates();
            LogAmountOverAmountAggregates();
            LogIntervalOverCountMetricAggregates();
            LogIntervalOverTotalRunTimeAggregates();
        }

        #endregion

        #region Private Methods

        //------------------------------------------------------------------------------
        //
        // Method: InitialisePrivateMembers
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises private members of the class.
        /// </summary>
        private void InitialisePrivateMembers()
        {
            countOverTimeUnitAggregateDefinitions = new List<MetricAggregateContainer<CountMetric>>();
            amountOverCountAggregateDefinitions = new List<MetricAggregateContainer<AmountMetric, CountMetric>>();
            amountOverTimeUnitAggregateDefinitions = new List<MetricAggregateContainer<AmountMetric>>();
            amountOverAmountAggregateDefinitions = new List<MetricAggregateContainer<AmountMetric, AmountMetric>>();
            intervalOverAmountAggregateDefinitions = new List<MetricAggregateContainer<IntervalMetric, CountMetric>>();
            intervalOverTotalRunTimeAggregateDefinitions = new List<MetricAggregateContainer<IntervalMetric>>();
            utilities = new ApplicationMetricsUtilities();
        }

        //------------------------------------------------------------------------------
        //
        // Method: LogCountOverTimeUnitAggregates
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Calculates and logs the value of all defined metric aggregates representing the number of occurrences of a count metric within the specified time unit.
        /// </summary>
        private void LogCountOverTimeUnitAggregates()
        {
            foreach (MetricAggregateContainer<CountMetric> currentAggregate in countOverTimeUnitAggregateDefinitions)
            {
                // Calculate the value
                long totalInstances;
                if (countMetricTotals.ContainsKey(currentAggregate.NumeratorMetricType) == true)
                {
                    totalInstances = countMetricTotals[currentAggregate.NumeratorMetricType].TotalCount;
                }
                else
                {
                    totalInstances = 0;
                }

                // Convert the number of elapsed milliseconds since starting to the time unit specified in the aggregate
                double totalElapsedTimeUnits = CalculateElapsedTimeSinceStart() / utilities.ConvertTimeUnitToMilliSeconds(currentAggregate.DenominatorTimeUnit);
                LogCountOverTimeUnitAggregate(currentAggregate, totalInstances, Convert.ToInt64(totalElapsedTimeUnits));
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: LogAmountOverCountAggregates
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Calculates and logs the value of all defined metric aggregates representing the total of an amount metric per occurrence of a count metric.
        /// </summary>
        private void LogAmountOverCountAggregates()
        {
            foreach (MetricAggregateContainer<AmountMetric, CountMetric> currentAggregate in amountOverCountAggregateDefinitions)
            {
                long totalAmount;
                if (amountMetricTotals.ContainsKey(currentAggregate.NumeratorMetricType) == true)
                {
                    totalAmount = amountMetricTotals[currentAggregate.NumeratorMetricType].Total;
                }
                else
                {
                    totalAmount = 0;
                }

                long totalInstances;
                if (countMetricTotals.ContainsKey(currentAggregate.DenominatorMetricType) == true)
                {
                    totalInstances = countMetricTotals[currentAggregate.DenominatorMetricType].TotalCount;
                }
                else
                {
                    totalInstances = 0;
                }

                LogAmountOverCountAggregate(currentAggregate, totalAmount, totalInstances);
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: LogAmountOverTimeUnitAggregates
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Calculates and logs the value of all defined metric aggregates representing the total of an amount metric within the specified time unit.
        /// </summary>
        private void LogAmountOverTimeUnitAggregates()
        {
            foreach (MetricAggregateContainer<AmountMetric> currentAggregate in amountOverTimeUnitAggregateDefinitions)
            {
                // Calculate the total
                long totalAmount;
                if (amountMetricTotals.ContainsKey(currentAggregate.NumeratorMetricType) == true)
                {
                    totalAmount = amountMetricTotals[currentAggregate.NumeratorMetricType].Total;
                }
                else
                {
                    totalAmount = 0;
                }

                // Convert the number of elapsed milliseconds since starting to the time unit specified in the aggregate
                double totalElapsedTimeUnits = CalculateElapsedTimeSinceStart() / utilities.ConvertTimeUnitToMilliSeconds(currentAggregate.DenominatorTimeUnit);
                LogAmountOverTimeUnitAggregate(currentAggregate, totalAmount, Convert.ToInt64(totalElapsedTimeUnits));
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: LogAmountOverAmountAggregates
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Calculates and logs the value of all defined metric aggregates representing the total of an amount metric divided by the total of another amount metric.
        /// </summary>
        private void LogAmountOverAmountAggregates()
        {
            foreach (MetricAggregateContainer<AmountMetric, AmountMetric> currentAggregate in amountOverAmountAggregateDefinitions)
            {
                long numeratorTotal;
                if (amountMetricTotals.ContainsKey(currentAggregate.NumeratorMetricType) == true)
                {
                    numeratorTotal = amountMetricTotals[currentAggregate.NumeratorMetricType].Total;
                }
                else
                {
                    numeratorTotal = 0;
                }

                long denominatorTotal;
                if (amountMetricTotals.ContainsKey(currentAggregate.DenominatorMetricType) == true)
                {
                    denominatorTotal = amountMetricTotals[currentAggregate.DenominatorMetricType].Total;
                }
                else
                {
                    denominatorTotal = 0;
                }

                LogAmountOverAmountAggregate(currentAggregate, numeratorTotal, denominatorTotal);
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: LogIntervalOverCountMetricAggregates
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Calculates and logs the value of all defined metric aggregates representing the total of an interval metric per occurrence of a count metric.
        /// </summary>
        private void LogIntervalOverCountMetricAggregates()
        {
            foreach (MetricAggregateContainer<IntervalMetric, CountMetric> currentAggregate in intervalOverAmountAggregateDefinitions)
            {
                long totalInterval;
                if (intervalMetricTotals.ContainsKey(currentAggregate.NumeratorMetricType) == true)
                {
                    totalInterval = intervalMetricTotals[currentAggregate.NumeratorMetricType].Total;
                }
                else
                {
                    totalInterval = 0;
                }

                long totalInstances;
                if (countMetricTotals.ContainsKey(currentAggregate.DenominatorMetricType) == true)
                {
                    totalInstances = countMetricTotals[currentAggregate.DenominatorMetricType].TotalCount;
                }
                else
                {
                    totalInstances = 0;
                }

                LogIntervalOverCountAggregate(currentAggregate, totalInterval, totalInstances);
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: LogIntervalOverTotalRunTimeAggregates
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Calculates and logs the value of all defined metric aggregates representing the total of an interval metric as a fraction of the total runtime of the logger.
        /// </summary>
        private void LogIntervalOverTotalRunTimeAggregates()
        {
            foreach (MetricAggregateContainer<IntervalMetric> currentAggregate in intervalOverTotalRunTimeAggregateDefinitions)
            {
                long totalInterval;
                if (intervalMetricTotals.ContainsKey(currentAggregate.NumeratorMetricType) == true)
                {
                    totalInterval = intervalMetricTotals[currentAggregate.NumeratorMetricType].Total;
                }
                else
                {
                    totalInterval = 0;
                }

                long totalElapsedMilliseconds = Convert.ToInt64(CalculateElapsedTimeSinceStart());

                LogIntervalOverTotalRunTimeAggregate(currentAggregate, totalInterval, totalElapsedMilliseconds);
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: CalculateElapsedTimeSinceStart
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Calculates the number of milliseconds that have elapsed since the Start() method was called.
        /// </summary>
        /// <returns>The time since starting in milliseconds.</returns>
        private double CalculateElapsedTimeSinceStart()
        {
            TimeSpan elapsedTimeSinceStart = dateTime.UtcNow.Subtract(startTime);
            return elapsedTimeSinceStart.TotalMilliseconds;
        }

        //------------------------------------------------------------------------------
        //
        // Method: CheckDuplicateAggregateName
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Checks all aggregate containers for an existing defined aggregate with the specified name, and throws an exception if an existing aggregate is found.
        /// </summary>
        /// <param name="name">The aggregate name to check for.</param>
        private void CheckDuplicateAggregateName(string name)
        {
            bool exists = false;

            foreach (MetricAggregateContainer<CountMetric> currentAggregate in countOverTimeUnitAggregateDefinitions)
            {
                if (currentAggregate.Name == name)
                {
                    exists = true;
                }
            }
            foreach (MetricAggregateContainer<AmountMetric, CountMetric> currentAggregate in amountOverCountAggregateDefinitions)
            {
                if (currentAggregate.Name == name)
                {
                    exists = true;
                }
            }
            foreach (MetricAggregateContainer<AmountMetric> currentAggregate in amountOverTimeUnitAggregateDefinitions)
            {
                if (currentAggregate.Name == name)
                {
                    exists = true;
                }
            }
            foreach (MetricAggregateContainer<AmountMetric, AmountMetric> currentAggregate in amountOverAmountAggregateDefinitions)
            {
                if (currentAggregate.Name == name)
                {
                    exists = true;
                }
            }
            foreach (MetricAggregateContainer<IntervalMetric, CountMetric> currentAggregate in intervalOverAmountAggregateDefinitions)
            {
                if (currentAggregate.Name == name)
                {
                    exists = true;
                }
            }
            foreach (MetricAggregateContainer<IntervalMetric> currentAggregate in intervalOverTotalRunTimeAggregateDefinitions)
            {
                if (currentAggregate.Name == name)
                {
                    exists = true;
                }
            }

            if (exists == true)
            {
                throw new Exception("Metric aggregate with name '" + name + "' has already been defined.");
            }
        }

        #endregion
    }
}

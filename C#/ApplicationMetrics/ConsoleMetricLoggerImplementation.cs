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
using System.Threading;
using OperatingSystemAbstraction;

namespace ApplicationMetrics
{
    //******************************************************************************
    //
    // Class: ConsoleMetricLoggerImplementation
    //
    //******************************************************************************
    /// <summary>
    /// Writes metric and instrumentation events for an application to the console.
    /// </summary>
    /// <remarks>This class provides underlying functionality for public class ConsoleMetricLogger.  ConsoleMetricLogger utilizes this class via composition rather than inheritance to allow the MetricAggregateLogger class to remain private within the ApplicationMetrics namespace.</remarks>
    class ConsoleMetricLoggerImplementation : MetricAggregateLogger
    {
        private const string separatorString = ": ";
        private IConsole console;

        //------------------------------------------------------------------------------
        //
        // Method: ConsoleMetricLoggerImplementation (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.ConsoleMetricLoggerImplementation class.
        /// </summary>
        /// <param name="dequeueOperationLoopInterval">The time to wait in between iterations of the worker thread which dequeues metric events and writes them to the console.</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        public ConsoleMetricLoggerImplementation(int dequeueOperationLoopInterval, bool intervalMetricChecking)
            : base(dequeueOperationLoopInterval, intervalMetricChecking)
        {
            console = new OperatingSystemAbstraction.Console();
        }

        //------------------------------------------------------------------------------
        //
        // Method: ConsoleMetricLoggerImplementation (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.ConsoleMetricLoggerImplementation class.  Note this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
        /// </summary>
        /// <param name="dequeueOperationLoopInterval">The time to wait in between iterations of the worker thread which dequeues metric events and writes them to the console.</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        /// <param name="console">A test (mock) console object.</param>
        /// <param name="dateTime">A test (mock) DateTime object.</param>
        /// <param name="exceptionHandler">A test (mock) exception handler object.</param>
        public ConsoleMetricLoggerImplementation(int dequeueOperationLoopInterval, bool intervalMetricChecking, IConsole console, IDateTime dateTime, IExceptionHandler exceptionHandler)
            : base(dequeueOperationLoopInterval, intervalMetricChecking, dateTime, exceptionHandler)
        {
            this.console = console;
        }

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
            console.Clear();
            console.WriteLine("---------------------------------------------------");
            console.WriteLine("-- Application metrics as of " + dateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " --");
            console.WriteLine("---------------------------------------------------");
            base.DequeueAndProcessMetricEvents();
        }

        //------------------------------------------------------------------------------
        //
        // Method: LogCountMetricTotal
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs the total of a count metric to the console.
        /// </summary>
        /// <param name="countMetric">The count metric to log.</param>
        /// <param name="value">The total.</param>
        protected override void LogCountMetricTotal(CountMetric countMetric, long value)
        {
            console.WriteLine(countMetric.Name + separatorString + value.ToString());
        }

        //------------------------------------------------------------------------------
        //
        // Method: LogAmountMetricTotal
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs the total of an amount metric to the console.
        /// </summary>
        /// <param name="amountMetric">The amount metric to log.</param>
        /// <param name="value">The total.</param>
        protected override void LogAmountMetricTotal(AmountMetric amountMetric, long value)
        {
            console.WriteLine(amountMetric.Name + separatorString + value.ToString());
        }

        //------------------------------------------------------------------------------
        //
        // Method: LogStatusMetricValue
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs the most recent value of a status metric to the console.
        /// </summary>
        /// <param name="statusMetric">The status metric to log.</param>
        /// <param name="value">The value.</param>
        protected override void LogStatusMetricValue(StatusMetric statusMetric, long value)
        {
            console.WriteLine(statusMetric.Name + separatorString + value.ToString());
        }

        //------------------------------------------------------------------------------
        //
        // Method: LogIntervalMetricTotal
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs the total of an interval metric to the console.
        /// </summary>
        /// <param name="intervalMetric">The interval metric to log.</param>
        /// <param name="value">The total.</param>
        protected override void LogIntervalMetricTotal(IntervalMetric intervalMetric, long value)
        {
            console.WriteLine(intervalMetric.Name + separatorString + value.ToString());
        }

        //------------------------------------------------------------------------------
        //
        // Method: LogCountOverTimeUnitAggregate
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs a metric aggregate representing the number of occurrences of a count metric within the specified time unit to the console.
        /// </summary>
        /// <param name="metricAggregate">The metric aggregate to log.</param>
        /// <param name="totalInstances">The number of occurrences of the count metric.</param>
        /// <param name="totalElapsedTimeUnits">The total elapsed time units.</param>
        protected override void LogCountOverTimeUnitAggregate(MetricAggregateContainer<CountMetric> metricAggregate, long totalInstances, long totalElapsedTimeUnits)
        {
            if (totalElapsedTimeUnits != 0)
            {
                double aggregateValue = Convert.ToDouble(totalInstances) / totalElapsedTimeUnits;
                console.WriteLine(metricAggregate.Name + separatorString + aggregateValue);
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: LogAmountOverCountAggregate
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs a metric aggregate representing the total of an amount metric per occurrence of a count metric to the console.
        /// </summary>
        /// <param name="metricAggregate">The metric aggregate to log.</param>
        /// <param name="totalAmount">The total of the amount metric.</param>
        /// <param name="totalInstances">The number of occurrences of the count metric.</param>
        protected override void LogAmountOverCountAggregate(MetricAggregateContainer<AmountMetric, CountMetric> metricAggregate, long totalAmount, long totalInstances)
        {
            if (totalInstances != 0)
            {
                double aggregateValue = Convert.ToDouble(totalAmount) / totalInstances;
                console.WriteLine(metricAggregate.Name + separatorString + aggregateValue);
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: LogAmountOverTimeUnitAggregate
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs a metric aggregate respresenting the total of an amount metric within the specified time unit to the console.
        /// </summary>
        /// <param name="metricAggregate">The metric aggregate to log.</param>
        /// <param name="totalAmount">The total of the amount metric.</param>
        /// <param name="totalElapsedTimeUnits">The total elapsed time units.</param>
        protected override void LogAmountOverTimeUnitAggregate(MetricAggregateContainer<AmountMetric> metricAggregate, long totalAmount, long totalElapsedTimeUnits)
        {
            if (totalElapsedTimeUnits != 0)
            {
                double aggregateValue = Convert.ToDouble(totalAmount) / totalElapsedTimeUnits;
                console.WriteLine(metricAggregate.Name + separatorString + aggregateValue);
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: LogAmountOverAmountAggregate
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs a metric aggregate respresenting the total of an amount metric divided by the total of another amount metric to the console.
        /// </summary>
        /// <param name="metricAggregate">The metric aggregate to log.</param>
        /// <param name="numeratorTotal">The total of the numerator amount metric.</param>
        /// <param name="denominatorTotal">The total of the denominator amount metric.</param>
        protected override void LogAmountOverAmountAggregate(MetricAggregateContainer<AmountMetric, AmountMetric> metricAggregate, long numeratorTotal, long denominatorTotal)
        {
            if (denominatorTotal != 0)
            {
                double aggregateValue = Convert.ToDouble(numeratorTotal) / denominatorTotal;
                console.WriteLine(metricAggregate.Name + separatorString + aggregateValue);
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: LogIntervalOverCountAggregate
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs a metric aggregate respresenting the total of an interval metric per occurrence of a count metric to the console.
        /// </summary>
        /// <param name="metricAggregate">The metric aggregate to log.</param>
        /// <param name="totalInterval">The total of the interval metric.</param>
        /// <param name="totalInstances">The number of occurrences of the count metric.</param>
        protected override void LogIntervalOverCountAggregate(MetricAggregateContainer<IntervalMetric, CountMetric> metricAggregate, long totalInterval, long totalInstances)
        {
            if (totalInstances != 0)
            {
                double aggregateValue = Convert.ToDouble(totalInterval) / totalInstances;
                console.WriteLine(metricAggregate.Name + separatorString + aggregateValue);
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: LogIntervalOverTotalRunTimeAggregate
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs a metric aggregate representing the total of an interval metric as a fraction of the total runtime of the logger to the console.
        /// </summary>
        /// <param name="metricAggregate">The metric aggregate to log.</param>
        /// <param name="totalInterval">The total of the interval metric.</param>
        /// <param name="totalRunTime">The total run time of the logger since starting in milliseonds.</param>
        protected override void LogIntervalOverTotalRunTimeAggregate(MetricAggregateContainer<IntervalMetric> metricAggregate, long totalInterval, long totalRunTime)
        {
            if (totalRunTime > 0)
            {
                double aggregateValue = Convert.ToDouble(totalInterval) / totalRunTime;
                console.WriteLine(metricAggregate.Name + separatorString + aggregateValue);
            }
        }

        #endregion
    }
}

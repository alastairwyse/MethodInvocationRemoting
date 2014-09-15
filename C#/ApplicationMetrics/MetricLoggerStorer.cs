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
    // Class: MetricLoggerStorer
    //
    //******************************************************************************
    /// <summary>
    /// Base class which buffers and totals instances of metric events.
    /// </summary>
    /// <remarks>Derived classes must implement methods which log the totals of metric events (e.g. LogCountMetricTotal()).  These methods are called from a worker thread after dequeueing the buffered metric events and calculating and storing their totals.</remarks>
    abstract class MetricLoggerStorer : MetricLoggerBuffer
    {
        /// <summary>Dictionary which stores the type of a count metric, and a container object holding the number of instances of that type of count metric event.</summary>
        protected Dictionary<Type, CountMetricTotalContainer> countMetricTotals;
        /// <summary>Dictionary which stores the type of an amount metric, and a container object holding the total amount of all instances of that type of amount metric event.</summary>
        protected Dictionary<Type, AmountMetricTotalContainer> amountMetricTotals;
        /// <summary>Dictionary which stores the type of a status metric, and the most recently logged value of that type of status metric. </summary>
        protected Dictionary<Type, StatusMetricValueContainer> statusMetricLatestValues;
        /// <summary>Dictionary which stores the type of an interval metric, and a container object holding the total amount of all instances of that type of interval metric event.</summary>
        protected Dictionary<Type, IntervalMetricTotalContainer> intervalMetricTotals;

        //------------------------------------------------------------------------------
        //
        // Method: MetricLoggerStorer (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MetricLoggerStorer class.
        /// </summary>
        /// <param name="dequeueOperationLoopInterval">The time to wait in between iterations of the worker thread which dequeues and processes metric events.</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        protected MetricLoggerStorer(int dequeueOperationLoopInterval, bool intervalMetricChecking)
            : base(dequeueOperationLoopInterval, intervalMetricChecking)
        {
            InitialisePrivateMembers();
        }

        //------------------------------------------------------------------------------
        //
        // Method: MetricLoggerStorer (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MetricLoggerStorer class.  Note this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
        /// </summary>
        /// <param name="dequeueOperationLoopInterval">The time to wait in between iterations of the worker thread which dequeues and processes metric events.</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        /// <param name="dateTime">A test (mock) DateTime object.</param>
        /// <param name="exceptionHandler">A test (mock) exception handler object.</param>
        protected MetricLoggerStorer(int dequeueOperationLoopInterval, bool intervalMetricChecking, IDateTime dateTime, IExceptionHandler exceptionHandler)
            : base(dequeueOperationLoopInterval, intervalMetricChecking, dateTime, exceptionHandler)
        {
            InitialisePrivateMembers();
        }

        #region Abstract Methods

        //------------------------------------------------------------------------------
        //
        // Method: LogCountMetricTotal
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs the total number of occurrences of a count metric.
        /// </summary>
        /// <param name="countMetric">The count metric.</param>
        /// <param name="value">The total number of occurrences.</param>
        protected abstract void LogCountMetricTotal(CountMetric countMetric, long value);

        //------------------------------------------------------------------------------
        //
        // Method: LogAmountMetricTotal
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs the total amount of all occurrences of an amount metric.
        /// </summary>
        /// <param name="amountMetric">The amount metric.</param>
        /// <param name="value">The total.</param>
        protected abstract void LogAmountMetricTotal(AmountMetric amountMetric, long value);

        //------------------------------------------------------------------------------
        //
        // Method: LogStatusMetricValue
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs the most recent value of a status metric.
        /// </summary>
        /// <param name="statusMetric">The status metric.</param>
        /// <param name="value">The value.</param>
        protected abstract void LogStatusMetricValue(StatusMetric statusMetric, long value);

        //------------------------------------------------------------------------------
        //
        // Method: LogIntervalMetricTotal
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs the total amount of all occurrences of an interval metric.
        /// </summary>
        /// <param name="intervalMetric">The interval metric.</param>
        /// <param name="value">The total.</param>
        protected abstract void LogIntervalMetricTotal(IntervalMetric intervalMetric, long value);

        #endregion

        #region Base Class Method Implementations

        //------------------------------------------------------------------------------
        //
        // Method: DequeueAndProcessMetricEvents
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Dequeues and logs metric events stored in the internal buffer.
        /// </summary>
        protected override void DequeueAndProcessMetricEvents()
        {
            base.DequeueAndProcessMetricEvents();
            LogCountMetricTotals();
            LogAmountMetricTotals();
            LogStatusMetricValues();
            LogIntervalMetricTotals();
        }

        //------------------------------------------------------------------------------
        //
        // Method: ProcessCountMetricEvent
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Adds the instance of the specified count metric event to the stored total.
        /// </summary>
        /// <param name="countMetricEvent">The count metric event.</param>
        protected override void ProcessCountMetricEvent(CountMetricEventInstance countMetricEvent)
        {
            if (countMetricTotals.ContainsKey(countMetricEvent.MetricType) == false)
            {
                countMetricTotals.Add(countMetricEvent.MetricType, new CountMetricTotalContainer(countMetricEvent.Metric));
            }
            countMetricTotals[countMetricEvent.MetricType].Increment();
        }

        //------------------------------------------------------------------------------
        //
        // Method: ProcessAmountMetricEvent
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Adds the value contained in the specified amount metric event to the stored total.
        /// </summary>
        /// <param name="amountMetricEvent">The amount metric event.</param>
        protected override void ProcessAmountMetricEvent(AmountMetricEventInstance amountMetricEvent)
        {
            if (amountMetricTotals.ContainsKey(amountMetricEvent.MetricType) == false)
            {
                amountMetricTotals.Add(amountMetricEvent.MetricType, new AmountMetricTotalContainer(amountMetricEvent.Metric));
            }
            amountMetricTotals[amountMetricEvent.MetricType].Add(amountMetricEvent.Metric.Amount);
        }

        //------------------------------------------------------------------------------
        //
        // Method: ProcessStatusMetricEvent
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Stores the value contained in the specified status metric event.
        /// </summary>
        /// <param name="statusMetricEvent">The status metric event.</param>
        protected override void ProcessStatusMetricEvent(StatusMetricEventInstance statusMetricEvent)
        {
            if (statusMetricLatestValues.ContainsKey(statusMetricEvent.MetricType) == false)
            {
                statusMetricLatestValues.Add(statusMetricEvent.MetricType, new StatusMetricValueContainer(statusMetricEvent.Metric));
            }
            statusMetricLatestValues[statusMetricEvent.MetricType].Set(statusMetricEvent.Metric.Value);
        }

        //------------------------------------------------------------------------------
        //
        // Method: ProcessIntervalMetricEvent
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Adds the duration for the specified interval metric event to the stored total.
        /// </summary>
        /// <param name="intervalMetricEvent">The interval metric event.</param>
        /// <param name="duration">The duration of the interval metric event in milliseconds.</param>
        protected override void ProcessIntervalMetricEvent(IntervalMetricEventInstance intervalMetricEvent, long duration)
        {
            if (intervalMetricTotals.ContainsKey(intervalMetricEvent.MetricType) == false)
            {
                intervalMetricTotals.Add(intervalMetricEvent.MetricType, new IntervalMetricTotalContainer(intervalMetricEvent.Metric));
            }
            intervalMetricTotals[intervalMetricEvent.MetricType].Add(duration);
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
            countMetricTotals = new Dictionary<Type, CountMetricTotalContainer>();
            amountMetricTotals = new Dictionary<Type, AmountMetricTotalContainer>();
            statusMetricLatestValues = new Dictionary<Type, StatusMetricValueContainer>();
            intervalMetricTotals = new Dictionary<Type, IntervalMetricTotalContainer>();
        }

        //------------------------------------------------------------------------------
        //
        // Method: LogCountMetricTotals
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs the totals of stored count metrics.
        /// </summary>
        private void LogCountMetricTotals()
        {
            foreach (CountMetricTotalContainer currentCountMetricTotal in countMetricTotals.Values)
            {
                LogCountMetricTotal(currentCountMetricTotal.CountMetric, currentCountMetricTotal.TotalCount);
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: LogAmountMetricTotals
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs the totals of stored amount metrics.
        /// </summary>
        private void LogAmountMetricTotals()
        {
            foreach (AmountMetricTotalContainer currentAmountMetricTotal in amountMetricTotals.Values)
            {
                LogAmountMetricTotal(currentAmountMetricTotal.AmountMetric, currentAmountMetricTotal.Total);
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: LogStatusMetricValues
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs the most recently logged values of stored status metrics.
        /// </summary>
        private void LogStatusMetricValues()
        {
            foreach (StatusMetricValueContainer currentStatusMetricValue in statusMetricLatestValues.Values)
            {
                LogStatusMetricValue(currentStatusMetricValue.StatusMetric, currentStatusMetricValue.Value);
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: LogIntervalMetricTotals
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Logs the totals of stored interval metrics.
        /// </summary>
        private void LogIntervalMetricTotals()
        {
            foreach (IntervalMetricTotalContainer currentIntervalMetricTotal in intervalMetricTotals.Values)
            {
                LogIntervalMetricTotal(currentIntervalMetricTotal.IntervalMetric, currentIntervalMetricTotal.Total);
            }
        }

        # endregion
    }
}

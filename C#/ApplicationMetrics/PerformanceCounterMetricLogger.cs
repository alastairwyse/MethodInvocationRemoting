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
    // Class: PerformanceCounterMetricLoggerImplementation
    //
    //******************************************************************************
    /// <summary>
    /// Writes metric and instrumentation events for an application to Windows performance counters.
    /// </summary>
    public class PerformanceCounterMetricLogger : IMetricLogger, IMetricAggregateLogger, IDisposable
    {
        protected bool disposed;
        private PerformanceCounterMetricLoggerImplementation loggerImplementation;

        //------------------------------------------------------------------------------
        //
        // Method: PerformanceCounterMetricLogger (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.PerformanceCounterMetricLogger class.
        /// </summary>
        /// <param name="metricCategoryName">The name of the performance counter category which the metric events should be logged under.</param>
        /// <param name="metricCategoryDescription">The description of the performance counter category which the metric events should be logged under.</param>
        /// <param name="dequeueOperationLoopInterval">The time to wait in between iterations of the worker thread which dequeues metric events and writes them to performance counters.</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        public PerformanceCounterMetricLogger(string metricCategoryName, string metricCategoryDescription, int dequeueOperationLoopInterval, bool intervalMetricChecking)
        {
            loggerImplementation = new PerformanceCounterMetricLoggerImplementation(metricCategoryName, metricCategoryDescription, dequeueOperationLoopInterval, intervalMetricChecking);
        }

        //------------------------------------------------------------------------------
        //
        // Method: PerformanceCounterMetricLogger (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.PerformanceCounterMetricLogger class.  Note this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
        /// </summary>
        /// <param name="metricCategoryName">The name of the performance counter category which the metric events should be logged under.</param>
        /// <param name="metricCategoryDescription">The description of the performance counter category which the metric events should be logged under.</param>
        /// <param name="dequeueOperationLoopInterval">The time to wait in between iterations of the worker thread which dequeues metric events and writes them to performance counters.</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        /// <param name="counterCreationDataCollection">A test (mock) counter creation data collection object.</param>
        /// <param name="counterCreationDataFactory">A test (mock) counter creation data factory object.</param>
        /// <param name="performanceCounterCategory">A test (mock) performance counter category object.</param>
        /// <param name="performanceCounterFactory">A test (mock) performance counter factory object.</param>
        /// <param name="dateTime">A test (mock) DateTime object.</param>
        /// <param name="exceptionHandler">A test (mock) exception handler object.</param>
        public PerformanceCounterMetricLogger(string metricCategoryName, string metricCategoryDescription, int dequeueOperationLoopInterval, bool intervalMetricChecking, ICounterCreationDataCollection counterCreationDataCollection, ICounterCreationDataFactory counterCreationDataFactory, IPerformanceCounterCategory performanceCounterCategory, IPerformanceCounterFactory performanceCounterFactory, IDateTime dateTime, IExceptionHandler exceptionHandler)
        {
            loggerImplementation = new PerformanceCounterMetricLoggerImplementation(metricCategoryName, metricCategoryDescription, dequeueOperationLoopInterval, intervalMetricChecking, counterCreationDataCollection, counterCreationDataFactory, performanceCounterCategory, performanceCounterFactory, dateTime, exceptionHandler);
        }

        //------------------------------------------------------------------------------
        //
        // Method: RegisterMetric
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Registers the specified metric to be written to the Windows performance counters.
        /// </summary>
        /// <param name="metric">The metric to register.</param>
        public void RegisterMetric(MetricBase metric)
        {
            loggerImplementation.RegisterMetric(metric);
        }

        //------------------------------------------------------------------------------
        //
        // Method: CreatePerformanceCounters()
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Creates Windows performance counters for the registered metrics and defined aggregates on the local computer.
        /// </summary>
        public void CreatePerformanceCounters()
        {
            loggerImplementation.CreatePerformanceCounters();
        }

        //------------------------------------------------------------------------------
        //
        // Method: Start
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Starts a worker thread which calls methods to dequeue, total, and log metric events and aggregates to performance counters, at an interval specified by constructor parameter 'dequeueOperationLoopInterval'.
        /// </summary>
        public void Start()
        {
            loggerImplementation.Start();
        }

        //------------------------------------------------------------------------------
        //
        // Method: Stop
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Stops the thread which dequeues metric events from the interal buffers.
        /// </summary>
        public void Stop()
        {
            loggerImplementation.Stop();
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricLogger.Increment(ApplicationMetrics.CountMetric)"]/*'/>
        public void Increment(CountMetric countMetric)
        {
            loggerImplementation.Increment(countMetric);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricLogger.Add(ApplicationMetrics.AmountMetric)"]/*'/>
        public void Add(AmountMetric amountMetric)
        {
            loggerImplementation.Add(amountMetric);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricLogger.Set(ApplicationMetrics.StatusMetric)"]/*'/>
        public void Set(StatusMetric statusMetric)
        {
            loggerImplementation.Set(statusMetric);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricLogger.Begin(ApplicationMetrics.IntervalMetric)"]/*'/>
        public void Begin(IntervalMetric intervalMetric)
        {
            loggerImplementation.Begin(intervalMetric);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricLogger.End(ApplicationMetrics.IntervalMetric)"]/*'/>
        public void End(IntervalMetric intervalMetric)
        {
            loggerImplementation.End(intervalMetric);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricAggregateLogger.DefineMetricAggregate(ApplicationMetrics.CountMetric,ApplicationMetrics.TimeUnit,System.String,System.String)"]/*'/>
        public void DefineMetricAggregate(CountMetric countMetric, TimeUnit timeUnit, string name, string description)
        {
            loggerImplementation.DefineMetricAggregate(countMetric, timeUnit, name, description);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricAggregateLogger.DefineMetricAggregate(ApplicationMetrics.AmountMetric,ApplicationMetrics.CountMetric,System.String,System.String)"]/*'/>
        public void DefineMetricAggregate(AmountMetric amountMetric, CountMetric countMetric, string name, string description)
        {
            loggerImplementation.DefineMetricAggregate(amountMetric, countMetric, name, description);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricAggregateLogger.DefineMetricAggregate(ApplicationMetrics.AmountMetric,ApplicationMetrics.TimeUnit,System.String,System.String)"]/*'/>
        public void DefineMetricAggregate(AmountMetric amountMetric, TimeUnit timeUnit, string name, string description)
        {
            loggerImplementation.DefineMetricAggregate(amountMetric, timeUnit, name, description);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricAggregateLogger.DefineMetricAggregate(ApplicationMetrics.AmountMetric,ApplicationMetrics.AmountMetric,System.String,System.String)"]/*'/>
        public void DefineMetricAggregate(AmountMetric numeratorAmountMetric, AmountMetric denominatorAmountMetric, string name, string description)
        {
            loggerImplementation.DefineMetricAggregate(numeratorAmountMetric, denominatorAmountMetric, name, description);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricAggregateLogger.DefineMetricAggregate(ApplicationMetrics.IntervalMetric,ApplicationMetrics.CountMetric,System.String,System.String)"]/*'/>
        public void DefineMetricAggregate(IntervalMetric intervalMetric, CountMetric countMetric, string name, string description)
        {
            loggerImplementation.DefineMetricAggregate(intervalMetric, countMetric, name, description);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricAggregateLogger.DefineMetricAggregate(ApplicationMetrics.IntervalMetric,System.String,System.String)"]/*'/>
        public void DefineMetricAggregate(IntervalMetric intervalMetric, string name, string description)
        {
            loggerImplementation.DefineMetricAggregate(intervalMetric, name, description);
        }

        #region Finalize / Dispose Methods

        /// <summary>
        /// Releases the unmanaged resources used by the PerformanceCounterMetricLogger.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~PerformanceCounterMetricLogger()
        {
            Dispose(false);
        }

        //------------------------------------------------------------------------------
        //
        // Method: Dispose
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Provides a method to free unmanaged resources used by this class.
        /// </summary>
        /// <param name="disposing">Whether the method is being called as part of an explicit Dispose routine, and hence whether managed resources should also be freed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                    if (loggerImplementation != null)
                    {
                        loggerImplementation.Dispose();
                        loggerImplementation = null;
                    }
                }
                // Free your own state (unmanaged objects).

                // Set large fields to null.
                
                disposed = true;
            }
        }

        #endregion
    }
}

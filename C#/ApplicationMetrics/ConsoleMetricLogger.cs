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
    // Class: ConsoleMetricLogger
    //
    //******************************************************************************
    /// <summary>
    /// Writes metric and instrumentation events for an application to the console.
    /// </summary>
    public class ConsoleMetricLogger : IMetricLogger, IMetricAggregateLogger
    {
        private ConsoleMetricLoggerImplementation loggerImplementation;

        //------------------------------------------------------------------------------
        //
        // Method: ConsoleMetricLogger (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.ConsoleMetricLogger class.
        /// </summary>
        /// <remarks>This constructor defaults to using the LoopingWorkerThreadBufferProcessor as the buffer processing strategy, and is maintained for backwards compatibility.</remarks>
        /// <param name="dequeueOperationLoopInterval">The time to wait (in milliseconds) between iterations of the worker thread which dequeues metric events and writes them to the console.</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        public ConsoleMetricLogger(int dequeueOperationLoopInterval, bool intervalMetricChecking)
        {
            loggerImplementation = new ConsoleMetricLoggerImplementation(new LoopingWorkerThreadBufferProcessor(dequeueOperationLoopInterval, false), intervalMetricChecking);
        }

        //------------------------------------------------------------------------------
        //
        // Method: ConsoleMetricLogger (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.ConsoleMetricLogger class.
        /// </summary>
        /// <param name="bufferProcessingStrategy">Object which implements a processing strategy for the buffers (queues).</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        public ConsoleMetricLogger(IBufferProcessingStrategy bufferProcessingStrategy, bool intervalMetricChecking)
        {
            loggerImplementation = new ConsoleMetricLoggerImplementation(bufferProcessingStrategy, intervalMetricChecking);
        }

        //------------------------------------------------------------------------------
        //
        // Method: ConsoleMetricLogger (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.ConsoleMetricLogger class.  Note this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
        /// </summary>
        /// <param name="bufferProcessingStrategy">Object which implements a processing strategy for the buffers (queues).</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).</param>
        /// <param name="console">A test (mock) console object.</param>
        /// <param name="dateTime">A test (mock) DateTime object.</param>
        /// <param name="exceptionHandler">A test (mock) exception handler object.</param>
        public ConsoleMetricLogger(IBufferProcessingStrategy bufferProcessingStrategy, bool intervalMetricChecking, IConsole console, IDateTime dateTime, IExceptionHandler exceptionHandler)
        {
            loggerImplementation = new ConsoleMetricLoggerImplementation(bufferProcessingStrategy, intervalMetricChecking, console, dateTime, exceptionHandler);
        }

        //------------------------------------------------------------------------------
        //
        // Method: Start
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Starts a worker thread which calls methods to dequeue metric events and write them to the console.
        /// </summary>
        /// <remarks>Although this method has been deprecated in base classes, in the case of ConsoleMetricLogger this Start() method should be called (rather than the Start() on the IBufferProcessingStrategy implementation) as it performs additional initialization specific to ConsoleMetricLogger.</remarks>
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
        /// Stops the worker thread.
        /// </summary>
        /// <remarks>This method is maintained on this class for backwards compatibility, as it is now available on interface IBufferProcessingStrategy.</remarks>
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

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IMetricLogger.CancelBegin(ApplicationMetrics.IntervalMetric)"]/*'/>
        public void CancelBegin(IntervalMetric intervalMetric)
        {
            loggerImplementation.CancelBegin(intervalMetric);
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
    }
}

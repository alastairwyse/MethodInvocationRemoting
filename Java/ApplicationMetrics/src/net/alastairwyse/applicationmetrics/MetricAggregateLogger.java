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

package net.alastairwyse.applicationmetrics;

import java.lang.Thread.*;
import java.util.*;
import java.util.concurrent.*;

import net.alastairwyse.operatingsystemabstraction.*;

/**
 * Base class which supports buffering and storing of metric events, and provides a base framework for classes which log aggregates of metric events.
 * Derived classes must implement methods which log defined metric aggregates (e.g. LogCountOverTimeUnitAggregate()).  These methods are called from a worker thread after dequeueing, totalling, and logging the base metric events.
 * @author Alastair Wyse
 */
abstract class MetricAggregateLogger extends MetricLoggerStorer implements IMetricAggregateLogger {

    // Containers for metric aggregates
    /** Container for aggregates which represent the number of occurrences of a count metric within the specified time unit */
    protected ArrayList<TimeUnitMetricAggregateContainer<CountMetric>> countOverTimeUnitAggregateDefinitions;
    /** Container for aggregates which represent the total of an amount metric per instance of a count metric. */
    protected ArrayList<MetricAggregateContainer<AmountMetric, CountMetric>> amountOverCountAggregateDefinitions;
    /** Container for aggregates which represent the total of an amount metric within the specified time unit. */
    protected ArrayList<TimeUnitMetricAggregateContainer<AmountMetric>> amountOverTimeUnitAggregateDefinitions;
    /** Container for aggregates which represent the total of an amount metric divided by the total of another amount metric. */
    protected ArrayList<MetricAggregateContainer<AmountMetric, AmountMetric>> amountOverAmountAggregateDefinitions;
    /** Container for aggregates which represent the total of an interval metric per instance of a count metric. */
    protected ArrayList<MetricAggregateContainer<IntervalMetric, CountMetric>> intervalOverAmountAggregateDefinitions;
    /** Container for aggregates which represent an interval metric as a fraction of the total runtime of the logger.  Note that the TimeUnit member of the MetricAggregateContainer class is not used in this case. */
    protected ArrayList<TimeUnitMetricAggregateContainer<IntervalMetric>> intervalOverTotalRunTimeAggregateDefinitions;

    /** The time the Start() method was called. */
    protected Calendar startTime;

    /**
     * Initialises a new instance of the MetricAggregateLogger class.
     * @param dequeueOperationLoopInterval  The time to wait in between iterations of the worker thread which dequeues and processes metric events, and logs metric events and aggregates.
     * @param intervalMetricChecking        Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).
     * @param exceptionHandler              Handler for any uncaught exceptions occurring on the worker thread.
     */
    protected MetricAggregateLogger(int dequeueOperationLoopInterval, boolean intervalMetricChecking, UncaughtExceptionHandler exceptionHandler) {
        super(dequeueOperationLoopInterval, intervalMetricChecking, exceptionHandler);
        InitialisePrivateMembers();
    }
    
    /**
     * Initialises a new instance of the MetricAggregateLogger class.  
     * <b>Note</b> this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
     * @param dequeueOperationLoopInterval        The time to wait in between iterations of the worker thread which dequeues and processes metric events, and logs metric events and aggregates.
     * @param intervalMetricChecking              Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).
     * @param exceptionHandler                    Handler for any uncaught exceptions occurring on the worker thread.
     * @param calendarProvider                    A test (mock) ICalendarProvider object.
     * @param dequeueOperationLoopCompleteSignal  Notifies test code that an iteration of the worker thread which dequeues and processes metric events has completed.
     */
    protected MetricAggregateLogger(int dequeueOperationLoopInterval, boolean intervalMetricChecking, UncaughtExceptionHandler exceptionHandler, ICalendarProvider calendarProvider, CountDownLatch dequeueOperationLoopCompleteSignal) {
        super(dequeueOperationLoopInterval, intervalMetricChecking, exceptionHandler, calendarProvider, dequeueOperationLoopCompleteSignal);
        InitialisePrivateMembers();
    }

    /**
     * Starts a worker thread which calls methods to dequeue metric events and total and store the values of them, at an interval specified by constructor parameter 'dequeueOperationLoopInterval'.
     */
    public void Start() {
        startTime = calendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId));
        super.Start();
    }

    @Override
    public void DefineMetricAggregate(CountMetric countMetric, TimeUnit timeUnit, String name, String description) throws Exception {
        CheckDuplicateAggregateName(name);
        countOverTimeUnitAggregateDefinitions.add(new TimeUnitMetricAggregateContainer<CountMetric>(countMetric, timeUnit, name, description));
    }

    @Override
    public void DefineMetricAggregate(AmountMetric amountMetric, CountMetric countMetric, String name, String description) throws Exception {
        CheckDuplicateAggregateName(name);
        amountOverCountAggregateDefinitions.add(new MetricAggregateContainer<AmountMetric, CountMetric>(amountMetric, countMetric, name, description));
    }

    @Override
    public void DefineMetricAggregate(AmountMetric amountMetric, TimeUnit timeUnit, String name, String description) throws Exception {
        CheckDuplicateAggregateName(name);
        amountOverTimeUnitAggregateDefinitions.add(new TimeUnitMetricAggregateContainer<AmountMetric>(amountMetric, timeUnit, name, description));
    }

    @Override
    public void DefineMetricAggregate(AmountMetric numeratorAmountMetric, AmountMetric denominatorAmountMetric, String name, String description) throws Exception {
        CheckDuplicateAggregateName(name);
        amountOverAmountAggregateDefinitions.add(new MetricAggregateContainer<AmountMetric, AmountMetric>(numeratorAmountMetric, denominatorAmountMetric, name, description));
    }

    @Override
    public void DefineMetricAggregate(IntervalMetric intervalMetric, CountMetric countMetric, String name, String description) throws Exception {
        CheckDuplicateAggregateName(name);
        intervalOverAmountAggregateDefinitions.add(new MetricAggregateContainer<IntervalMetric, CountMetric>(intervalMetric, countMetric, name, description));
    }

    @Override
    public void DefineMetricAggregate(IntervalMetric intervalMetric, String name, String description) throws Exception {
        CheckDuplicateAggregateName(name);
        intervalOverTotalRunTimeAggregateDefinitions.add(new TimeUnitMetricAggregateContainer<IntervalMetric>(intervalMetric, TimeUnit.SECONDS, name, description));
    }

    /**
     * Logs a metric aggregate representing the number of occurrences of a count metric within the specified time unit.
     * @param metricAggregate        The metric aggregate to log.
     * @param totalInstances         The number of occurrences of the count metric.
     * @param totalElapsedTimeUnits  The total elapsed time units.
     */
    protected abstract void LogCountOverTimeUnitAggregate(TimeUnitMetricAggregateContainer<CountMetric> metricAggregate, long totalInstances, long totalElapsedTimeUnits);

    /**
     * Logs a metric aggregate representing the total of an amount metric per occurrence of a count metric.
     * @param metricAggregate  The metric aggregate to log.
     * @param totalAmount      The total of the amount metric.
     * @param totalInstances  The number of occurrences of the count metric.
     */
    protected abstract void LogAmountOverCountAggregate(MetricAggregateContainer<AmountMetric, CountMetric> metricAggregate, long totalAmount, long totalInstances);

    /**
     * Logs a metric aggregate representing the total of an amount metric within the specified time unit.
     * @param metricAggregate        The metric aggregate to log.
     * @param totalAmount            The total of the amount metric.
     * @param totalElapsedTimeUnits  The total elapsed time units.
     */
    protected abstract void LogAmountOverTimeUnitAggregate(TimeUnitMetricAggregateContainer<AmountMetric> metricAggregate, long totalAmount, long totalElapsedTimeUnits);

    /**
     * Logs a metric aggregate representing the total of an amount metric divided by the total of another amount metric.
     * @param metricAggregate   The metric aggregate to log.
     * @param numeratorTotal    The total of the numerator amount metric.
     * @param denominatorTotal  The total of the denominator amount metric.
     */
    protected abstract void LogAmountOverAmountAggregate(MetricAggregateContainer<AmountMetric, AmountMetric> metricAggregate, long numeratorTotal, long denominatorTotal);

    /**
     * Logs a metric aggregate representing the total of an interval metric per occurrence of a count metric.
     * @param metricAggregate  The metric aggregate to log.
     * @param totalInterval    The total of the interval metric.
     * @param totalInstances   The number of occurrences of the count metric.
     */
    protected abstract void LogIntervalOverCountAggregate(MetricAggregateContainer<IntervalMetric, CountMetric> metricAggregate, long totalInterval, long totalInstances);

    /**
     * Logs a metric aggregate representing the total of an interval metric as a fraction of the total runtime of the logger.
     * @param metricAggregate  The metric aggregate to log.
     * @param totalInterval    The total of the interval metric.
     * @param totalRunTime     The total run time of the logger since starting in milliseconds.
     */
    protected abstract void LogIntervalOverTotalRunTimeAggregate(TimeUnitMetricAggregateContainer<IntervalMetric> metricAggregate, long totalInterval, long totalRunTime);

    /**
     * Dequeues and logs metric events stored in the internal buffer, and logs any defined metric aggregates.
     */
    protected void DequeueAndProcessMetricEvents() throws Exception {
        super.DequeueAndProcessMetricEvents();
        LogCountOverTimeUnitAggregates();
        LogAmountOverCountAggregates();
        LogAmountOverTimeUnitAggregates();
        LogAmountOverAmountAggregates();
        LogIntervalOverCountMetricAggregates();
        LogIntervalOverTotalRunTimeAggregates();
    }

    /**
     * Initialises private members of the class.
     */
    private void InitialisePrivateMembers() {
        countOverTimeUnitAggregateDefinitions = new ArrayList<TimeUnitMetricAggregateContainer<CountMetric>>();
        amountOverCountAggregateDefinitions = new ArrayList<MetricAggregateContainer<AmountMetric, CountMetric>>();
        amountOverTimeUnitAggregateDefinitions = new ArrayList<TimeUnitMetricAggregateContainer<AmountMetric>>();
        amountOverAmountAggregateDefinitions = new ArrayList<MetricAggregateContainer<AmountMetric, AmountMetric>>();
        intervalOverAmountAggregateDefinitions = new ArrayList<MetricAggregateContainer<IntervalMetric, CountMetric>>();
        intervalOverTotalRunTimeAggregateDefinitions = new ArrayList<TimeUnitMetricAggregateContainer<IntervalMetric>>();
    }

    /**
     * Calculates and logs the value of all defined metric aggregates representing the number of occurrences of a count metric within the specified time unit.
     */
    private void LogCountOverTimeUnitAggregates() {
        for (TimeUnitMetricAggregateContainer<CountMetric> currentAggregate : countOverTimeUnitAggregateDefinitions) {
            // Calculate the value
            long totalInstances;
            if (countMetricTotals.containsKey(currentAggregate.getNumeratorMetricType()) == true) {
                totalInstances = countMetricTotals.get(currentAggregate.getNumeratorMetricType()).getTotalCount();
            }
            else {
                totalInstances = 0;
            }

            // Convert the number of elapsed milliseconds since starting to the time unit specified in the aggregate
            long totalElapsedTimeUnits = currentAggregate.getDenominatorTimeUnit().convert(CalculateElapsedTimeSinceStart(), TimeUnit.MILLISECONDS);
            LogCountOverTimeUnitAggregate(currentAggregate, totalInstances, totalElapsedTimeUnits);
        }
    }

    /**
     * Calculates and logs the value of all defined metric aggregates representing the total of an amount metric per occurrence of a count metric.
     */
    private void LogAmountOverCountAggregates() {
        for (MetricAggregateContainer<AmountMetric, CountMetric> currentAggregate : amountOverCountAggregateDefinitions) {
            long totalAmount;
            if (amountMetricTotals.containsKey(currentAggregate.getNumeratorMetricType()) == true) {
                totalAmount = amountMetricTotals.get(currentAggregate.getNumeratorMetricType()).getTotal();
            }
            else {
                totalAmount = 0;
            }

            long totalInstances;
            if (countMetricTotals.containsKey(currentAggregate.getDenominatorMetricType()) == true) {
                totalInstances = countMetricTotals.get(currentAggregate.getDenominatorMetricType()).getTotalCount();
            }
            else {
                totalInstances = 0;
            }

            LogAmountOverCountAggregate(currentAggregate, totalAmount, totalInstances);
        }
    }

    /**
     * Calculates and logs the value of all defined metric aggregates representing the total of an amount metric within the specified time unit.
     */
    private void LogAmountOverTimeUnitAggregates() {
        for (TimeUnitMetricAggregateContainer<AmountMetric> currentAggregate : amountOverTimeUnitAggregateDefinitions) {
            // Calculate the total
            long totalAmount;
            if (amountMetricTotals.containsKey(currentAggregate.getNumeratorMetricType()) == true) {
                totalAmount = amountMetricTotals.get(currentAggregate.getNumeratorMetricType()).getTotal();
            }
            else {
                totalAmount = 0;
            }

            // Convert the number of elapsed milliseconds since starting to the time unit specified in the aggregate
            long totalElapsedTimeUnits = currentAggregate.getDenominatorTimeUnit().convert(CalculateElapsedTimeSinceStart(), TimeUnit.MILLISECONDS);
            LogAmountOverTimeUnitAggregate(currentAggregate, totalAmount, totalElapsedTimeUnits);
        }
    }

    /**
     * Calculates and logs the value of all defined metric aggregates representing the total of an amount metric divided by the total of another amount metric.
     */
    private void LogAmountOverAmountAggregates() {
        for (MetricAggregateContainer<AmountMetric, AmountMetric> currentAggregate : amountOverAmountAggregateDefinitions) {
            long numeratorTotal;
            if (amountMetricTotals.containsKey(currentAggregate.getNumeratorMetricType()) == true) {
                numeratorTotal = amountMetricTotals.get(currentAggregate.getNumeratorMetricType()).getTotal();
            }
            else {
                numeratorTotal = 0;
            }

            long denominatorTotal;
            if (amountMetricTotals.containsKey(currentAggregate.getDenominatorMetricType()) == true) {
                denominatorTotal = amountMetricTotals.get(currentAggregate.getDenominatorMetricType()).getTotal();
            }
            else {
                denominatorTotal = 0;
            }

            LogAmountOverAmountAggregate(currentAggregate, numeratorTotal, denominatorTotal);
        }
    }

    /**
     * Calculates and logs the value of all defined metric aggregates representing the total of an interval metric per occurrence of a count metric.
     */
    private void LogIntervalOverCountMetricAggregates() {
        for (MetricAggregateContainer<IntervalMetric, CountMetric> currentAggregate : intervalOverAmountAggregateDefinitions) {
            long totalInterval;
            if (intervalMetricTotals.containsKey(currentAggregate.getNumeratorMetricType()) == true) {
                totalInterval = intervalMetricTotals.get(currentAggregate.getNumeratorMetricType()).getTotal();
            }
            else {
                totalInterval = 0;
            }

            long totalInstances;
            if (countMetricTotals.containsKey(currentAggregate.getDenominatorMetricType()) == true) {
                totalInstances = countMetricTotals.get(currentAggregate.getDenominatorMetricType()).getTotalCount();
            }
            else {
                totalInstances = 0;
            }

            LogIntervalOverCountAggregate(currentAggregate, totalInterval, totalInstances);
        }
    }

    /**
     * Calculates and logs the value of all defined metric aggregates representing the total of an interval metric as a fraction of the total runtime of the logger.
     */
    private void LogIntervalOverTotalRunTimeAggregates()
    {
        for (TimeUnitMetricAggregateContainer<IntervalMetric> currentAggregate : intervalOverTotalRunTimeAggregateDefinitions) {
            long totalInterval;
            if (intervalMetricTotals.containsKey(currentAggregate.getNumeratorMetricType()) == true) {
                totalInterval = intervalMetricTotals.get(currentAggregate.getNumeratorMetricType()).getTotal();
            }
            else {
                totalInterval = 0;
            }

            long totalElapsedMilliseconds = CalculateElapsedTimeSinceStart();

            LogIntervalOverTotalRunTimeAggregate(currentAggregate, totalInterval, totalElapsedMilliseconds);
        }
    }
    
    /**
     * Calculates the number of milliseconds that have elapsed since the Start() method was called.
     * @return  The time since starting in milliseconds.
     */
    private long CalculateElapsedTimeSinceStart() {
        return calendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)).getTimeInMillis() - startTime.getTimeInMillis();
    }

    /**
     * Checks all aggregate containers for an existing defined aggregate with the specified name, and throws an exception if an existing aggregate is found.
     * @param   name       The aggregate name to check for.
     * @throws  Exception  if a metric aggregate with the specified name has already been defined.
     */
    private void CheckDuplicateAggregateName(String name) throws Exception {
        boolean exists = false;

        for (TimeUnitMetricAggregateContainer<CountMetric> currentAggregate : countOverTimeUnitAggregateDefinitions) {
            if (currentAggregate.getName() == name) {
                exists = true;
            }
        }
        for (MetricAggregateContainer<AmountMetric, CountMetric> currentAggregate : amountOverCountAggregateDefinitions) {
            if (currentAggregate.getName() == name) {
                exists = true;
            }
        }
        for (TimeUnitMetricAggregateContainer<AmountMetric> currentAggregate : amountOverTimeUnitAggregateDefinitions) {
            if (currentAggregate.getName() == name) {
                exists = true;
            }
        }
        for (MetricAggregateContainer<AmountMetric, AmountMetric> currentAggregate : amountOverAmountAggregateDefinitions) {
            if (currentAggregate.getName() == name) {
                exists = true;
            }
        }
        for (MetricAggregateContainer<IntervalMetric, CountMetric> currentAggregate : intervalOverAmountAggregateDefinitions) {
            if (currentAggregate.getName() == name) {
                exists = true;
            }
        }
        for (TimeUnitMetricAggregateContainer<IntervalMetric> currentAggregate : intervalOverTotalRunTimeAggregateDefinitions) {
            if (currentAggregate.getName() == name) {
                exists = true;
            }
        }

        if (exists == true) {
            throw new Exception("Metric aggregate with name '" + name + "' has already been defined.");
        }
    }
}

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
 * Base class which buffers and totals instances of metric events.
 * Derived classes must implement methods which log the totals of metric events (e.g. LogCountMetricTotal()).  These methods are called from a worker thread after dequeueing the buffered metric events and calculating and storing their totals.
 * @author Alastair Wyse
 */
abstract class MetricLoggerStorer extends MetricLoggerBuffer {

    /** HashMap which stores the type of a count metric, and a container object holding the number of instances of that type of count metric event. */
    protected HashMap<Class<?>, CountMetricTotalContainer> countMetricTotals;
    /** HashMap which stores the type of an amount metric, and a container object holding the total amount of all instances of that type of amount metric event. */
    protected HashMap<Class<?>, AmountMetricTotalContainer> amountMetricTotals;
    /** HashMap which stores the type of a status metric, and the most recently logged value of that type of status metric.  */
    protected HashMap<Class<?>, StatusMetricValueContainer> statusMetricLatestValues;
    /** HashMap which stores the type of an interval metric, and a container object holding the total amount of all instances of that type of interval metric event. */
    protected HashMap<Class<?>, IntervalMetricTotalContainer> intervalMetricTotals;

    /**
     * Initialises a new instance of the MetricLoggerStorer class.
     * @param dequeueOperationLoopInterval  The time to wait in between iterations of the worker thread which dequeues and processes metric events.
     * @param intervalMetricChecking        Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).
     * @param exceptionHandler              Handler for any uncaught exceptions occurring on the worker thread.
     */
    protected MetricLoggerStorer(int dequeueOperationLoopInterval, boolean intervalMetricChecking, UncaughtExceptionHandler exceptionHandler) {
        super(dequeueOperationLoopInterval, intervalMetricChecking, exceptionHandler);
        InitialisePrivateMembers();
    }
    
    /**
     * Initialises a new instance of the MetricLoggerStorer class.  
     * <b>Note</b> this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
     * @param dequeueOperationLoopInterval        The time to wait in between iterations of the worker thread which dequeues and processes metric events.
     * @param intervalMetricChecking              Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).
     * @param exceptionHandler                    Handler for any uncaught exceptions occurring on the worker thread.
     * @param calendarProvider                    A test (mock) ICalendarProvider object.
     * @param dequeueOperationLoopCompleteSignal  Notifies test code that an iteration of the worker thread which dequeues and processes metric events has completed.
     */
    protected MetricLoggerStorer(int dequeueOperationLoopInterval, boolean intervalMetricChecking, UncaughtExceptionHandler exceptionHandler, ICalendarProvider calendarProvider, CountDownLatch dequeueOperationLoopCompleteSignal) {
        super(dequeueOperationLoopInterval, intervalMetricChecking, exceptionHandler, calendarProvider, dequeueOperationLoopCompleteSignal);
        InitialisePrivateMembers();
    }

    /**
     * Logs the total number of occurrences of a count metric.
     * @param countMetric  The count metric.
     * @param value        The total number of occurrences.
     */
    protected abstract void LogCountMetricTotal(CountMetric countMetric, long value);

    /**
     * Logs the total amount of all occurrences of an amount metric.
     * @param amountMetric  The amount metric.
     * @param value         The total.
     */
    protected abstract void LogAmountMetricTotal(AmountMetric amountMetric, long value);

    /**
     * Logs the most recent value of a status metric.
     * @param statusMetric  The status metric.
     * @param value         The value.
     */
    protected abstract void LogStatusMetricValue(StatusMetric statusMetric, long value);

    /**
     * Logs the total amount of all occurrences of an interval metric.
     * @param intervalMetric  The interval metric.
     * @param value           The total.
     */
    protected abstract void LogIntervalMetricTotal(IntervalMetric intervalMetric, long value);

    /**
     * Dequeues and logs metric events stored in the internal buffer.
     */
    protected void DequeueAndProcessMetricEvents() throws Exception {
        super.DequeueAndProcessMetricEvents();
        LogCountMetricTotals();
        LogAmountMetricTotals();
        LogStatusMetricValues();
        LogIntervalMetricTotals();
    }

    @Override
    protected void ProcessCountMetricEvent(CountMetricEventInstance countMetricEvent) throws Exception {
        if (countMetricTotals.containsKey(countMetricEvent.getMetricType()) == false) {
            countMetricTotals.put(countMetricEvent.getMetricType(), new CountMetricTotalContainer(countMetricEvent.getMetric()));
        }
        countMetricTotals.get(countMetricEvent.getMetricType()).Increment();
    }

    @Override
    protected void ProcessAmountMetricEvent(AmountMetricEventInstance amountMetricEvent) throws Exception {
        if (amountMetricTotals.containsKey(amountMetricEvent.getMetricType()) == false){
            amountMetricTotals.put(amountMetricEvent.getMetricType(), new AmountMetricTotalContainer(amountMetricEvent.getMetric()));
        }
        amountMetricTotals.get(amountMetricEvent.getMetricType()).Add(amountMetricEvent.getMetric().getAmount());
    }

    @Override
    protected void ProcessStatusMetricEvent(StatusMetricEventInstance statusMetricEvent) throws Exception {
        if (statusMetricLatestValues.containsKey(statusMetricEvent.getMetricType()) == false) {
            statusMetricLatestValues.put(statusMetricEvent.getMetricType(), new StatusMetricValueContainer(statusMetricEvent.getMetric()));
        }
        statusMetricLatestValues.get(statusMetricEvent.getMetricType()).Set(statusMetricEvent.getMetric().getValue());
    }

    @Override
    protected void ProcessIntervalMetricEvent(IntervalMetricEventInstance intervalMetricEvent, long duration) throws Exception {
        if (intervalMetricTotals.containsKey(intervalMetricEvent.getMetricType()) == false) {
            intervalMetricTotals.put(intervalMetricEvent.getMetricType(), new IntervalMetricTotalContainer(intervalMetricEvent.getMetric()));
        }
        intervalMetricTotals.get(intervalMetricEvent.getMetricType()).Add(duration);
    }

    /**
     * Initialises private members of the class.
     */
    private void InitialisePrivateMembers() {
        countMetricTotals = new HashMap<Class<?>, CountMetricTotalContainer>();
        amountMetricTotals = new HashMap<Class<?>, AmountMetricTotalContainer>();
        statusMetricLatestValues = new HashMap<Class<?>, StatusMetricValueContainer>();
        intervalMetricTotals = new HashMap<Class<?>, IntervalMetricTotalContainer>();
    }
    
    /**
     * Logs the totals of stored count metrics.
     */
    private void LogCountMetricTotals() {
        for (CountMetricTotalContainer currentCountMetricTotal : countMetricTotals.values()) {
            LogCountMetricTotal(currentCountMetricTotal.getCountMetric(), currentCountMetricTotal.getTotalCount());
        }
    }

    /**
     * Logs the totals of stored amount metrics.
     */
    private void LogAmountMetricTotals() {
        for (AmountMetricTotalContainer currentAmountMetricTotal : amountMetricTotals.values()) {
            LogAmountMetricTotal(currentAmountMetricTotal.getAmountMetric(), currentAmountMetricTotal.getTotal());
        }
    }

    /**
     * Logs the most recently logged values of stored status metrics.
     */
    private void LogStatusMetricValues() {
        for (StatusMetricValueContainer currentStatusMetricValue : statusMetricLatestValues.values()) {
            LogStatusMetricValue(currentStatusMetricValue.getStatusMetric(), currentStatusMetricValue.getValue());
        }
    }

    /**
     * Logs the totals of stored interval metrics.
     */
    private void LogIntervalMetricTotals() {
        for (IntervalMetricTotalContainer currentIntervalMetricTotal : intervalMetricTotals.values()) {
            LogIntervalMetricTotal(currentIntervalMetricTotal.getIntervalMetric(), currentIntervalMetricTotal.getTotal());
        }
    }
}

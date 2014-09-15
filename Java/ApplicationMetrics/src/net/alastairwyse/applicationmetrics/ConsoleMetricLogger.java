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
import java.text.*;
import java.util.*;
import java.util.concurrent.*;

import net.alastairwyse.operatingsystemabstraction.*;

/**
 * Writes metric and instrumentation events for an application to the console.
 * @author Alastair Wyse
 */
public class ConsoleMetricLogger extends MetricAggregateLogger {

    private final String separatorString = ": ";
    private IPrintStream printStream;
    
    /**
     * Initialises a new instance of the ConsoleMetricLogger class.
     * @param dequeueOperationLoopInterval  The time to wait in between iterations of the worker thread which dequeues metric events and writes them to the console.
     * @param intervalMetricChecking        Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).
     * @param exceptionHandler              Handler for any uncaught exceptions occurring on the worker thread.
     */
    public ConsoleMetricLogger(int dequeueOperationLoopInterval, boolean intervalMetricChecking, UncaughtExceptionHandler exceptionHandler) {
        super(dequeueOperationLoopInterval, intervalMetricChecking, exceptionHandler);
        printStream = new PrintStream();
    }
    
    /**
     * Initialises a new instance of the ConsoleMetricLogger class.  
     * <b>Note</b> this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
     * @param dequeueOperationLoopInterval        The time to wait in between iterations of the worker thread which dequeues metric events and writes them to the console.
     * @param intervalMetricChecking              Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).
     * @param exceptionHandler                    Handler for any uncaught exceptions occurring on the worker thread.
     * @param printStream                         A test (mock) IPrintStream object.
     * @param calendarProvider                    A test (mock) ICalendarProvider object.
     * @param dequeueOperationLoopCompleteSignal  Notifies test code that an iteration of the worker thread which dequeues and processes metric events has completed.
     */
    public ConsoleMetricLogger(int dequeueOperationLoopInterval, boolean intervalMetricChecking, UncaughtExceptionHandler exceptionHandler, IPrintStream printStream, ICalendarProvider calendarProvider, CountDownLatch dequeueOperationLoopCompleteSignal) {
        super(dequeueOperationLoopInterval, intervalMetricChecking, exceptionHandler, calendarProvider, dequeueOperationLoopCompleteSignal);
        this.printStream = printStream;
    }

    /**
     * Dequeues and logs metric events stored in the internal buffer, and logs any defined metric aggregates.
     */
    protected void DequeueAndProcessMetricEvents() throws Exception {
        SimpleDateFormat dateFormatter = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
        printStream.println();
        printStream.println();
        printStream.println("---------------------------------------------------");
        printStream.println("-- Application metrics as of " + dateFormatter.format(calendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)).getTime()) + " --");
        printStream.println("---------------------------------------------------");
        super.DequeueAndProcessMetricEvents();
    }
    
    @Override
    protected void LogCountMetricTotal(CountMetric countMetric, long value) {
        printStream.println(countMetric.getName() + separatorString + value);
    }

    @Override
    protected void LogAmountMetricTotal(AmountMetric amountMetric, long value) {
        printStream.println(amountMetric.getName() + separatorString + value);
    }

    @Override
    protected void LogStatusMetricValue(StatusMetric statusMetric, long value) {
        printStream.println(statusMetric.getName() + separatorString + value);
    }

    @Override
    protected void LogIntervalMetricTotal(IntervalMetric intervalMetric, long value) {
        printStream.println(intervalMetric.getName() + separatorString + value);
    }

    @Override
    protected void LogCountOverTimeUnitAggregate(TimeUnitMetricAggregateContainer<CountMetric> metricAggregate, long totalInstances, long totalElapsedTimeUnits) {
        if (totalElapsedTimeUnits != 0) {
            double aggregateValue = Long.valueOf(totalInstances).doubleValue() / totalElapsedTimeUnits;
            printStream.println(metricAggregate.getName() + separatorString + aggregateValue);
        }
    }

    @Override
    protected void LogAmountOverCountAggregate(MetricAggregateContainer<AmountMetric, CountMetric> metricAggregate, long totalAmount, long totalInstances) {
        if (totalInstances != 0) {
            double aggregateValue = Long.valueOf(totalAmount).doubleValue() / totalInstances;
            printStream.println(metricAggregate.getName() + separatorString + aggregateValue);
        }
    }

    @Override
    protected void LogAmountOverTimeUnitAggregate(TimeUnitMetricAggregateContainer<AmountMetric> metricAggregate, long totalAmount, long totalElapsedTimeUnits) {
        if (totalElapsedTimeUnits != 0) {
            double aggregateValue = Long.valueOf(totalAmount).doubleValue() / totalElapsedTimeUnits;
            printStream.println(metricAggregate.getName() + separatorString + aggregateValue);
        }
    }

    @Override
    protected void LogAmountOverAmountAggregate(MetricAggregateContainer<AmountMetric, AmountMetric> metricAggregate, long numeratorTotal, long denominatorTotal) {
        if (denominatorTotal != 0) {
            double aggregateValue = Long.valueOf(numeratorTotal).doubleValue() / denominatorTotal;
            printStream.println(metricAggregate.getName() + separatorString + aggregateValue);
        }
    }

    @Override
    protected void LogIntervalOverCountAggregate(MetricAggregateContainer<IntervalMetric, CountMetric> metricAggregate, long totalInterval, long totalInstances) {
        if (totalInstances != 0) {
            double aggregateValue = Long.valueOf(totalInterval).doubleValue() / totalInstances;
            printStream.println(metricAggregate.getName() + separatorString + aggregateValue);
        }
    }

    @Override
    protected void LogIntervalOverTotalRunTimeAggregate(TimeUnitMetricAggregateContainer<IntervalMetric> metricAggregate, long totalInterval, long totalRunTime) {
        if (totalRunTime > 0) {
            double aggregateValue = Long.valueOf(totalInterval).doubleValue() / totalRunTime;
            printStream.println(metricAggregate.getName() + separatorString + aggregateValue);
        }
    }
}

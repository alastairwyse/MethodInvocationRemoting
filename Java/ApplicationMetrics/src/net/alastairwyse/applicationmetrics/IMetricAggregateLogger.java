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

import java.util.concurrent.TimeUnit;

/**
 * Defines methods to register aggregates of metric events, allowing the values of these aggregates to be recorded and logged when the underlying metric events occur.
 * @author Alastair Wyse
 */
public interface IMetricAggregateLogger {

    /**
     * Defines a metric aggregate which represents the number of occurrences of a count metric within the specified time unit.
     * This metric aggregate could be used to represent the number of messages sent to a remote system each minute, or the number of disk reads per second.
     * @param countMetric  The count metric recorded as part of the aggregate.
     * @param timeUnit     The unit of time in which the number of occurrences of the count metric is recorded.
     * @param name         The name of the metric aggregate.
     * @param description  A description of the metric aggregate, explaining what it measures and/or represents.
     * @throws Exception   if an error occurs defining the metric aggregate.
     */
    public void DefineMetricAggregate(CountMetric countMetric, TimeUnit timeUnit, String name, String description) throws Exception;

    /**
     * Defines a metric aggregate which represents the total amount of the specified amount metric per occurrence of the specified count metric.
     * This metric aggregate could be used to represent the number of bytes per message sent to a remote system, or the number of bytes read per disk read.
     * @param amountMetric  The amount metric recorded as part of the aggregate (effectively the numerator).
     * @param countMetric   The count metric per which the total amount of the amount metric(s) are aggregated (effectively the denominator).
     * @param name          The name of the metric aggregate.
     * @param description   A description of the metric aggregate, explaining what it measures and/or represents.
     * @throws Exception    if an error occurs defining the metric aggregate.
     */
    public void DefineMetricAggregate(AmountMetric amountMetric, CountMetric countMetric, String name, String description) throws Exception;

    /**
     * Defines a metric aggregate which represents the total amount of the specified amount metric within the specified time unit.
     * This metric aggregate could be used to represent the number of bytes sent to a remote system each minute, or the number of bytes read from disk per second.
     * @param amountMetric  The amount metric recorded as part of the aggregate.
     * @param timeUnit      The unit of time in which the amount associated with the specified amount metric is recorded.
     * @param name          The name of the metric aggregate.
     * @param description   A description of the metric aggregate, explaining what it measures and/or represents.
     * @throws Exception    if an error occurs defining the metric aggregate.
     */
    public void DefineMetricAggregate(AmountMetric amountMetric, TimeUnit timeUnit, String name, String description) throws Exception;

    /**
     * Defines a metric aggregate which represents the ratio of one amount metric to another.
     * This metric aggregate could be used to represent the size of a compressed file against the size of the same file uncompressed, effectively recording the overall compression ratio.
     * @param numeratorAmountMetric    The amount metric which is the numerator in the ratio.
     * @param denominatorAmountMetric  The amount metric which is the denominator in the ratio.
     * @param name                     The name of the metric aggregate.
     * @param description              A description of the metric aggregate, explaining what it measures and/or represents.
     * @throws Exception               if an error occurs defining the metric aggregate.
     */
    public void DefineMetricAggregate(AmountMetric numeratorAmountMetric, AmountMetric denominatorAmountMetric, String name, String description) throws Exception;

    /**
     * Defines a metric aggregate which represents the total time of the specified interval metric per occurrence of the specified count metric.
     * This metric aggregate could be used to represent the average time to send a message to a remote system, or the average time to read perform a disk read.
     * @param intervalMetric  The interval metric recorded as part of the aggregate (effectively the numerator).
     * @param countMetric     The count metric per which the total time of the interval metric(s) are aggregated (effectively the denominator).
     * @param name            The name of the metric aggregate.
     * @param description     A description of the metric aggregate, explaining what it measures and/or represents.
     * @throws Exception      if an error occurs defining the metric aggregate.
     */
    public void DefineMetricAggregate(IntervalMetric intervalMetric, CountMetric countMetric, String name, String description) throws Exception;

    /**
     * Defines a metric which represents the total time of all occurrences of the specified interval metric as a fraction of the total runtime of the logger.
     * This metric aggregate could be used to represent the percentage of total runtime spent sending messages to a remote system.
     * @param intervalMetric  The interval metric recorded as part of the aggregate.
     * @param name            The name of the metric aggregate.
     * @param description     A description of the metric aggregate, explaining what it measures and/or represents.
     * @throws Exception      if an error occurs defining the metric aggregate.
     */
    public void DefineMetricAggregate(IntervalMetric intervalMetric, String name, String description) throws Exception;
}

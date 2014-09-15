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

/**
 * Base class for metrics representing an event which has a value associated with it, and where the value represents a point in time status which may fluctuate over time but does not accumulate.
 * Examples of derived classes could be metrics representing the amount of free memory in the system, or the number of active worker threads in a multi-threaded application at a given time.  The distinction between metrics represented by this class and those represented by the AmountMetric class, is that the accumulated value of amount metrics over a time period has meaning (e.g. to find the average amount over a series of instances of the metric, or over a time period).  The summing of instances of status metrics over time however, has little meaning.
 * @author Alastair Wyse
 */
public abstract class StatusMetric extends MetricBase {

    /** The value associated with the metric. */
    protected long value;

    /**
     * @return  The value associated with the metric.
     */
    public long getValue() {
        return value;
    }
}

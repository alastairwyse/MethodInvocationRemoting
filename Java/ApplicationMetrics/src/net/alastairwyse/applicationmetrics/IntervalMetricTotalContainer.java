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
 * Container class which stores an interval metric, and the total interval of all instances of the metric.
 * @author Alastair Wyse
 */
class IntervalMetricTotalContainer {

    private IntervalMetric intervalMetric;
    private long total;

    /**
     * @return  The interval metric for which the total is stored.
     */
    public IntervalMetric getIntervalMetric() {
        return intervalMetric;
    }

    /**
     * @return  The total interval of all instances of the metric.
     */
    public long getTotal() {
        return total;
    }

    /**
     * Initialises a new instance of the IntervalMetricTotalContainer class.
     * @param intervalMetric  The interval metric for which the total stored.
     */
    public IntervalMetricTotalContainer(IntervalMetric intervalMetric) {
        this.intervalMetric = intervalMetric;
        total = 0;
    }

    /**
     * Adds the specified amount to the stored total.
     * @param amount  The amount to add.
     */
    public void Add(long amount) {
        if ((Long.MAX_VALUE - total) >= amount) {
            total = total + amount;
        }
    }
}

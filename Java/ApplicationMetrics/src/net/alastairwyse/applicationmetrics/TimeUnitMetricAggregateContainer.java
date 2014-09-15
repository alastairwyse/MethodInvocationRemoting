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

import java.util.concurrent.*;

/**
 * Container class which stores definitions of aggregates of metrics where the denominator of the metric is a unit of time.
 * @author Alastair Wyse
 *
 * @param <TNumerator>  The type of the numerator of the metric aggregate.
 */
class TimeUnitMetricAggregateContainer<TNumerator> extends MetricAggregateContainerBase<TNumerator> {

    /** The time unit representing the denominator of the metric aggregate. */
    protected TimeUnit timeUnit;

    /**
     * @return  The time unit representing the denominator of the metric aggregate.
     */
    public TimeUnit getDenominatorTimeUnit() {
        return timeUnit;
    }

    /**
     * Initialises a new instance of the TimeUnitMetricAggregateContainer class.
     * @param numeratorMetric  The metric which is the numerator of the aggregate.
     * @param timeUnit         The time unit representing the denominator of the metric aggregate.
     * @param name             The name of the metric aggregate.
     * @param description      A description of the metric aggregate, explaining what it measures and/or represents.
     */
    public TimeUnitMetricAggregateContainer(TNumerator numeratorMetric, TimeUnit timeUnit, String name, String description) {
        super(numeratorMetric, name, description);
        this.timeUnit = timeUnit;
    }
}

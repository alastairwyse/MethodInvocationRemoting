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
 * Base class for metric aggregate containers containing common properties.
 * @author Alastair Wyse
 *
 * @param <TNumerator>  The type representing the numerator of the aggregate.
 */
class MetricAggregateContainerBase<TNumerator> {

    /** The metric representing the numerator of the aggregate. */
    protected TNumerator numeratorMetric;
    /** The name of the metric aggregate. */
    protected String name;
    /** A description of the metric aggregate, explaining what it measures and/or represents. */
    protected String description;

    /**
     * @return  The type of the numerator of the metric aggregate.
     */
    public Class<?> getNumeratorMetricType() {
        return numeratorMetric.getClass();
    }

    public String getName() {
        return name;
    }

    /**
     * @return  A description of the metric aggregate, explaining what it measures and/or represents.
     */
    public String getDescription() {
        return description;
    }

    /**
     * Initialises a new instance of the MetricAggregateContainerBase class.
     * @param numeratorMetric  The metric which is the numerator of the aggregate.
     * @param name             The name of the metric aggregate.
     * @param description      A description of the metric aggregate, explaining what it measures and/or represents.
     */
    protected MetricAggregateContainerBase(TNumerator numeratorMetric, String name, String description) {
        this.numeratorMetric = numeratorMetric;

        if (name.trim().isEmpty() != true) {
            this.name = name;
        }
        else {
            throw new IllegalArgumentException("Argument 'name' cannot be blank.");
        }

        if (description.trim().isEmpty() != true) {
            this.description = description;
        }
        else {
            throw new IllegalArgumentException("Argument 'description' cannot be blank.");
        }
    }
}

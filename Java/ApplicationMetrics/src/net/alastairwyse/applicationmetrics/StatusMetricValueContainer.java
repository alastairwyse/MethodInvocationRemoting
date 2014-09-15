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
 * Container class which stores a status metric, and the value of the most recent instance of the metric.
 * @author Alastair Wyse
 */
class StatusMetricValueContainer {

    private StatusMetric statusMetric;
    private long value;

    /**
     * @return  The status metric for which the most recent value is stored.
     */
    public StatusMetric getStatusMetric() {
        return statusMetric;
    }

    /**
     * @return  The value of the most recent instance of the metric.
     */
    public long getValue() {
        return value;
    }

    /**
     * Initialises a new instance of the StatusMetricValueContainer class.
     * @param statusMetric  The status metric for which the most recent value stored.
     */
    public StatusMetricValueContainer(StatusMetric statusMetric) {
        this.statusMetric = statusMetric;
        value = 0;
    }

    /**
     * Sets the stored value.
     * @param value  The value.
     */
    public void Set(long value) {
        this.value = value;
    }
}

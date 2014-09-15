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
 * Container class which stores a count metric, and the total of the number of instances of the metric.
 * @author Alastair Wyse
 */
class CountMetricTotalContainer {

    private CountMetric countMetric;
    private long totalCount;

    /**
     * @return  The count metric for which the total number of instances is stored.
     */
    public CountMetric getCountMetric() {
        return countMetric;
    }

    /**
     * @return  The total number of instances of the count metric.
     */
    public long getTotalCount() {
        return totalCount;
    }

    /**
     * Initialises a new instance of the CountMetricTotalContainer class.
     * @param countMetric  The count metric for which the total number of instances is stored.
     */
    public CountMetricTotalContainer(CountMetric countMetric) {
        this.countMetric = countMetric;
        totalCount = 0;
    }

    /**
     * Increments the total number of instances by 1.
     */
    public void Increment() {
        if (totalCount != Long.MAX_VALUE) {
            totalCount++;
        }
    }
}

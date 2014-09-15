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

import java.util.*;

/**
 * Base container class which stores information about the occurrence of a metric event.
 * @author Alastair Wyse
 *
 * @param <T>  The type of metric the event information should be stored for.
 */
abstract class MetricEventInstance<T extends MetricBase> {

    /** The metric that occurred. */
    protected T metric;
    /** The date and time the event occurred, expressed as UTC. */
    protected Calendar eventTime;
    
    /**
     * @return  The metric that occurred.
     */
    public T getMetric() {
        return metric;
    }

    /**
     * @return  
     */
    public Class<?> getMetricType()
    {
        return metric.getClass();
    }

    /**
     * @return  The date and time the event occurred, expressed as UTC.
     */
    public Calendar getEventTime() {
        return eventTime;
    }
}

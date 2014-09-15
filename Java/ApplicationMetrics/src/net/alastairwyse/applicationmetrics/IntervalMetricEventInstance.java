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

enum IntervalMetricEventTimePoint {
    
    /** The start of the interval metric event. */
    Start, 
    /** The completion of the interval metric event. */
    End
}

/**
 * Container class which stores information about the occurrence of an interval metric event.
 * @author Alastair Wyse
 */
class IntervalMetricEventInstance extends MetricEventInstance<IntervalMetric> {

    private IntervalMetricEventTimePoint timePoint;
    
    /**
     * @return  Whether the event represents the start or the end of the interval metric.
     */
    public IntervalMetricEventTimePoint getTimePoint() {
        return timePoint;
    }
    
    /**
     * Initialises a new instance of the IntervalMetricEventInstance class.
     * @param intervalMetric  The metric which occurred.
     * @param timePoint       Whether the event represents the start or the end of the interval metric.
     * @param eventTime       The date and time the metric event started, expressed as UTC.
     */
    public IntervalMetricEventInstance(IntervalMetric intervalMetric, IntervalMetricEventTimePoint timePoint, Calendar eventTime) {
        super.metric = intervalMetric;
        this.timePoint = timePoint;
        super.eventTime = eventTime;
    }
}

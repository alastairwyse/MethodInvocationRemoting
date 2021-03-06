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
 * Container class which stores information about the occurrence of an amount metric event.
 * @author Alastair Wyse
 */
class AmountMetricEventInstance extends MetricEventInstance<AmountMetric> {

    /**
     * Initialises a new instance of the AmountMetricEventInstance class.
     * @param amountMetric  The metric which occurred.
     * @param eventTime     The date and time the metric event occurred, expressed as UTC.
     */
    public AmountMetricEventInstance(AmountMetric amountMetric, Calendar eventTime)
    {
        super.metric = amountMetric;
        super.eventTime = eventTime;
    }
}

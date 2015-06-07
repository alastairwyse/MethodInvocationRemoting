/*
 * Copyright 2015 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
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
 * Defines methods to record metric and instrumentation events for an application.
 * @author Alastair Wyse
 */
public interface IMetricLogger {

    /**
     * Records a single instance of the specified count event.
     * @param  countMetric  The count metric that occurred.
     */
    public void Increment(CountMetric countMetric);

    /**
     * Records an instance of the specified amount metric event, and the associated amount.
     * @param  amountMetric  The amount metric that occurred.
     */
    public void Add(AmountMetric amountMetric);

    /**
     * Records an instance of the specified status metric event, and the associated value.
     * @param  statusMetric  The status metric that occurred.
     */
    public void Set(StatusMetric statusMetric);

    /**
     * Records the starting of the specified interval event.
     * @param  intervalMetric  The interval metric that started.
     */
    public void Begin(IntervalMetric intervalMetric);

    /**
     * Records the completion of the specified interval event.
     * @param  intervalMetric  The interval metric that completed.
     */
    public void End(IntervalMetric intervalMetric);
    
    /**
     * Cancels the starting of the specified interval event (e.g. in the case that an exception occurs between the starting and completion of the interval event).
     * @param  intervalMetric  The interval metric that should be cancelled.
     */
    public void CancelBegin(IntervalMetric intervalMetric);
}

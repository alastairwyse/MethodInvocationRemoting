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
 * Implementation of the IMetricLogger interface which does not perform any metric logging.
 * An instance of this class can be used as the default IMetricLogger implementation inside a client class, to prevent occurrences of NullPointerException.
 * @author Alastair Wyse
 */
public class NullMetricLogger implements IMetricLogger {

    /**
     * Initialises a new instance of the NullMetricLogger class.
     */
    public NullMetricLogger() {
        
    }
    
    @Override
    public void Increment(CountMetric countMetric) {

    }

    @Override
    public void Add(AmountMetric amountMetric) {

    }

    @Override
    public void Set(StatusMetric statusMetric) {

    }

    @Override
    public void Begin(IntervalMetric intervalMetric) {

    }

    @Override
    public void End(IntervalMetric intervalMetric) {

    }
}

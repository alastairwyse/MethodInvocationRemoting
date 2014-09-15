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
 * Base class for metrics containing common properties.
 * @author Alastair Wyse
 */
public abstract class MetricBase {
    
    /** The name of the metric. */
    protected String name;
    /** A description of the metric, explaining what it measures and/or represents. */
    protected String description;
    
    /**
     * @return  The name of the metric.
     */
    public String getName() {
        return name;
    }
    
    /**
     * @return  A description of the metric, explaining what it measures and/or represents.
     */
    public String getDescription() {
        return description;
    }
}

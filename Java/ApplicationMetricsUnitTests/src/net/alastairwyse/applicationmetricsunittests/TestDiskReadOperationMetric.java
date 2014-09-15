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

package net.alastairwyse.applicationmetricsunittests;

import net.alastairwyse.applicationmetrics.*;

/**
 * A sample count metric for testing implementations of interface IMetricLogger.
 * @author Alastair Wyse
 */
class TestDiskReadOperationMetric extends CountMetric {

    public TestDiskReadOperationMetric() {
        super.name = "DiskReadOperation";
        super.description = "A single instance of this metric represents a single disk read operation.";
    }
}

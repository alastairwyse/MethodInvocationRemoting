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

using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationMetrics
{
    //******************************************************************************
    //
    // Class: AmountMetric
    //
    //******************************************************************************
    /// <summary>
    /// Base class for metrics representing an event which has an amount associated with it, and where the total of these amounts across multiple occurrences of the event would accumulate.
    /// </summary>
    /// <remarks>Examples of derived classes could be metrics representing the number of bytes in a message sent to a remote system, or the number of blocks read from disk in a read operation.</remarks>
    public abstract class AmountMetric : MetricBase
    {
        /// <summary>The actual amount recorded in a single occurrence of the metric.</summary>
        protected long amount;

        /// <summary>
        /// The actual amount recorded in a single occurrence of the metric.
        /// </summary>
        public long Amount
        {
            get
            {
                return amount;
            }
        }
    }
}

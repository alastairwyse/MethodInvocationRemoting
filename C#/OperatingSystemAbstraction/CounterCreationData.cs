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

namespace OperatingSystemAbstraction
{
    //******************************************************************************
    //
    // Class: CounterCreationData
    //
    //******************************************************************************
    /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="T:OperatingSystemAbstraction.ICounterCreationData"]/*'/>
    public class CounterCreationData : ICounterCreationData
    {
        private System.Diagnostics.CounterCreationData counterCreationData;

        //------------------------------------------------------------------------------
        //
        // Method: CounterCreationData (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the CounterCreationData.PerformanceCounter class.
        /// </summary>
        /// <param name="counterName">The name of the counter, which must be unique within its category.</param>
        /// <param name="counterHelp">The text that describes the counter's behavior.</param>
        /// <param name="counterType">A PerformanceCounterType that identifies the counter's behavior.</param>
        public CounterCreationData(string counterName, string counterHelp, System.Diagnostics.PerformanceCounterType counterType)
        {
            counterCreationData = new System.Diagnostics.CounterCreationData(counterName, counterHelp, counterType);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="P:OperatingSystemAbstraction.ICounterCreationData.CreationData"]/*'/>
        public System.Diagnostics.CounterCreationData CreationData
        {
            get
            {
                return counterCreationData;
            }
        }
    }
}

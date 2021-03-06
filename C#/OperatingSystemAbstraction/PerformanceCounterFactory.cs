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
    // Class: PerformanceCounterFactory
    //
    //******************************************************************************
    /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="T:OperatingSystemAbstraction.IPerformanceCounterFactory"]/*'/>
    public class PerformanceCounterFactory : IPerformanceCounterFactory
    {
        //------------------------------------------------------------------------------
        //
        // Method: PerformanceCounterFactory (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the OperatingSystemAbstraction.PerformanceCounterFactory class.
        /// </summary>
        public PerformanceCounterFactory()
        {
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:OperatingSystemAbstraction.IPerformanceCounterFactory.Create(System.String,System.String,System.Boolean)"]/*'/>
        public IPerformanceCounter Create(string categoryName, string counterName, bool readOnly)
        {
            return new PerformanceCounter(categoryName, counterName, readOnly);
        }
    }
}

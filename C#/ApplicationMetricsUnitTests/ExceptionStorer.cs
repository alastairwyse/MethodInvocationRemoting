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
using ApplicationMetrics;

namespace ApplicationMetricsUnitTests
{
    //******************************************************************************
    //
    // Class: ExceptionStorer
    //
    //******************************************************************************
    /// <summary>
    /// Stores an exception passed to the Handle() method, and exposes it in property StoredException so that its properties can be tested by unit tests.
    /// </summary>
    class ExceptionStorer : IExceptionHandler
    {
        Exception storedException;

        public Exception StoredException
        {
            get
            {
                return storedException;
            }
        }

        public void Handle(Exception e)
        {
            storedException = e;
        }
    }
}

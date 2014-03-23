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

namespace ApplicationLoggingUnitTests
{
    //******************************************************************************
    //
    // Class: CustomStackTraceException
    //
    //******************************************************************************
    /// <summary>
    /// Derives from System.Exception, and overrides the StackTrace property to return a fixed string for use in unit tests.
    /// </summary>
    class CustomStackTraceException : Exception
    {
        //******************************************************************************
        //
        // Method: CustomStackTraceException (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the ApplicationLoggingUnitTests.CustomStackTraceException class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CustomStackTraceException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Returns a fixed string mocking a stace trace.
        /// </summary>
        public override string StackTrace
        {
            get
            {
                return @"   at ApplicationLoggingUnitTests.LogSuccessTests() in C:\MethodInvocationRemoting\C#\ApplicationLoggingUnitTests\FileApplicationLoggerTests.cs:line 123" + Environment.NewLine + @"   at ApplicationLoggingUnitTests.LogSuccessTests() in C:\MethodInvocationRemoting\C#\ApplicationLoggingUnitTests\FileApplicationLoggerTests.cs:line 456";
            }
        }
    }
}

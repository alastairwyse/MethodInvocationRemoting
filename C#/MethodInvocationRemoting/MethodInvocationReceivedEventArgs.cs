/*
 * Copyright 2013 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
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

namespace MethodInvocationRemoting
{
    //******************************************************************************
    //
    // Class: MethodInvocationReceivedEventArgs
    //
    //******************************************************************************
    /// <summary>
    /// Provides data for the MethodInvocationRemoting.MethodInvocationReceived event.
    /// </summary>
    public class MethodInvocationReceivedEventArgs : EventArgs
    {
        private IMethodInvocation methodInvocation;

        //------------------------------------------------------------------------------
        //
        // Method: MethodInvocationReceivedEventArgs (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MethodInvocationReceivedEventArgs class.
        /// </summary>
        /// <param name="methodInvocation">The method invocation which was received via the event.</param>
        public MethodInvocationReceivedEventArgs(IMethodInvocation methodInvocation)
        {
            this.methodInvocation = methodInvocation;
        }

        /// <summary>
        /// The method invocation which was received via the event.
        /// </summary>
        public IMethodInvocation MethodInvocation
        {
            get
            {
                return methodInvocation;
            }
        }
    }
}

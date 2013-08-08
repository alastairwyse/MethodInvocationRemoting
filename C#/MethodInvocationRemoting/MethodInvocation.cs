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
    // Class: MethodInvocation
    //
    //******************************************************************************
    /// <summary>
    /// Container for properties of a method and the parameters used when the method is called or invoked.
    /// </summary>
    /// <remarks>[Serializable] attribute is included to allow serializing and deserializing with the System.Runtime.Serialization.Formatters.Soap.SoapFormatter class.</remarks>
    [Serializable]
    public class MethodInvocation : IMethodInvocation
    {
        private string name;
        private object[] parameters;
        private Type returnType;

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="P:MethodInvocationRemoting.IMethodInvocation.Name"]/*'/>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="P:MethodInvocationRemoting.IMethodInvocation.Parameters"]/*'/>
        public object[] Parameters
        {
            get
            {
                return parameters;
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="P:MethodInvocationRemoting.IMethodInvocation.ReturnType"]/*'/>
        public Type ReturnType
        {
            get
            {
                return returnType;
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: MethodInvocation (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MethodInvocation class.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="methodParameters">The parameters sent when the method is invoked.</param>
        /// <param name="methodReturnType">The return type of the method.</param>
        public MethodInvocation(string methodName, object[] methodParameters, Type methodReturnType) 
            : this(methodName, methodParameters)
        {
            returnType = methodReturnType;
        }

        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MethodInvocation class.  Should be used for methods with a void return type.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="methodParameters">The parameters sent when the method is invoked.</param>
        public MethodInvocation(string methodName, object[] methodParameters)
            : this(methodName)
        {
            CheckParametersSize(methodParameters, "methodParameters");
            parameters = methodParameters;
        }

        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MethodInvocation class.  Should be used for parameterless methods.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="methodReturnType">The return type of the method.</param>
        public MethodInvocation(string methodName, Type methodReturnType) 
            : this(methodName)
        {
            returnType = methodReturnType;
        }

        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MethodInvocation class.  Should be used for parameterless methods, with a void return type.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        public MethodInvocation(string methodName)
        {
            CheckName(methodName, "methodName");
            name = methodName;
        }

        //------------------------------------------------------------------------------
        //
        // Method: CheckParametersSize
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Throws an exception if the inputted method parameters array has zero length. 
        /// </summary>
        /// <param name="methodParameters">The method parameters array to check.</param>
        /// <param name="parameterName">The parameter name to use in the exception message if an exception is thrown.</param>
        private void CheckParametersSize(object[] methodParameters, string parameterName)
        {
            if (methodParameters.Length == 0)
            {
                throw new ArgumentException("The method invocation parameters cannot be empty.", parameterName);
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: CheckName
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Throws an exception if the inputted method name contains no non-whitespace characters.
        /// </summary>
        /// <param name="name">The method name to check.</param>
        /// <param name="parameterName">The parameter name to use in the exception message if an exception is thrown.</param>
        private void CheckName(string name, string parameterName)
        {
            if (name.Trim().Length == 0)
            {
                throw new ArgumentException("The method name cannot be blank.", parameterName);
            }
        }
    }
}

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
    // Class: SerializationException
    //
    //******************************************************************************
    /// <summary>
    /// The exception that is thrown when serialization of an object fails.
    /// </summary>
    public class SerializationException : Exception
    {
        private object targetObject;

        /// <summary>
        /// The object which when attempting to serialize caused the current exception.
        /// </summary>
        public object TargetObject
        {
            get
            {
                return targetObject;
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: SerializationException (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.SerializationException class.
        /// </summary>
        public SerializationException()
            : base()
        {
        }

        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.SerializationException class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SerializationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.SerializationException class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="targetObject">The object which when attempting to serialize caused the current exception.</param>
        /// <remarks>Parameter 'targetObject' should typically contain either a MethodInvocation object, or a method invocation return value.</remarks>
        public SerializationException(string message, object targetObject)
            : base(message)
        {
            this.targetObject = targetObject;
        }

        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.SerializationException class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public SerializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.SerializationException class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="targetObject">The object which when attempting to serialize caused the current exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        /// <remarks>Parameter 'targetObject' should typically contain either a MethodInvocation object, or a method invocation return value.</remarks>
        public SerializationException(string message, object targetObject, Exception innerException)
            : base(message, innerException)
        {
            this.targetObject = targetObject;
        }
    }
}

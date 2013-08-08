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
    // Class: DeserializationException
    //
    //******************************************************************************
    /// <summary>
    /// The exception that is thrown when deserialization of an object fails.
    /// </summary>
    public class DeserializationException : Exception
    {
        private string serializedObject;

        /// <summary>
        /// The serialized object that caused the current exception.
        /// </summary>
        public string SerializedObject
        {
            get
            {
                return serializedObject;
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: DeserializationException (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.DeserializationException class.
        /// </summary>
        public DeserializationException()
            : base()
        {
        }

        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.DeserializationException class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DeserializationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.DeserializationException class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="serializedObject">The serialized object that caused the current exception.</param>
        public DeserializationException(string message, string serializedObject)
            : base(message)
        {
            this.serializedObject = serializedObject;
        }

        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.DeserializationException class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public DeserializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.DeserializationException class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="serializedObject">The serialized object that caused the current exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public DeserializationException(string message, string serializedObject, Exception innerException)
            : base(message, innerException)
        {
            this.serializedObject = serializedObject;
        }
    }
}

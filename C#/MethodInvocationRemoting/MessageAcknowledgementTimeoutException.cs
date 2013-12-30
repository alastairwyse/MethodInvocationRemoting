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
    // Class: MessageAcknowledgementTimeoutException
    //
    //******************************************************************************
    /// <summary>
    /// The exception that is thrown when a timeout occurs whilst waiting for acknowledgement of the receipt of a message.
    /// </summary>
    public class MessageAcknowledgementTimeoutException : Exception
    {
        //------------------------------------------------------------------------------
        //
        // Method: MessageAcknowledgementTimeoutException (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MessageAcknowledgementTimeoutException class.
        /// </summary>
        public MessageAcknowledgementTimeoutException()
            : base()
        {
        }

        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MessageAcknowledgementTimeoutException class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MessageAcknowledgementTimeoutException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MessageAcknowledgementTimeoutException class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public MessageAcknowledgementTimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

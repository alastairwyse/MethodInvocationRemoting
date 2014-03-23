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
using System.Runtime.Serialization.Formatters.Soap;
using System.IO;

namespace MethodInvocationRemoting
{
    //******************************************************************************
    //
    // Class: SoapMethodInvocationSerializer
    //
    //******************************************************************************
    /// <summary>
    /// Implements serialization and deserialization of MethodInvocationRemoting.MethodInvocation objects to and from soap messages.
    /// </summary>
    public class SoapMethodInvocationSerializer : IMethodInvocationSerializer
    {
        private SoapFormatter internalSoapFormatter;
        private SerializerUtilities serializerUtilities;
        private const string voidReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnType>void</ReturnType>";

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="P:MethodInvocationRemoting.IMethodInvocationSerializer.VoidReturnValue"]/*'/>
        public string VoidReturnValue
        {
            get
            {
                return voidReturnValue;
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: SoapMethodInvocationSerializer (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.SoapMethodInvocationSerializer class.
        /// </summary>
        public SoapMethodInvocationSerializer()
        {
            internalSoapFormatter = new SoapFormatter();
            serializerUtilities = new SerializerUtilities(Encoding.UTF8);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationSerializer.Serialize(MethodInvocationRemoting.IMethodInvocation)"]/*'/>
        public string Serialize(IMethodInvocation inputMethodInvocation)
        {
            try
            {
                return SerializeObject(inputMethodInvocation);
            }
            catch (Exception e)
            {
                throw new SerializationException("Failed to serialize method invocation '" + inputMethodInvocation.Name + "'.", inputMethodInvocation, e);
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationSerializer.Deserialize(System.String)"]/*'/>
        public MethodInvocation Deserialize(string serializedMethodInvocation)
        {
            try
            {
                return (MethodInvocation)DeserializeObject(serializedMethodInvocation);
            }
            catch (Exception e)
            {
                throw new DeserializationException("Failed to deserialize method invocation.", serializedMethodInvocation, e);
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationSerializer.SerializeReturnValue(System.Object)"]/*'/>
        public string SerializeReturnValue(object inputReturnValue)
        {
            return SerializeObject(inputReturnValue);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationSerializer.DeserializeReturnValue(System.String)"]/*'/>
        public object DeserializeReturnValue(string serializedReturnValue)
        {
            return DeserializeObject(serializedReturnValue);
        }

        //------------------------------------------------------------------------------
        //
        // Method: SerializeObject
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Serializes the inputted object using the SoapFormatter.
        /// </summary>
        /// <param name="inputObject">The object to serialize.</param>
        /// <returns>The serialized object.</returns>
        private string SerializeObject(object inputObject)
        {
            // Objects to convert the Stream produced by the SoapFormatter into a string.
            string returnString;

            using (MemoryStream targetStream = new MemoryStream())
            {
                try
                {
                    internalSoapFormatter.Serialize(targetStream, inputObject);
                }
                catch (Exception e)
                {
                    throw new SerializationException("Failed to serialize object.", inputObject, e);
                }
                targetStream.Flush();
                returnString = serializerUtilities.ConvertMemoryStreamToString(targetStream);
                targetStream.Close();
            }
            return returnString;
        }

        //------------------------------------------------------------------------------
        //
        // Method: DeserializeObject
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Deserializes the inputted object using the SoapFormatter.
        /// </summary>
        /// <param name="inputString">The string to deserialize from.</param>
        /// <returns>The deserialized object.</returns>
        private object DeserializeObject(string inputString)
        {
            object returnObject;

            using (MemoryStream sourceStream = serializerUtilities.ConvertStringToMemoryStream(inputString))
            {
                try
                {
                    returnObject = internalSoapFormatter.Deserialize(sourceStream);
                }
                catch (Exception e)
                {
                    throw new DeserializationException("Failed to deserialize object.", inputString, e);
                }
                sourceStream.Close();
            }
            return returnObject;
        }
    }
}

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
using System.Xml;

namespace MethodInvocationRemoting
{
    /// <summary>
    /// Structure used to map a native .NET System.Type object to operations to serialize and deserialize objects of that type.
    /// </summary>
    public struct NativeTypeSerialisationOperationsMap
    {
        /// <summary>The native .NET type in the mapping.</summary>
        public Type nativeType;
        /// <summary>The method used to serialize objects of the type.</summary>
        public XmlSerializationOperation xmlSerializationOperation;
        /// <summary>The method used to deserialize objects of the type.</summary>
        public XmlDeserializationOperation xmlDeserializationOperation;

        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.NativeTypeSerialisationOperationsMap structure.
        /// </summary>
        /// <param name="nativeType">The native .NET type in the mapping.</param>
        /// <param name="xmlSerializationOperation">The method used to serialize objects of the type.</param>
        /// <param name="xmlDeserializationOperation">The method used to deserialize objects of the type.</param>
        public NativeTypeSerialisationOperationsMap(Type nativeType, XmlSerializationOperation xmlSerializationOperation, XmlDeserializationOperation xmlDeserializationOperation)
        {
            this.nativeType = nativeType;
            this.xmlSerializationOperation = xmlSerializationOperation;
            this.xmlDeserializationOperation = xmlDeserializationOperation;
        }
    }

    /// <summary>
    /// Structure used to map the string representation of a type to operations to serialize and deserialize objects of that type.
    /// </summary>
    public struct SerializedTypeSerialisationOperationsMap
    {
        /// <summary>The string representation of the type in the mapping.</summary>
        public string serializedType;
        /// <summary>The method used to serialize objects of the type.</summary> 
        public XmlSerializationOperation xmlSerializationOperation;
        /// <summary>The method used to deserialize objects of the type.</summary>
        public XmlDeserializationOperation xmlDeserializationOperation;

        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.SerializedTypeSerialisationOperationsMap structure.
        /// </summary>
        /// <param name="serializedType">The string representation of the type in the mapping.</param>
        /// <param name="xmlSerializationOperation">The method used to serialize objects of the type.</param>
        /// <param name="xmlDeserializationOperation">The method used to deserialize objects of the type.</param>
        public SerializedTypeSerialisationOperationsMap(string serializedType, XmlSerializationOperation xmlSerializationOperation, XmlDeserializationOperation xmlDeserializationOperation)
        {
            this.serializedType = serializedType;
            this.xmlSerializationOperation = xmlSerializationOperation;
            this.xmlDeserializationOperation = xmlDeserializationOperation;
        }
    }

    //******************************************************************************
    //
    // Class: SerializerOperationMap
    //
    //******************************************************************************
    /// <summary>
    /// Stores mappings between a native .NET System.Type object, the type serialized as a string, and the operations to serialize and deserialize objects of that type.
    /// </summary>
    public class SerializerOperationMap : ISerializerOperationMap
    {
        private Dictionary<string, NativeTypeSerialisationOperationsMap> serializedToNativeMap;
        private Dictionary<Type, SerializedTypeSerialisationOperationsMap> nativeToSerializedMap;

        //------------------------------------------------------------------------------
        //
        // Method: SerializerOperationMap (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.SerializerOperationMap class.
        /// </summary>
        public SerializerOperationMap()
        {
            serializedToNativeMap = new Dictionary<string, NativeTypeSerialisationOperationsMap>();
            nativeToSerializedMap = new Dictionary<Type, SerializedTypeSerialisationOperationsMap>();
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.ISerializerOperationMap.AddMapping(System.Type,System.String,MethodInvocationRemoting.XmlSerializationOperation,MethodInvocationRemoting.XmlDeserializationOperation)"]/*'/>
        public void AddMapping(Type nativeType, string serializedType, XmlSerializationOperation serializationOperation, XmlDeserializationOperation deserializationOperation)
        {
            // Check for null parameters
            CheckAddUpdateParameters(nativeType, serializedType, serializationOperation, deserializationOperation);

            // Check for attempt to add duplicate entry
            if (serializedToNativeMap.ContainsKey(serializedType) == true)
            {
                throw new ArgumentException("The serialized type '" + serializedType + "' already exists in the map.", "serializedType");
            }
            if (nativeToSerializedMap.ContainsKey(nativeType) == true)
            {
                throw new ArgumentException("The native type '" + nativeType.FullName + "' already exists in the map.", "nativeType");
            }

            // Add the mapping
            serializedToNativeMap.Add(serializedType, new NativeTypeSerialisationOperationsMap(nativeType, serializationOperation, deserializationOperation));
            nativeToSerializedMap.Add(nativeType, new SerializedTypeSerialisationOperationsMap(serializedType, serializationOperation, deserializationOperation));
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.ISerializerOperationMap.UpdateMapping(System.Type,System.String,MethodInvocationRemoting.XmlSerializationOperation,MethodInvocationRemoting.XmlDeserializationOperation)"]/*'/>
        public void UpdateMapping(Type nativeType, string serializedType, XmlSerializationOperation serializationOperation, XmlDeserializationOperation deserializationOperation)
        {
            // Check for null parameters
            CheckAddUpdateParameters(nativeType, serializedType, serializationOperation, deserializationOperation);

            // Check that the mapping already exists
            if (nativeToSerializedMap.ContainsKey(nativeType) == false)
            {
                throw new ArgumentException("The native type '" + nativeType.FullName + "' does not exist in the map.", "nativeType");
            }

            // Remove the existing mapping
            serializedToNativeMap.Remove(serializedType);
            nativeToSerializedMap.Remove(nativeType);

            // Add the new mapping
            AddMapping(nativeType, serializedType, serializationOperation, deserializationOperation);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.ISerializerOperationMap.GetSerializedType(System.Type)"]/*'/>
        public string GetSerializedType(Type nativeType)
        {
            if (nativeToSerializedMap.ContainsKey(nativeType) == true)
            {
                return nativeToSerializedMap[nativeType].serializedType;
            }
            else
            {
                return null;
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.ISerializerOperationMap.GetNativeType(System.String)"]/*'/>
        public Type GetNativeType(string serializedType)
        {
            if (serializedToNativeMap.ContainsKey(serializedType) == true)
            {
                return serializedToNativeMap[serializedType].nativeType;
            }
            else
            {
                return null;
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.ISerializerOperationMap.GetSerializationOperation(System.Type)"]/*'/>
        public XmlSerializationOperation GetSerializationOperation(Type nativeType)
        {
            if (nativeToSerializedMap.ContainsKey(nativeType) == true)
            {
                return nativeToSerializedMap[nativeType].xmlSerializationOperation;
            }
            else
            {
                return null;
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.ISerializerOperationMap.GetDeserializationOperation(System.String)"]/*'/>
        public XmlDeserializationOperation GetDeserializationOperation(string serializedType)
        {
            if (serializedToNativeMap.ContainsKey(serializedType) == true)
            {
                return serializedToNativeMap[serializedType].xmlDeserializationOperation;
            }
            else
            {
                return null;
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: CheckAddUpdateParameters
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Checks the inputted parameters, and throws an exception if any are null.
        /// </summary>
        /// <param name="nativeType">The native .NET type in the mapping.</param>
        /// <param name="serializedType">A string representation of the type.</param>
        /// <param name="serializationOperation">A method which serializes objects of the type.</param>
        /// <param name="deserializationOperation">A method which deserializes objects of the type.</param>
        private void CheckAddUpdateParameters(Type nativeType, string serializedType, XmlSerializationOperation serializationOperation, XmlDeserializationOperation deserializationOperation)
        {
            if (nativeType == null)
            {
                throw new ArgumentNullException("nativeType", "Parameter 'nativeType' cannot be null.");
            }
            if (serializedType == null)
            {
                throw new ArgumentNullException("serializedType", "Parameter 'serializedType' cannot be null.");
            }
            if (serializationOperation == null)
            {
                throw new ArgumentNullException("serializationOperation", "Parameter 'serializationOperation' cannot be null.");
            }
            if (deserializationOperation == null)
            {
                throw new ArgumentNullException("deserializationOperation", "Parameter 'deserializationOperation' cannot be null.");
            }
        }
    }
}

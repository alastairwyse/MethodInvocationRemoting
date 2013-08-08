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
    /// Defines an operation to serialize an object to XML using a System.Xml.XmlWriter.
    /// </summary>
    /// <remarks>This operation should write an XML representation of the object to the XmlWriter, similar to the IXmlSerializable.WriteXml method.</remarks>
    /// <param name="inputObject">The object to serialize.</param>
    /// <param name="writer">The XmlWriter to serialize to.</param>
    public delegate void XmlSerializationOperation(object inputObject, XmlWriter writer);

    /// <summary>
    /// Defines an operation to deserialize an object from XML contained in a System.Xml.XmlReader.
    /// </summary>
    /// <remarks>This operation should generate an object from its XML representation in the XmlReader, similar to the IXmlSerializable.ReadXml method.</remarks>
    /// <param name="reader">The XmlReader to deserialize from.</param>
    /// <returns>The deserialized object.</returns>
    public delegate object XmlDeserializationOperation(XmlReader reader);

    //******************************************************************************
    //
    // Interface: ISerializerOperationMap
    //
    //******************************************************************************
    /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="T:MethodInvocationRemoting.ISerializerOperationMap"]/*'/>
    public interface ISerializerOperationMap
    {
        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.ISerializerOperationMap.AddMapping(System.Type,System.String,MethodInvocationRemoting.XmlSerializationOperation,MethodInvocationRemoting.XmlDeserializationOperation)"]/*'/>
        void AddMapping(Type nativeType, string serializedType, XmlSerializationOperation serializationOperation, XmlDeserializationOperation deserializationOperation);

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.ISerializerOperationMap.UpdateMapping(System.Type,System.String,MethodInvocationRemoting.XmlSerializationOperation,MethodInvocationRemoting.XmlDeserializationOperation)"]/*'/>
        void UpdateMapping(Type nativeType, string serializedType, XmlSerializationOperation serializationOperation, XmlDeserializationOperation deserializationOperation);

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.ISerializerOperationMap.GetSerializedType(System.Type)"]/*'/>
        string GetSerializedType(Type nativeType);

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.ISerializerOperationMap.GetNativeType(System.String)"]/*'/>
        Type GetNativeType(string serializedType);

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.ISerializerOperationMap.GetSerializationOperation(System.Type)"]/*'/>
        XmlSerializationOperation GetSerializationOperation(Type nativeType);

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.ISerializerOperationMap.GetDeserializationOperation(System.String)"]/*'/>
        XmlDeserializationOperation GetDeserializationOperation(string serializedType);
    }
}

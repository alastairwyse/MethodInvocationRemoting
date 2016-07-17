/*
 * Copyright 2016 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
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
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;
using ApplicationLogging;
using ApplicationMetrics;
using MethodInvocationRemotingMetrics;

namespace MethodInvocationRemoting
{
    //******************************************************************************
    //
    // Class: MethodInvocationSerializer
    //
    //******************************************************************************
    /// <summary>
    /// Implements serialization and deserialization of MethodInvocationRemoting.IMethodInvocation objects.
    /// </summary>
    public class MethodInvocationSerializer : IMethodInvocationSerializer
    {
        // Constants used in XML document
        /// <summary>The name of the root element in the XML document written and read by the class.</summary>
        protected string rootElementName = "MethodInvocation";
        /// <summary>The name of the element storing the method name in the XML document written and read by the class.</summary>
        protected string methodNameElementName = "MethodName";
        /// <summary>The name of the element storing the method parameters in the XML document written and read by the class.</summary>
        protected string parametersElementName = "Parameters";
        /// <summary>The name of the element storing an individual method parameter in the XML document written and read by the class.</summary>
        protected string parameterElementName = "Parameter";
        /// <summary>The name of the element storing the data type of a method parameter, or return value in the XML document written and read by the class.</summary>
        protected string dataTypeElementName = "DataType";
        /// <summary>The name of the element storing a method parameter, or return value in the XML document written and read by the class.</summary>
        protected string dataElementName = "Data";
        /// <summary>The name of the element storing the method return type in the XML document written and read by the class.</summary>
        protected string returnTypeElementName = "ReturnType";
        /// <summary>The name of the element storing the method return value in the XML document written and read by the class.</summary>
        protected string returnValueElementName = "ReturnValue";
        /// <summary>The name of the element storing the data type of an array element in the XML document written and read by the class.</summary>
        protected string arrayElementDataTypeElementName = "ElementDataType";
        /// <summary>The name of the element storing the array element in the XML document written and read by the class.</summary>
        protected string arrayElementElementName = "Element";
        private const string voidReturnValueName = "void";
        private const string emptyIndicatorElementName = "Empty";

        /// <summary>The number of digits to write to the right of the decimal point when serializing System.Single objects.</summary>
        protected int singleFloatingPointDigits = 8;
        /// <summary>The number of digits to write to the right of the decimal point when serializing System.Double objects.</summary>
        protected int doubleFloatingPointDigits = 16;
        /// <summary>The System.Globalization.CultureInfo to use when serializing and deserializing.</summary>
        protected CultureInfo defaultCulture = CultureInfo.InvariantCulture;
        /// <summary>The serializer utilities object to use for converting strings to streams and vice versa.</summary>
        protected SerializerUtilities serializerUtilities;

        private ISerializerOperationMap operationMap;
        private IApplicationLogger logger;
        private LoggingUtilities loggingUtilities;
        private MetricsUtilities metricsUtilities;

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="P:MethodInvocationRemoting.IMethodInvocationSerializer.VoidReturnValue"]/*'/>
        public string VoidReturnValue
        {
            get
            {
                string returnString = "";

                using (MemoryStream outputStream = new MemoryStream())
                using (XmlWriter writer = XmlWriter.Create(outputStream))
                {
                    try
                    {
                        writer.WriteElementString(returnTypeElementName, voidReturnValueName);
                    }
                    catch (Exception e)
                    {
                        throw new SerializationException("Failed to serialize void return value.", e);
                    }

                    writer.Flush();
                    returnString = serializerUtilities.ConvertMemoryStreamToString(outputStream);
                    writer.Close();
                }

                return returnString;
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: MethodInvocationSerializer (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MethodInvocationSerializer class.
        /// </summary>
        /// <param name="operationMap">The serializer operation map to use to store mappings between object types and methods to serialize and deserialize the types.</param>
        public MethodInvocationSerializer(ISerializerOperationMap operationMap)
        {
            serializerUtilities = new SerializerUtilities(Encoding.UTF8);
            this.operationMap = operationMap;
            logger = new ConsoleApplicationLogger(LogLevel.Information, '|', "  ");
            loggingUtilities = new LoggingUtilities(logger);
            metricsUtilities = new MetricsUtilities(new NullMetricLogger());

            operationMap.AddMapping(typeof(Int32), "integer", new XmlSerializationOperation(SerializeInt32), new XmlDeserializationOperation(DeserializeInt32));
            operationMap.AddMapping(typeof(String), "string", new XmlSerializationOperation(SerializeString), new XmlDeserializationOperation(DeserializeString));
            operationMap.AddMapping(typeof(SByte), "signedByte", new XmlSerializationOperation(SerializeSByte), new XmlDeserializationOperation(DeserializeSByte));
            operationMap.AddMapping(typeof(Int16), "shortInteger", new XmlSerializationOperation(SerializeInt16), new XmlDeserializationOperation(DeserializeInt16));
            operationMap.AddMapping(typeof(Int64), "longInteger", new XmlSerializationOperation(SerializeInt64), new XmlDeserializationOperation(DeserializeInt64));
            operationMap.AddMapping(typeof(Single), "float", new XmlSerializationOperation(SerializeSingle), new XmlDeserializationOperation(DeserializeSingle));
            operationMap.AddMapping(typeof(Double), "double", new XmlSerializationOperation(SerializeDouble), new XmlDeserializationOperation(DeserializeDouble));
            operationMap.AddMapping(typeof(Char), "char", new XmlSerializationOperation(SerializeChar), new XmlDeserializationOperation(DeserializeChar));
            operationMap.AddMapping(typeof(Boolean), "bool", new XmlSerializationOperation(SerializeBoolean), new XmlDeserializationOperation(DeserializeBoolean));
            operationMap.AddMapping(typeof(Decimal), "decimal", new XmlSerializationOperation(SerializeDecimal), new XmlDeserializationOperation(DeserializeDecimal));
            operationMap.AddMapping(typeof(DateTime), "dateTime", new XmlSerializationOperation(SerializeDateTime), new XmlDeserializationOperation(DeserializeDateTime));
            operationMap.AddMapping(typeof(Int32[]), "integerArray", new XmlSerializationOperation(SerializeArray), new XmlDeserializationOperation(DeserializeArray));
            operationMap.AddMapping(typeof(String[]), "stringArray", new XmlSerializationOperation(SerializeArray), new XmlDeserializationOperation(DeserializeArray));
            operationMap.AddMapping(typeof(SByte[]), "signedByteArray", new XmlSerializationOperation(SerializeArray), new XmlDeserializationOperation(DeserializeArray));
            operationMap.AddMapping(typeof(Int16[]), "shortIntegerArray", new XmlSerializationOperation(SerializeArray), new XmlDeserializationOperation(DeserializeArray));
            operationMap.AddMapping(typeof(Int64[]), "longIntegerArray", new XmlSerializationOperation(SerializeArray), new XmlDeserializationOperation(DeserializeArray));
            operationMap.AddMapping(typeof(Single[]), "floatArray", new XmlSerializationOperation(SerializeArray), new XmlDeserializationOperation(DeserializeArray));
            operationMap.AddMapping(typeof(Double[]), "doubleArray", new XmlSerializationOperation(SerializeArray), new XmlDeserializationOperation(DeserializeArray));
            operationMap.AddMapping(typeof(Char[]), "charArray", new XmlSerializationOperation(SerializeArray), new XmlDeserializationOperation(DeserializeArray));
            operationMap.AddMapping(typeof(Boolean[]), "boolArray", new XmlSerializationOperation(SerializeArray), new XmlDeserializationOperation(DeserializeArray));
            operationMap.AddMapping(typeof(Decimal[]), "decimalArray", new XmlSerializationOperation(SerializeArray), new XmlDeserializationOperation(DeserializeArray));
            operationMap.AddMapping(typeof(DateTime[]), "dateTimeArray", new XmlSerializationOperation(SerializeArray), new XmlDeserializationOperation(DeserializeArray));
        }

        //------------------------------------------------------------------------------
        //
        // Method: MethodInvocationSerializer (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MethodInvocationSerializer class.
        /// </summary>
        /// <param name="operationMap">The serializer operation map to use to store mappings between object types and methods to serialize and deserialize the types.</param>
        /// <param name="logger">The logger to write log events to.</param>
        public MethodInvocationSerializer(ISerializerOperationMap operationMap, IApplicationLogger logger)
            : this(operationMap)
        {
            this.logger = logger;
            loggingUtilities = new LoggingUtilities(logger);
        }

        //------------------------------------------------------------------------------
        //
        // Method: MethodInvocationSerializer (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MethodInvocationSerializer class.
        /// </summary>
        /// <param name="operationMap">The serializer operation map to use to store mappings between object types and methods to serialize and deserialize the types.</param>
        /// <param name="metricLogger">The metric logger to write metric and instrumentation events to.</param>
        public MethodInvocationSerializer(ISerializerOperationMap operationMap, IMetricLogger metricLogger)
            : this(operationMap)
        {
            metricsUtilities = new MetricsUtilities(metricLogger);
        }

        //------------------------------------------------------------------------------
        //
        // Method: MethodInvocationSerializer (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MethodInvocationSerializer class.
        /// </summary>
        /// <param name="operationMap">The serializer operation map to use to store mappings between object types and methods to serialize and deserialize the types.</param>
        /// <param name="logger">The logger to write log events to.</param>
        /// <param name="metricLogger">The metric logger to write metric and instrumentation events to.</param>
        public MethodInvocationSerializer(ISerializerOperationMap operationMap, IApplicationLogger logger, IMetricLogger metricLogger)
            : this(operationMap)
        {
            this.logger = logger;
            loggingUtilities = new LoggingUtilities(logger);
            metricsUtilities = new MetricsUtilities(metricLogger);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationSerializer.Serialize(MethodInvocationRemoting.IMethodInvocation)"]/*'/>
        public string Serialize(IMethodInvocation inputMethodInvocation)
        {
            metricsUtilities.Begin(new MethodInvocationSerializeTime());

            string returnString = "";

            using (MemoryStream outputStream = new MemoryStream())
            using (XmlWriter writer = XmlWriter.Create(outputStream))
            {
                try
                {
                    // Write the root tag (e.g. <MethodInvocation>)
                    writer.WriteStartElement(rootElementName);

                    // Write the method name
                    SerializeMethodName(inputMethodInvocation.Name, writer);

                    // Write the parameters
                    SerializeParameters(inputMethodInvocation.Parameters, writer);

                    // Write the return type
                    SerializeReturnType(inputMethodInvocation.ReturnType, writer);

                    // Write the root end tag (e.g. </MethodInvocation>)
                    writer.WriteEndElement();
                }
                catch (Exception e)
                {
                    metricsUtilities.CancelBegin(new MethodInvocationSerializeTime());
                    throw new SerializationException("Failed to serialize invocation of method '" + inputMethodInvocation.Name + "'.", inputMethodInvocation, e);
                }

                writer.Flush();
                returnString = serializerUtilities.ConvertMemoryStreamToString(outputStream);
                writer.Close();
            }

            metricsUtilities.End(new MethodInvocationSerializeTime());
            metricsUtilities.Increment(new MethodInvocationSerialized());
            metricsUtilities.Add(new SerializedMethodInvocationSize(returnString.Length));
            loggingUtilities.LogSerializedItem(this, returnString, "method invocation");

            return returnString;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationSerializer.Deserialize(System.String)"]/*'/>
        public MethodInvocation Deserialize(string serializedMethodInvocation)
        {
            metricsUtilities.Begin(new MethodInvocationDeserializeTime());

            string methodName;
            ArrayList parameterArray;
            Type returnType = null;

            using (MemoryStream sourceStream = serializerUtilities.ConvertStringToMemoryStream(serializedMethodInvocation))
            using (XmlReader reader = XmlReader.Create(sourceStream))
            {
                try
                {
                    // Consume the root tag (e.g. <MethodInvocation>)
                    reader.ReadStartElement(rootElementName);

                    // Read the method name
                    methodName = DeserializeMethodName(reader);

                    // Read the parameters
                    parameterArray = DeserializeParameters(reader);

                    // Read the return type
                    returnType = DeserializeReturnType(reader);

                    // Consume the root end tag (e.g. </MethodInvocation>)
                    reader.ReadEndElement();
                }
                catch (Exception e)
                {
                    metricsUtilities.CancelBegin(new MethodInvocationDeserializeTime());
                    throw new DeserializationException("Failed to deserialize method invocation.", serializedMethodInvocation, e);
                }
                finally
                {
                    reader.Close();
                }
            }

            metricsUtilities.End(new MethodInvocationDeserializeTime());
            metricsUtilities.Increment(new MethodInvocationDeserialized());
            loggingUtilities.Log(this, LogLevel.Information, "Deserialized string to method invocation '" + methodName + "'.");

            return BuildMethodInvocation(methodName, parameterArray, returnType);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationSerializer.SerializeReturnValue(System.Object)"]/*'/>
        public string SerializeReturnValue(object inputReturnValue)
        {
            metricsUtilities.Begin(new ReturnValueSerializeTime());

            string returnString = "";

            using (MemoryStream outputStream = new MemoryStream())
            using (XmlWriter writer = XmlWriter.Create(outputStream))
            {
                try
                {
                    try
                    {
                        SerializeItem(inputReturnValue, returnValueElementName, writer);
                    }
                    catch (Exception e)
                    {
                        throw new SerializationException("Failed to serialize return value.", inputReturnValue, e);
                    }

                    writer.Flush();
                    returnString = serializerUtilities.ConvertMemoryStreamToString(outputStream);
                    writer.Close();
                }
                catch (Exception e)
                {
                    metricsUtilities.CancelBegin(new ReturnValueSerializeTime());
                    throw;
                }
            }

            metricsUtilities.End(new ReturnValueSerializeTime());
            metricsUtilities.Increment(new ReturnValueSerialized());
            metricsUtilities.Add(new SerializedReturnValueSize(returnString.Length));
            loggingUtilities.LogSerializedItem(this, returnString, "return value");

            return returnString;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationSerializer.DeserializeReturnValue(System.String)"]/*'/>
        public object DeserializeReturnValue(string serializedReturnValue)
        {
            metricsUtilities.Begin(new ReturnValueDeserializeTime());

            object returnValue = null;

            using (MemoryStream sourceStream = serializerUtilities.ConvertStringToMemoryStream(serializedReturnValue))
            using (XmlReader reader = XmlReader.Create(sourceStream))
            {
                try
                {
                    // Skip over the XML header
                    reader.MoveToContent();
                    returnValue = DeserializeItem(returnValueElementName, reader);
                }
                catch (Exception e)
                {
                    metricsUtilities.CancelBegin(new ReturnValueDeserializeTime());
                    throw new DeserializationException("Failed to deserialize return value.", serializedReturnValue, e);
                }
                finally
                {
                    reader.Close();
                }
            }

            metricsUtilities.End(new ReturnValueDeserializeTime());
            metricsUtilities.Increment(new ReturnValueDeserialized());
            loggingUtilities.LogDeserializedReturnValue(this, returnValue);

            return returnValue;
        }

        //------------------------------------------------------------------------------
        //
        // Method: AddIXmlSerializableOperations
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Adds the specified IXmlSerializable type and serialization of the type to the internal serialization operation map, and sets internal operations to serialize and deserialize objects of that type.
        /// </summary>
        /// <param name="nativeType">The type to add.  Objects of the type must implement interface System.Xml.Serialization.IXmlSerializable.</param>
        /// <param name="serializedType">A string representation of the type.</param>
        public void AddIXmlSerializableOperations(Type nativeType, string serializedType)
        {
            if (typeof(IXmlSerializable).IsAssignableFrom(nativeType) == false)
            {
                throw new ArgumentException("The native type '" + nativeType.FullName + "' does not implement interface " + typeof(IXmlSerializable).FullName + ".", "nativeType");
            }
            operationMap.AddMapping(nativeType, serializedType, new XmlSerializationOperation(SerializeIXmlSerializable), new XmlDeserializationOperation(DeserializeIXmlSerializable));
        }

        #region Private Methods

        //------------------------------------------------------------------------------
        //
        // Method: GetSerializedTypeFromMap
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Attempts to retrieve a serialized type corresponding to an inputted native type from the operation map, and throws an exception if the native type cannot be found.
        /// </summary>
        /// <param name="nativeType">The native type to search the operation map for.</param>
        /// <returns>The corresponding serialized type.</returns>
        private string GetSerializedTypeFromMap(Type nativeType)
        {
            string returnType = operationMap.GetSerializedType(nativeType);

            if (returnType == null)
            {
                throw new Exception("Native type '" + nativeType.FullName + "' does not exist in the operation map.");
            }

            return returnType;
        }

        //------------------------------------------------------------------------------
        //
        // Method: GetDeserializedTypeFromMap
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Attempts to retrieve a native type corresponding to an inputted serialized type from the operation map, and throws an exception if the serialized type cannot be found.
        /// </summary>
        /// <param name="serializedType">The serialized type to search the operation map for.</param>
        /// <returns>The corresponding native type.</returns>
        private Type GetDeserializedTypeFromMap(string serializedType)
        {
            Type returnType = operationMap.GetNativeType(serializedType);

            if (returnType == null)
            {
                throw new Exception("Serialized type '" + serializedType + "' does not exist in the operation map.");
            }

            return returnType;
        }

        //------------------------------------------------------------------------------
        //
        // Method: SerializeMethodName
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Serializes the method name to the inputted XmlWriter.
        /// </summary>
        /// <param name="methodName">The method name.</param>
        /// <param name="writer">An XmlWriter object to serialize to.</param>
        private void SerializeMethodName(string methodName, XmlWriter writer)
        {
            writer.WriteElementString(methodNameElementName, methodName);
        }

        //------------------------------------------------------------------------------
        //
        // Method: SerializeParameters
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Serializes the method invocation parameters to the inputted XmlWriter.
        /// </summary>
        /// <param name="parameters">The method invocation parameters.</param>
        /// <param name="writer">An XmlWriter object to serialize to.</param>
        private void SerializeParameters(object[] parameters, XmlWriter writer)
        {
            // Write parameters start tag (e.g. <Parameters>)
            writer.WriteStartElement(parametersElementName);
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i = i + 1)
                {
                    SerializeItem(parameters[i], parameterElementName, writer);
                    loggingUtilities.LogParameter(this, "Serialized", parameters[i]);
                }
            }
            // Write parameters end tag (e.g. </Parameters>)
            writer.WriteEndElement();
        }

        //------------------------------------------------------------------------------
        //
        // Method: SerializeItem
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Serializes an item to the inputted XmlWriter.  The item includes data type information as well as the data itself.  The method can be used to serialize parameters, return values, array elements, etc...
        /// </summary>
        /// <param name="item">The object to serialize as an item.</param>
        /// <param name="elementName">The name of the surrounding XML tag.</param>
        /// <param name="writer">An XmlWriter object to serialize to.</param>
        private void SerializeItem(object item, string elementName, XmlWriter writer)
        {
            // Write item start tag (e.g. <Parameter>, <ReturnValue>)
            writer.WriteStartElement(elementName);
            if (item != null)
            {
                string itemSerializedType = GetSerializedTypeFromMap(item.GetType());
                writer.WriteElementString(dataTypeElementName, itemSerializedType);
                SerializeObject(item, writer);
            }
            // Write item end tag (e.g. <Parameter>, <ReturnValue>)
            writer.WriteEndElement();
        }

        //------------------------------------------------------------------------------
        //
        // Method: SerializeObject
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Serializes an individual object to the inputted XmlWriter.
        /// </summary>
        /// <param name="inputObject">The object to serialize.</param>
        /// <param name="writer">An XmlWriter object to serialize to.</param>
        private void SerializeObject(object inputObject, XmlWriter writer)
        {
            XmlSerializationOperation serializeOperation = operationMap.GetSerializationOperation(inputObject.GetType());
            // Write data start tag (e.g. <Data>)
            writer.WriteStartElement(dataElementName);
            serializeOperation(inputObject, writer);
            // Write data end tag (e.g. </Data>)
            writer.WriteEndElement();
        }

        //------------------------------------------------------------------------------
        //
        // Method: SerializeReturnType
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Serializes the method return type to the inputted XmlWriter.
        /// </summary>
        /// <param name="returnType">The return type.</param>
        /// <param name="writer">An XmlWriter object to serialize to.</param>
        private void SerializeReturnType(Type returnType, XmlWriter writer)
        {
            // Write return type start tag (e.g. <ReturnType>)
            writer.WriteStartElement(returnTypeElementName);
            if (returnType != null)
            {
                writer.WriteElementString(dataTypeElementName, GetSerializedTypeFromMap(returnType));
            }
            // Write return type end tag (e.g. </ReturnType>)
            writer.WriteEndElement();
        }

        //------------------------------------------------------------------------------
        //
        // Method: DeserializeMethodName
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Deserializes the method name from the inputted XmlReader.
        /// </summary>
        /// <param name="reader">An XmlReader object to deserialize from.</param>
        /// <returns>The method name.</returns>
        private string DeserializeMethodName(XmlReader reader)
        {
            return reader.ReadElementString(methodNameElementName);
        }

        //------------------------------------------------------------------------------
        //
        // Method: DeserializeParameters
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Deserializes the method invocation parameters from the inputted XmlReader.
        /// </summary>
        /// <param name="reader">An XmlReader object to deserialize from.</param>
        /// <returns>The deserialized objects.</returns>
        private ArrayList DeserializeParameters(XmlReader reader)
        {
            ArrayList returnParameterArray = new ArrayList();

            if (reader.IsEmptyElement == true)
            {
                // Consume parameters self closing tag (e.g. <Parameters />)
                reader.ReadElementString(parametersElementName);
            }
            else
            {
                // Consume parameters start tag (e.g. <Parameters>)
                reader.ReadStartElement(parametersElementName);
                // If IsStartElement() returns true there are parameters to read.  If it returns false, the next tag is a parameters end tag (e.g. </Parameters>).
                if (IsStartElement(parameterElementName, reader) == true)
                {
                    int baseDepth = reader.Depth;
                    while (reader.Depth >= baseDepth)
                    {
                        object parameter = DeserializeItem(parameterElementName, reader);
                        returnParameterArray.Add(parameter);
                        loggingUtilities.LogParameter(this, "Deserialized", parameter);
                    }
                }
                // Consume parameters end tag (e.g. </Parameters>)
                reader.ReadEndElement();
            }

            return returnParameterArray;
        }

        //------------------------------------------------------------------------------
        //
        // Method: DeserializeItem
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Deserializes an item from the inputted XmlReader.  The item includes data type information as well as the data itself.  The method can be used to deserialize parameters, return values, array elements, etc...
        /// </summary>
        /// <param name="elementName">The name of the surrounding XML tag.</param>
        /// <param name="reader">An XmlReader object to deserialize from.</param>
        /// <returns>The deserialized object.</returns>
        private object DeserializeItem(string elementName, XmlReader reader)
        {
            string datatype;
            object data = null;

            if (reader.IsEmptyElement == true)
            {
                // Consume parameter self closing tag (e.g. <Parameter />, <ReturnValue />).  Item is null.
                reader.ReadElementString(elementName);
            }
            else
            {
                // Consume parameter start tag (e.g. <Parameter>, <ReturnValue>)
                reader.ReadStartElement(elementName);
                // If IsStartElement() returns true the item is non-null.  If it returns false, the next tag is an end tag (e.g. </Parameter>, </ReturnValue>), and the item is null.
                if (IsStartElement(dataTypeElementName, reader) == true)
                {
                    datatype = reader.ReadElementString(dataTypeElementName);
                    data = DeserializeObject(datatype, reader);
                }
                // Consume parameter end tag (e.g. </Parameter>, </ReturnValue>)
                reader.ReadEndElement();
            }

            return data;
        }

        //------------------------------------------------------------------------------
        //
        // Method: DeserializeObject
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Deserializes an individual object (parameter or return value) from the inputted XmlReader.
        /// </summary>
        /// <param name="dataType">The data type of the object.</param>
        /// <param name="reader">An XmlReader object to deserialize the object from.</param>
        /// <returns>The object.</returns>
        private object DeserializeObject(string dataType, XmlReader reader)
        {
            object returnObject;

            Type objectType = GetDeserializedTypeFromMap(dataType);
            XmlDeserializationOperation deserializeOperation = operationMap.GetDeserializationOperation(dataType);
            // Consume data start tag (e.g. <Data>)
            reader.ReadStartElement(dataElementName);
            returnObject = deserializeOperation(reader);
            // Consume data end tag (e.g. </Data>)
            reader.ReadEndElement();

            return returnObject;
        }

        //------------------------------------------------------------------------------
        //
        // Method: DeserializeReturnType
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Deserializes the method return type from the inputted XmlReader.
        /// </summary>
        /// <param name="reader">An XmlReader object to deserialize from.</param>
        /// <returns>The return type.</returns>
        private Type DeserializeReturnType(XmlReader reader)
        {
            Type returnType = null;

            if (reader.IsEmptyElement == true)
            {
                // Consume return type self closing tag (e.g. <ReturnType />)
                reader.ReadElementString(returnTypeElementName);
            }
            else
            {
                // Consume return type start tag (e.g. <ReturnType>)
                reader.ReadStartElement(returnTypeElementName);
                // If IsStartElement() returns true there is a return type to read.  If it returns false, the next tag is a return type end tag (e.g. </ReturnType>).
                if (IsStartElement(dataTypeElementName, reader) == true)
                {
                    string serializedReturnType = reader.ReadElementString(dataTypeElementName);
                    returnType = GetDeserializedTypeFromMap(serializedReturnType);
                }
                // Consume return type end tag (e.g. </ReturnType>)
                reader.ReadEndElement();
            }

            return returnType;
        }

        //------------------------------------------------------------------------------
        //
        // Method: IsStartElement
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Returns true if the current node in the inputted XmlReader is a start element and the name of the element matches the name parameter.  Returns false if the node is an end element.
        /// </summary>
        /// <param name="name">The qualified name of the element.</param>
        /// <param name="reader">The XmlReader to read from.</param>
        /// <returns>Indicates whether the current node is a start element (true) or an end element (false).</returns>
        protected bool IsStartElement(String name, XmlReader reader)
        {
            bool returnValue;

            if (reader.NodeType == XmlNodeType.EndElement)
            {
                returnValue = false;
            }
            else if (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name != name)
                {
                    throw new Exception("Element '" + name + "' was not found.");
                }
                else
                {
                    returnValue = true;
                }
            }
            else
            {
                throw new Exception("Encountered node type '" + reader.NodeType.ToString() + "' when expecting either a start or an end element.");
            }

            return returnValue;
        }

        //------------------------------------------------------------------------------
        //
        // Method: BuildMethodInvocation
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Builds a method invocation object from the inputted name, parameters, and return type.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameterList">The parameters of the method invocation.</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <returns>The method invocation.</returns>
        private MethodInvocation BuildMethodInvocation(string name, ArrayList parameterList, Type returnType)
        {
            MethodInvocation returnMethodInvocation;

            try
            {
                if (parameterList.Count == 0)
                {
                    if (returnType == null)
                    {
                        returnMethodInvocation = new MethodInvocation(name);
                    }
                    else
                    {
                        returnMethodInvocation = new MethodInvocation(name, returnType);
                    }
                }
                else
                {
                    object[] parameters = new object[parameterList.Count];
                    parameterList.CopyTo(parameters);

                    if (returnType == null)
                    {
                        returnMethodInvocation = new MethodInvocation(name, parameters);
                    }
                    else
                    {
                        returnMethodInvocation = new MethodInvocation(name, parameters, returnType);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to build method invocation object.", e);
            }

            return returnMethodInvocation;
        }

        # endregion

        # region Serialization Operation Methods

        private void SerializeInt32(object inputObject, XmlWriter writer)
        {
            Int32 inputInt32 = (Int32)inputObject;
            writer.WriteString(inputInt32.ToString(defaultCulture));
        }

        private object DeserializeInt32(XmlReader reader)
        {
            return Convert.ToInt32(reader.ReadString(), defaultCulture);
        }

        private void SerializeString(object inputObject, XmlWriter writer)
        {
            if ((string)inputObject == "")
            {
                writer.WriteElementString(emptyIndicatorElementName, "");
            }
            else
            {
                writer.WriteString(inputObject.ToString());
            }
        }

        private object DeserializeString(XmlReader reader)
        {
            string returnValue;

            if (reader.IsEmptyElement == true)
            {
                // Consume empty indicator self closing tag (e.g. <Empty />).  String is empty.
                reader.ReadElementString(emptyIndicatorElementName);
                returnValue = "";
            }
            else if (reader.NodeType == XmlNodeType.Element)
            {
                // Consume empty indicator start and end tags (e.g. <Empty></Empty>).  String is empty.
                reader.ReadStartElement(emptyIndicatorElementName);
                reader.ReadEndElement();
                returnValue = "";
            }
            else
            {
                returnValue = reader.ReadString();
            }

            return returnValue;
        }

        private void SerializeSByte(object inputObject, XmlWriter writer)
        {
            SByte inputSByte = (SByte)inputObject;
            writer.WriteString(inputSByte.ToString(defaultCulture));
        }

        private object DeserializeSByte(XmlReader reader)
        {
            return Convert.ToSByte(reader.ReadString(), defaultCulture);
        }

        private void SerializeInt16(object inputObject, XmlWriter writer)
        {
            Int16 inputInt16 = (Int16)inputObject;
            writer.WriteString(inputInt16.ToString(defaultCulture));
        }

        private object DeserializeInt16(XmlReader reader)
        {
            return Convert.ToInt16(reader.ReadString(), defaultCulture);
        }

        private void SerializeInt64(object inputObject, XmlWriter writer)
        {
            Int64 inputInt64 = (Int64)inputObject;
            writer.WriteString(inputInt64.ToString(defaultCulture));
        }

        private object DeserializeInt64(XmlReader reader)
        {
            return Convert.ToInt64(reader.ReadString(), defaultCulture);
        }

        private void SerializeSingle(object inputObject, XmlWriter writer)
        {
            Single inputSingle = (Single)inputObject;
            writer.WriteString(inputSingle.ToString("e" + singleFloatingPointDigits, defaultCulture));
        }

        private object DeserializeSingle(XmlReader reader)
        {
            return Convert.ToSingle(reader.ReadString(), defaultCulture);
        }

        private void SerializeDouble(object inputObject, XmlWriter writer)
        {
            Double inputDouble = (Double)inputObject;
            writer.WriteString(inputDouble.ToString("e" + doubleFloatingPointDigits, defaultCulture));
        }

        private object DeserializeDouble(XmlReader reader)
        {
            return Convert.ToDouble(reader.ReadString(), defaultCulture);
        }

        private void SerializeChar(object inputObject, XmlWriter writer)
        {
            Char inputChar = (Char)inputObject;
            writer.WriteString(inputChar.ToString(defaultCulture));
        }

        private object DeserializeChar(XmlReader reader)
        {
            return Convert.ToChar(reader.ReadString(), defaultCulture);
        }

        private void SerializeBoolean(object inputObject, XmlWriter writer)
        {
            Boolean inputBoolean = (Boolean)inputObject;
            writer.WriteString(inputBoolean.ToString().ToLower(defaultCulture));
        }

        private object DeserializeBoolean(XmlReader reader)
        {
            return Convert.ToBoolean(reader.ReadString(), defaultCulture);
        }

        private void SerializeDecimal(object inputObject, XmlWriter writer)
        {
            Decimal inputDecimal = (Decimal)inputObject;
            writer.WriteString(inputDecimal.ToString(defaultCulture));
        }

        private object DeserializeDecimal(XmlReader reader)
        {
            return Convert.ToDecimal(reader.ReadString(), defaultCulture);
        }

        private void SerializeDateTime(object inputObject, XmlWriter writer)
        {
            DateTime inputDateTime = (DateTime)inputObject;
            string serializedDateTime = inputDateTime.ToString("yyyy-MM-ddTHH:mm:ss.FFF", defaultCulture);
            // If milliseconds are 0 the above format mask will not write them to the string, hence need to explicitly add
            if (inputDateTime.Millisecond == 0)
            {
                serializedDateTime = serializedDateTime + ".000";
            }
            writer.WriteString(serializedDateTime);
        }

        private object DeserializeDateTime(XmlReader reader)
        {
            DateTime returnDateTime = DateTime.ParseExact(reader.ReadString(), "yyyy-MM-ddTHH:mm:ss.FFF", defaultCulture);
            return returnDateTime;
        }

        /// <summary>
        /// Serializes an array of objects to the inputted XmlWriter.
        /// </summary>
        /// <param name="inputObject">The array to serialize.</param>
        /// <param name="writer">An XmlWriter object to serialize to.</param>
        protected void SerializeArray(object inputObject, XmlWriter writer)
        {
            ICollection inputCollection;

            try
            {
                inputCollection = (ICollection)inputObject;
            }
            catch (Exception e)
            {
                throw new Exception("Array parameter could not be cast to type ICollection.", e);
            }
            string elementSerializedType = GetSerializedTypeFromMap(inputObject.GetType().GetElementType());
            writer.WriteElementString(arrayElementDataTypeElementName, elementSerializedType);
            if (inputCollection.Count > 0)
            {
                IEnumerator enumerator = inputCollection.GetEnumerator();
                while (enumerator.MoveNext() == true)
                {
                    SerializeItem(enumerator.Current, arrayElementElementName, writer);
                }
            }
        }

        /// <summary>
        /// Deserializes as array of objects from the inputted XmlReader.
        /// </summary>
        /// <param name="reader">An XmlReader object to deserialize from.</param>
        /// <returns>The deserialized array.</returns>
        protected object DeserializeArray(XmlReader reader)
        {
            object returnArray;
            ArrayList returnArrayList = new ArrayList();

            int baseDepth = reader.Depth;
            // Get the data type of the array elements
            string elementSerializedType = reader.ReadElementString(arrayElementDataTypeElementName);
            Type elementType = GetDeserializedTypeFromMap(elementSerializedType);
            // Read and deserialize all remaining items at this depth of the XML document
            while (reader.Depth >= baseDepth)
            {
                object elementObject = DeserializeItem(arrayElementElementName, reader);
                returnArrayList.Add(elementObject);
            }
            // Convert the ArrayList to an array
            returnArray = Array.CreateInstance(elementType, returnArrayList.Count);
            returnArrayList.CopyTo((System.Array)returnArray);

            return returnArray;
        }

        private void SerializeIXmlSerializable(object inputObject, XmlWriter writer)
        {
            string serializedType = operationMap.GetSerializedType(inputObject.GetType());
            writer.WriteElementString(dataTypeElementName, serializedType);
            // This method utilizes the IXmlSerializable.WriteXml method.  However for the corresponding IXmlSerializable.ReadXml method to work correctly, the contents written by WriteXml must be wrapped by XML element tags which indicate the type of the object.
            writer.WriteStartElement(serializedType);
            // Method AddIXmlSerializableOperations would have already checked that the type of inputObject implemented IXmlSerializable, so error handling is not required on the following cast
            IXmlSerializable serializableObject = (IXmlSerializable)inputObject;
            serializableObject.WriteXml(writer);
            writer.WriteEndElement();
        }

        private object DeserializeIXmlSerializable(XmlReader reader)
        {
            // Get the data type of the object
            string datatype = reader.ReadElementString(dataTypeElementName);
            Type objectType = GetDeserializedTypeFromMap(datatype);
            // Create the object
            IXmlSerializable returnObject = (IXmlSerializable)Activator.CreateInstance(objectType);
            returnObject.ReadXml(reader);

            return returnObject;
        }

        /* Support for generics will be enabled when supported on the Java side
        private void SerializeList(object inputObject, XmlWriter writer)
        {
            ICollection inputCollection = (ICollection)inputObject;

            string elementSerializedType = GetSerializedTypeFromMap(inputObject.GetType().GetGenericArguments()[0]);
            writer.WriteElementString(arrayElementDataTypeElementName, elementSerializedType);
            if (inputCollection.Count > 0)
            {
                IEnumerator enumerator = inputCollection.GetEnumerator();
                while (enumerator.MoveNext() == true)
                {
                    SerializeItem(enumerator.Current, arrayElementElementName, writer);
                }
            }
        }

        private object DeserializeList(XmlReader reader)
        {
            int baseDepth = reader.Depth;
            // Get the data type of the list elements
            string elementSerializedType = reader.ReadElementString(arrayElementDataTypeElementName);
            Type elementType = GetDeserializedTypeFromMap(elementSerializedType);
            // Create the list to return
            Type listType = typeof(List<>).MakeGenericType(elementType);
            IList returnList = (IList)Activator.CreateInstance(listType);
            // Read and deserialize all remaining items at this depth of the XML document
            while (reader.Depth >= baseDepth)
            {
                object elementObject = DeserializeItem(arrayElementElementName, reader);
                returnList.Add(elementObject);
            }

            return returnList;
        }
        */
        # endregion
    }
}

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

package net.alastairwyse.methodinvocationremoting;

import java.io.*;
import java.util.*;
import java.text.*;
import java.math.*;
import java.lang.reflect.*;
import javax.xml.stream.*;

/**
 * Implements serialization and deserialization of IMethodInvocation objects.
 * @author Alastair Wyse
 */
public class MethodInvocationSerializer implements IMethodInvocationSerializer {

    // Constants used in XML document
    protected String rootElementName = "MethodInvocation";
    protected String methodNameElementName = "MethodName";
    protected String parametersElementName = "Parameters";
    protected String parameterElementName = "Parameter";
    protected String dataTypeElementName = "DataType";
    protected String dataElementName = "Data";
    protected String returnTypeElementName = "ReturnType";
    protected String returnValueElementName = "ReturnValue";
    protected String arrayElementElementName = "Element";
    protected String arrayElementDataTypeElementName = "ElementDataType";
    private final String voidReturnValueName = "void";
    private final String emptyIndicatorElementName = "Empty";
    private final String xmlVersion = "1.0";
    private final String xmlEncoding = "utf-8";

    protected Integer singleFloatingPointDigits = 8;
    protected Integer doubleFloatingPointDigits = 16;
    protected Locale defaultLocale = Locale.US;
    
    protected ArraySerializer genericArraySerializer;
    private ISerializerOperationMap operationMap;
    
    /**
     * Initialises a new instance of the MethodInvocationSerializer class.
     * @param operationMap  The serializer operation map to use for serializing and deserializing.
     */
    public MethodInvocationSerializer(ISerializerOperationMap operationMap) {
        this.operationMap = operationMap;
        
        genericArraySerializer = new ArraySerializer();
        
        operationMap.AddMapping(Integer.class, "integer", new IntegerSerializer());
        operationMap.AddMapping(String.class, "string", new StringSerializer());
        operationMap.AddMapping(Byte.class, "signedByte", new ByteSerializer());
        operationMap.AddMapping(Short.class, "shortInteger", new ShortSerializer());
        operationMap.AddMapping(Long.class, "longInteger", new LongSerializer());
        operationMap.AddMapping(Float.class, "float", new FloatSerializer());
        operationMap.AddMapping(Double.class, "double", new DoubleSerializer());
        operationMap.AddMapping(Character.class, "char", new CharacterSerializer());
        operationMap.AddMapping(Boolean.class, "bool", new BooleanSerializer());
        operationMap.AddMapping(BigDecimal.class, "decimal", new BigDecimalSerializer());
        operationMap.AddMapping(GregorianCalendar.class, "dateTime", new GregorianCalendarSerializer());
        operationMap.AddMapping(Integer[].class, "integerArray", genericArraySerializer);
        operationMap.AddMapping(String[].class, "stringArray", genericArraySerializer);
        operationMap.AddMapping(Byte[].class, "signedByteArray", genericArraySerializer);
        operationMap.AddMapping(Short[].class, "shortIntegerArray", genericArraySerializer);
        operationMap.AddMapping(Long[].class, "longIntegerArray", genericArraySerializer);
        operationMap.AddMapping(Float[].class, "floatArray", genericArraySerializer);
        operationMap.AddMapping(Double[].class, "doubleArray", genericArraySerializer);
        operationMap.AddMapping(Character[].class, "charArray", genericArraySerializer);
        operationMap.AddMapping(Boolean[].class, "boolArray", genericArraySerializer);
        operationMap.AddMapping(BigDecimal[].class, "decimalArray", genericArraySerializer);
        operationMap.AddMapping(GregorianCalendar[].class, "dateTimeArray", genericArraySerializer);
    }
    
    @Override
    public String getVoidReturnValue() throws SerializationException {
        String returnString;
        ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
        
        try {
            XMLOutputFactory xmlFactory = XMLOutputFactory.newInstance();
            XMLStreamWriter writer = xmlFactory.createXMLStreamWriter(outputStream, xmlEncoding);
            
            // Write document start tag (e.g. <?xml version="1.0" encoding="utf-8"?>)
            writer.writeStartDocument(xmlEncoding, xmlVersion);
            WriteElementString(writer, returnTypeElementName, voidReturnValueName);
            writer.writeEndDocument();
            
            outputStream.flush();
            returnString = outputStream.toString();
            writer.close();
        }
        catch (Exception e) {
            throw new SerializationException("Failed to serialize void return value.", e);
        }

        return returnString;
    }

    @Override
    public String Serialize(IMethodInvocation inputMethodInvocation) throws SerializationException {
        String returnString;
        ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
        
        try {
            XMLOutputFactory xmlFactory = XMLOutputFactory.newInstance();
            XMLStreamWriter writer = xmlFactory.createXMLStreamWriter(outputStream, xmlEncoding);
            
            // Write document start tag (e.g. <?xml version="1.0" encoding="utf-8"?>)
            writer.writeStartDocument(xmlEncoding, xmlVersion);
            
            // Write the root tag (e.g. <MethodInvocation>)
            writer.writeStartElement(rootElementName);
            
            // Write the method name
            SerializeMethodName(inputMethodInvocation.getName(), writer);
            
            // Write the parameters
            SerializeParameters(inputMethodInvocation.getParameters(), writer);
            
            // Write the return type
            SerializeReturnType(inputMethodInvocation.getReturnType(), writer);
            
            // Write the root end tag (e.g. </MethodInvocation>)
            writer.writeEndElement();
            writer.writeEndDocument();
            
            outputStream.flush();
            returnString = outputStream.toString();
            writer.close();
        }
        catch (Exception e) {
            throw new SerializationException("Failed to serialize invocation of method '" + inputMethodInvocation.getName() + "'.", inputMethodInvocation, e);
        }

        return returnString;
    }

    @Override
    public MethodInvocation Deserialize(String serializedMethodInvocation) throws DeserializationException {
        String methodName;
        ArrayList parameterArray;
        Class<?> returnType = null;
        MethodInvocation returnMethodInvocation;
        ByteArrayInputStream sourceStream = new ByteArrayInputStream(serializedMethodInvocation.getBytes());
        XMLInputFactory xmlFactory = XMLInputFactory.newInstance();
        // Set coalescing property so that text/character elements are are returned in a contiguous block
        xmlFactory.setProperty("javax.xml.stream.isCoalescing", true);
        try {
            XMLStreamReader reader = xmlFactory.createXMLStreamReader(sourceStream);
            SimplifiedXMLStreamReader simpleReader = new SimplifiedXMLStreamReader(reader);
            
            // Consume the root tag (e.g. <MethodInvocation>)
            simpleReader.ReadStartElement(rootElementName);
            
            // Read the method name
            methodName = DeserializeMethodName(simpleReader);
            
            // Read the parameters
            parameterArray = DeserializeParameters(simpleReader);
            
            // Read the return type
            returnType = DeserializeReturnType(simpleReader);

            // Consume the root end tag (e.g. </MethodInvocation>)
            simpleReader.ReadEndElement();
            
            reader.close();
            returnMethodInvocation = BuildMethodInvocation(methodName, parameterArray, returnType);
        }
        catch (Exception e) {
            throw new DeserializationException("Failed to deserialize method invocation.", serializedMethodInvocation, e);
        }
        
        return returnMethodInvocation;
    }

    @Override
    public String SerializeReturnValue(Object inputReturnValue) throws SerializationException {
        String returnString;
        ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
        
        try {
            XMLOutputFactory xmlFactory = XMLOutputFactory.newInstance();
            XMLStreamWriter writer = xmlFactory.createXMLStreamWriter(outputStream, xmlEncoding);
            
            // Write document start tag (e.g. <?xml version="1.0" encoding="utf-8"?>)
            writer.writeStartDocument(xmlEncoding, xmlVersion);
            SerializeItem(inputReturnValue, returnValueElementName, writer);
            writer.writeEndDocument();
            
            outputStream.flush();
            returnString = outputStream.toString();
            writer.close();
        }
        catch (Exception e) {
            throw new SerializationException("Failed to serialize return value.", inputReturnValue, e);
        }

        return returnString;
    }

    @Override
    public Object DeserializeReturnValue(String serializedReturnValue) throws DeserializationException {
        Object returnValue = null;
        
        ByteArrayInputStream sourceStream = new ByteArrayInputStream(serializedReturnValue.getBytes());
        XMLInputFactory xmlFactory = XMLInputFactory.newInstance();
        // Set coalescing property so that text/character elements are are returned in a contiguous block
        xmlFactory.setProperty("javax.xml.stream.isCoalescing", true);
        try {
            XMLStreamReader reader = xmlFactory.createXMLStreamReader(sourceStream);
            SimplifiedXMLStreamReader simpleReader = new SimplifiedXMLStreamReader(reader);

            // Consume the root tag (e.g. <ReturnValue>)
            simpleReader.ReadStartElement(returnValueElementName);
            returnValue = DeserializeItem(simpleReader);
            
            reader.close();
        }
        catch (Exception e) {
            throw new DeserializationException("Failed to deserialize return value.", serializedReturnValue, e);
        }
        
        return returnValue;
    }

    /**
     * Attempts to retrieve a serialized type corresponding to an inputted native type from the operation map, and throws an exception if the native type cannot be found.
     * @param nativeType  The native type to search the operation map for.
     * @return            The corresponding serialized type.
     * @throws Exception
     */
    private String GetSerializedTypeFromMap(Class<?> nativeType) throws Exception {
        String returnType = operationMap.GetSerializedType(nativeType);

        if (returnType == null)
        {
            throw new Exception("Native type '" + nativeType.getName() + "' does not exist in the operation map.");
        }

        return returnType;
    }

    /**
     * Attempts to retrieve a native type corresponding to an inputted serialized type from the operation map, and throws an exception if the serialized type cannot be found.
     * @param serializedType  The serialized type to search the operation map for.
     * @return                The corresponding native type.
     * @throws Exception
     */
    private Class<?> GetDeserializedTypeFromMap(String serializedType) throws Exception {
        Class<?> returnType = operationMap.GetNativeType(serializedType);

        if (returnType == null)
        {
            throw new Exception("Serialized type '" + serializedType + "' does not exist in the operation map.");
        }

        return returnType;
    }

    /**
     * Serializes the method name to the inputted XMLStreamWriter.
     * @param methodName           The method name.
     * @param writer               An XMLStreamWriter object to serialize to.
     * @throws XMLStreamException
     */
    private void SerializeMethodName(String methodName, XMLStreamWriter writer) throws XMLStreamException {
        WriteElementString(writer, methodNameElementName, methodName);
    }

    /**
     * Serializes the method invocation parameters to the inputted XMLStreamWriter.
     * @param parameters           The method invocation parameters.
     * @param writer               An XMLStreamWriter object to serialize to.
     * @throws XMLStreamException
     * @throws Exception
     */
    private void SerializeParameters(Object[] parameters, XMLStreamWriter writer) throws XMLStreamException, Exception {
        // Write parameters start tag (e.g. <Parameters>)
        writer.writeStartElement(parametersElementName);
        if (parameters != null)
        {
            for (int i = 0; i < parameters.length; i = i + 1)
            {
                SerializeItem(parameters[i], parameterElementName, writer);
            }
        }
        // Write parameters end tag (e.g. </Parameters>)
        writer.writeEndElement();
    }
    
    /**
     * Serializes an item to the inputted XmlWriter.  The item includes data type information as well as the data itself.  The method can be used to serialize parameters, return values, array elements, etc...
     * @param item                 The object to serialize as an item.
     * @param elementName          The name of the surrounding XML tag.
     * @param writer               An XMLStreamWriter object to serialize to.
     * @throws XMLStreamException
     * @throws Exception
     */
    private void SerializeItem(Object item, String elementName, XMLStreamWriter writer) throws XMLStreamException, Exception {
        // Write item start tag (e.g. <Parameter>, <ReturnValue>)
        writer.writeStartElement(elementName);
        if (item != null) {
            String itemSerializedType = GetSerializedTypeFromMap(item.getClass());
            WriteElementString(writer, dataTypeElementName, itemSerializedType);
            SerializeObject(item, writer);
        }
        // Write item end tag (e.g. <Parameter>, <ReturnValue>)
        writer.writeEndElement();
    }
    
    /**
     * Serializes an individual object (parameter or return value) to the inputted XMLStreamWriter.
     * @param inputObject         The object to serialize.
     * @param writer              An XMLStreamWriter object to serialize to.
     * @throws XMLStreamException
     * @throws Exception
     */
    private void SerializeObject(Object inputObject, XMLStreamWriter writer) throws XMLStreamException, Exception {
        IObjectSerializer serializer = operationMap.GetSerializer(inputObject.getClass());
        // Write data start tag (e.g. <Data>)
        writer.writeStartElement(dataElementName);
        serializer.Serialize(inputObject, writer);
        // Write data end tag (e.g. </Data>)
        writer.writeEndElement();
    }

    /**
     * Serializes the method return type to the inputted XMLStreamWriter.
     * @param returnType           The return type.
     * @param writer               An XMLStreamWriter object to serialize to.
     * @throws XMLStreamException
     * @throws Exception
     */
    private void SerializeReturnType(Class<?> returnType, XMLStreamWriter writer) throws XMLStreamException, Exception {
        // Write return type start tag (e.g. <ReturnType>)
        writer.writeStartElement(returnTypeElementName);
        if (returnType != null)
        {
            WriteElementString(writer, dataTypeElementName, GetSerializedTypeFromMap(returnType));
        }
        // Write return type end tag (e.g. </ReturnType>)
        writer.writeEndElement();
    }

    /**
     * Deserializes the method name from the inputted SimplifiedXMLStreamReader.
     * @param reader               An SimplifiedXMLStreamReader object to deserialize from.
     * @return                     The method name.
     * @throws XMLStreamException
     */
    private String DeserializeMethodName(SimplifiedXMLStreamReader reader) throws XMLStreamException {
        return reader.ReadElementString(methodNameElementName);
    }

    /**
     * Deserializes the method invocation parameters from the inputted SimplifiedXMLStreamReader.
     * @param reader               A SimplifiedXMLStreamReader object to deserialize from.
     * @return                     The deserialized objects.
     * @throws XMLStreamException
     * @throws Exception
     */
    private ArrayList DeserializeParameters(SimplifiedXMLStreamReader reader) throws XMLStreamException, Exception {
        ArrayList returnParameterArray = new ArrayList();

        // Consume parameters start tag (e.g. <Parameters>)
        reader.ReadStartElement(parametersElementName);
        // Attempt to consume parameter start tag (e.g. <Parameter>)
        //   If IsNextNodeStartElement() call returns false then the closing </Parameters> tag is consumed
        if(reader.IsNextNodeStartElement(parameterElementName) == true) {
            // At this point the <Parameter> tag would have been already read, so need to set base depth 1 level lower than this at <Parameters>
            int baseDepth = (reader.getDepth() - 1);
            while(reader.getDepth() >= baseDepth) {
                Object parameter = DeserializeItem(reader);
                returnParameterArray.add(parameter);
                // Consume parameter start tag (e.g. <Parameter>)
                //   If IsNextNodeStartElement() call returns false then the closing </Parameters> tag is consumed, baseDepth becomes lower than depth, and the loop will end
                reader.IsNextNodeStartElement(parameterElementName);
            }
        }
        
        return returnParameterArray;
    }

    /**
     * Deserializes an item from the inputted XmlReader.  The item includes data type information as well as the data itself.  The method can be used to deserialize parameters, return values, array elements, etc...
     * @param reader               A SimplifiedXMLStreamReader object to deserialize from.
     * @return                     The deserialized object.
     * @throws XMLStreamException
     * @throws Exception
     */
    private Object DeserializeItem(SimplifiedXMLStreamReader reader) throws XMLStreamException, Exception {
        String datatype;
        Object data = null;

        if(reader.IsNextNodeStartElement(dataTypeElementName) == true) {
            datatype = reader.ReadString();
            // Consume data type end tag (e.g. </DataType>)
            reader.ReadEndElement();
            data = DeserializeObject(datatype, reader);
            // Consume parameter end tag (e.g. </Parameter>, <ReturnValue>)
            reader.ReadEndElement();
        }
        
        return data;
    }
    
    /**
     * Deserializes an individual object (parameter or return value) from the inputted SimplifiedXMLStreamReader.
     * @param dataType             The data type of the object.
     * @param reader               A SimplifiedXMLStreamReader object to deserialize the object from.
     * @return                     The object.
     * @throws XMLStreamException
     * @throws Exception
     */
    private Object DeserializeObject(String dataType, SimplifiedXMLStreamReader reader) throws XMLStreamException, Exception {
        Object returnObject;

        Class<?> objectType = GetDeserializedTypeFromMap(dataType);
        IObjectSerializer serializer = operationMap.GetSerializer(dataType);
        // Consume data start tag (e.g. <Data>)
        reader.ReadStartElement(dataElementName);
        returnObject = serializer.Deserialize(reader);
        // End data end tag (e.g. </Data>) is consumed in serializer.Deserialize() routine

        return returnObject;
    }

    /**
     * Deserializes the method return type from the inputted SimplifiedXMLStreamReader.
     * @param reader               A SimplifiedXMLStreamReader object to deserialize from.
     * @return                     The return type.
     * @throws XMLStreamException
     * @throws Exception
     */
    private Class<?> DeserializeReturnType(SimplifiedXMLStreamReader reader) throws XMLStreamException, Exception {
        Class<?> returnType = null;

        // Consume return type start tag (e.g. <ReturnType>)
        reader.ReadStartElement(returnTypeElementName);
        // Attempt to consume data type start tag (e.g. <DataType>)
        if(reader.IsNextNodeStartElement(dataTypeElementName) == true) {
            String serializedReturnType = reader.ReadString();
            returnType = GetDeserializedTypeFromMap(serializedReturnType);
            // Consume data type end tag (e.g. </DataType>)
            reader.ReadEndElement();
            // Consume return type end tag (e.g. </ReturnType>)
            reader.ReadEndElement();
        }

        return returnType;
    }

    /**
     * Builds a method invocation object from the inputted name, parameters, and return type.
     * @param name           The name of the method.
     * @param parameterList  The parameters of the method invocation.
     * @param returnType     The return type of the method.
     * @return               The method invocation.
     * @throws Exception
     */
    private MethodInvocation BuildMethodInvocation(String name, ArrayList parameterList, Class<?> returnType) throws Exception {
        MethodInvocation returnMethodInvocation;

        try
        {
            if (parameterList.size() == 0)
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
                Object[] parameters = parameterList.toArray();

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
    
    /**
     * Writes a complete element with the specified name and value.
     * @param writer               The XMLStreamWriter to write to.
     * @param localName            The name of the element.
     * @param value                The value of the element. 
     * @throws XMLStreamException
     */
    protected void WriteElementString(XMLStreamWriter writer, String localName, String value) throws XMLStreamException {
        writer.writeStartElement(localName);
        writer.writeCharacters(value);
        writer.writeEndElement();
    }
    
    /**
     * Creates and returns a DecimalFormat object which will format a float or double in exponential notation using the inputted number of digits.
     * @param digits  The number of digits which should appear after the decimal point.
     * @return        The DecimalFormat object.
     */
    protected DecimalFormat BuildDecimalFormatter(Integer digits) {
        // Create the relevant format string (e.g. if digits = 4, formatString becomes "#.####E0")
        String formatString = "#." + new String(new char[digits]).replace("\0", "#") + "E0";
        return new DecimalFormat(formatString);
    }
    
    private class IntegerSerializer implements IObjectSerializer<Integer> {

        @Override
        public void Serialize(Integer inputInteger, XMLStreamWriter writer) throws XMLStreamException {
            writer.writeCharacters(String.format(defaultLocale, "%d", inputInteger));
        }

        @Override
        public Integer Deserialize(SimplifiedXMLStreamReader reader) throws XMLStreamException {
            Integer returnInteger = Integer.parseInt(reader.ReadString());
            // Consume data end tag (e.g. </Data>)
            reader.ReadEndElement();
            
            return returnInteger;
        }
    }
    
    private class StringSerializer implements IObjectSerializer<String> {

        @Override
        public void Serialize(String inputString, XMLStreamWriter writer) throws XMLStreamException {
            if(inputString == "") {
                writer.writeStartElement(emptyIndicatorElementName);
                writer.writeEndElement();
            }
            else {
                writer.writeCharacters(inputString.toString());
            }
        }

        @Override
        public String Deserialize(SimplifiedXMLStreamReader reader) throws XMLStreamException {
            String returnString;
            
            // Attempt to consume empty indicator start tag (e.g. <Empty>)
            //   If IsNextNodeStartElement() call returns false then string characters are consumed
            if(reader.IsNextNodeStartElement(emptyIndicatorElementName, XMLStreamConstants.CHARACTERS) == true) {
                returnString = "";
                // Consume empty indicator end tag (e.g. </Empty>)
                reader.ReadEndElement();
            }
            else {
                returnString = reader.getText();
            }
            // Consume data end tag (e.g. </Data>)
            reader.ReadEndElement();
            
            return returnString;
        }
    }
    
    private class ByteSerializer implements IObjectSerializer<Byte> {

        @Override
        public void Serialize(Byte inputByte, XMLStreamWriter writer) throws Exception {
            writer.writeCharacters(String.format(defaultLocale, "%d", inputByte));
        }

        @Override
        public Byte Deserialize(SimplifiedXMLStreamReader reader) throws Exception {
            Byte returnByte = Byte.parseByte(reader.ReadString());
            // Consume data end tag (e.g. </Data>)
            reader.ReadEndElement();
            
            return returnByte;
        }
    }
    
    private class ShortSerializer implements IObjectSerializer<Short> {

        @Override
        public void Serialize(Short inputShort, XMLStreamWriter writer) throws Exception {
            writer.writeCharacters(String.format(defaultLocale, "%d", inputShort));
        }

        @Override
        public Short Deserialize(SimplifiedXMLStreamReader reader) throws Exception {
            Short returnShort = Short.parseShort(reader.ReadString());
            // Consume data end tag (e.g. </Data>)
            reader.ReadEndElement();
            
            return returnShort;
        }
    }
    
    private class LongSerializer implements IObjectSerializer<Long> {

        @Override
        public void Serialize(Long inputLong, XMLStreamWriter writer) throws Exception {
            writer.writeCharacters(String.format(defaultLocale, "%d", inputLong));
        }

        @Override
        public Long Deserialize(SimplifiedXMLStreamReader reader) throws Exception {
            Long returnLong = Long.parseLong(reader.ReadString());
            // Consume data end tag (e.g. </Data>)
            reader.ReadEndElement();
            
            return returnLong;
        }
    }
    
    private class FloatSerializer implements IObjectSerializer<Float> {

        @Override
        public void Serialize(Float inputFloat, XMLStreamWriter writer) throws Exception {
            if(Float.isInfinite(inputFloat) == true) {
                // Infinite floats seem to be converted to "Infinity" and "-Infinity" regardless of locale, hence just use toString() method
                writer.writeCharacters(inputFloat.toString());
            }
            else {
                DecimalFormat formatter = BuildDecimalFormatter(singleFloatingPointDigits);
                writer.writeCharacters(formatter.format(inputFloat));
            }
        }

        @Override
        public Float Deserialize(SimplifiedXMLStreamReader reader) throws Exception {
            Float returnFloat = Float.parseFloat(reader.ReadString());
            // Consume data end tag (e.g. </Data>)
            reader.ReadEndElement();
            
            return returnFloat;
        }
    }
    
    private class DoubleSerializer implements IObjectSerializer<Double> {

        @Override
        public void Serialize(Double inputDouble, XMLStreamWriter writer) throws Exception {

            if(Double.isInfinite(inputDouble) == true) {
                // Infinite doubles seem to be converted to "Infinity" and "-Infinity" regardless of locale, hence just use toString() method
                writer.writeCharacters(inputDouble.toString());
            }
            else {
                DecimalFormat formatter = BuildDecimalFormatter(doubleFloatingPointDigits);
                writer.writeCharacters(formatter.format(inputDouble));
            }
        }

        @Override
        public Double Deserialize(SimplifiedXMLStreamReader reader) throws Exception {
            Double returnDouble = Double.parseDouble(reader.ReadString());
            // Consume data end tag (e.g. </Data>)
            reader.ReadEndElement();
            
            return returnDouble;
        }
    }
    
    private class CharacterSerializer implements IObjectSerializer<Character> {

        @Override
        public void Serialize(Character inputCharacter, XMLStreamWriter writer) throws Exception {
            writer.writeCharacters(String.format(defaultLocale, "%c", inputCharacter));
        }

        @Override
        public Character Deserialize(SimplifiedXMLStreamReader reader) throws Exception {
            Character returnLong = reader.ReadString().charAt(0);
            // Consume data end tag (e.g. </Data>)
            reader.ReadEndElement();
            
            return returnLong;
        }
    }
    
    private class BooleanSerializer implements IObjectSerializer<Boolean> {

        @Override
        public void Serialize(Boolean inputBoolean, XMLStreamWriter writer) throws Exception {
            writer.writeCharacters(String.format(defaultLocale, "%b", inputBoolean));
        }

        @Override
        public Boolean Deserialize(SimplifiedXMLStreamReader reader) throws Exception {
            Boolean returnBoolean = Boolean.parseBoolean(reader.ReadString());
            // Consume data end tag (e.g. </Data>)
            reader.ReadEndElement();
            
            return returnBoolean;
        }
    }
    
    private class BigDecimalSerializer implements IObjectSerializer<BigDecimal> {

        // Upper and lower boundary values must be set to maintain compatibility with C# Decimal type
        private BigDecimal minAllowedValue = new BigDecimal("-79228162514264337593543950335");
        private BigDecimal maxAllowedValue = new BigDecimal("79228162514264337593543950335");
        
        @Override
        public void Serialize(BigDecimal inputBigDecimal, XMLStreamWriter writer) throws Exception {
            if (inputBigDecimal.compareTo(minAllowedValue) == -1) {
                throw new Exception("BigDecimal value exceeds minimum allowed size of " + minAllowedValue.toString() + ".");
            }
            if (inputBigDecimal.compareTo(maxAllowedValue) == 1) {
                throw new Exception("BigDecimal value exceeds maximum allowed size of " + maxAllowedValue.toString() + ".");
            }
            
            writer.writeCharacters(inputBigDecimal.toString());
        }

        @Override
        public BigDecimal Deserialize(SimplifiedXMLStreamReader reader) throws Exception {
            BigDecimal returnBigDecimal = new BigDecimal(reader.ReadString());
            // Consume data end tag (e.g. </Data>)
            reader.ReadEndElement();
            
            return returnBigDecimal;
        }
    }
    
    private class GregorianCalendarSerializer implements IObjectSerializer<GregorianCalendar> {

        // Upper boundary for year must be set to maintain compatibility with C# DateTime type
        private final int maxYear = 9999;
        
        @Override
        public void Serialize(GregorianCalendar inputCalendar, XMLStreamWriter writer) throws Exception {
            SimpleDateFormat dateFormatter = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", defaultLocale);
            
            if (inputCalendar.get(Calendar.YEAR) > maxYear) {
                throw new Exception("Year value exceeds maximum allowed size of " + maxYear + ".");
            }
            // Create a string with the date/time portion of the GregorianCalendar serialized (i.e. no milliseconds)
            String serializedDate = dateFormatter.format(inputCalendar.getTime());
            // Add a 'T' to the 11th position to follow XML convention
            String xmlSerializedDate = serializedDate.substring(0, 10) + "T" + serializedDate.substring(11, 19);
            // Add the milliseconds 
            Integer milliSeconds = inputCalendar.get(Calendar.MILLISECOND);
            xmlSerializedDate = xmlSerializedDate + "." + String.format(defaultLocale, "%03d", milliSeconds);
            
            writer.writeCharacters(xmlSerializedDate);
        }

        @Override
        public GregorianCalendar Deserialize(SimplifiedXMLStreamReader reader) throws Exception {
            SimpleDateFormat dateFormatter = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", defaultLocale);
            
            // Read the complete serialized date from the XML stream
            String xmlSerializedDate = reader.ReadString();
            // Remove the 'T' character from the 11th position
            String serializedDate = xmlSerializedDate.substring(0, 10) + " " + xmlSerializedDate.substring(11, 19);
            Date deserializedDate = dateFormatter.parse(serializedDate);
            // Convert the Date to a GregorianCalendar
            GregorianCalendar returnCalendar = new GregorianCalendar();
            returnCalendar.setTime(deserializedDate);
            // Set the milliseconds
            Integer milliSeconds = Integer.parseInt(xmlSerializedDate.substring(20));
            returnCalendar.set(Calendar.MILLISECOND, milliSeconds);

            // Consume data end tag (e.g. </Data>)
            reader.ReadEndElement();
            
            return returnCalendar;
        }
    }
    
    protected class ArraySerializer implements IObjectSerializer<Object[]> {
        
        @Override
        public void Serialize(Object[] inputObject, XMLStreamWriter writer) throws XMLStreamException, Exception {
            String elementSerializedType = GetSerializedTypeFromMap(inputObject.getClass().getComponentType());
            WriteElementString(writer, arrayElementDataTypeElementName, elementSerializedType);
            // Write the elements
            for(int i = 0; i < inputObject.length; i = i + 1) {
                SerializeItem(inputObject[i], arrayElementElementName, writer);
            }
        }

        @Override
        public Object[] Deserialize(SimplifiedXMLStreamReader reader) throws XMLStreamException, Exception {
            Object[] returnArray;
            ArrayList returnArrayList = new ArrayList();
            
            // Get the data type of the array elements
            String datatype = reader.ReadElementString(arrayElementDataTypeElementName);
            Class<?> objectType = GetDeserializedTypeFromMap(datatype);
            IObjectSerializer serializer = operationMap.GetSerializer(objectType);
            // Attempt to consume element start tag (e.g. <Data>)
            //   If IsNextNodeStartElement() call returns false then the closing outer </Data> tag (i.e. closing the data of the parameter not the element) is consumed.  The array is empty.
            if(reader.IsNextNodeStartElement(arrayElementElementName) == true) {
                // At this point the <Element> tag would have been already read, so need to set base depth 1 level lower than this at the <Data> tag
                int baseDepth = (reader.getDepth() - 1);
                while(reader.getDepth() >= baseDepth) {
                    Object elementObject = DeserializeItem(reader);
                    returnArrayList.add(elementObject);
                    // Element end tag (e.g. </Element>) is consumed in DeserializeItem() routine
                    // Consume element start tag (e.g. <Element>)
                    //   If IsNextNodeStartElement() call returns false then the closing </Data> tag is consumed, baseDepth becomes lower than depth, and the loop will end
                    reader.IsNextNodeStartElement(arrayElementElementName);
                }
            }
            // Convert the ArrayList to an array
            returnArray = (Object[])Array.newInstance(objectType, returnArrayList.size());
            for (int i = 0; i < returnArray.length; i = i + 1) {
                Array.set(returnArray, i, returnArrayList.get(i));
            }
            
            return returnArray;
        }
    }
}

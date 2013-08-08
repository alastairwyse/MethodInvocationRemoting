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

package net.alastairwyse.methodinvocationremotingunittests;

import static org.junit.Assert.*;
import java.util.Date;

import javax.xml.stream.XMLStreamReader;
import javax.xml.stream.XMLStreamWriter;

import org.junit.Before;
import org.junit.Test;
import net.alastairwyse.methodinvocationremoting.*;

/**
 * Unit tests for class methodinvocationremoting.SerializerOperationMap.
 * @author Alastair Wyse
 */
public class SerializerOperationMapTests {

    private SerializerOperationMap testSerializerOperationMap;
    
    @Before
    public void setUp() throws Exception {
        testSerializerOperationMap = new SerializerOperationMap();
    }

    @Test
    public void AddMappingNativeTypeNull() {
        try{
            testSerializerOperationMap.AddMapping(null, "string", new StringSerializer());
            fail("Exception was not thrown.");
        }
        catch(IllegalArgumentException e){
            assertTrue(e.getMessage().contains("Parameter 'nativeType' cannot be null."));
        }
    }
    
    @Test
    public void AddMappingSerializedTypeNull() {
        try{
            testSerializerOperationMap.AddMapping(String.class, null, new StringSerializer());
            fail("Exception was not thrown.");
        }
        catch(IllegalArgumentException e){
            assertTrue(e.getMessage().contains("Parameter 'serializedType' cannot be null."));
        }
    }
    
    @Test
    public void AddMappingSerializerNull() {
        try{
            testSerializerOperationMap.AddMapping(String.class, "string", null);
            fail("Exception was not thrown.");
        }
        catch(IllegalArgumentException e){
            assertTrue(e.getMessage().contains("Parameter 'serializer' cannot be null."));
        }
    }
    
    @Test 
    public void AddMappingNativeTypeExists() {
        testSerializerOperationMap.AddMapping(String.class, "string", new StringSerializer());
        try{
            testSerializerOperationMap.AddMapping(String.class, "string2", new StringSerializer());
            fail("Exception was not thrown.");
        }
        catch(IllegalArgumentException e){
            assertTrue(e.getMessage().contains("The native type 'java.lang.String' already exists in the map."));
        }
    }
    
    @Test 
    public void AddMappingSerializedTypeExists() {
        testSerializerOperationMap.AddMapping(int.class, "integer", new IntegerSerializer());
        try{
            testSerializerOperationMap.AddMapping(long.class, "integer", new IntegerSerializer());
            fail("Exception was not thrown.");
        }
        catch(IllegalArgumentException e){
            assertTrue(e.getMessage().contains("The serialized type 'integer' already exists in the map."));
        }
    }

    @Test
    public void UpdateMappingNativeTypeNull() {
        testSerializerOperationMap.AddMapping(String.class, "string", new StringSerializer());

        try{
            testSerializerOperationMap.UpdateMapping(null, "string", new StringSerializer());
            fail("Exception was not thrown.");
        }
        catch(IllegalArgumentException e){
            assertTrue(e.getMessage().contains("Parameter 'nativeType' cannot be null."));
        }
    }

    @Test
    public void UpdateMappingSerializedTypeNull() {
        testSerializerOperationMap.AddMapping(String.class, "string", new StringSerializer());

        try{
            testSerializerOperationMap.UpdateMapping(String.class, null, new StringSerializer());
            fail("Exception was not thrown.");
        }
        catch(IllegalArgumentException e){
            assertTrue(e.getMessage().contains("Parameter 'serializedType' cannot be null."));
        }
    }

    @Test
    public void UpdateMappingSerializerNull() {
        testSerializerOperationMap.AddMapping(String.class, "string", new StringSerializer());

        try{
            testSerializerOperationMap.UpdateMapping(String.class, "string", null);
            fail("Exception was not thrown.");
        }
        catch(IllegalArgumentException e){
            assertTrue(e.getMessage().contains("Parameter 'serializer' cannot be null."));
        }
    }

    @Test
    public void UpdateMappingNativeTypeDoesNotExist() {
        testSerializerOperationMap.AddMapping(String.class, "string", new StringSerializer());

        try{
            testSerializerOperationMap.UpdateMapping(Integer.class, "integer", new StringSerializer());
            fail("Exception was not thrown.");
        }
        catch(IllegalArgumentException e){
            assertTrue(e.getMessage().contains("The native type 'java.lang.Integer' does not exist in the map."));
        }
    }

    @Test
    public void UpdateMappingSuccessTests() {
        StringSerializer testStringSerializer = new StringSerializer();
        IntegerSerializer testIntegerSerializer = new IntegerSerializer();
        
        testSerializerOperationMap.AddMapping(String.class, "string", testStringSerializer);
        testSerializerOperationMap.AddMapping(Integer.class, "integer", testIntegerSerializer);
        testSerializerOperationMap.UpdateMapping(String.class, "UpdatedString", testIntegerSerializer);
        assertEquals("UpdatedString", testSerializerOperationMap.GetSerializedType(String.class));
        IObjectSerializer returnedObjectSerializer = testSerializerOperationMap.GetSerializer(String.class);
        assertEquals(testIntegerSerializer, returnedObjectSerializer);
        returnedObjectSerializer = testSerializerOperationMap.GetSerializer("UpdatedString");
        assertEquals(testIntegerSerializer, returnedObjectSerializer);
    }

    @Test
    public void GetSerializedTypeTypeDoesNotExist()
    {
        testSerializerOperationMap.AddMapping(String.class, "string", new StringSerializer());
        String returnedString = testSerializerOperationMap.GetSerializedType(Date.class);
        assertNull(returnedString);
    }
    
    @Test
    public void GetSerializedTypeTypeExists() {
        testSerializerOperationMap.AddMapping(String.class, "string", new StringSerializer());
        testSerializerOperationMap.AddMapping(int.class, "integer", new IntegerSerializer());
        String returnedString = testSerializerOperationMap.GetSerializedType(String.class);
        assertEquals("string", returnedString);
    }
    
    @Test 
    public void GetNativeTypeTypeDoesNotExist() {
        testSerializerOperationMap.AddMapping(String.class, "string", new IntegerSerializer());
        testSerializerOperationMap.AddMapping(long.class, "long", new IntegerSerializer());
        Class<?> returnedType = testSerializerOperationMap.GetNativeType("double");
        assertNull(returnedType);
    }
    
    @Test
    public void GetNativeTypeTypeExists()
    {
        testSerializerOperationMap.AddMapping(double.class, "Double", new IntegerSerializer());
        testSerializerOperationMap.AddMapping(Date.class, "Date", new StringSerializer());
        testSerializerOperationMap.AddMapping(String.class, "String", new StringSerializer());
        Class<?> returnedType = testSerializerOperationMap.GetNativeType("Date");
        assertEquals(Date.class, returnedType);
    }
    
    @Test
    public void GetSerializerWithNativeTypeTypeDoesNotExist() {
        testSerializerOperationMap.AddMapping(String.class, "string", new StringSerializer());
        testSerializerOperationMap.AddMapping(Integer.class, "integer", new IntegerSerializer());
        IObjectSerializer<?> returnedSerializer = testSerializerOperationMap.GetSerializer(Date.class);
        assertNull(returnedSerializer);
    }
    
    @Test
    public void GetSerializerWithNativeTypeTypeExists() {
        StringSerializer testStringSerializer = new StringSerializer();
        
        testSerializerOperationMap.AddMapping(String.class, "string", testStringSerializer);
        testSerializerOperationMap.AddMapping(Integer.class, "integer", new IntegerSerializer());
        IObjectSerializer<?> returnedSerializer = testSerializerOperationMap.GetSerializer(String.class);
        assertEquals(testStringSerializer, returnedSerializer);
    }
    
    @Test
    public void GetSerializerWithSerializedTypeTypeDoesNotExist() {
        testSerializerOperationMap.AddMapping(String.class, "string", new StringSerializer());
        testSerializerOperationMap.AddMapping(Integer.class, "integer", new IntegerSerializer());
        IObjectSerializer<?> returnedSerializer = testSerializerOperationMap.GetSerializer("date");
        assertNull(returnedSerializer);
    }
    
    @Test
    public void GetSerializerWithSerializedTypeTypeExists() {
        StringSerializer testStringSerializer = new StringSerializer();
        
        testSerializerOperationMap.AddMapping(String.class, "string", testStringSerializer);
        testSerializerOperationMap.AddMapping(Integer.class, "integer", new IntegerSerializer());
        IObjectSerializer<?> returnedSerializer = testSerializerOperationMap.GetSerializer("string");
        assertEquals(testStringSerializer, returnedSerializer);
    }
    
    private class StringSerializer implements IObjectSerializer<String> {

        @Override
        public void Serialize(String inputObject, XMLStreamWriter writer) {
        }

        @Override
        public String Deserialize(SimplifiedXMLStreamReader reader) {
            return null;
        }
    }
    
    private class IntegerSerializer implements IObjectSerializer<Integer> {

        @Override
        public void Serialize(Integer inputObject, XMLStreamWriter writer) {
        }

        @Override
        public Integer Deserialize(SimplifiedXMLStreamReader reader) {
            return null;
        }
    }
}

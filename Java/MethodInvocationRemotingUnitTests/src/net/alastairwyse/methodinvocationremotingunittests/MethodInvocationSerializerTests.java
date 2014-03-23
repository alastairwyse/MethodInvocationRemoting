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

import org.junit.Before;
import org.junit.Test;
import net.alastairwyse.methodinvocationremoting.*;
import java.util.*;
import java.math.*;

/**
 * Unit tests for class methodinvocationremoting.MethodInvocationSerializer.
 * @author Alastair Wyse
 */
public class MethodInvocationSerializerTests {

    
    private MethodInvocationSerializer testMethodInvocationSerializer;
    
    @Before
    public void setUp() throws Exception {
        testMethodInvocationSerializer = new MethodInvocationSerializer(new SerializerOperationMap());
    }
    
    @Test
    public void DeserializeMissingStartTag() {
        String testXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocationX></MethodInvocationX>";
        
        try {
            testMethodInvocationSerializer.Deserialize(testXml);
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            assertTrue(e instanceof DeserializationException);
            assertTrue(e.getMessage().contains("Failed to deserialize method invocation."));
        }
    }
    
    @Test
    public void DeserializeMissingMethodNameTag() {
        String testXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodNameX></MethodNameX></MethodInvocation>";
        
        try {
            testMethodInvocationSerializer.Deserialize(testXml);
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            assertTrue(e instanceof DeserializationException);
            assertTrue(e.getMessage().contains("Failed to deserialize method invocation."));
        }
    }
    
    @Test
    public void DeserializeBlankMethodName() {
        String testXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName /><Parameters /><ReturnType /></MethodInvocation>";
        
        try {
            testMethodInvocationSerializer.Deserialize(testXml);
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            assertTrue(e.getMessage().contains("Failed to deserialize method invocation."));
            assertTrue(e.getCause().getMessage().contains("Failed to build method invocation object."));
        }
    }
    
    @Test
    public void DeserializeUnsupportedParameterDataType() {
        String testXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>falseDataType</DataType><Data>546</Data></Parameter></Parameters><ReturnType/></MethodInvocation>";
        
        try {
            testMethodInvocationSerializer.Deserialize(testXml);
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            assertTrue(e instanceof DeserializationException);
            assertTrue(e.getMessage().contains("Failed to deserialize method invocation."));
            assertTrue(e.getCause().getMessage().contains("Serialized type 'falseDataType' does not exist in the operation map."));
        }
    }
    
    @Test
    public void SerializeUnsupportedParameterDataType() {
        HashMap<Integer, Integer> unsupportedParameter = new HashMap<Integer, Integer>();
        MethodInvocation testMethodInvocation = new MethodInvocation("MyMethod", new Object[] { unsupportedParameter });

        try {
            testMethodInvocationSerializer.Serialize(testMethodInvocation);
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            assertTrue(e instanceof SerializationException);
            assertTrue(e.getMessage().contains("Failed to serialize invocation of method 'MyMethod'."));
            assertTrue(e.getCause().getMessage().contains("Native type 'java.util.HashMap' does not exist in the operation map."));
        }
    }

    @Test
    public void SerializeMethodSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter><DataType>integer</DataType><Data>-456</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Integer)123, (Integer)(-456) }, Integer.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }

    @Test
    public void DeserializeMethodSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter><DataType>integer</DataType><Data>-456</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(2, returnedMethodInvocation.getParameters().length);
        assertEquals((Integer)123, (Integer)(returnedMethodInvocation.getParameters()[0]));
        assertEquals((Integer)(-456), (Integer)(returnedMethodInvocation.getParameters()[1]));
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
    }
    
    @Test
    public void SerializeReturnValueSuccessTests() throws SerializationException {
        String expectedSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>integer</DataType><Data>789</Data></ReturnValue>";

        String actualSerializedReturnValue = testMethodInvocationSerializer.SerializeReturnValue(789);
        assertEquals(expectedSerializedReturnValue, actualSerializedReturnValue);
    }

    @Test
    public void DeserializeReturnValueSuccessTests() throws DeserializationException {
        String serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>integer</DataType><Data>789</Data></ReturnValue>";

        Integer returnValue = (Integer)testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
        assertEquals((Integer)789, returnValue);
    }
    
    //******************************************************************************
    // SerializationException and DeserializationException Field Tests
    //******************************************************************************
    
    @Test
    public void DeserializeExceptionFieldTest() {
        String testXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>falseDataType</DataType><Data>546</Data></Parameter><InvalidTag></InvalidTag></Parameters><ReturnType/></MethodInvocation>";
        
        try {
            testMethodInvocationSerializer.Deserialize(testXml);
            fail("Exception was not thrown.");
        }
        catch(DeserializationException e) {
            assertTrue(e.getMessage().contains("Failed to deserialize method invocation."));
            assertEquals(testXml, e.getSerializedObject());
        }
    }
    
    @Test
    public void DeserializeReturnValueExceptionFieldTest() {
        String serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><InvalidTag></InvalidTag></ReturnValue>";
        
        try {
            testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
            fail("Exception was not thrown.");
        }
        catch(DeserializationException e) {
            assertTrue(e.getMessage().contains("Failed to deserialize return value."));
            assertEquals(serializedReturnValue, e.getSerializedObject());
        }
    }

    @Test
    public void SerializeExceptionFieldTest() {
        Object[] methodParameters = new Object[] { new HashMap<Integer, Integer>() };
        MethodInvocation testMethodInvocation = new MethodInvocation("TestMethod", methodParameters);

        try {
            testMethodInvocationSerializer.Serialize(testMethodInvocation);
            fail("Exception was not thrown.");
        }
        catch(SerializationException e) {
            assertTrue(e.getMessage().contains("Failed to serialize invocation of method 'TestMethod'."));
            assertSame(testMethodInvocation, e.getTargetObject());
        }
    }

    @Test
    public void SerializeReturnValueExceptionFieldTest() {
        HashMap<Integer, String> returnValue = new HashMap<Integer, String>();

        try {
            testMethodInvocationSerializer.SerializeReturnValue(returnValue);
            fail("Exception was not thrown.");
        }
        catch(SerializationException e) {
            assertTrue(e.getMessage().contains("Failed to serialize return value."));
            assertSame(returnValue, e.getTargetObject());
        }
    }
    
    //******************************************************************************
    // MethodInvocation Permutation Success Tests
    //******************************************************************************
    
    @Test
    public void VoidReturnValueSuccessTests() throws SerializationException {
        String expectedVoidReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnType>void</ReturnType>";

        assertEquals(expectedVoidReturnValue, testMethodInvocationSerializer.getVoidReturnValue());
    }
    
    @Test
    public void SerializeParameterlessVoidMethodSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters></Parameters><ReturnType></ReturnType></MethodInvocation>";

        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod"));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }

    @Test
    public void DeserializeParameterlessVoidMethodSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters /><ReturnType /></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertNull(returnedMethodInvocation.getParameters());
        assertNull(returnedMethodInvocation.getReturnType());

        // Test again without self closing tags for parameters and return type
        serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters></Parameters><ReturnType></ReturnType></MethodInvocation>";

        returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertNull(returnedMethodInvocation.getParameters());
        assertNull(returnedMethodInvocation.getReturnType());
    }
    
    @Test
    public void SerializeVoidMethodSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter><DataType>integer</DataType><Data>-456</Data></Parameter></Parameters><ReturnType></ReturnType></MethodInvocation>";

        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Integer)123, (Integer)(-456) }));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }

    @Test
    public void DeserializeVoidMethodSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter><DataType>integer</DataType><Data>-456</Data></Parameter></Parameters><ReturnType /></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(2, returnedMethodInvocation.getParameters().length);
        assertEquals((Integer)123, (Integer)(returnedMethodInvocation.getParameters()[0]));
        assertEquals((Integer)(-456), (Integer)(returnedMethodInvocation.getParameters()[1]));
        assertNull(returnedMethodInvocation.getReturnType());

        // Test again without self closing tags for return type
        serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter><DataType>integer</DataType><Data>-456</Data></Parameter></Parameters><ReturnType></ReturnType></MethodInvocation>";

        returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(2, returnedMethodInvocation.getParameters().length);
        assertEquals((Integer)123, (Integer)(returnedMethodInvocation.getParameters()[0]));
        assertEquals((Integer)(-456), (Integer)(returnedMethodInvocation.getParameters()[1]));
        assertNull(returnedMethodInvocation.getReturnType());
    }
    
    @Test
    public void SerializeParameterlessMethodSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", Integer.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }

    @Test
    public void DeserializeParameterlessMethodSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters /><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertNull(returnedMethodInvocation.getParameters());
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());

        // Test again without self closing tags for parameters
        serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertNull(returnedMethodInvocation.getParameters());
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
    }
    
    //******************************************************************************
    // Null Parameter Tests
    //******************************************************************************
    
    @Test
    public void SerializeNullParameterMethodSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter></Parameter><Parameter><DataType>integer</DataType><Data>-456</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Integer)123, null, (Integer)(-456) }, Integer.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }

    @Test
    public void DeserializeNullParameterMethodSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter /><Parameter><DataType>integer</DataType><Data>-456</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(3, returnedMethodInvocation.getParameters().length);
        assertEquals((Integer)123, (Integer)(returnedMethodInvocation.getParameters()[0]));
        assertNull(returnedMethodInvocation.getParameters()[1]);
        assertEquals((Integer)(-456), (Integer)(returnedMethodInvocation.getParameters()[2]));
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());

        // Test again without self closing tags for null parameter
        serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter></Parameter><Parameter><DataType>integer</DataType><Data>-456</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";
        returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(3, returnedMethodInvocation.getParameters().length);
        assertEquals((Integer)123, (Integer)(returnedMethodInvocation.getParameters()[0]));
        assertNull(returnedMethodInvocation.getParameters()[1]);
        assertEquals((Integer)(-456), (Integer)(returnedMethodInvocation.getParameters()[2]));
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
    }

    @Test
    public void SerializeNullReturnValueSuccessTests() throws SerializationException {
        String expectedSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue></ReturnValue>";
        String returnValue = null;
        
        String actualSerializedReturnValue = testMethodInvocationSerializer.SerializeReturnValue(returnValue);
        assertEquals(expectedSerializedReturnValue, actualSerializedReturnValue);
    }

    @Test
    public void DeserializeNullReturnValueSuccessTests() throws DeserializationException {
        String serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue />";
    
        Object returnValue = testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
        assertNull(returnValue);
    
        // Test again without self closing tag for return type
        serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue></ReturnValue>";
    
        returnValue = testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
        assertNull(returnValue);
    }
    
    //******************************************************************************
    // Array Parameter Tests
    //******************************************************************************
    
    @Test
    public void SerializeEmptyArrayParameterSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType></Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Integer)9, new Integer[] { } , (Integer)(8) }, Integer.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }
    
    @Test
    public void DeserializeEmptyArrayParameterSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType></Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(3, returnedMethodInvocation.getParameters().length);
        assertEquals(9, returnedMethodInvocation.getParameters()[0]);
        Integer[] arrayParameter = (Integer[])returnedMethodInvocation.getParameters()[1];
        assertEquals(0, arrayParameter.length);
        assertEquals((Integer)8, returnedMethodInvocation.getParameters()[2]);
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
    }
    
    @Test
    public void SerializeEmptyArrayReturnValueSuccessTests() throws SerializationException {
        String expectedSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType></Data></ReturnValue>";

        String actualSerializedReturnValue = testMethodInvocationSerializer.SerializeReturnValue(new Integer[] { });
        assertEquals(expectedSerializedReturnValue, actualSerializedReturnValue);
    }

    @Test
    public void DeserializeEmptyArrayReturnValueSuccessTests() throws DeserializationException {
        String serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType></Data></ReturnValue>";

        Integer[] returnValue = (Integer[])testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
        assertEquals(0, returnValue.length);
    }        
    
    @Test
    public void SerializeArrayParameterSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType><Element><DataType>integer</DataType><Data>123</Data></Element><Element><DataType>integer</DataType><Data>-456</Data></Element></Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Integer)9, new Integer[] { 123, -456 }, (Integer)(8) }, Integer.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }
    
    @Test
    public void DeserializeArrayParameterSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType><Element><DataType>integer</DataType><Data>123</Data></Element><Element><DataType>integer</DataType><Data>-456</Data></Element></Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(3, returnedMethodInvocation.getParameters().length);
        assertEquals(9, returnedMethodInvocation.getParameters()[0]);
        Integer[] arrayParameter = (Integer[])returnedMethodInvocation.getParameters()[1];
        assertEquals((Integer)123, arrayParameter[0]);
        assertEquals((Integer)(-456), arrayParameter[1]);
        assertEquals((Integer)8, returnedMethodInvocation.getParameters()[2]);
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
    }
    
    @Test
    public void SerializeArrayReturnValueSuccessTests() throws SerializationException {
        String expectedSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType><Element><DataType>integer</DataType><Data>123</Data></Element><Element><DataType>integer</DataType><Data>-456</Data></Element></Data></ReturnValue>";

        String actualSerializedReturnValue = testMethodInvocationSerializer.SerializeReturnValue(new Integer[] { 123, -456 });
        assertEquals(expectedSerializedReturnValue, actualSerializedReturnValue);
    }

    @Test
    public void DeserializeArrayReturnValueSuccessTests() throws DeserializationException {
        String serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType><Element><DataType>integer</DataType><Data>123</Data></Element><Element><DataType>integer</DataType><Data>-456</Data></Element></Data></ReturnValue>";

        Integer[] returnValue = (Integer[])testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
        assertEquals(2, returnValue.length);
        assertEquals((Integer)123, returnValue[0]);
        assertEquals((Integer)(-456), returnValue[1]);
    }
    
    @Test
    public void SerializeNullElementArrayParameterSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>101</Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>String 1</Data></Element><Element></Element><Element><DataType>string</DataType><Data>String 3</Data></Element></Data></Parameter><Parameter><DataType>integer</DataType><Data>202</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Integer)101, new String[] { "String 1", null, "String 3" }, (Integer)202 }, Integer.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }

    @Test
    public void DeserializeNullElementArrayParameterSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>101</Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>String 1</Data></Element><Element></Element><Element><DataType>string</DataType><Data>String 3</Data></Element></Data></Parameter><Parameter><DataType>integer</DataType><Data>202</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(3, returnedMethodInvocation.getParameters().length);
        assertEquals((Integer)101, (Integer)returnedMethodInvocation.getParameters()[0]);
        String[] arrayParameter = (String[])returnedMethodInvocation.getParameters()[1];
        assertEquals("String 1", arrayParameter[0]);
        assertNull(arrayParameter[1]);
        assertEquals("String 3", arrayParameter[2]);
        assertEquals(3, arrayParameter.length);
        assertEquals((Integer)202, (Integer)returnedMethodInvocation.getParameters()[2]);
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());

        // Test again without self closing tags for null element
        serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>101</Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>String 1</Data></Element><Element></Element><Element><DataType>string</DataType><Data>String 3</Data></Element></Data></Parameter><Parameter><DataType>integer</DataType><Data>202</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";
        returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(3, returnedMethodInvocation.getParameters().length);
        assertEquals((Integer)101, (Integer)returnedMethodInvocation.getParameters()[0]);
        arrayParameter = (String[])returnedMethodInvocation.getParameters()[1];
        assertEquals("String 1", arrayParameter[0]);
        assertNull(arrayParameter[1]);
        assertEquals("String 3", arrayParameter[2]);
        assertEquals(3, arrayParameter.length);
        assertEquals((Integer)202, (Integer)returnedMethodInvocation.getParameters()[2]);
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
    }

    @Test
    public void SerializeNullElementArrayReturnValueSuccessTests() throws SerializationException {
        String expectedSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>String 1</Data></Element><Element></Element><Element><DataType>string</DataType><Data>String 3</Data></Element></Data></ReturnValue>";

        String actualSerializedReturnValue = testMethodInvocationSerializer.SerializeReturnValue(new String[] { "String 1", null, "String 3" });
        assertEquals(expectedSerializedReturnValue, actualSerializedReturnValue);
    }

    @Test
    public void DeserializeNullElementArrayReturnValueSuccessTests() throws DeserializationException {
        String serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>String 1</Data></Element><Element /><Element><DataType>string</DataType><Data>String 3</Data></Element></Data></ReturnValue>";

        String[] returnValue = (String[])testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
        assertEquals("String 1", returnValue[0]);
        assertNull(returnValue[1]);
        assertEquals("String 3", returnValue[2]);
        assertEquals(3, returnValue.length);
        
        // Test again without self closing tags for null element
        serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>String 1</Data></Element><Element></Element><Element><DataType>string</DataType><Data>String 3</Data></Element></Data></ReturnValue>";
        returnValue = (String[])testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
        assertEquals("String 1", returnValue[0]);
        assertNull(returnValue[1]);
        assertEquals("String 3", returnValue[2]);
        assertEquals(3, returnValue.length);
    }
    
    //******************************************************************************
    // Integer Parameter Tests
    //******************************************************************************
    
    @Test
    public void SerializeIntegerParameterSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>-2147483648</Data></Parameter><Parameter><DataType>integer</DataType><Data>2147483647</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Integer)(-2147483648), (Integer)2147483647 }, Integer.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }

    @Test
    public void DeserializeIntegerParameterSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>-2147483648</Data></Parameter><Parameter><DataType>integer</DataType><Data>2147483647</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(2, returnedMethodInvocation.getParameters().length);
        assertEquals(-2147483648, returnedMethodInvocation.getParameters()[0]);
        assertEquals(2147483647, returnedMethodInvocation.getParameters()[1]);
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
    }
    
    //******************************************************************************
    // String Parameter Tests
    //******************************************************************************
    
    @Test
    public void SerializeEmptyStringParameterSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>string</DataType><Data><Empty></Empty></Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Integer)9, (String)"", (Integer)(8) }, Integer.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }

    @Test
    public void DeserializeEmptyStringParameterSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>string</DataType><Data><Empty /></Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(3, returnedMethodInvocation.getParameters().length);
        assertEquals(9, returnedMethodInvocation.getParameters()[0]);
        assertEquals("", (String)returnedMethodInvocation.getParameters()[1]);
        assertEquals(8, returnedMethodInvocation.getParameters()[2]);
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());

        // Test again without self closing tags for empty string
        serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>string</DataType><Data><Empty></Empty></Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";
        returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(3, returnedMethodInvocation.getParameters().length);
        assertEquals(9, returnedMethodInvocation.getParameters()[0]);
        assertEquals("", (String)returnedMethodInvocation.getParameters()[1]);
        assertEquals(8, returnedMethodInvocation.getParameters()[2]);
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
    }
    
    @Test
    public void SerializeEmptyStringReturnValueSuccessTests() throws SerializationException {
        String expectedSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data><Empty></Empty></Data></ReturnValue>";

        String actualSerializedReturnValue = testMethodInvocationSerializer.SerializeReturnValue("");
        assertEquals(expectedSerializedReturnValue, actualSerializedReturnValue);
    }

    @Test
    public void DeserializeEmptyStringReturnValueSuccessTests() throws DeserializationException {
        String serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data><Empty /></Data></ReturnValue>";

        String returnValue = (String)testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
        assertEquals("", returnValue);

        // Test again without self closing tags for blank string
        serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data><Empty></Empty></Data></ReturnValue>";

        returnValue = (String)testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
        assertEquals("", returnValue);
    }
    
    @Test
    public void SerializeStringParameterSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>string</DataType><Data>Test string with &lt;embedded&gt;XML tag&lt;/embedded&gt;</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>string</DataType></ReturnType></MethodInvocation>";

        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Integer)9, (String)"Test string with <embedded>XML tag</embedded>", (Integer)(8) }, String.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }

    @Test
    public void DeserializeStringParameterSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>string</DataType><Data>Test string with &lt;embedded&gt;XML tag&lt;/embedded&gt;</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>string</DataType></ReturnType></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(3, returnedMethodInvocation.getParameters().length);
        assertEquals(9, returnedMethodInvocation.getParameters()[0]);
        assertEquals("Test string with <embedded>XML tag</embedded>", (String)returnedMethodInvocation.getParameters()[1]);
        assertEquals(8, returnedMethodInvocation.getParameters()[2]);
        assertEquals(String.class, returnedMethodInvocation.getReturnType());
    }
    
    @Test
    public void SerializeStringReturnValueSuccessTests() throws SerializationException {
        String expectedSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data>Test string with &lt;embedded&gt;XML tag&lt;/embedded&gt;</Data></ReturnValue>";

        String actualSerializedReturnValue = testMethodInvocationSerializer.SerializeReturnValue("Test string with <embedded>XML tag</embedded>");
        assertEquals(expectedSerializedReturnValue, actualSerializedReturnValue);
    }

    @Test
    public void DeserializeStringReturnValueSuccessTests() throws DeserializationException {
        String serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data>Test string with &lt;embedded&gt;XML tag&lt;/embedded&gt;</Data></ReturnValue>";

        String returnValue = (String)testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
        assertEquals("Test string with <embedded>XML tag</embedded>", returnValue);
    }
    
    //******************************************************************************
    // Byte Parameter Tests
    //******************************************************************************
    
    @Test
    public void SerializeSByteParameterSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>signedByte</DataType><Data>-128</Data></Parameter><Parameter><DataType>signedByte</DataType><Data>127</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Integer)9, (byte)(-128), (byte)(127), (Integer)8 }, Integer.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }

    @Test
    public void DeserializeSByteParameterSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>signedByte</DataType><Data>-128</Data></Parameter><Parameter><DataType>signedByte</DataType><Data>127</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(4, returnedMethodInvocation.getParameters().length);
        assertEquals(9, returnedMethodInvocation.getParameters()[0]);
        assertEquals(-128, (byte)returnedMethodInvocation.getParameters()[1]);
        assertEquals(127, (byte)returnedMethodInvocation.getParameters()[2]);
        assertEquals(8, returnedMethodInvocation.getParameters()[3]);
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
    }
    
    //******************************************************************************
    // Short Integer Parameter Tests
    //******************************************************************************
    
    @Test
    public void SerializeShortParameterSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>shortInteger</DataType><Data>-32768</Data></Parameter><Parameter><DataType>shortInteger</DataType><Data>32767</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Integer)9, (short)(-32768), (short)32767, (Integer)8 }, Integer.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }

    @Test
    public void DeserializeShortParameterSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>shortInteger</DataType><Data>-32768</Data></Parameter><Parameter><DataType>shortInteger</DataType><Data>32767</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(4, returnedMethodInvocation.getParameters().length);
        assertEquals(9, returnedMethodInvocation.getParameters()[0]);
        assertEquals(-32768, (short)returnedMethodInvocation.getParameters()[1]);
        assertEquals(32767, (short)returnedMethodInvocation.getParameters()[2]);
        assertEquals(8, returnedMethodInvocation.getParameters()[3]);
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
    }
    
    //******************************************************************************
    // Long Integer Parameter Tests
    //******************************************************************************
    
    @Test
    public void SerializeLongParameterSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>longInteger</DataType><Data>-9223372036854775808</Data></Parameter><Parameter><DataType>longInteger</DataType><Data>9223372036854775807</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Integer)9, (long)(-9223372036854775808L), (long)9223372036854775807L, (Integer)8 }, Integer.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }

    @Test
    public void DeserializeLongParameterSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>longInteger</DataType><Data>-9223372036854775808</Data></Parameter><Parameter><DataType>longInteger</DataType><Data>9223372036854775807</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(4, returnedMethodInvocation.getParameters().length);
        assertEquals(9, returnedMethodInvocation.getParameters()[0]);
        assertEquals(-9223372036854775808L, (long)returnedMethodInvocation.getParameters()[1]);
        assertEquals(9223372036854775807L, (long)returnedMethodInvocation.getParameters()[2]);
        assertEquals(8, returnedMethodInvocation.getParameters()[3]);
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
    }
    
    //******************************************************************************
    // Float Parameter Tests
    //******************************************************************************
    
    @Test
    public void SerializeFloatParameterSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>float</DataType><Data>-3.14159261E-38</Data></Parameter><Parameter><DataType>float</DataType><Data>3.14159272E38</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Integer)9, (float)(-3.14159265358979323846264338327E-38F), (float)3.14159265358979323846264338327E+38F, (Integer)8 }, Integer.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }

    @Test
    public void DeserializeFloatParameterSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>float</DataType><Data>-3.14159261e-038</Data></Parameter><Parameter><DataType>float</DataType><Data>3.14159272e+038</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(4, returnedMethodInvocation.getParameters().length);
        assertEquals(9, returnedMethodInvocation.getParameters()[0]);
        assertEquals(-3.14159265358979323846264338327E-38F, (float)returnedMethodInvocation.getParameters()[1], (double)1e-50);
        assertEquals(3.14159265358979323846264338327E+38F, (float)returnedMethodInvocation.getParameters()[2], (double)1e-50);
        assertEquals(8, returnedMethodInvocation.getParameters()[3]);
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
        
        // Test again with values serialized in C# format
        serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>float</DataType><Data>-3.14159261e-038</Data></Parameter><Parameter><DataType>float</DataType><Data>3.14159272e+038</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";
        returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(4, returnedMethodInvocation.getParameters().length);
        assertEquals(9, returnedMethodInvocation.getParameters()[0]);
        assertEquals(-3.14159265358979323846264338327E-38F, (float)returnedMethodInvocation.getParameters()[1], (double)1e-50);
        assertEquals(3.14159265358979323846264338327E+38F, (float)returnedMethodInvocation.getParameters()[2], (double)1e-50);
        assertEquals(8, returnedMethodInvocation.getParameters()[3]);
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
    }
    
    @Test
    public void SerializeInfinityFloatParameterSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>float</DataType><Data>-Infinity</Data></Parameter><Parameter><DataType>float</DataType><Data>Infinity</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Integer)9, Float.NEGATIVE_INFINITY, Float.POSITIVE_INFINITY, (Integer)8 }, Integer.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }
    
    @Test
    public void DeserializeInfinityFloatParameterSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>float</DataType><Data>-Infinity</Data></Parameter><Parameter><DataType>float</DataType><Data>Infinity</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(4, returnedMethodInvocation.getParameters().length);
        assertEquals(9, returnedMethodInvocation.getParameters()[0]);
        assertEquals(Float.NEGATIVE_INFINITY, (float)returnedMethodInvocation.getParameters()[1], (double)1e-50);
        assertEquals(Float.POSITIVE_INFINITY, (float)returnedMethodInvocation.getParameters()[2], (double)1e-50);
        assertEquals(8, returnedMethodInvocation.getParameters()[3]);
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
    }
    
    //******************************************************************************
    // Double Parameter Tests
    //******************************************************************************
    
    @Test
    public void SerializeDoubleParameterSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>double</DataType><Data>-1.6976931348623213E-308</Data></Parameter><Parameter><DataType>double</DataType><Data>1.6976931348623213E308</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Integer)9, (double)(-1.6976931348623214159265E-308D), (double)1.6976931348623214159265E+308D, (Integer)8 }, Integer.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }

    @Test
    public void DeserializeDoubleParameterSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>double</DataType><Data>-1.6976931348623213E-308</Data></Parameter><Parameter><DataType>double</DataType><Data>1.6976931348623213E308</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(4, returnedMethodInvocation.getParameters().length);
        assertEquals(9, returnedMethodInvocation.getParameters()[0]);
        assertEquals(-1.6976931348623214159265E-308D, (double)returnedMethodInvocation.getParameters()[1], (double)1e-323);
        assertEquals(1.6976931348623214159265E+308D, (double)returnedMethodInvocation.getParameters()[2], (double)1e-323);
        assertEquals(8, returnedMethodInvocation.getParameters()[3]);
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
        
        // Test again with values serialized in C# format
        serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>double</DataType><Data>-1.6976931348623213e-308</Data></Parameter><Parameter><DataType>double</DataType><Data>1.6976931348623213e+308</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";
        returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(4, returnedMethodInvocation.getParameters().length);
        assertEquals(9, returnedMethodInvocation.getParameters()[0]);
        assertEquals(-1.6976931348623214159265E-308D, (double)returnedMethodInvocation.getParameters()[1], (double)1e-323);
        assertEquals(1.6976931348623214159265E+308D, (double)returnedMethodInvocation.getParameters()[2], (double)1e-323);
        assertEquals(8, returnedMethodInvocation.getParameters()[3]);
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
    }
    
    @Test
    public void SerializeInfinityDoubleParameterSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>double</DataType><Data>-Infinity</Data></Parameter><Parameter><DataType>double</DataType><Data>Infinity</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Integer)9, Double.NEGATIVE_INFINITY, Double.POSITIVE_INFINITY, (Integer)8 }, Integer.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }
    
    @Test
    public void DeserializeInfinityDoubleParameterSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>double</DataType><Data>-Infinity</Data></Parameter><Parameter><DataType>double</DataType><Data>Infinity</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(4, returnedMethodInvocation.getParameters().length);
        assertEquals(9, returnedMethodInvocation.getParameters()[0]);
        assertEquals(Double.NEGATIVE_INFINITY, (double)returnedMethodInvocation.getParameters()[1], (double)1e-323);
        assertEquals(Double.POSITIVE_INFINITY, (double)returnedMethodInvocation.getParameters()[2], (double)1e-323);
        assertEquals(8, returnedMethodInvocation.getParameters()[3]);
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
    }
    
    //******************************************************************************
    // Character Parameter Tests
    //******************************************************************************
    
    @Test
    public void SerializeCharParameterSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>char</DataType><Data>A</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Integer)9, (char)'A', (Integer)8 }, Integer.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }

    @Test
    public void DeserializeCharParameterSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>char</DataType><Data>A</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(3, returnedMethodInvocation.getParameters().length);
        assertEquals(9, returnedMethodInvocation.getParameters()[0]);
        assertEquals('A', (char)returnedMethodInvocation.getParameters()[1]);
        assertEquals(8, returnedMethodInvocation.getParameters()[2]);
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
    }
    
    //******************************************************************************
    // Boolean Parameter Tests
    //******************************************************************************
    
    @Test
    public void SerializeBooleanParameterSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>bool</DataType><Data>true</Data></Parameter><Parameter><DataType>bool</DataType><Data>false</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Integer)9, true, false, (Integer)8 }, Integer.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }

    @Test
    public void DeserializeBooleanParameterSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>bool</DataType><Data>true</Data></Parameter><Parameter><DataType>bool</DataType><Data>false</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(4, returnedMethodInvocation.getParameters().length);
        assertEquals(9, returnedMethodInvocation.getParameters()[0]);
        assertEquals(true, (boolean)returnedMethodInvocation.getParameters()[1]);
        assertEquals(false, (boolean)returnedMethodInvocation.getParameters()[2]);
        assertEquals(8, returnedMethodInvocation.getParameters()[3]);
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
    }
    
    //******************************************************************************
    // BigDecimal Parameter Tests
    //******************************************************************************
    
    @Test
    public void SerializeBigDecimalParameterSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>decimal</DataType><Data>-79228162514264337593543950335</Data></Parameter><Parameter><DataType>decimal</DataType><Data>79228162514264337593543950335</Data></Parameter><Parameter><DataType>decimal</DataType><Data>0.000000338641967689845557742</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Integer)9, new BigDecimal("-79228162514264337593543950335"), new BigDecimal("79228162514264337593543950335"), new BigDecimal("0.000000338641967689845557742"), (Integer)8 }, Integer.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }

    @Test
    public void DeserializeBigDecimalParameterSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>decimal</DataType><Data>-79228162514264337593543950335</Data></Parameter><Parameter><DataType>decimal</DataType><Data>79228162514264337593543950335</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(4, returnedMethodInvocation.getParameters().length);
        assertEquals(9, returnedMethodInvocation.getParameters()[0]);
        assertEquals(0, new BigDecimal("-79228162514264337593543950335").compareTo((BigDecimal)returnedMethodInvocation.getParameters()[1]));
        assertEquals(0, new BigDecimal("79228162514264337593543950335").compareTo((BigDecimal)returnedMethodInvocation.getParameters()[2]));
        assertEquals(8, returnedMethodInvocation.getParameters()[3]);
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
    }

    @Test
    public void SerializeBigDecimalValueTooLarge() {
        try {
            testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { new BigDecimal("-79228162514264337593543950336") }, Integer.class));
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            assertTrue(e.getMessage().contains("Failed to serialize invocation of method 'TestMethod'."));
            assertTrue(e.getCause().getMessage().contains("BigDecimal value exceeds minimum allowed size of -79228162514264337593543950335."));
        }

        try {
            testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { new BigDecimal("79228162514264337593543950336") }, Integer.class));
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            assertTrue(e.getMessage().contains("Failed to serialize invocation of method 'TestMethod'."));
            assertTrue(e.getCause().getMessage().contains("BigDecimal value exceeds maximum allowed size of 79228162514264337593543950335."));
        }
    }
    
    //******************************************************************************
    // GregorianCalendar Parameter Tests
    //******************************************************************************
    
    @Test
    public void SerializeGregorianCalendarParameterSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>dateTime</DataType><Data>2013-04-05T06:07:08.009</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        GregorianCalendar calendarParameter = new GregorianCalendar();
        calendarParameter.clear();
        // Note that month parameter in below method is 0 based, hence April represented by 3 not 4
        calendarParameter.set(2013, 3, 5, 6, 7, 8);
        calendarParameter.set(Calendar.MILLISECOND, 9);
        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Integer)9, calendarParameter, (Integer)8 }, Integer.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        
        // Test again with 24 hour hour
        expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>dateTime</DataType><Data>2013-04-05T23:07:08.009</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        calendarParameter.clear();
        calendarParameter.set(2013, 3, 5, 23, 7, 8);
        calendarParameter.set(Calendar.MILLISECOND, 9);
        actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Integer)9, calendarParameter, (Integer)8 }, Integer.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }

    @Test
    public void DeserializeGregorianCalendarParameterSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>dateTime</DataType><Data>2013-04-05T06:07:08.009</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(3, returnedMethodInvocation.getParameters().length);
        assertEquals(9, returnedMethodInvocation.getParameters()[0]);
        GregorianCalendar calendarParameter = (GregorianCalendar)returnedMethodInvocation.getParameters()[1];
        assertEquals(2013, calendarParameter.get(Calendar.YEAR));
        // Note that month returned by below method is 0 based, hence April represented by 3 not 4
        assertEquals(3, calendarParameter.get(Calendar.MONTH));
        assertEquals(5, calendarParameter.get(Calendar.DATE));
        assertEquals(6, calendarParameter.get(Calendar.HOUR_OF_DAY));
        assertEquals(7, calendarParameter.get(Calendar.MINUTE));
        assertEquals(8, calendarParameter.get(Calendar.SECOND));
        assertEquals(9, calendarParameter.get(Calendar.MILLISECOND));
        assertEquals(8, returnedMethodInvocation.getParameters()[2]);
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
        
        // Test again with 24 hour hour
        serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>dateTime</DataType><Data>2013-04-05T23:07:08.009</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

        returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("TestMethod", returnedMethodInvocation.getName());
        assertEquals(3, returnedMethodInvocation.getParameters().length);
        assertEquals(9, returnedMethodInvocation.getParameters()[0]);
        calendarParameter = (GregorianCalendar)returnedMethodInvocation.getParameters()[1];
        assertEquals(2013, calendarParameter.get(Calendar.YEAR));
        assertEquals(3, calendarParameter.get(Calendar.MONTH));
        assertEquals(5, calendarParameter.get(Calendar.DATE));
        assertEquals(23, calendarParameter.get(Calendar.HOUR_OF_DAY));
        assertEquals(7, calendarParameter.get(Calendar.MINUTE));
        assertEquals(8, calendarParameter.get(Calendar.SECOND));
        assertEquals(9, calendarParameter.get(Calendar.MILLISECOND));
        assertEquals(8, returnedMethodInvocation.getParameters()[2]);
        assertEquals(Integer.class, returnedMethodInvocation.getReturnType());
    }
    
    @Test
    public void SerializeGregorianCalendarYearTooLarge() {
        try {
            GregorianCalendar calendarParameter = new GregorianCalendar();
            calendarParameter.clear();
            calendarParameter.set(10000, 0, 1, 0, 0, 0);
            calendarParameter.set(Calendar.MILLISECOND, 0);
            testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { calendarParameter }, Integer.class));
            fail("Exception was not thrown.");
        }
        catch(Exception e) {
            assertTrue(e.getMessage().contains("Failed to serialize invocation of method 'TestMethod'."));
            assertTrue(e.getCause().getMessage().contains("Year value exceeds maximum allowed size of 9999."));
        }
    }
    
    //******************************************************************************
    // Multi Parameter Tests
    //******************************************************************************
    
    @Test
    public void SerializeMultiParameterSuccessTests() throws SerializationException {
        String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>MethodWithAllDataTypesAsParameters</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter><DataType>dateTimeArray</DataType><Data><ElementDataType>dateTime</ElementDataType><Element><DataType>dateTime</DataType><Data>0001-01-01T00:00:00.000</Data></Element><Element><DataType>dateTime</DataType><Data>9999-12-31T23:59:59.999</Data></Element></Data></Parameter><Parameter><DataType>string</DataType><Data>&lt;TestString&gt;This is a test string &lt;&gt;?/:\";''[]{}+=_-)(*&amp;^%$#@!|\\&lt;/TestString&gt;</Data></Parameter><Parameter><DataType>decimalArray</DataType><Data><ElementDataType>decimal</ElementDataType><Element><DataType>decimal</DataType><Data>-79228162514264337593543950335</Data></Element><Element><DataType>decimal</DataType><Data>79228162514264337593543950335</Data></Element></Data></Parameter><Parameter><DataType>signedByte</DataType><Data>8</Data></Parameter><Parameter><DataType>boolArray</DataType><Data><ElementDataType>bool</ElementDataType><Element><DataType>bool</DataType><Data>false</Data></Element><Element><DataType>bool</DataType><Data>true</Data></Element></Data></Parameter><Parameter><DataType>shortInteger</DataType><Data>-16343</Data></Parameter><Parameter><DataType>charArray</DataType><Data><ElementDataType>char</ElementDataType><Element><DataType>char</DataType><Data>M</Data></Element><Element><DataType>char</DataType><Data>&lt;</Data></Element></Data></Parameter><Parameter><DataType>longInteger</DataType><Data>76543</Data></Parameter><Parameter><DataType>doubleArray</DataType><Data><ElementDataType>double</ElementDataType><Element><DataType>double</DataType><Data>-1.6976931348623213E-308</Data></Element><Element><DataType>double</DataType><Data>1.6976931348623213E308</Data></Element></Data></Parameter><Parameter><DataType>float</DataType><Data>-Infinity</Data></Parameter><Parameter><DataType>floatArray</DataType><Data><ElementDataType>float</ElementDataType><Element><DataType>float</DataType><Data>-3.14159261E-38</Data></Element><Element><DataType>float</DataType><Data>3.14159272E38</Data></Element></Data></Parameter><Parameter><DataType>double</DataType><Data>Infinity</Data></Parameter><Parameter><DataType>longIntegerArray</DataType><Data><ElementDataType>longInteger</ElementDataType><Element><DataType>longInteger</DataType><Data>-9223372036854775808</Data></Element><Element><DataType>longInteger</DataType><Data>9223372036854775807</Data></Element></Data></Parameter><Parameter><DataType>char</DataType><Data>!</Data></Parameter><Parameter><DataType>shortIntegerArray</DataType><Data><ElementDataType>shortInteger</ElementDataType><Element><DataType>shortInteger</DataType><Data>-32768</Data></Element><Element><DataType>shortInteger</DataType><Data>32767</Data></Element></Data></Parameter><Parameter><DataType>bool</DataType><Data>true</Data></Parameter><Parameter><DataType>signedByteArray</DataType><Data><ElementDataType>signedByte</ElementDataType><Element><DataType>signedByte</DataType><Data>-128</Data></Element><Element><DataType>signedByte</DataType><Data>127</Data></Element></Data></Parameter><Parameter><DataType>decimal</DataType><Data>40958609456.39898479845</Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus in nulla magna, non cursus elit. Vivamus velit dolor, aliquet a.</Data></Element><Element><DataType>string</DataType><Data><Empty></Empty></Data></Element></Data></Parameter><Parameter><DataType>dateTime</DataType><Data>2013-05-01T12:43:56.654</Data></Parameter><Parameter><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType><Element><DataType>integer</DataType><Data>-2147483648</Data></Element><Element><DataType>integer</DataType><Data>2147483647</Data></Element></Data></Parameter><Parameter><DataType>decimalArray</DataType><Data><ElementDataType>decimal</ElementDataType></Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut aliquet quam sit amet massa tincidunt a interdum purus interdum. Cras.</Data></Element><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc vitae velit tellus, non interdum urna. Quisque vehicula augue eu mauris.</Data></Element><Element></Element></Data></Parameter><Parameter></Parameter></Parameters><ReturnType><DataType>dateTime</DataType></ReturnType></MethodInvocation>";

        Object[] parameters = new Object[25];
        // Integer parameter
        parameters[0] = (Integer)123;
        // GregorianCalendar array parameter
        GregorianCalendar[] calendarArrayParameter = new GregorianCalendar[2];
        GregorianCalendar minDate = new GregorianCalendar();
        minDate.clear();
        minDate.set(1, 0, 1, 0, 0, 0);
        minDate.set(Calendar.MILLISECOND, 0);
        calendarArrayParameter[0] = minDate;
        GregorianCalendar maxDate = new GregorianCalendar();
        maxDate.clear();
        maxDate.set(9999, 11, 31, 23, 59, 59);
        maxDate.set(Calendar.MILLISECOND, 999);
        calendarArrayParameter[1] = maxDate;
        parameters[1] = calendarArrayParameter;
        // String parameter
        parameters[2] = "<TestString>This is a test string <>?/:\";''[]{}+=_-)(*&^%$#@!|\\</TestString>";
        // BigDecimal array parameter
        BigDecimal[] bigDecimalArrayParameter = new BigDecimal[2];
        bigDecimalArrayParameter[0] = new BigDecimal("-79228162514264337593543950335");
        bigDecimalArrayParameter[1] = new BigDecimal("79228162514264337593543950335");
        parameters[3] = bigDecimalArrayParameter;
        // Byte parameter
        parameters[4] = (byte)8;
        // Boolean array parameter
        Boolean[] booleanArrayParameter = new Boolean[2];
        booleanArrayParameter[0] = false;
        booleanArrayParameter[1] = true;
        parameters[5] = booleanArrayParameter;
        // Short parameter
        parameters[6] = (short)-16343;
        // Char array parameter
        Character[] charArrayParameter = new Character[2];
        charArrayParameter[0] = 'M';
        charArrayParameter[1] = '<';
        parameters[7] = charArrayParameter;
        // Long parameter
        parameters[8] = (long)76543;
        // Double array parameter
        Double[] doubleArrayParameter = new Double[2];
        doubleArrayParameter[0] = -1.6976931348623214159265E-308D;
        doubleArrayParameter[1] = 1.6976931348623214159265E+308D;
        parameters[9] = doubleArrayParameter;
        // Float parameter
        parameters[10] = Float.NEGATIVE_INFINITY;
        // Float array parameter
        Float[] floatArrayParameter = new Float[2];
        floatArrayParameter[0] = -3.14159265358979323846264338327E-38F;
        floatArrayParameter[1] = 3.14159265358979323846264338327E+38F;
        parameters[11] = floatArrayParameter;
        // Double parameter
        parameters[12] = Double.POSITIVE_INFINITY;
        // Long array parameter
        Long[] longArrayParameter = new Long[2];
        longArrayParameter[0] = -9223372036854775808L;
        longArrayParameter[1] = 9223372036854775807L;
        parameters[13] = longArrayParameter;
        // Character parameter
        parameters[14] = (char)'!';
        // Short array parameter
        Short[] shortArrayParameter = new Short[2];
        shortArrayParameter[0] = -32768;
        shortArrayParameter[1] = 32767;
        parameters[15] = shortArrayParameter;
        // Boolean parameter
        parameters[16] = true;
        // Byte array parameter
        Byte[] byteArrayParameter = new Byte[2];
        byteArrayParameter[0] = -128;
        byteArrayParameter[1] = 127;
        parameters[17] = byteArrayParameter;
        // BigDecimal parameter
        parameters[18] = new BigDecimal("40958609456.39898479845");
        // String array parameter
        String[] stringArrayParameter = new String[2];
        stringArrayParameter[0] = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus in nulla magna, non cursus elit. Vivamus velit dolor, aliquet a.";
        stringArrayParameter[1] = "";
        parameters[19] = stringArrayParameter;
        // GregorianCalendar parameter
        GregorianCalendar dateParameter = new GregorianCalendar();
        dateParameter.clear();
        dateParameter.set(2013, 4, 1, 12, 43, 56);
        dateParameter.set(Calendar.MILLISECOND, 654);
        parameters[20] = dateParameter;
        // Integer array parameter
        Integer[] integerParameter = new Integer[2];
        integerParameter[0] = -2147483648;
        integerParameter[1] = 2147483647;
        parameters[21] = integerParameter;
        // Empty array
        parameters[22] = new BigDecimal[0];
        // Array with null parameter
        String[] nullStringArrayParameter = new String[3];
        nullStringArrayParameter[0] = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut aliquet quam sit amet massa tincidunt a interdum purus interdum. Cras.";
        nullStringArrayParameter[1] = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc vitae velit tellus, non interdum urna. Quisque vehicula augue eu mauris.";
        nullStringArrayParameter[2] = null;
        parameters[23] = nullStringArrayParameter;
        // Null parameter
        parameters[24] = null;
        String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("MethodWithAllDataTypesAsParameters", parameters , GregorianCalendar.class));
        assertEquals(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
    }

    @Test
    public void DeserializeMultiParameterSuccessTests() throws DeserializationException {
        String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>MethodWithAllDataTypesAsParameters</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter><DataType>dateTimeArray</DataType><Data><ElementDataType>dateTime</ElementDataType><Element><DataType>dateTime</DataType><Data>0001-01-01T00:00:00.000</Data></Element><Element><DataType>dateTime</DataType><Data>9999-12-31T23:59:59.999</Data></Element></Data></Parameter><Parameter><DataType>string</DataType><Data>&lt;TestString&gt;This is a test string &lt;&gt;?/:\";''[]{}+=_-)(*&amp;^%$#@!|\\&lt;/TestString&gt;</Data></Parameter><Parameter><DataType>decimalArray</DataType><Data><ElementDataType>decimal</ElementDataType><Element><DataType>decimal</DataType><Data>-79228162514264337593543950335</Data></Element><Element><DataType>decimal</DataType><Data>79228162514264337593543950335</Data></Element></Data></Parameter><Parameter><DataType>signedByte</DataType><Data>8</Data></Parameter><Parameter><DataType>boolArray</DataType><Data><ElementDataType>bool</ElementDataType><Element><DataType>bool</DataType><Data>false</Data></Element><Element><DataType>bool</DataType><Data>true</Data></Element></Data></Parameter><Parameter><DataType>shortInteger</DataType><Data>-16343</Data></Parameter><Parameter><DataType>charArray</DataType><Data><ElementDataType>char</ElementDataType><Element><DataType>char</DataType><Data>M</Data></Element><Element><DataType>char</DataType><Data>&lt;</Data></Element></Data></Parameter><Parameter><DataType>longInteger</DataType><Data>76543</Data></Parameter><Parameter><DataType>doubleArray</DataType><Data><ElementDataType>double</ElementDataType><Element><DataType>double</DataType><Data>-1.6976931348623213E-308</Data></Element><Element><DataType>double</DataType><Data>1.6976931348623213E308</Data></Element></Data></Parameter><Parameter><DataType>float</DataType><Data>-Infinity</Data></Parameter><Parameter><DataType>floatArray</DataType><Data><ElementDataType>float</ElementDataType><Element><DataType>float</DataType><Data>-3.14159261E-38</Data></Element><Element><DataType>float</DataType><Data>3.14159272E38</Data></Element></Data></Parameter><Parameter><DataType>double</DataType><Data>Infinity</Data></Parameter><Parameter><DataType>longIntegerArray</DataType><Data><ElementDataType>longInteger</ElementDataType><Element><DataType>longInteger</DataType><Data>-9223372036854775808</Data></Element><Element><DataType>longInteger</DataType><Data>9223372036854775807</Data></Element></Data></Parameter><Parameter><DataType>char</DataType><Data>!</Data></Parameter><Parameter><DataType>shortIntegerArray</DataType><Data><ElementDataType>shortInteger</ElementDataType><Element><DataType>shortInteger</DataType><Data>-32768</Data></Element><Element><DataType>shortInteger</DataType><Data>32767</Data></Element></Data></Parameter><Parameter><DataType>bool</DataType><Data>true</Data></Parameter><Parameter><DataType>signedByteArray</DataType><Data><ElementDataType>signedByte</ElementDataType><Element><DataType>signedByte</DataType><Data>-128</Data></Element><Element><DataType>signedByte</DataType><Data>127</Data></Element></Data></Parameter><Parameter><DataType>decimal</DataType><Data>40958609456.39898479845</Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus in nulla magna, non cursus elit. Vivamus velit dolor, aliquet a.</Data></Element><Element><DataType>string</DataType><Data><Empty></Empty></Data></Element></Data></Parameter><Parameter><DataType>dateTime</DataType><Data>2013-05-01T12:43:56.654</Data></Parameter><Parameter><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType><Element><DataType>integer</DataType><Data>-2147483648</Data></Element><Element><DataType>integer</DataType><Data>2147483647</Data></Element></Data></Parameter><Parameter><DataType>decimalArray</DataType><Data><ElementDataType>decimal</ElementDataType></Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut aliquet quam sit amet massa tincidunt a interdum purus interdum. Cras.</Data></Element><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc vitae velit tellus, non interdum urna. Quisque vehicula augue eu mauris.</Data></Element><Element></Element></Data></Parameter><Parameter></Parameter></Parameters><ReturnType><DataType>dateTime</DataType></ReturnType></MethodInvocation>";

        IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("MethodWithAllDataTypesAsParameters", returnedMethodInvocation.getName());
        Object[] parameters = returnedMethodInvocation.getParameters();
        assertEquals(25, parameters.length);

        // Test parameters
        // Integer parameter
        assertEquals((int)123, (int)parameters[0]);
        // GregorianCalendar array parameter
        GregorianCalendar[] calendarArrayParameter = (GregorianCalendar[])parameters[1];
        GregorianCalendar minDate = new GregorianCalendar();
        minDate.clear();
        minDate.set(1, 0, 1, 0, 0, 0);
        minDate.set(Calendar.MILLISECOND, 0);
        GregorianCalendar maxDate = new GregorianCalendar();
        maxDate.clear();
        maxDate.set(9999, 11, 31, 23, 59, 59);
        maxDate.set(Calendar.MILLISECOND, 999);
        assertEquals(minDate, calendarArrayParameter[0]);
        assertEquals(maxDate, calendarArrayParameter[1]);
        // String parameter
        assertEquals("<TestString>This is a test string <>?/:\";''[]{}+=_-)(*&^%$#@!|\\</TestString>", (String)parameters[2]);
        // BigDecimal array parameter
        BigDecimal[] bigDecimalArrayParameter = (BigDecimal[])parameters[3];
        BigDecimal minDecimal = new BigDecimal("-79228162514264337593543950335");
        BigDecimal maxDecimal = new BigDecimal("79228162514264337593543950335");
        assertEquals(0, minDecimal.compareTo(bigDecimalArrayParameter[0]));
        assertEquals(0, maxDecimal.compareTo(bigDecimalArrayParameter[1]));
        // Byte parameter
        assertEquals((byte)8, (byte)parameters[4]);
        // Boolean array parameter
        Boolean[] booleanArrayParameter = (Boolean[])parameters[5];
        assertEquals(false, booleanArrayParameter[0]);
        assertEquals(true, booleanArrayParameter[1]);
        // Short parameter
        assertEquals((short)-16343, (short)parameters[6]);
        // Char array parameter
        Character[] charArrayParameter = (Character[])parameters[7];
        assertEquals('M', (char)charArrayParameter[0]);
        assertEquals('<', (char)charArrayParameter[1]);
        // Long parameter
        assertEquals((long)76543, (long)parameters[8]);
        // Double array parameter
        Double[] doubleArrayParameter = (Double[])parameters[9];
        assertEquals(-1.6976931348623214159265E-308D, (double)doubleArrayParameter[0], (double)1e-323);
        assertEquals(1.6976931348623214159265E+308D, (double)doubleArrayParameter[1], (double)1e-323);
        // Float parameter
        assertEquals(Float.NEGATIVE_INFINITY, (float)parameters[10], (double)1e-50);
        // Float array parameter
        Float[] floatArrayParameter = (Float[])parameters[11];
        floatArrayParameter[0] = -3.14159265358979323846264338327E-38F;
        floatArrayParameter[1] = 3.14159265358979323846264338327E+38F;
        assertEquals(-3.14159265358979323846264338327E-38F, (float)floatArrayParameter[0], (double)1e-50);
        assertEquals(3.14159265358979323846264338327E+38F, (float)floatArrayParameter[1], (double)1e-50);
        // Double parameter
        assertEquals(Double.POSITIVE_INFINITY, (double)parameters[12], (double)1e-323);
        // Long array parameter
        Long[] longArrayParameter = (Long[])parameters[13];
        assertEquals(-9223372036854775808L, (long)longArrayParameter[0]);
        assertEquals(9223372036854775807L, (long)longArrayParameter[1]);
        // Character parameter
        assertEquals('!', (char)parameters[14]);
        // Short array parameter
        Short[] shortArrayParameter = (Short[])parameters[15];
        assertEquals(-32768, (short)shortArrayParameter[0]);
        assertEquals(32767, (short)shortArrayParameter[1]);
        // Boolean parameter
        assertEquals(true, (boolean)parameters[16]);
        // Byte array parameter
        Byte[] byteArrayParameter = (Byte[])parameters[17];
        assertEquals(-128, (byte)byteArrayParameter[0]);
        assertEquals(127, (byte)byteArrayParameter[1]);
        // BigDecimal parameter
        BigDecimal expectedBigDecimal = new BigDecimal("40958609456.39898479845");
        assertEquals(0, expectedBigDecimal.compareTo((BigDecimal)parameters[18]));
        // String array parameter
        String[] stringArrayParameter = (String[])parameters[19];
        assertEquals("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus in nulla magna, non cursus elit. Vivamus velit dolor, aliquet a.", (String)stringArrayParameter[0]);
        assertEquals("", (String)stringArrayParameter[1]);
        // GregorianCalendar parameter
        GregorianCalendar expectedDateParameter = new GregorianCalendar();
        expectedDateParameter.clear();
        expectedDateParameter.set(2013, 4, 1, 12, 43, 56);
        expectedDateParameter.set(Calendar.MILLISECOND, 654);
        assertEquals(expectedDateParameter, (GregorianCalendar)parameters[20]);
        // Integer array parameter
        Integer[] integerParameter = (Integer[])parameters[21];
        assertEquals((Integer)(-2147483648), (Integer)integerParameter[0]);
        assertEquals((Integer)2147483647, (Integer)integerParameter[1]);
        // Empty array
        BigDecimal[] bigDecimalParameter = (BigDecimal[])parameters[22];
        assertEquals(0, bigDecimalParameter.length);
        // Array with null parameter
        String[] nullStringArrayParameter = (String[])parameters[23];
        assertEquals("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut aliquet quam sit amet massa tincidunt a interdum purus interdum. Cras.", (String)nullStringArrayParameter[0]);
        assertEquals("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc vitae velit tellus, non interdum urna. Quisque vehicula augue eu mauris.", (String)nullStringArrayParameter[1]);
        assertNull((String)nullStringArrayParameter[2]);
        // Null parameter
        assertNull(parameters[24]);
     
        // Test return type
        assertEquals(GregorianCalendar.class, returnedMethodInvocation.getReturnType());

        
        // Test again with C# version of the same method invocation
        serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>MethodWithAllDataTypesAsParameters</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter><DataType>dateTimeArray</DataType><Data><ElementDataType>dateTime</ElementDataType><Element><DataType>dateTime</DataType><Data>0001-01-01T00:00:00.000</Data></Element><Element><DataType>dateTime</DataType><Data>9999-12-31T23:59:59.999</Data></Element></Data></Parameter><Parameter><DataType>string</DataType><Data>&lt;TestString&gt;This is a test string &lt;&gt;?/:\";''[]{}+=_-)(*&amp;^%$#@!|\\&lt;/TestString&gt;</Data></Parameter><Parameter><DataType>decimalArray</DataType><Data><ElementDataType>decimal</ElementDataType><Element><DataType>decimal</DataType><Data>-79228162514264337593543950335</Data></Element><Element><DataType>decimal</DataType><Data>79228162514264337593543950335</Data></Element></Data></Parameter><Parameter><DataType>signedByte</DataType><Data>8</Data></Parameter><Parameter><DataType>boolArray</DataType><Data><ElementDataType>bool</ElementDataType><Element><DataType>bool</DataType><Data>false</Data></Element><Element><DataType>bool</DataType><Data>true</Data></Element></Data></Parameter><Parameter><DataType>shortInteger</DataType><Data>-16343</Data></Parameter><Parameter><DataType>charArray</DataType><Data><ElementDataType>char</ElementDataType><Element><DataType>char</DataType><Data>M</Data></Element><Element><DataType>char</DataType><Data>&lt;</Data></Element></Data></Parameter><Parameter><DataType>longInteger</DataType><Data>76543</Data></Parameter><Parameter><DataType>doubleArray</DataType><Data><ElementDataType>double</ElementDataType><Element><DataType>double</DataType><Data>-1.6976931348623213e-308</Data></Element><Element><DataType>double</DataType><Data>1.6976931348623213e+308</Data></Element></Data></Parameter><Parameter><DataType>float</DataType><Data>-Infinity</Data></Parameter><Parameter><DataType>floatArray</DataType><Data><ElementDataType>float</ElementDataType><Element><DataType>float</DataType><Data>-3.14159261e-038</Data></Element><Element><DataType>float</DataType><Data>3.14159272e+038</Data></Element></Data></Parameter><Parameter><DataType>double</DataType><Data>Infinity</Data></Parameter><Parameter><DataType>longIntegerArray</DataType><Data><ElementDataType>longInteger</ElementDataType><Element><DataType>longInteger</DataType><Data>-9223372036854775808</Data></Element><Element><DataType>longInteger</DataType><Data>9223372036854775807</Data></Element></Data></Parameter><Parameter><DataType>char</DataType><Data>!</Data></Parameter><Parameter><DataType>shortIntegerArray</DataType><Data><ElementDataType>shortInteger</ElementDataType><Element><DataType>shortInteger</DataType><Data>-32768</Data></Element><Element><DataType>shortInteger</DataType><Data>32767</Data></Element></Data></Parameter><Parameter><DataType>bool</DataType><Data>true</Data></Parameter><Parameter><DataType>signedByteArray</DataType><Data><ElementDataType>signedByte</ElementDataType><Element><DataType>signedByte</DataType><Data>-128</Data></Element><Element><DataType>signedByte</DataType><Data>127</Data></Element></Data></Parameter><Parameter><DataType>decimal</DataType><Data>40958609456.39898479845</Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus in nulla magna, non cursus elit. Vivamus velit dolor, aliquet a.</Data></Element><Element><DataType>string</DataType><Data><Empty /></Data></Element></Data></Parameter><Parameter><DataType>dateTime</DataType><Data>2013-05-01T12:43:56.654</Data></Parameter><Parameter><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType><Element><DataType>integer</DataType><Data>-2147483648</Data></Element><Element><DataType>integer</DataType><Data>2147483647</Data></Element></Data></Parameter><Parameter><DataType>decimalArray</DataType><Data><ElementDataType>decimal</ElementDataType></Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut aliquet quam sit amet massa tincidunt a interdum purus interdum. Cras.</Data></Element><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc vitae velit tellus, non interdum urna. Quisque vehicula augue eu mauris.</Data></Element><Element /></Data></Parameter><Parameter /></Parameters><ReturnType><DataType>dateTime</DataType></ReturnType></MethodInvocation>";

        returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
        assertEquals("MethodWithAllDataTypesAsParameters", returnedMethodInvocation.getName());
        parameters = returnedMethodInvocation.getParameters();
        assertEquals(25, parameters.length);

        // Test parameters
        // Integer parameter
        assertEquals((int)123, (int)parameters[0]);
        // GregorianCalendar array parameter
        calendarArrayParameter = (GregorianCalendar[])parameters[1];
        minDate = new GregorianCalendar();
        minDate.clear();
        minDate.set(1, 0, 1, 0, 0, 0);
        minDate.set(Calendar.MILLISECOND, 0);
        maxDate = new GregorianCalendar();
        maxDate.clear();
        maxDate.set(9999, 11, 31, 23, 59, 59);
        maxDate.set(Calendar.MILLISECOND, 999);
        assertEquals(minDate, calendarArrayParameter[0]);
        assertEquals(maxDate, calendarArrayParameter[1]);
        // String parameter
        assertEquals("<TestString>This is a test string <>?/:\";''[]{}+=_-)(*&^%$#@!|\\</TestString>", (String)parameters[2]);
        // BigDecimal array parameter
        bigDecimalArrayParameter = (BigDecimal[])parameters[3];
        minDecimal = new BigDecimal("-79228162514264337593543950335");
        maxDecimal = new BigDecimal("79228162514264337593543950335");
        assertEquals(0, minDecimal.compareTo(bigDecimalArrayParameter[0]));
        assertEquals(0, maxDecimal.compareTo(bigDecimalArrayParameter[1]));
        // Byte parameter
        assertEquals((byte)8, (byte)parameters[4]);
        // Boolean array parameter
        booleanArrayParameter = (Boolean[])parameters[5];
        assertEquals(false, booleanArrayParameter[0]);
        assertEquals(true, booleanArrayParameter[1]);
        // Short parameter
        assertEquals((short)-16343, (short)parameters[6]);
        // Char array parameter
        charArrayParameter = (Character[])parameters[7];
        assertEquals('M', (char)charArrayParameter[0]);
        assertEquals('<', (char)charArrayParameter[1]);
        // Long parameter
        assertEquals((long)76543, (long)parameters[8]);
        // Double array parameter
        doubleArrayParameter = (Double[])parameters[9];
        assertEquals(-1.6976931348623214159265E-308D, (double)doubleArrayParameter[0], (double)1e-323);
        assertEquals(1.6976931348623214159265E+308D, (double)doubleArrayParameter[1], (double)1e-323);
        // Float parameter
        assertEquals(Float.NEGATIVE_INFINITY, (float)parameters[10], (double)1e-50);
        // Float array parameter
        floatArrayParameter = (Float[])parameters[11];
        floatArrayParameter[0] = -3.14159265358979323846264338327E-38F;
        floatArrayParameter[1] = 3.14159265358979323846264338327E+38F;
        assertEquals(-3.14159265358979323846264338327E-38F, (float)floatArrayParameter[0], (double)1e-50);
        assertEquals(3.14159265358979323846264338327E+38F, (float)floatArrayParameter[1], (double)1e-50);
        // Double parameter
        assertEquals(Double.POSITIVE_INFINITY, (double)parameters[12], (double)1e-323);
        // Long array parameter
        longArrayParameter = (Long[])parameters[13];
        assertEquals(-9223372036854775808L, (long)longArrayParameter[0]);
        assertEquals(9223372036854775807L, (long)longArrayParameter[1]);
        // Character parameter
        assertEquals('!', (char)parameters[14]);
        // Short array parameter
        shortArrayParameter = (Short[])parameters[15];
        assertEquals(-32768, (short)shortArrayParameter[0]);
        assertEquals(32767, (short)shortArrayParameter[1]);
        // Boolean parameter
        assertEquals(true, (boolean)parameters[16]);
        // Byte array parameter
        byteArrayParameter = (Byte[])parameters[17];
        assertEquals(-128, (byte)byteArrayParameter[0]);
        assertEquals(127, (byte)byteArrayParameter[1]);
        // BigDecimal parameter
        expectedBigDecimal = new BigDecimal("40958609456.39898479845");
        assertEquals(0, expectedBigDecimal.compareTo((BigDecimal)parameters[18]));
        // String array parameter
        stringArrayParameter = (String[])parameters[19];
        assertEquals("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus in nulla magna, non cursus elit. Vivamus velit dolor, aliquet a.", (String)stringArrayParameter[0]);
        assertEquals("", (String)stringArrayParameter[1]);
        // GregorianCalendar parameter
        expectedDateParameter = new GregorianCalendar();
        expectedDateParameter.clear();
        expectedDateParameter.set(2013, 4, 1, 12, 43, 56);
        expectedDateParameter.set(Calendar.MILLISECOND, 654);
        assertEquals(expectedDateParameter, (GregorianCalendar)parameters[20]);
        // Integer array parameter
        integerParameter = (Integer[])parameters[21];
        assertEquals((Integer)(-2147483648), (Integer)integerParameter[0]);
        assertEquals((Integer)2147483647, (Integer)integerParameter[1]);
        // Empty array
        bigDecimalParameter = (BigDecimal[])parameters[22];
        assertEquals(0, bigDecimalParameter.length);
        // Array with null parameter
        nullStringArrayParameter = (String[])parameters[23];
        assertEquals("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut aliquet quam sit amet massa tincidunt a interdum purus interdum. Cras.", (String)nullStringArrayParameter[0]);
        assertEquals("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc vitae velit tellus, non interdum urna. Quisque vehicula augue eu mauris.", (String)nullStringArrayParameter[1]);
        assertNull((String)nullStringArrayParameter[2]);
        // Null parameter
        assertNull(parameters[24]);
     
        // Test return type
        assertEquals(GregorianCalendar.class, returnedMethodInvocation.getReturnType());
    }
}

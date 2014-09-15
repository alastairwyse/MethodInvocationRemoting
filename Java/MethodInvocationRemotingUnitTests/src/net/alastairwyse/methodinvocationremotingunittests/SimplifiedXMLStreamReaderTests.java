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

import org.junit.Test;
import java.io.ByteArrayInputStream;
import javax.xml.stream.*;
import net.alastairwyse.methodinvocationremoting.*;

/**
 * Unit tests for class methodinvocationremoting.SimplifiedXMLStreamReader.
 * @author Alastair Wyse
 */
public class SimplifiedXMLStreamReaderTests {

    private SimplifiedXMLStreamReader testSimplifiedXMLStreamReader;
    
    @Test
    public void ReadStartElementSuccessTests() throws XMLStreamException {
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters/><ReturnType/></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        assertEquals(1, testSimplifiedXMLStreamReader.getDepth());
    }
    
    @Test
    public void ReadStartElementNodeNotElement() throws XMLStreamException {
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation property=\"ABC\"><MethodName>TestMethod</MethodName><Parameters/><ReturnType/></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodName");
        try {
            testSimplifiedXMLStreamReader.ReadStartElement("MethodName");
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("Node read was not a start element."));
        }
    }
    
    @Test
    public void ReadStartElementIncorrectName() throws XMLStreamException {
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation property=\"ABC\"><MethodName>TestMethod</MethodName><Parameters/><ReturnType/></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        try {
            testSimplifiedXMLStreamReader.ReadStartElement("MethodNameX");
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("Element 'MethodNameX' was not found."));
        }
    }
    
    @Test
    public void ReadEndElementNodeNotElement() throws XMLStreamException {
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation property=\"ABC\"><MethodName>TestMethod</MethodName><Parameters/><ReturnType/></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        try {
            testSimplifiedXMLStreamReader.ReadEndElement();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("Node read was not an end element."));
        }
    }
    
    @Test
    public void ReadEndElementSuccessTests() throws XMLStreamException {
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName></MethodName></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodName");
        assertEquals(2, testSimplifiedXMLStreamReader.getDepth());
        testSimplifiedXMLStreamReader.ReadEndElement();
        assertEquals(1, testSimplifiedXMLStreamReader.getDepth());
        testSimplifiedXMLStreamReader.ReadEndElement();
        assertEquals(0, testSimplifiedXMLStreamReader.getDepth());
    }
    
    @Test
    public void ReadElementStringStartNodeNotElement() throws XMLStreamException {
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation property=\"ABC\"><MethodName>TestMethod</MethodName><Parameters/><ReturnType/></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodName");
        try {
            testSimplifiedXMLStreamReader.ReadElementString("MethodName");
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("Node read was not a start element."));
        }
    }
    
    @Test
    public void ReadElementStringIncorrectName() throws XMLStreamException {
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters/><ReturnType/></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        try {
            testSimplifiedXMLStreamReader.ReadStartElement("MethodNameX");
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("Element 'MethodNameX' was not found."));
        }
    }
    
    @Test
    public void ReadElementStringEndNodeNotEndElement() throws XMLStreamException {
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters/><ReturnType/></MethodInvocation>");
        try {
            testSimplifiedXMLStreamReader.ReadElementString("MethodInvocation");
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("Node read was not an end element."));
        }
    }
    
    @Test
    public void ReadElementStringSuccessTests() throws XMLStreamException {
        // Test for non-blank element
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>MyMethod</MethodName><Parameters/></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        assertEquals(1, testSimplifiedXMLStreamReader.getDepth());
        String elementString = testSimplifiedXMLStreamReader.ReadElementString("MethodName");
        assertEquals(1, testSimplifiedXMLStreamReader.getDepth());
        assertEquals("MyMethod", elementString);
        // As this method advances the cursor in the reader multiple times, check that is has correctly consumed the closing </MethodName> node
        testSimplifiedXMLStreamReader.ReadStartElement("Parameters");
        
        // Test for a blank element (<item></item>)
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName></MethodName><Parameters/></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        assertEquals(1, testSimplifiedXMLStreamReader.getDepth());
        elementString = testSimplifiedXMLStreamReader.ReadElementString("MethodName");
        assertEquals(1, testSimplifiedXMLStreamReader.getDepth());
        assertEquals("", elementString);
        testSimplifiedXMLStreamReader.ReadStartElement("Parameters");
        
        // Test for a blank element with self-closing tag (<item/>)
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName/><Parameters/></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        assertEquals(1, testSimplifiedXMLStreamReader.getDepth());
        elementString = testSimplifiedXMLStreamReader.ReadElementString("MethodName");
        assertEquals(1, testSimplifiedXMLStreamReader.getDepth());
        assertEquals("", elementString);
        testSimplifiedXMLStreamReader.ReadStartElement("Parameters");
    }
    
    @Test 
    public void ReadStringNotCharacterNode() throws XMLStreamException {
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters/><ReturnType/></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        try {
            testSimplifiedXMLStreamReader.ReadString();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("Node read did not contain characters."));
        }
    }
    
    @Test
    public void ReadStringSuccessTests() throws XMLStreamException {
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>MyMethod</MethodName></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodName");
        assertEquals(2, testSimplifiedXMLStreamReader.getDepth());
        String elementString = testSimplifiedXMLStreamReader.ReadString();
        assertEquals(2, testSimplifiedXMLStreamReader.getDepth());
        assertEquals("MyMethod", elementString);
    }
    
    @Test
    public void IsNextNodeStartElementIncorrectName() throws XMLStreamException {
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>MyMethod</MethodName></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        try {
            testSimplifiedXMLStreamReader.IsNextNodeStartElement("MethodNameX");
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("Element 'MethodNameX' was not found."));
        }
    }
    
    @Test
    public void IsNextNodeStartElementNodeNotElement() throws XMLStreamException {
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>MyMethod</MethodName></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodName");
        try {
            testSimplifiedXMLStreamReader.IsNextNodeStartElement("MethodNameX");
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("Node read was neither a start nor an end element."));
        }
    }
    
    @Test
    public void IsNextNodeStartElementStartElementSuccessTests() throws XMLStreamException {
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>MyMethod</MethodName></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        assertEquals(1, testSimplifiedXMLStreamReader.getDepth());
        Boolean result = testSimplifiedXMLStreamReader.IsNextNodeStartElement("MethodName");
        assertEquals(2, testSimplifiedXMLStreamReader.getDepth());
        assertTrue(result);
    }
    
    @Test
    public void IsNextNodeStartElementEndElementSuccessTests() throws XMLStreamException {
        // Test for a blank element (<item></item>)
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName></MethodName></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodName");
        assertEquals(2, testSimplifiedXMLStreamReader.getDepth());
        Boolean result = testSimplifiedXMLStreamReader.IsNextNodeStartElement("ABC");
        assertEquals(1, testSimplifiedXMLStreamReader.getDepth());
        assertFalse(result);
        
        // Test for a blank element with self-closing tag (<item/>)
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName/></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodName");
        assertEquals(2, testSimplifiedXMLStreamReader.getDepth());
        result = testSimplifiedXMLStreamReader.IsNextNodeStartElement("ABC");
        assertEquals(1, testSimplifiedXMLStreamReader.getDepth());
        assertFalse(result);
    }

    
    
    @Test
    public void IsNextNodeStartElementAlternateIncorrectName() throws XMLStreamException {
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>MyMethod</MethodName></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        try {
            testSimplifiedXMLStreamReader.IsNextNodeStartElement("MethodNameX", XMLStreamConstants.END_ELEMENT);
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("Element 'MethodNameX' was not found."));
        }
    }
    
    @Test
    public void IsNextNodeStartElementAlternateNodeNotElement() throws XMLStreamException {
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>MyMethod</MethodName></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodName");
        try {
            testSimplifiedXMLStreamReader.IsNextNodeStartElement("MethodNameX", XMLStreamConstants.END_ELEMENT);
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("Node read was neither a start element nor the specified alternate node type."));
        }
    }
    
    @Test
    public void IsNextNodeStartElementAlternateStartElementSuccessTests() throws XMLStreamException {
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>MyMethod</MethodName></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        assertEquals(1, testSimplifiedXMLStreamReader.getDepth());
        Boolean result = testSimplifiedXMLStreamReader.IsNextNodeStartElement("MethodName", XMLStreamConstants.END_ELEMENT);
        assertEquals(2, testSimplifiedXMLStreamReader.getDepth());
        assertTrue(result);
    }
    
    @Test
    public void IsNextNodeStartElementAlternateAlternateNodeSuccessTests() throws XMLStreamException {
        // Test where the specified alternate node contains characters
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>MyMethod</MethodName></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodName");
        assertEquals(2, testSimplifiedXMLStreamReader.getDepth());
        Boolean result = testSimplifiedXMLStreamReader.IsNextNodeStartElement("MethodName", XMLStreamConstants.CHARACTERS);
        assertEquals(2, testSimplifiedXMLStreamReader.getDepth());
        assertFalse(result);
        
        // Test where the specified alternate node is an end node
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName></MethodName></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodName");
        assertEquals(2, testSimplifiedXMLStreamReader.getDepth());
        result = testSimplifiedXMLStreamReader.IsNextNodeStartElement("ABC", XMLStreamConstants.END_ELEMENT);
        assertEquals(1, testSimplifiedXMLStreamReader.getDepth());
        assertFalse(result);
        
        // Test where the specified alternate node is an end node with self-closing tag (<item/>)
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName/></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodName");
        assertEquals(2, testSimplifiedXMLStreamReader.getDepth());
        result = testSimplifiedXMLStreamReader.IsNextNodeStartElement("ABC", XMLStreamConstants.END_ELEMENT);
        assertEquals(1, testSimplifiedXMLStreamReader.getDepth());
        assertFalse(result);
    }
    
    @Test
    public void GetTextNonCharacterNode() throws XMLStreamException {
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>MyMethod</MethodName></MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        try {
            testSimplifiedXMLStreamReader.getText();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("Node read was not a character element."));
        }
    }
    
    @Test
    public void GetTextSuccessTests() throws XMLStreamException {
        SetupTestClass("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation>Test string with &lt;embedded&gt;XML tag&lt;/embedded&gt;</MethodInvocation>");
        testSimplifiedXMLStreamReader.ReadStartElement("MethodInvocation");
        testSimplifiedXMLStreamReader.ReadString();
        String text = testSimplifiedXMLStreamReader.getText();
        assertEquals("Test string with <embedded>XML tag</embedded>", text);
    }
    
    private void SetupTestClass(String inputXml) throws XMLStreamException {
        ByteArrayInputStream inputStream = new ByteArrayInputStream(inputXml.getBytes());
        XMLInputFactory factory = XMLInputFactory.newInstance();
        // Set coalescing property so that text/character elements are are returned in a contiguous block
        factory.setProperty("javax.xml.stream.isCoalescing", true);
        XMLStreamReader streamReader = factory.createXMLStreamReader(inputStream);
        testSimplifiedXMLStreamReader = new SimplifiedXMLStreamReader(streamReader);
    }
}

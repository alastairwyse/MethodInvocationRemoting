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

import javax.xml.stream.*;

/**
 * Adapts the javax.xml.stream.XMLStreamReader class to offer similar methods to those available in the .NET System.Xml.XmlReader class.
 * Allows the code inside class MethodInvocationSerializer to be more consistent with the C# MethodInvocationSerializer.
 * @author Alastair Wyse
 */
public class SimplifiedXMLStreamReader {

    private XMLStreamReader underlyingStreamReader;
    private int depth;
    
    /**
     * Initialises a new instance of the SimplifiedXMLStreamReader class.
     * @param underlyingStreamReader  The stream reader to adapt.
     */
    public SimplifiedXMLStreamReader(XMLStreamReader underlyingStreamReader) {
        this.underlyingStreamReader = underlyingStreamReader;
        depth = 0;
    }
    
    /**
     * Gets the depth of the current node in the XML document.
     * @return The depth of the current node in the XML document.
     */
    public int getDepth() {
        return depth;
    }
    
    /**
     * Returns the text content of the node, if the current node in the XML document is a text node.
     * <b>Note</b> that the property "javax.xml.stream.isCoalescing" must be set to true on the XMLInputFactory that creates the XMLStreamReader which is adapted by this class, for this method to return correctly. 
     * @return                     The text content of the current node.
     * @throws XMLStreamException
     */
    public String getText() throws XMLStreamException {
        String returnValue;
        if(underlyingStreamReader.isCharacters() == true) {
            returnValue = underlyingStreamReader.getText();
        }
        else {
            throw new XMLStreamException("Node read was not a character element.", underlyingStreamReader.getLocation());
        }
        return returnValue;
    }
    
    /**
     * Checks that the current content node is an element with the given name and advances the reader to the next node.
     * @param name                 The qualified name of the element. 
     * @throws XMLStreamException
     */
    public void ReadStartElement(String name) throws XMLStreamException {
        underlyingStreamReader.next();
        if(underlyingStreamReader.getEventType() != XMLStreamConstants.START_ELEMENT) {
            throw new XMLStreamException("Node read was not a start element.", underlyingStreamReader.getLocation());
        }
        if (underlyingStreamReader.getName().getLocalPart() != name) {
            throw new XMLStreamException("Element '" + name + "' was not found.", underlyingStreamReader.getLocation());
        }
        depth = depth + 1;
    }
    
    /**
     * Checks that the current content node is an end tag and advances the reader to the next node.
     * @throws XMLStreamException
     */
    public void ReadEndElement() throws XMLStreamException {
        underlyingStreamReader.next();
        if(underlyingStreamReader.getEventType() != XMLStreamConstants.END_ELEMENT) { 
            throw new XMLStreamException("Node read was not an end element.", underlyingStreamReader.getLocation());
        }
        depth = depth - 1;
    }
    
    /**
     * Checks that the name property of the element found matches the given string before reading a text-only element.
     * @return  The text contained in the element that was read. An empty string if the element is empty (<item></item> or <item/>).
     * @throws XMLStreamException
     */
    public String ReadElementString(String name) throws XMLStreamException {
        String returnString;
        ReadStartElement(name);
        underlyingStreamReader.next();
        if(underlyingStreamReader.getEventType() == XMLStreamConstants.CHARACTERS) { 
            returnString = underlyingStreamReader.getText();
            ReadEndElement();
        }
        else if(underlyingStreamReader.getEventType() == XMLStreamConstants.END_ELEMENT) {
            returnString = "";
            depth = depth - 1;
        }
        else {
            throw new XMLStreamException("Node read was not an end element.", underlyingStreamReader.getLocation());
        }
        
        return returnString;
    }
    
    /**
     * Reads the contents of an element as a string. 
     * @return                     The contents of the element.
     * @throws XMLStreamException
     */
    public String ReadString() throws XMLStreamException {
        String returnString;
        underlyingStreamReader.next();
        if(underlyingStreamReader.getEventType() == XMLStreamConstants.CHARACTERS) { 
            returnString = underlyingStreamReader.getText();
        }
        else {
            throw new XMLStreamException("Node read did not contain characters.", underlyingStreamReader.getLocation());
        }
        
        return returnString;
    }
    
    /**
     * Reads the next node and returns true if the node is a start element and the name of the element matches the parameter.  Returns false if the node is an end element.
     * @param name                 The qualified name of the element. 
     * @return                     Indicates whether the next node is a start element (true) or an end element (false).
     * @throws XMLStreamException
     */
    public Boolean IsNextNodeStartElement(String name) throws XMLStreamException {
        Boolean returnValue;
        
        underlyingStreamReader.next();
        if(underlyingStreamReader.getEventType() == XMLStreamConstants.START_ELEMENT) {
            if(underlyingStreamReader.getName().getLocalPart() == name) {
                depth = depth + 1;
                returnValue = true;
            }
            else {
                throw new XMLStreamException("Element '" + name + "' was not found.", underlyingStreamReader.getLocation());
            }
        }
        else if(underlyingStreamReader.getEventType() == XMLStreamConstants.END_ELEMENT) {
            depth = depth - 1;
            returnValue = false;
        }
        else {
            throw new XMLStreamException("Node read was neither a start nor an end element.", underlyingStreamReader.getLocation());
        }
        
        return returnValue;
    }
    
    /**
     * Reads the next node and returns true if the node is a start element and the name of the element matches the parameter.  Returns false if the node is of the specified alternate type.
     * @param name                             The qualified name of the element. 
     * @param alternateNodeTypeStreamConstant  The type of alternate node that will return false if encountered.  Note that this should be an enumeration of javax.xml.stream.XMLStreamConstants.
     * @return                                 Indicates whether the next node is a start element (true) or the alternate element type (false).
     * @throws XMLStreamException
     */
    public Boolean IsNextNodeStartElement(String name, int alternateNodeTypeStreamConstant) throws XMLStreamException {
        Boolean returnValue;
        underlyingStreamReader.next();
        if(underlyingStreamReader.getEventType() == XMLStreamConstants.START_ELEMENT) {
            if(underlyingStreamReader.getName().getLocalPart() == name) {
                depth = depth + 1;
                returnValue = true;
            }
            else {
                throw new XMLStreamException("Element '" + name + "' was not found.", underlyingStreamReader.getLocation());
            }
        }
        else if(underlyingStreamReader.getEventType() == alternateNodeTypeStreamConstant) {
            if(underlyingStreamReader.getEventType() == XMLStreamConstants.END_ELEMENT) {
                depth = depth - 1;
            }
            returnValue = false;
        }
        else {
            throw new XMLStreamException("Node read was neither a start element nor the specified alternate node type.", underlyingStreamReader.getLocation());
        }
        
        return returnValue;
    }
}

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
 * Defines operations to serialize and deserialize objects to and from XML using a javax.xml.stream.XMLStreamReader (contained in a SimplifiedXMLStreamReader) and XMLStreamWriter.
 * @author Alastair Wyse
 */
public interface IObjectSerializer<T> {

    /**
     * Serializes an object to XML using a javax.xml.stream.XMLStreamWriter.
     * @param inputObject  The object to serialize.
     * @param writer       The XMLStreamWriter to serialize to.
     * @throws Exception   if an error occurs when serializing the object.
     */
    public void Serialize(T inputObject, XMLStreamWriter writer) throws Exception;
    
    /**
     * Deserializes an object from XML contained in a SimplifiedXMLStreamReader.
     * @param reader      The SimplifiedXMLStreamReader to deserialize from.
     * @return            The deserialized object.
     * @throws Exception  if an error occurs when deserializing the object.
     */
    public T Deserialize(SimplifiedXMLStreamReader reader) throws Exception;
}

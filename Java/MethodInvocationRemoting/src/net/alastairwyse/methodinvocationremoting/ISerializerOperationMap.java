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

/**
 * Defines methods to store bi-directional mappings between a native Java class type, the type serialized as a string, and the operations to serialize and deserialize objects of that type.
 * @author Alastair Wyse
 */
public interface ISerializerOperationMap {

    /**
     * Adds a mapping.
     * @param nativeType      The native Java type in the mapping.
     * @param serializedType  A string representation of the type.
     * @param serializer      An IObjectSerializer object which performs serialization and deserialization of the type.
     */
    void AddMapping(Class<?> nativeType, String serializedType, IObjectSerializer<?> serializer);
    
    /**
     * Updates (i.e. replaces) an existing mapping.
     * @param nativeType      The native Java type in the mapping.
     * @param serializedType  A string representation of the type.
     * @param serializer      An IObjectSerializer object which performs serialization and deserialization of the type.
     */
    void UpdateMapping(Class<?> nativeType, String serializedType, IObjectSerializer<?> serializer);
    
    /**
     * Retrieves the string representation of a type, based on the inputted native type.
     * @param nativeType  The native type to return the string representation for.
     * @return            The type represented as a string.  Contains null if no mapping entry was found for the native type.
     */
    String GetSerializedType(Class<?> nativeType);
    
    /**
     * Retrieves a native Java type, based on the inputted string representation of the type.
     * @param serializedType  The string representation of the native type.
     * @return                The corresponding native type.  Contains null if no mapping entry was found for the string representation.
     */
    Class<?> GetNativeType(String serializedType);
    
    /**
     * Retrieves the serializer used to serialize and deserialize the type to and from XML.
     * @param nativeType  The native type to return the serializer for.
     * @return            The corresponding serializer.  Contains null if no mapping entry was found for the native type.
     */
    IObjectSerializer<?> GetSerializer(Class<?> nativeType);
    
    /**
     * Retrieves the serializer used to serialize and deserialize the type to and from XML.
     * @param serializedType  The serialized type to return the serializer for.
     * @return                The corresponding serializer.  Contains null if no mapping entry was found for the serialized type.
     */
    IObjectSerializer<?> GetSerializer(String serializedType);
}

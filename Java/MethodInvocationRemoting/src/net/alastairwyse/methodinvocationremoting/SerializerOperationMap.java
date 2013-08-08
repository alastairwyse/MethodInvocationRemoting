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

import java.util.*;

/**
 * Stores bi-directional mappings between native Java class types, and their serialized string equivalent.
 * @author Alastair Wyse
 */
public class SerializerOperationMap implements ISerializerOperationMap {

    private HashMap<String, NativeTypeSerializerMap> serializedToNativeMap;
    private HashMap<Class<?>, SerializedTypeSerializerMap> nativeToSerializedMap;
    
    /**
     * Initializes a new instance of the SerializerOperationMap class.
     */
    public SerializerOperationMap() {
        serializedToNativeMap = new HashMap<String, NativeTypeSerializerMap>();
        nativeToSerializedMap = new HashMap<Class<?>, SerializedTypeSerializerMap>();
    }
    
    @Override
    public void AddMapping(Class<?> nativeType, String serializedType, IObjectSerializer<?> serializer) {
        // Check for null parameters
        CheckAddUpdateParameters(nativeType, serializedType, serializer);
        
        // Check for attempt to add duplicate entry
        if(serializedToNativeMap.containsKey(serializedType) == true) {
            throw new IllegalArgumentException("The serialized type '" + serializedType + "' already exists in the map.");
        }
        if(nativeToSerializedMap.containsKey(nativeType) == true){
            throw new IllegalArgumentException("The native type '" + nativeType.getName() + "' already exists in the map.");
        }

        // Add the mapping
        serializedToNativeMap.put(serializedType, new NativeTypeSerializerMap(nativeType, serializer));
        nativeToSerializedMap.put(nativeType, new SerializedTypeSerializerMap(serializedType, serializer));
    }

    @Override
    public void UpdateMapping(Class<?> nativeType, String serializedType, IObjectSerializer<?> serializer) {
        // Check for null parameters
        CheckAddUpdateParameters(nativeType, serializedType, serializer);

        // Check that the mapping already exists
        if (nativeToSerializedMap.containsKey(nativeType) == false)
        {
            throw new IllegalArgumentException("The native type '" + nativeType.getName() + "' does not exist in the map.");
        }

        // Remove the existing mapping
        serializedToNativeMap.remove(serializedType);
        nativeToSerializedMap.remove(nativeType);

        // Add the new mapping
        AddMapping(nativeType, serializedType, serializer);
    }
    
    @Override
    public String GetSerializedType(Class<?> nativeType) {
        if (nativeToSerializedMap.containsKey(nativeType) == true)
        {
            return nativeToSerializedMap.get(nativeType).serializedType;
        }
        else
        {
            return null;
        }
    }
    
    @Override
    public Class<?> GetNativeType(String serializedType) {
        if (serializedToNativeMap.containsKey(serializedType) == true)
        {
            return serializedToNativeMap.get(serializedType).nativeType;
        }
        else
        {
            return null;
        }
    }

    @Override
    public IObjectSerializer<?> GetSerializer(Class<?> nativeType) {
        if (nativeToSerializedMap.containsKey(nativeType) == true)
        {
            return nativeToSerializedMap.get(nativeType).objectSerializer;
        }
        else
        {
            return null;
        }
    }

    @Override
    public IObjectSerializer<?> GetSerializer(String serializedType) {
        if (serializedToNativeMap.containsKey(serializedType) == true)
        {
            return serializedToNativeMap.get(serializedType).objectSerializer;
        }
        else
        {
            return null;
        }
    }

    /**
     * Checks the inputted parameters, and throws an exception if any are null.
     * @param nativeType                 The native Java type in the mapping.
     * @param serializedType             A string representation of the type.
     * @param serializer                 An IObjectSerializer object which performs serialization and deserialization of the type.
     * @throws IllegalArgumentException
     */
    private void CheckAddUpdateParameters(Class<?> nativeType, String serializedType, IObjectSerializer<?> serializer) {
        if (nativeType == null)
        {
            throw new IllegalArgumentException("Parameter 'nativeType' cannot be null.");
        }
        if (serializedType == null)
        {
            throw new IllegalArgumentException("Parameter 'serializedType' cannot be null.");
        }
        if (serializer == null)
        {
            throw new IllegalArgumentException("Parameter 'serializer' cannot be null.");
        }
    }
    
    /**
     * Container used to map a native Java class type to a serializer for objects of that type.
     */
    private class NativeTypeSerializerMap {
        
        public Class<?> nativeType;
        public IObjectSerializer<?> objectSerializer;
        
        public NativeTypeSerializerMap(Class<?> nativeType, IObjectSerializer<?> objectSerializer) {
            this.nativeType = nativeType;
            this.objectSerializer = objectSerializer;
        }
    }
    
    /**
     * Container used to map the string representation of a type to a serializer for objects of that type.
     */
    private class SerializedTypeSerializerMap {
        
        public String serializedType;
        public IObjectSerializer<?> objectSerializer;
        
        public SerializedTypeSerializerMap(String serializedType, IObjectSerializer<?> objectSerializer) {
            this.serializedType = serializedType;
            this.objectSerializer = objectSerializer;
        }
    }
}

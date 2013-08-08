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
 * Defines methods which allow for serializing and deserializing of IMethodInvocation objects.
 * @author Alastair Wyse
 */
public interface IMethodInvocationSerializer {

    /**
     * @return                         The serialized representation of a void return value.
     * @throws SerializationException  if an error occurs while serializing the return value.
     */
    public String getVoidReturnValue() throws SerializationException;
    
    /**
     * Serializes a method invocation.
     * @param inputMethodInvocation    The method invocation to serialize.
     * @return                         The serialized method invocation.
     * @throws SerializationException  if an error occurs when serializing the method invocation.
     */
    public String Serialize(IMethodInvocation inputMethodInvocation) throws SerializationException;
    
    /**
     * Deserializes a method invocation.
     * @param serializedMethodInvocation  The serialized method invocation to deserialize.
     * @return                            The method invocation.
     * @throws DeserializationException   if an error occurs when deserializing the method invocation.
     */
    public MethodInvocation Deserialize(String serializedMethodInvocation) throws DeserializationException;
    
    /**
     * Serializes the return value of a method invocation.
     * @param inputReturnValue         The return value to serialize.
     * @return                         The serialized return value.
     * @throws SerializationException  if an error occurs when serializing the return value.
     */
    public String SerializeReturnValue(Object inputReturnValue) throws SerializationException;

    /**
     * Deserializes the return value of a method invocation.
     * @param serializedReturnValue       The serialized return value to deserialize.
     * @return                            The return value.
     * @throws DeserializationException   if an error occurs when deserializing the return value.
     */
    public Object DeserializeReturnValue(String serializedReturnValue) throws DeserializationException;
}

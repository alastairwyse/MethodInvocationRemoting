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
 * The exception that is thrown when deserialization of an object fails.
 * @author Alastair Wyse
 */
public class DeserializationException extends Exception {

    private String serializedObject;
    
    /**
     * @return  The serialized object that caused the current exception.
     */
    public String getSerializedObject() {
        return serializedObject;
    }
    
    /**
     * Initialises a new instance of the DeserializationException class.
     * @param message  The message that describes the error.
     */
    public DeserializationException(String message) {
        super(message);
    }
    
    /**
     * Initialises a new instance of the DeserializationException class.
     * @param message           The message that describes the error.
     * @param serializedObject  The serialized object that caused the current exception.
     */
    public DeserializationException(String message, String serializedObject) {
        super(message);
        this.serializedObject = serializedObject;
    }
    
    /**
     * Initialises a new instance of the DeserializationException class.
     * @param message  The message that describes the error.
     * @param cause    The exception that is the cause of the current exception.
     */
    public DeserializationException(String message, Throwable cause) {
        super(message, cause);
    }
    
    /**
     * Initialises a new instance of the DeserializationException class.
     * @param message           The message that describes the error.
     * @param serializedObject  The serialized object that caused the current exception.
     * @param cause             The exception that is the cause of the current exception.
     */
    public DeserializationException(String message, String serializedObject, Throwable cause) {
        super(message, cause);
        this.serializedObject = serializedObject;
    }
}

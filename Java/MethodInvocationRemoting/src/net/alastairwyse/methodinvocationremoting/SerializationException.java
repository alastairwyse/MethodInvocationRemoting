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
 * The exception that is thrown when serialization of an object fails.
 * @author Alastair Wyse
 */
public class SerializationException extends Exception {

    private Object targetObect;
    
    /**
     * @return  The object which when attempting to serialize caused the current exception.
     */
    public Object getTargetObject() {
        return targetObect;
    }
    
    /**
     * Initialises a new instance of the SerializationException class.
     * @param message  The message that describes the error.
     */
    public SerializationException(String message) {
        super(message);
    }
    
    /**
     * Initialises a new instance of the SerializationException class.
     * @param message       The message that describes the error.
     * @param targetObject  The object which when attempting to serialize caused the current exception.  This should typically contain either a MethodInvocation object, or a method invocation return value.
     */
    public SerializationException(String message, Object targetObject) {
        super(message);
        this.targetObect = targetObject;
    }
    
    /**
     * Initialises a new instance of the SerializationException class.
     * @param message  The message that describes the error.
     * @param cause    The exception that is the cause of the current exception.
     */
    public SerializationException(String message, Throwable cause) {
        super(message, cause);
    }
    
    /**
     * Initialises a new instance of the SerializationException class.
     * @param message       The message that describes the error.
     * @param targetObject  The object which when attempting to serialize caused the current exception.  This should typically contain either a MethodInvocation object, or a method invocation return value.
     * @param cause         The exception that is the cause of the current exception.
     */
    public SerializationException(String message, Object targetObject, Throwable cause) {
        super(message, cause);
        this.targetObect = targetObject;
    }
}

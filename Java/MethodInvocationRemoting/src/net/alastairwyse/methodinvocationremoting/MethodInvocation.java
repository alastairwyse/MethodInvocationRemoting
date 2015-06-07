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
 * Container for properties of a method and the parameters used when the method is called or invoked.
 * @author Alastair Wyse
 */
public class MethodInvocation implements IMethodInvocation {

    private String name;
    private Object[] parameters;
    private Class<?> returnType;
    
    /**
     * Initializes a new instance of the MethodInvocation class.
     * @param methodName        The name of the method.
     * @param methodParameters  The parameters sent when the method is invoked.
     * @param methodReturnType  The return type of the method.
     */
    public MethodInvocation(String methodName, Object[] methodParameters, Class<?> methodReturnType) {
        this(methodName, methodParameters);
        returnType = methodReturnType;
    }
    
    /**
     * Initializes a new instance of the MethodInvocation class.  Should be used for methods with a void return type.
     * @param methodName        The name of the method.
     * @param methodParameters  The parameters sent when the method is invoked.
     */
    public MethodInvocation(String methodName, Object[] methodParameters) {
        this(methodName);
        CheckParametersSize(methodParameters);
        parameters = methodParameters;
    }
    
    /**
     * Initializes a new instance of the MethodInvocation class.  Should be used for parameterless methods.
     * @param methodName        The name of the method.
     * @param methodReturnType  The return type of the method.
     */
    public MethodInvocation(String methodName, Class<?> methodReturnType) {
        this(methodName);
        returnType = methodReturnType;
    }
    
    /**
     * Initializes a new instance of the MethodInvocation class.  Should be used for parameterless methods, with a void return type.
     * @param methodName  The name of the method.
     */
    public MethodInvocation(String methodName) {
        CheckName(methodName);
        name = methodName;
    }

    public String getName() {
        return name;
    }

    public Object[] getParameters() {
        return parameters;
    }

    public Class<?> getReturnType() {
        return returnType;
    }
    
    /**
     * Throws an exception if the inputted method parameters array has zero length.
     * @param methodParameters  The method parameters array to check.
     */
    private void CheckParametersSize(Object[] methodParameters) {
        if (methodParameters.length == 0) {
            throw new IllegalArgumentException("The method invocation parameters cannot be empty.");
        }
    }
    
    /**
     * Throws an exception if the inputted method name contains no non-whitespace characters.
     * @param name  Throws an exception if the inputted method name contains no non-whitespace characters.
     */
    private void CheckName(String name) {
        if (name.trim().isEmpty() == true) {
            throw new IllegalArgumentException("The method name cannot be blank.");
        }
    }
}

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
 * Defines methods to send method invocations (represented by IMethodInvocation objects) to remote locations.
 * @author Alastair Wyse
 */
public interface IMethodInvocationRemoteSender {

    /**
     * Invokes a method remotely.
     * @param inputMethodInvocation  The method to invoke.
     * @return                       The value returned from invoking the method.
     * @throws Exception             if an error occurs when invoking the method.
     */
    public Object InvokeMethod(IMethodInvocation inputMethodInvocation) throws Exception;
    
    /**
     * Invokes a void method remotely.
     * @param inputMethodInvocation  The method to invoke.
     * @throws Exception             if an error occurs when invoking the void method.  
     */
    public void InvokeVoidMethod(IMethodInvocation inputMethodInvocation)  throws Exception;
}

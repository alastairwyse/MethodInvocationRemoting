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
 * Encapsulates a method that handles when a method invocation is received.
 * @author Alastair Wyse
 */
public interface IMethodInvocationReceivedEventHandler {

    /**
     * Handles when a method invocation is received.
     * @param source                    The receiver object which received the method invocation.
     * @param receivedMethodInvocation  The method invocation received.
     */
    public void MethodInvocationReceived(IMethodInvocationRemoteReceiver source, IMethodInvocation receivedMethodInvocation);
    
    /**
     * Handles the case that an exception occurs whilst receiving a method invocation.
     * @param source  The receiver object which threw the exception.
     * @param e       The exception thrown.
     */
    public void MethodInvocationReceiveException(IMethodInvocationRemoteReceiver source, Exception e);
}

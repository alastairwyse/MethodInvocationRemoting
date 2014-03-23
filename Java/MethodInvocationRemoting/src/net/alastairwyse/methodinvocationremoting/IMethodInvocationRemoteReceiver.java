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
 * Defines methods to receive method invocations (represented by IMethodInvocation objects) from remote locations.
 * @author Alastair Wyse
 */
public interface IMethodInvocationRemoteReceiver {

    /**
     * Sets the object which should handle received method invocations.
     * @param receivedEventHandler  The object to handle received method invocations.
     */
    public void setReceivedEventHandler(IMethodInvocationReceivedEventHandler receivedEventHandler);
    
    /**
     * Starts an asynchronous operation to receive method invocations.  Clients should assign the object which handles the received method invocations using method setReceivedEventHandler.
     * @throws Exception  if the received event handler has not been set, or an error occurs when receiving a method invocation.
     */
    public void Receive() throws Exception;
    
    /**
     * Passes the return value of the method invocation to the sender, after the method invocation has been completed.
     * @param returnValue  The return value.
     * @throws Exception   if an error occurs when sending the return value.
     */
    public void SendReturnValue(Object returnValue) throws Exception;
    
    /**
     * Notifies the sender that the method invocation has been completed, in the case of a void method.
     * @throws Exception  if an error occurs when sending the void return value.
     */
    public void SendVoidReturn() throws Exception;
    
    /**
     * Stops the operation to receive method invocations.
     * @throws Exception  if an error occurs when attempting to cancel the receive operation.
     */
    public void CancelReceive() throws Exception;
}

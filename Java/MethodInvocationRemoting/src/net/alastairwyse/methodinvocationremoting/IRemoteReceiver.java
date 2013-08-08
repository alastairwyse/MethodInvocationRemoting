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
 * Defines methods which allow for receiving messages from a remote location.
 * @author Alastair Wyse
 */
public interface IRemoteReceiver {

    /**
     * Receives a message.
     * Receive operation should be synchronous, i.e. should not return control to the client until a message has been received.  If a message is not available, the receive operation should wait (block) until a message becomes available, and then receive it.
     * @return            The message received.
     * @throws Exception  if an error occurs when attempting to receive a message.
     */
    String Receive() throws Exception;
    
    /**
     * Cancels the receive operation in the case it is waiting for a message.
     * Must be called by a separate thread to the thread that called Receive().
     */
    void CancelReceive();
}

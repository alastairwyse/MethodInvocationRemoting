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

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MethodInvocationRemoting
{
    //******************************************************************************
    //
    // Class: MethodInvocationRemoteReceiver
    //
    //******************************************************************************
    /// <summary>
    /// Receives method invocations (represented by MethodInvocationRemoting.IMethodInvocation objects) from remote locations.
    /// </summary>
    public class MethodInvocationRemoteReceiver : IMethodInvocationRemoteReceiver
    {
        private IMethodInvocationSerializer serializer;
        private IRemoteSender sender;
        private IRemoteReceiver receiver;
        private Thread receiveLoopThread;
        private volatile bool cancelRequest;

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="E:MethodInvocationRemoting.IMethodInvocationRemoteReceiver.MethodInvocationReceived"]/*'/>
        public event MethodInvocationReceivedEventHandler MethodInvocationReceived;

        //------------------------------------------------------------------------------
        //
        // Method: MethodInvocationRemoteReceiver (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MethodInvocationRemoteReceiver class.
        /// </summary>
        /// <param name="serializer">Object to use to deserialize method invocations.</param>
        /// <param name="sender">Object to use to send serialized method invocation return values.</param>
        /// <param name="receiver">Object to use to receive serialized method invocations.</param>
        public MethodInvocationRemoteReceiver(IMethodInvocationSerializer serializer, IRemoteSender sender, IRemoteReceiver receiver)
        {
            this.serializer = serializer;
            this.sender = sender;
            this.receiver = receiver;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationRemoteReceiver.Receive"]/*'/>
        public void Receive()
        {
            cancelRequest = false;

            receiveLoopThread = new Thread(delegate()
            {
                while (cancelRequest == false)
                {
                    try
                    {
                        string serializedMethodInvocation = receiver.Receive();
                        if (serializedMethodInvocation != "")
                        {
                            IMethodInvocation receivedMethodInvocation = serializer.Deserialize(serializedMethodInvocation);
                            MethodInvocationReceived(this, new MethodInvocationReceivedEventArgs(receivedMethodInvocation));
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Failed to invoke method.", e);
                    }
                }
            });
            receiveLoopThread.Start();
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationRemoteReceiver.SendReturnValue(System.Object)"]/*'/>
        public void SendReturnValue(object ReturnValue)
        {
            try
            {
                string serializedReturnValue = serializer.SerializeReturnValue(ReturnValue);
                sender.Send(serializedReturnValue);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to send return value.", e);
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationRemoteReceiver.SendVoidReturn"]/*'/>
        public void SendVoidReturn()
        {
            try
            {
                sender.Send(serializer.VoidReturnValue);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to send void return value.", e);
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationRemoteReceiver.CancelReceive"]/*'/>
        public void CancelReceive()
        {
            cancelRequest = true;
            receiver.CancelReceive();
            receiveLoopThread.Join();
        }
    }
}

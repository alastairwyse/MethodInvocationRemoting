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

namespace MethodInvocationRemoting
{
    /// <summary>
    /// Represents a method that will handle an event when a method invocation is received.
    /// </summary>
    /// <param name="sender">The object which raised the event.</param>
    /// <param name="e">A MethodInvocationRemoting.MethodInvocationReceivedEventArgs object which contains the method invocation represented as a MethodInvocationRemoting.IMethodInvocation object.</param>
    public delegate void MethodInvocationReceivedEventHandler(object sender, MethodInvocationReceivedEventArgs e);

    //******************************************************************************
    //
    // Interface: IMethodInvocationRemoteReceiver
    //
    //******************************************************************************
    /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="T:MethodInvocationRemoting.IMethodInvocationRemoteReceiver"]/*'/>
    public interface IMethodInvocationRemoteReceiver
    {
        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="E:MethodInvocationRemoting.IMethodInvocationRemoteReceiver.MethodInvocationReceived"]/*'/>
        event MethodInvocationReceivedEventHandler MethodInvocationReceived;

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationRemoteReceiver.Receive"]/*'/>
        void Receive();

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationRemoteReceiver.SendReturnValue(System.Object)"]/*'/>
        void SendReturnValue(object ReturnValue);

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationRemoteReceiver.SendVoidReturn"]/*'/>
        void SendVoidReturn();

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationRemoteReceiver.CancelReceive"]/*'/>
        void CancelReceive();
    }
}

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
    //******************************************************************************
    //
    // Interface: IMethodInvocationSerializer
    //
    //******************************************************************************
    /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="T:MethodInvocationRemoting.IMethodInvocationSerializer"]/*'/>
    public interface IMethodInvocationSerializer
    {
        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="P:MethodInvocationRemoting.IMethodInvocationSerializer.VoidReturnValue"]/*'/>
        string VoidReturnValue
        {
            get;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationSerializer.Serialize(MethodInvocationRemoting.IMethodInvocation)"]/*'/>
        string Serialize(IMethodInvocation inputMethodInvocation);

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationSerializer.Deserialize(System.String)"]/*'/>
        MethodInvocation Deserialize(string serializedMethodInvocation);

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationSerializer.SerializeReturnValue(System.Object)"]/*'/>
        string SerializeReturnValue(object inputReturnValue);

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationSerializer.DeserializeReturnValue(System.String)"]/*'/>
        object DeserializeReturnValue(string serializedReturnValue);
    }
}

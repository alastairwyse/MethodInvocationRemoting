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

namespace OperatingSystemAbstraction
{
    //******************************************************************************
    //
    // Interface: IFile
    //
    //******************************************************************************
    /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="T:OperatingSystemAbstraction.IFileSystem"]/*'/>
    public interface IFileSystem
    {
        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:OperatingSystemAbstraction.IFileSystem.CheckFileExists(System.String)"]/*'/>
        bool CheckFileExists(string path);

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:OperatingSystemAbstraction.IFileSystem.DeleteFile(System.String)"]/*'/>
        void DeleteFile(string path);
    }
}

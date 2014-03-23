/*
 * Copyright 2014 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
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

namespace ApplicationLogging
{
    //******************************************************************************
    //
    // Interface: IApplicationLogger
    //
    //******************************************************************************
    /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="T:ApplicationLogging.IApplicationLogger"]/*'/>
    public interface IApplicationLogger
    {
        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(ApplicationLogging.LogLevel,System.String)"]/*'/>
        void Log(LogLevel level, string text);

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(System.Object,ApplicationLogging.LogLevel,System.String)"]/*'/>
        void Log(object source, LogLevel level, string text);

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(System.Int32,ApplicationLogging.LogLevel,System.String)"]/*'/>
        void Log(int eventIdentifier, LogLevel level, string text);

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(System.Object,System.Int32,ApplicationLogging.LogLevel,System.String)"]/*'/>
        void Log(object source, int eventIdentifier, LogLevel level, string text);

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(ApplicationLogging.LogLevel,System.String,System.Exception)"]/*'/>
        void Log(LogLevel level, string text, Exception sourceException);

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(System.Object,ApplicationLogging.LogLevel,System.String,System.Exception)"]/*'/>
        void Log(object source, LogLevel level, string text, Exception sourceException);

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(System.Int32,ApplicationLogging.LogLevel,System.String,System.Exception)"]/*'/>
        void Log(int eventIdentifier, LogLevel level, string text, Exception sourceException);

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationLogging.IApplicationLogger.Log(System.Object,System.Int32,ApplicationLogging.LogLevel,System.String,System.Exception)"]/*'/>
        void Log(object source, int eventIdentifier, LogLevel level, string text, Exception sourceException);
    }
}

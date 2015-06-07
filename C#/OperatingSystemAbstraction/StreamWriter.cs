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

namespace OperatingSystemAbstraction
{   
    //******************************************************************************
    //
    // Class: StreamWriter
    //
    //******************************************************************************
    /// <summary>
    /// Provides an abstraction of the System.IO.StreamWriter class, to facilitate mocking and unit testing.
    /// </summary>
    public class StreamWriter : IStreamWriter
    {
        protected bool disposed;
        private System.IO.StreamWriter streamWriter;

        //******************************************************************************
        //
        // Method: StreamWriter (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the OperatingSystemAbstraction.StreamWriter class.
        /// </summary>
        /// <param name="path">The complete file path to write to.</param>
        /// <param name="append">true to append data to the file; false to overwrite the file. If the specified file does not exist, this parameter has no effect, and the constructor creates a new file.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public StreamWriter(string path, bool append, Encoding encoding)
        {
            streamWriter = new System.IO.StreamWriter(path, append, encoding);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:OperatingSystemAbstraction.IStreamWriter.Close"]/*'/>
        public void Close()
        {
            streamWriter.Close();
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:OperatingSystemAbstraction.IStreamWriter.Flush"]/*'/>
        public void Flush()
        {
            streamWriter.Flush();
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:OperatingSystemAbstraction.IStreamWriter.WriteLine(System.String)"]/*'/>
        public void WriteLine(string value)
        {
            streamWriter.WriteLine(value);
        }

        #region Finalize / Dispose Methods

        /// <summary>
        /// Releases the unmanaged resources used by the StreamWriter.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #pragma warning disable 1591
        ~StreamWriter()
        {
            Dispose(false);
        }
        #pragma warning restore 1591

        //******************************************************************************
        //
        // Method: Dispose
        //
        //******************************************************************************
        /// <summary>
        /// Provides a method to free unmanaged resources used by this class.
        /// </summary>
        /// <param name="disposing">Whether the method is being called as part of an explicit Dispose routine, and hence whether managed resources should also be freed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                    if (streamWriter != null)
                    {
                        streamWriter.Dispose();
                        streamWriter = null;
                    }
                }
                // Free your own state (unmanaged objects).
                
                // Set large fields to null.

                disposed = true;
            }
        }

        #endregion
    }
}

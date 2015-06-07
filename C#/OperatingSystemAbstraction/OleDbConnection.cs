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
    // Class: OleDbConnection
    //
    //******************************************************************************
    /// <summary>
    /// Provides an abstraction of the System.Data.OleDb.OleDbConnection class, to facilitate mocking and unit testing.
    /// </summary>
    public class OleDbConnection : IOleDbConnection
    {
        protected bool disposed;
        private System.Data.OleDb.OleDbConnection oleDbConnection;


        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="P:OperatingSystemAbstraction.IOleDbConnection.ConnectionString"]/*'/>
        public string ConnectionString
        {
            get
            {
                return oleDbConnection.ConnectionString;
            }
            set
            {
                oleDbConnection.ConnectionString = value;
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="P:OperatingSystemAbstraction.IOleDbConnection.State"]/*'/>
        public System.Data.ConnectionState State
        {
            get
            {
                return oleDbConnection.State;
            }
        }

        /// <summary>
        /// The connection underlying the abstraction.
        /// </summary>
        public System.Data.OleDb.OleDbConnection Connection
        {
            get
            {
                return oleDbConnection;
            }
        }


        //------------------------------------------------------------------------------
        //
        // Method: OleDbConnection (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the OperatingSystemAbstraction.OleDbConnection class.
        /// </summary>
        public OleDbConnection()
        {
            oleDbConnection = new System.Data.OleDb.OleDbConnection();
            disposed = false;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:OperatingSystemAbstraction.IOleDbConnection.Open"]/*'/>
        public void Open()
        {
            oleDbConnection.Open();
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:OperatingSystemAbstraction.IOleDbConnection.Close"]/*'/>
        public void Close()
        {
            oleDbConnection.Close();
        }

        #region Finalize / Dispose Methods

        /// <summary>
        /// Releases the unmanaged resources used by the OleDbConnection.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #pragma warning disable 1591
        ~OleDbConnection()
        {
            Dispose(false);
        }
        #pragma warning restore 1591

        //------------------------------------------------------------------------------
        //
        // Method: Dispose
        //
        //------------------------------------------------------------------------------
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
                }
                // Free your own state (unmanaged objects).
                if (oleDbConnection != null)
                {
                    oleDbConnection.Close();
                    oleDbConnection.Dispose();
                }
                // Set large fields to null.
                oleDbConnection = null;

                disposed = true;
            }
        }

        #endregion
    }
}

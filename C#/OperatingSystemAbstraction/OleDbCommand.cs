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
    // Class: OleDbCommand
    //
    //******************************************************************************
    /// <summary>
    /// Provides an abstraction of the System.Data.OleDb.OleDbCommand class, to facilitate mocking and unit testing.
    /// </summary>
    public class OleDbCommand : IOleDbCommand
    {
        protected bool disposed;
        private System.Data.OleDb.OleDbCommand oleDbCommand;

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="P:OperatingSystemAbstraction.IOleDbConnection.CommandText"]/*'/>
        public string CommandText
        {
            get
            {
                return oleDbCommand.CommandText;
            }
            set
            {
                oleDbCommand.CommandText = value;
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: OleDbCommand (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the OperatingSystemAbstraction.OleDbCommand class.
        /// </summary>
        public OleDbCommand()
        {
            oleDbCommand = new System.Data.OleDb.OleDbCommand();
            disposed = false;
        }

        //------------------------------------------------------------------------------
        //
        // Method: OleDbCommand (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the OperatingSystemAbstraction.OleDbCommand class.
        /// </summary>
        /// <param name="cmdText">The text of the query.</param>
        /// <param name="connection">An OleDbConnection that represents the connection to a data source.</param>
        public OleDbCommand(String cmdText, System.Data.OleDb.OleDbConnection connection)
        {
            oleDbCommand = new System.Data.OleDb.OleDbCommand(cmdText, connection);
            disposed = false;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:OperatingSystemAbstraction.IOleDbConnection.ExecuteNonQuery"]/*'/>
        public int ExecuteNonQuery()
        {
            return oleDbCommand.ExecuteNonQuery();
        }

        #region Finalize / Dispose Methods

        /// <summary>
        /// Releases the unmanaged resources used by the OleDbCommand.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~OleDbCommand()
        {
            Dispose(false);
        }

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
                if (oleDbCommand != null)
                {
                    oleDbCommand.Dispose();
                }
                // Set large fields to null.
                oleDbCommand = null;

                disposed = true;
            }
        }

        #endregion
    }
}

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
using OperatingSystemAbstraction;

namespace MethodInvocationRemoting
{
    //******************************************************************************
    //
    // Class: FileRemoteReceiver
    //
    //******************************************************************************
    /// <summary>
    /// Receives messages from a remote location via the file system.
    /// </summary>
    public class FileRemoteReceiver : IRemoteReceiver, IDisposable
    {
        private IFile messageFile;
        private IFileSystem fileSystem;
        private string messageFilePath;
        private string lockFilePath;
        private int readLoopTimeout;
        private volatile bool waitingForTimeout = false;
        private volatile bool cancelRequest;
        /// <summary>
        /// Indicates whether the object has been disposed.
        /// </summary>
        protected bool disposed;

        //******************************************************************************
        //
        // Method: FileRemoteReceiver (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.FileRemoteReceiver class.
        /// </summary>
        /// <param name="messageFilePath">The full path of the file used to receive messages.</param>
        /// <param name="lockFilePath">The full path of the file used to indicate when the message file is locked for writing.</param>
        /// <param name="readLoopTimeout">The time to wait between attempts to read the file in milliseconds.</param>
        public FileRemoteReceiver(string messageFilePath, string lockFilePath, int readLoopTimeout)
        {
            if (messageFile == null)
            {
                messageFile = new File(messageFilePath);
            }
            if (fileSystem == null)
            {
                fileSystem = new FileSystem();
            }
            this.messageFilePath = messageFilePath;
            this.lockFilePath = lockFilePath;
            this.readLoopTimeout = readLoopTimeout;
            disposed = false;
        }

        //******************************************************************************
        //
        // Method: FileRemoteReceiver (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.FileRemoteReceiver class.  Note this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
        /// </summary>
        /// <param name="messageFilePath">The full path of the file used to receive messages.</param>
        /// <param name="lockFilePath">The full path of the file used to indicate when the message file is locked for writing.</param>
        /// <param name="readLoopTimeout">The time to wait between attempts to read the file in milliseconds.</param>
        /// <param name="messageFile">A test (mock) message file.</param>
        /// <param name="fileSystem">A test (mock) file system.</param>
        public FileRemoteReceiver(string messageFilePath, string lockFilePath, int readLoopTimeout, IFile messageFile, IFileSystem fileSystem)
            : this(messageFilePath, lockFilePath, readLoopTimeout)
        {
            this.messageFile = messageFile;
            this.fileSystem = fileSystem;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IRemoteReceiver.Receive"]/*'/>
        public string Receive()
        {
            CheckNotDisposed();
            string returnMessage = "";
            cancelRequest = false;

            try
            {
                while (cancelRequest == false)
                {
                    if (fileSystem.CheckFileExists(messageFilePath) == true)
                    {
                        if (fileSystem.CheckFileExists(lockFilePath) == false)
                        {
                            returnMessage = messageFile.ReadAll();
                            fileSystem.DeleteFile(messageFilePath);
                            break;
                        }
                    }
                    else
                    {
                        waitingForTimeout = true;
                        if (readLoopTimeout > 0)
                        {
                            System.Threading.Thread.Sleep(readLoopTimeout);
                        }
                        waitingForTimeout = false;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error receiving message.", e);
            }

            return returnMessage;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IRemoteReceiver.CancelReceive"]/*'/>
        public void CancelReceive()
        {
            cancelRequest = true;
            while (waitingForTimeout == true) ;
        }

        #region Finalize / Dispose Methods

        /// <summary>
        /// Releases the unmanaged resources used by the FileRemoteReceiver.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~FileRemoteReceiver()
        {
            Dispose(false);
        }

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
                }
                // Free your own state (unmanaged objects).
                messageFile.Dispose();
                // Set large fields to null.
                messageFilePath = null;
                lockFilePath = null;
                disposed = true;
            }
        }

        //******************************************************************************
        //
        // Method: CheckNotDisposed
        //
        //******************************************************************************
        /// <summary>
        /// Throws an exception if the disposed property is true.
        /// </summary>
        protected void CheckNotDisposed()
        {
            if (disposed == true)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        #endregion
    }
}

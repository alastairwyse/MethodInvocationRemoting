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
using Apache.NMS;
using Apache.NMS.Util;
using Apache.NMS.ActiveMQ;

namespace MethodInvocationRemoting
{
    //******************************************************************************
    //
    // Class: ActiveMqRemoteConnectionBase
    //
    //******************************************************************************
    /// <summary>
    /// Provides common connection functionality for classes connecting to an Apache ActiveMQ message broker.
    /// </summary>
    public abstract class ActiveMqRemoteConnectionBase
    {
        /// <summary>Used in the properties of a message to identify the message filter.</summary>
        protected const string filterIdentifier = "Filter";
        /// <summary>Indicates whether the object has been disposed.</summary>
        protected bool disposed;

        /// <summary>Uniform resource identifier of the ActiveMQ broker to connect to.</summary>
        protected string connectUriName;
        /// <summary>The name of the queue to connect to.</summary>
        protected string queueName;
        /// <summary>The value of the message filter.</summary>
        protected string messageFilter;
        /// <summary>Object representation of the uniform resource identifier of the ActiveMQ broker to connect to.</summary>
        protected Uri connectUri;
        /// <summary>The NMS connection factory to use when connecting the ActiveMQ broker.</summary>
        protected IConnectionFactory connectionFactory;
        /// <summary>The NMS connection to use when connecting the ActiveMQ broker.</summary>
        protected IConnection connection;
        /// <summary>The NMS session to use when connecting the ActiveMQ broker.</summary>
        protected ISession session;
        /// <summary>The NMS destination to use when connecting the ActiveMQ broker.</summary>
        protected IDestination destination;
        /// <summary>Indicates whether the object is currently connected to the ActiveMQ broker.</summary>
        protected bool connected;
        /// <summary>Indicates that the object was instantiated using the test constructor.</summary>
        protected bool testConstructor; 

        //------------------------------------------------------------------------------
        //
        // Get / set methods
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether the object is currently connected to a remote queue.
        /// </summary>
        public bool Connected
        {
            get
            {
                return connected;
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: ActiveMqRemoteConnectionBase (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.ActiveMqRemoteSender class.
        /// </summary>
        /// <param name="connectUriName">The uniform resource identifier of the ActiveMQ broker to connect to.</param>
        /// <param name="queueName">The name of the queue to connect to.</param>
        /// <param name="messageFilter">The filter to apply to the queue.  Allows multiple remote senders and receivers to use the same queue by each applying their own unique filter.</param>
        protected ActiveMqRemoteConnectionBase(string connectUriName, string queueName, string messageFilter)
        {
            disposed = false;
            connected = false;
            testConstructor = false;

            this.connectUriName = connectUriName;
            this.queueName = queueName;
            this.messageFilter = messageFilter;
        }

        //------------------------------------------------------------------------------
        //
        // Method: ActiveMqRemoteConnectionBase (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.ActiveMqRemoteSender class.  Note this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
        /// </summary>
        /// <param name="connectUriName">The uniform resource identifier of the ActiveMQ broker to connect to.</param>
        /// <param name="queueName">The name of the queue to connect to.</param>
        /// <param name="messageFilter">The filter to apply to the queue.  Allows multiple remote senders and receivers to use the same queue by each applying their own unique filter.</param>
        /// <param name="testConnectionFactory">A test (mock) NMS connection factory.</param>
        /// <param name="testConnection">A test (mock) NMS connection.</param>
        /// <param name="testSession">A test (mock) NMS session.</param>
        /// <param name="testDestination">A test (mock) NMS destination.</param>
        protected ActiveMqRemoteConnectionBase(string connectUriName, string queueName, string messageFilter, IConnectionFactory testConnectionFactory, IConnection testConnection, ISession testSession, IDestination testDestination) 
            :this(connectUriName, queueName, messageFilter)
        {
            testConstructor = true;
            connectionFactory = testConnectionFactory;
            connection = testConnection;
            session = testSession;
            destination = testDestination;
        }

        //------------------------------------------------------------------------------
        //
        // Method: Connect
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Connects to the message queue.
        /// </summary>
        public virtual void Connect()
        {
            CheckNotDisposed();
            if (Connected == true)
            {
                throw new Exception("Connection to message queue has already been opened.");
            }

            try
            {
                connectUri = new Uri(connectUriName);
                if (testConstructor == false)
                {
                    connectionFactory = new ConnectionFactory(connectUri);
                    connection = connectionFactory.CreateConnection();
                    session = connection.CreateSession();
                    destination = SessionUtil.GetDestination(session, "queue://" + queueName);
                }
                connection.Start();
                connected = true;
            }
            catch (Exception e)
            {
                throw new Exception("Error connecting to message queue.", e);
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: Disconnect
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Disconnects from the message queue.
        /// </summary>
        public virtual void Disconnect()
        {
            CheckNotDisposed();
            if (connected == true)
            {
                session.Close();
                connection.Stop();
                connection.Close();
                connected = false;
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: CheckConnectionOpen
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Throws an exception if the object is currently not in a connected state.
        /// </summary>
        protected void CheckConnectionOpen()
        {
            if (connected == false)
            {
                throw new Exception("Connection to message queue is not open.");
            }
        }

        #region Finalize / Dispose Methods

        /// <summary>
        /// Releases the unmanaged resources used by the ActiveMqRemoteConnectionBase.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        #pragma warning disable 1591
        ~ActiveMqRemoteConnectionBase()
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
                DisposeIfNotNull(session);
                DisposeIfNotNull(connection);
                // Set large fields to null.
                connected = false;
                disposed = true;
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: CheckNotDisposed
        //
        //------------------------------------------------------------------------------
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

        //------------------------------------------------------------------------------
        //
        // Method: DisposeIfNotNull
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Disposes the inputted object if it has been instantiated.
        /// </summary>
        /// <param name="disposableObject">The object to attempt to dispose.</param>
        protected void DisposeIfNotNull(IDisposable disposableObject)
        {
            if (disposableObject != null)
            {
                disposableObject.Dispose();
            }
        }

        #endregion
    }
}
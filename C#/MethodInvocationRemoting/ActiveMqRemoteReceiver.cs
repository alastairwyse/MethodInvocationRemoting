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
    // Class: ActiveMqRemoteReceiver
    //
    //******************************************************************************
    /// <summary>
    /// Receives messages from a remote location via Apache ActiveMQ.
    /// </summary>
    public class ActiveMqRemoteReceiver : ActiveMqRemoteConnectionBase, IRemoteReceiver, IDisposable
    {
        private IMessageConsumer consumer;
        private int connectLoopTimeout;
        private volatile bool cancelRequest;
        private volatile bool waitingForMessage = false;

        //******************************************************************************
        //
        // Method: ActiveMqRemoteReceiver (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.ActiveMqRemoteReceiver class.
        /// </summary>
        /// <param name="connectUriName">The unform resource identifier of the ActiveMQ broker to connect to.</param>
        /// <param name="queueName">The name of the queue to connect to.</param>
        /// <param name="messageFilter">The filter to apply to the queue.  Allows multiple remote senders and receivers to use the same queue by each applying their own unique filter.</param>
        /// <param name="connectLoopTimeout">The time to wait for a message before retrying in milliseconds.</param>
        public ActiveMqRemoteReceiver(string connectUriName, string queueName, string messageFilter, int connectLoopTimeout) 
            : base(connectUriName, queueName, messageFilter)
        {
            this.connectLoopTimeout = connectLoopTimeout;
        }

        //******************************************************************************
        //
        // Method: ActiveMqRemoteReceiver (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.ActiveMqRemoteReceiver class.  Note this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
        /// </summary>
        /// <param name="connectUriName">The uniform resource identifier of the ActiveMQ broker to connect to.</param>
        /// <param name="queueName">The name of the queue to connect to.</param>
        /// <param name="messageFilter">The filter to apply to the queue.  Allows multiple remote senders and receivers to use the same queue by each applying their own unique filter.</param>
        /// <param name="connectLoopTimeout">The time to wait for a message before retrying in milliseconds.</param>
        /// <param name="testConnectionFactory">A test (mock) NMS connection factory.</param>
        /// <param name="testConnection">A test (mock) NMS connection.</param>
        /// <param name="testSession">A test (mock) NMS session.</param>
        /// <param name="testDestination">A test (mock) NMS destination.</param>
        /// <param name="testConsumer">A test (mock) NMS message consumer.</param>
        public ActiveMqRemoteReceiver(string connectUriName, string queueName, string messageFilter, int connectLoopTimeout, IConnectionFactory testConnectionFactory, IConnection testConnection, ISession testSession, IDestination testDestination, IMessageConsumer testConsumer) 
            :base(connectUriName, queueName, messageFilter, testConnectionFactory, testConnection, testSession, testDestination) 
        {
            this.connectLoopTimeout = connectLoopTimeout;
            consumer = testConsumer;
        }

        //******************************************************************************
        //
        // Method: Connect
        //
        //******************************************************************************
        /// <summary>
        /// Connects to the message queue.
        /// </summary>
        public override void Connect()
        {
            base.Connect();
            try
            {
                if (testConstructor == false)
                {
                    consumer = session.CreateConsumer(destination, filterIdentifier + " = '" + messageFilter + "'");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error creating message consumer.", e);
            }
        }

        //******************************************************************************
        //
        // Method: Disconnect
        //
        //******************************************************************************
        /// <summary>
        /// Disconnects from the message queue.
        /// </summary>
        public override void Disconnect()
        {
            CheckNotDisposed();
            if (connected == true)
            {
                consumer.Close();
                base.Disconnect();
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IRemoteReceiver.Receive"]/*'/>
        public string Receive()
        {
            string returnMessage = "";
            cancelRequest = false;

            CheckNotDisposed();
            CheckConnectionOpen();

            try
            {
                IMessage receivedMessage;
                while (cancelRequest == false)
                {
                    waitingForMessage = true;
                    receivedMessage = consumer.Receive(new TimeSpan(0, 0, 0, 0, connectLoopTimeout));
                    waitingForMessage = false;
                    if (receivedMessage != null)
                    {
                        returnMessage = (string)receivedMessage.Properties.GetString("Text");
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error receiving message.", e);
            }
            finally
            {
                waitingForMessage = false;
            }

            return returnMessage;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IRemoteReceiver.CancelReceive"]/*'/>
        public void CancelReceive()
        {
            cancelRequest = true;
            while (waitingForMessage == true);
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
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                }
                // Free your own state (unmanaged objects).
                DisposeIfNotNull(consumer);
            }
            // Call Dispose in the base class.
            base.Dispose(disposing);
        }
    }
}

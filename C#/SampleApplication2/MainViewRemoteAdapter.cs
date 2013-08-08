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
using MethodInvocationRemoting;

namespace SampleApplication2
{
    //******************************************************************************
    //
    // Class: MainViewRemoteAdapter
    //
    //******************************************************************************
    /// <summary>
    /// Forwards operations on a main view to a remote location using the MethodInvocationRemoting framework and Apache ActiveMQ.
    /// </summary>
    public class MainViewRemoteAdapter : IMainView
    {
        private IContactListPresenter presenter;
        // Objects for sending method invocations
        private MethodInvocationRemoteSender methodInvocationSender;
        private ActiveMqRemoteSender outgoingSender;
        private ActiveMqRemoteReceiver outgoingReceiver;
        private MethodInvocationSerializer outgoingMethodSerializer;
        // Objects for receiving method invocations
        private MethodInvocationRemoteReceiver methodInvocationReceiver;
        private ActiveMqRemoteSender incomingSender;
        private ActiveMqRemoteReceiver incomingReceiver;
        private MethodInvocationSerializer incomingMethodSerializer;

        //------------------------------------------------------------------------------
        //
        // Method: MainViewRemoteAdapter (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the SampleApplication2.MainViewRemoteAdapter class.
        /// </summary>
        /// <param name="connectUriName">The uniform resource name of the ActiveMq broker to connect to.</param>
        /// <param name="outgoingQueueName">The name of the ActiveMq queue to use for outgoing method calls.</param>
        /// <param name="incomingQueueName">The name of the ActiveMq queue to use for incoming method calls.</param>
        /// <param name="requestFilter">The message filter to use for method invocation requests (i.e. calls).</param>
        /// <param name="responseFilter">The message file to use for method invocation responses (i.e. return values).</param>
        public MainViewRemoteAdapter(string connectUriName, string outgoingQueueName, string incomingQueueName, string requestFilter, string responseFilter)
        {
            // Setup objects for sending method invocations
            outgoingMethodSerializer = new MethodInvocationSerializer(new SerializerOperationMap());
            outgoingSender = new ActiveMqRemoteSender(connectUriName, outgoingQueueName, requestFilter);
            outgoingReceiver = new ActiveMqRemoteReceiver(connectUriName, outgoingQueueName, responseFilter, 200);
            methodInvocationSender = new MethodInvocationRemoteSender(outgoingMethodSerializer, outgoingSender, outgoingReceiver);

            // Setup objects for receiving method invocations
            incomingMethodSerializer = new MethodInvocationSerializer(new SerializerOperationMap());
            incomingSender = new ActiveMqRemoteSender(connectUriName, incomingQueueName, responseFilter);
            incomingReceiver = new ActiveMqRemoteReceiver(connectUriName, incomingQueueName, requestFilter, 200);
            methodInvocationReceiver = new MethodInvocationRemoteReceiver(incomingMethodSerializer, incomingSender, incomingReceiver);
            // Hook the 'MethodInvocationReceived' event on the receiver up to the local method which handles recieving method invocations
            methodInvocationReceiver.MethodInvocationReceived += new MethodInvocationReceivedEventHandler(ReceiveMethodInvocation);
        }

        //------------------------------------------------------------------------------
        //
        // Method: Connect
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Connects and initialises the underlying MethodInvocationRemoting and ActiveMQ components.
        /// </summary>
        public void Connect()
        {
            outgoingSender.Connect();
            outgoingReceiver.Connect();
            incomingSender.Connect();
            incomingReceiver.Connect();
            methodInvocationReceiver.Receive();
        }

        //------------------------------------------------------------------------------
        //
        // Method: Disconnect
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Disconnects and cleans up the underlying MethodInvocationRemoting and ActiveMQ components.
        /// </summary>
        public void Disconnect()
        {
            methodInvocationReceiver.CancelReceive();
            incomingReceiver.CancelReceive();
            incomingReceiver.Disconnect();
            incomingSender.Disconnect();
            outgoingReceiver.CancelReceive();
            outgoingReceiver.Disconnect();
            outgoingSender.Disconnect();

            // Call dispose on the sender and receiver objects, as they hold unmanaged connections to ActiveMQ
            incomingReceiver.Dispose();
            incomingSender.Dispose();
            outgoingReceiver.Dispose();
            outgoingSender.Dispose();
        }

        public void SetPresenter(IContactListPresenter presenter)
        {
            this.presenter = presenter;
        }

        public void Initialise()
        {
            methodInvocationSender.InvokeVoidMethod(new MethodInvocation("Initialise"));
        }

        public void Show()
        {
            methodInvocationSender.InvokeVoidMethod(new MethodInvocation("Show"));
        }

        public void Close()
        {
            methodInvocationSender.InvokeVoidMethod(new MethodInvocation("Close"));
        }

        public void AddUpdateContactInGrid(string name, string category, string phoneNumber, string emailAddress)
        {
            Object[] parameters = new Object[4];
            parameters[0] = name;
            parameters[1] = category;
            parameters[2] = phoneNumber;
            parameters[3] = emailAddress;
            methodInvocationSender.InvokeVoidMethod(new MethodInvocation("AddUpdateContactInGrid", parameters));
        }

        public void DeleteContactFromGrid(string name)
        {
            methodInvocationSender.InvokeVoidMethod(new MethodInvocation("DeleteContactFromGrid", new Object[] { name }));
        }

        public void PopulateCategories(string[] categories)
        {
            methodInvocationSender.InvokeVoidMethod(new MethodInvocation("PopulateCategories", new Object[] { categories }));
        }

        public void DisplayErrorMessage(string errorMessage)
        {
            methodInvocationSender.InvokeVoidMethod(new MethodInvocation("DisplayErrorMessage", new Object[] { errorMessage }));
        }

        //------------------------------------------------------------------------------
        //
        // Method: ReceiveMethodInvocation
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Method called when a remote method invocation is received.
        /// </summary>
        /// <param name="sender">The object which received the method invocation.</param>
        /// <param name="e">The event arguments of the receive operation, include the method invocation.</param>
        private void ReceiveMethodInvocation(object sender, MethodInvocationReceivedEventArgs e)
        {
            // Call the relevant methods on the presenter
            if (e.MethodInvocation.Name == "AddUpdateContact")
            {
                object[] parameters = e.MethodInvocation.Parameters;

                presenter.AddUpdateContact((string)parameters[0], (string)parameters[1], (string)parameters[2], (string)parameters[3]);
                ((IMethodInvocationRemoteReceiver)sender).SendVoidReturn();
            }
            else if (e.MethodInvocation.Name == "DeleteContact")
            {
                presenter.DeleteContact((string)e.MethodInvocation.Parameters[0]);
                ((IMethodInvocationRemoteReceiver)sender).SendVoidReturn();
            }
                else if (e.MethodInvocation.Name == "Exit")
            {
                presenter.Exit();
                ((IMethodInvocationRemoteReceiver)sender).SendVoidReturn();
            }
            else
            {
                throw new NotImplementedException("Received unhandled method invocation '" + e.MethodInvocation.Name + "'.");
            }
        }
    }
}

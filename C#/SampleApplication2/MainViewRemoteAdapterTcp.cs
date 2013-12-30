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
    // Class: MainViewRemoteAdapterTcp
    //
    //******************************************************************************
    /// <summary>
    /// Forwards operations on a main view to a remote location using the MethodInvocationRemoting framework via a TCP network.
    /// </summary>
    public class MainViewRemoteAdapterTcp : IMainView
    {
        private IContactListPresenter presenter;
        // Objects for sending method invocations
        private MethodInvocationRemoteSender methodInvocationSender;
        private TcpRemoteSender outgoingSender;
        private TcpRemoteReceiver outgoingReceiver;
        private MethodInvocationSerializer outgoingMethodSerializer;
        // Objects for receiving method invocations
        private MethodInvocationRemoteReceiver methodInvocationReceiver;
        private TcpRemoteSender incomingSender;
        private TcpRemoteReceiver incomingReceiver;
        private MethodInvocationSerializer incomingMethodSerializer;

        //------------------------------------------------------------------------------
        //
        // Method: MainViewRemoteAdapterTcp (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the SampleApplication2.MainViewRemoteAdapterTcp class.
        /// </summary>
        /// <param name="remoteIpAddress">The IP address of the machine where the ContactListPresenterRemoteAdapterTcp object is located.</param>
        /// <param name="outgoingSenderPort">The TCP port on which to send outgoing method invocation requests (i.e. calls).</param>
        /// <param name="outgoingReceiverPort">The TCP port on which to receive outgoing method invocation responses (i.e. return values).</param>
        /// <param name="incomingSenderPort">The TCP port on which to send incoming method invocation responses (i.e. return values).</param>
        /// <param name="incomingReceiverPort">The TCP port on which to receive incoming method invocation requests (i.e. calls).</param>
        /// <param name="connectRetryCount">The number of times to retry when initially connecting, or attempting to reconnect the underlying TcpRemoteSender and TcpRemoteReceiver objects.</param>
        /// <param name="connectRetryInterval">The interval between retries to connect or reconnect in milliseconds.</param>
        /// <param name="acknowledgementReceiveTimeout">The maximum time the TcpRemoteSender should wait for an acknowledgement of a message in milliseconds.</param>
        /// <param name="acknowledgementReceiveRetryInterval">The time the TcpRemoteSender should wait between retries to check for an acknowledgement in milliseconds.</param>
        /// <param name="receiveRetryInterval">The time the TcpRemoteReceiver should wait between attempts to receive a message in milliseconds.</param>
        public MainViewRemoteAdapterTcp(System.Net.IPAddress remoteIpAddress, int outgoingSenderPort, int outgoingReceiverPort, int incomingSenderPort, int incomingReceiverPort, int connectRetryCount, int connectRetryInterval, int acknowledgementReceiveTimeout, int acknowledgementReceiveRetryInterval, int receiveRetryInterval)
        {
            // Setup objects for sending method invocations
            outgoingMethodSerializer = new MethodInvocationSerializer(new SerializerOperationMap());
            outgoingSender = new TcpRemoteSender(remoteIpAddress, outgoingSenderPort, connectRetryCount, connectRetryInterval, acknowledgementReceiveTimeout, acknowledgementReceiveRetryInterval);
            outgoingReceiver = new TcpRemoteReceiver(outgoingReceiverPort, connectRetryCount, connectRetryInterval, receiveRetryInterval);
            methodInvocationSender = new MethodInvocationRemoteSender(outgoingMethodSerializer, outgoingSender, outgoingReceiver);

            // Setup objects for receiving method invocations
            incomingMethodSerializer = new MethodInvocationSerializer(new SerializerOperationMap());
            incomingSender = new TcpRemoteSender(remoteIpAddress, incomingSenderPort, connectRetryCount, connectRetryInterval, acknowledgementReceiveTimeout, acknowledgementReceiveRetryInterval);
            incomingReceiver = new TcpRemoteReceiver(incomingReceiverPort, connectRetryCount, connectRetryInterval, receiveRetryInterval);
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
        /// Connects and initialises the underlying MethodInvocationRemoting components.
        /// </summary>
        public void Connect()
        {
            incomingReceiver.Connect();
            incomingSender.Connect();
            outgoingReceiver.Connect();
            outgoingSender.Connect();
            methodInvocationReceiver.Receive();
        }

        //------------------------------------------------------------------------------
        //
        // Method: Disconnect
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Disconnects and cleans up the underlying MethodInvocationRemoting components.
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

            // Call dispose on the sender and receiver objects
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

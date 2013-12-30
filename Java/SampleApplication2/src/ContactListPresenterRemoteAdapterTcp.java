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

import java.net.*;
import net.alastairwyse.methodinvocationremoting.*;

/**
 * Forwards operations on a contact list presenter to a remote location using the MethodInvocationRemoting framework via a TCP network.
 * @author Alastair Wyse
 */
public class ContactListPresenterRemoteAdapterTcp implements IContactListPresenter {

    private IMainView mainView;
    // Objects used to store and interrogate whether exceptions occurred whilst receiving method invocations
    private Object exceptionLock;
    private volatile Exception occurredException; 
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

    /**
     * Initialises a new instance of the ContactListPresenterRemoteAdapterTcp class.
     * @param remoteIpAddress                      The IP address of the machine where the ContactListPresenterRemoteAdapterTcp object is located.
     * @param outgoingSenderPort                   The TCP port on which to send outgoing method invocation requests (i.e. calls).
     * @param outgoingReceiverPort                 The TCP port on which to receive outgoing method invocation responses (i.e. return values).
     * @param incomingSenderPort                   The TCP port on which to send incoming method invocation responses (i.e. return values).
     * @param incomingReceiverPort                 The TCP port on which to receive incoming method invocation requests (i.e. calls).
     * @param connectRetryCount                    The number of times to retry when initially connecting, or attempting to reconnect the underlying TcpRemoteSender and TcpRemoteReceiver objects.
     * @param connectRetryInterval                 The interval between retries to connect or reconnect in milliseconds.
     * @param acknowledgementReceiveTimeout        The maximum time the TcpRemoteSender should wait for an acknowledgement of a message in milliseconds.
     * @param acknowledgementReceiveRetryInterval  The time the TcpRemoteSender should wait between retries to check for an acknowledgement in milliseconds.
     * @param receiveRetryInterval                 The time the TcpRemoteReceiver should wait between attempts to receive a message in milliseconds.
     */
    public ContactListPresenterRemoteAdapterTcp(InetAddress remoteIpAddress, int outgoingSenderPort, int outgoingReceiverPort, int incomingSenderPort, int incomingReceiverPort, int connectRetryCount, int connectRetryInterval, int acknowledgementReceiveTimeout, int acknowledgementReceiveRetryInterval, int receiveRetryInterval) {
        exceptionLock = new Object();
        synchronized(exceptionLock) {
            occurredException = null;
        }
        
        // Setup objects for sending method invocations
        outgoingMethodSerializer = new MethodInvocationSerializer(new SerializerOperationMap());
        outgoingSender = new TcpRemoteSender(remoteIpAddress, outgoingSenderPort, connectRetryCount, connectRetryInterval, acknowledgementReceiveTimeout, acknowledgementReceiveRetryInterval);
        outgoingReceiver = new TcpRemoteReceiver(outgoingReceiverPort, connectRetryCount, connectRetryInterval, receiveRetryInterval, 1024);
        methodInvocationSender = new MethodInvocationRemoteSender(outgoingMethodSerializer, outgoingSender, outgoingReceiver);

        // Setup objects for receiving method invocations
        incomingMethodSerializer = new MethodInvocationSerializer(new SerializerOperationMap());
        incomingSender = new TcpRemoteSender(remoteIpAddress, incomingSenderPort, connectRetryCount, connectRetryInterval, acknowledgementReceiveTimeout, acknowledgementReceiveRetryInterval);
        incomingReceiver = new TcpRemoteReceiver(incomingReceiverPort, connectRetryCount, connectRetryInterval, receiveRetryInterval, 1024);
        methodInvocationReceiver = new MethodInvocationRemoteReceiver(incomingMethodSerializer, incomingSender, incomingReceiver);
        methodInvocationReceiver.setReceivedEventHandler(new MessageReceivedHandler());
    }
    
    /**
     * Returns an exception that occurred whilst the presenter was operating, or null if no exception has occurred.
     * As the ContactListPresenterRemoteAdapterTcp creates additional threads as part of the MethodInvocationRemoting framework, this method can be used to determine if any exceptions have occurred on spawned threads.
     * @return  The exception that occurred, or null if no exception has occurred.
     */
    public Exception getOccurredException() {
        Exception returnException;
        synchronized(exceptionLock) {
            returnException = occurredException;
        }
        return returnException;
    }
    
    /**
     * Sets the main view which this presenter should interact with.
     * @param mainView  The view.
     */
    public void setMainView(IMainView mainView) {
        this.mainView = mainView;
    }
    
    /**
     * Connects and initialises the underlying MethodInvocationRemoting components.
     */
    public void Connect() throws Exception {
        outgoingSender.Connect();
        outgoingReceiver.Connect();
        incomingSender.Connect();
        incomingReceiver.Connect();
        methodInvocationReceiver.Receive();
    }
    
    /**
     * Disconnects and cleans up the underlying MethodInvocationRemoting components.
     */
    public void Disconnect() throws Exception {
        methodInvocationReceiver.CancelReceive();
        incomingReceiver.CancelReceive();
        incomingReceiver.Disconnect();
        incomingSender.Disconnect();
        outgoingReceiver.CancelReceive();
        outgoingReceiver.Disconnect();
        outgoingSender.Disconnect();
    }
    
    @Override
    public void AddUpdateContact(String name, String category, String phoneNumber, String emailAddress) {
        MethodInvocation addUpdateContactInvocation = new MethodInvocation("AddUpdateContact", new Object[] { name, category, phoneNumber, emailAddress });
        try {
            methodInvocationSender.InvokeVoidMethod(addUpdateContactInvocation);
        } catch (Exception e) {
            SetOccurredException(e);
        }
    }

    @Override
    public void DeleteContact(String name) {
        try {
            methodInvocationSender.InvokeVoidMethod(new MethodInvocation("DeleteContact", new Object[] { name }));
        } catch (Exception e) {
            SetOccurredException(e);
        }
    }

    @Override
    public void Exit() {
        try {
            methodInvocationSender.InvokeVoidMethod(new MethodInvocation("Exit"));
        } catch (Exception e) {
            
        }
    }

    /**
     * Sets the occurredException field using a 'synchronized' block.
     * @param e  The exception which occurred.
     */
    private void SetOccurredException(Exception e) {
        synchronized(exceptionLock) {
            occurredException = e;
        }
    }
    
    /**
     * Handles when a remote method invocation is received.
     */
    private class MessageReceivedHandler implements IMethodInvocationReceivedEventHandler {

        @Override
        public void MethodInvocationReceived(IMethodInvocationRemoteReceiver source, IMethodInvocation receivedMethodInvocation) {
            String methodName = receivedMethodInvocation.getName();
            Object[] parameters = receivedMethodInvocation.getParameters();
            
            try {
                if (methodName.equals("Initialise")) {
                    mainView.Initialise();
                    source.SendVoidReturn();
                }
                else if (methodName.equals("Show")) {
                    mainView.Show();
                    source.SendVoidReturn();
                }
                else if (methodName.equals("Close")) {
                    mainView.Close();
                    source.SendVoidReturn();
                }
                else if (methodName.equals("AddUpdateContactInGrid")) {
                    mainView.AddUpdateContactInGrid((String)parameters[0], (String)parameters[1], (String)parameters[2], (String)parameters[3]);
                    source.SendVoidReturn();
                }
                else if (methodName.equals("DeleteContactFromGrid")) {
                    mainView.DeleteContactFromGrid((String)parameters[0]);
                    source.SendVoidReturn();
                }
                else if (methodName.equals("PopulateCategories")) {
                    mainView.PopulateCategories((String[])parameters[0]);
                    source.SendVoidReturn();
                }
                else if (methodName.equals("DisplayErrorMessage")) {
                    mainView.DisplayErrorMessage((String)parameters[0]);
                    source.SendVoidReturn();
                }
                else {
                    throw new Exception("Received unhandled method invocation '" + methodName + "'.");
                }
            }
            catch (Exception e) {
                MethodInvocationReceiveException(methodInvocationReceiver, e);
            }
        }

        @Override
        public void MethodInvocationReceiveException(IMethodInvocationRemoteReceiver source, Exception e) {
            SetOccurredException(e);
        }
    }
}

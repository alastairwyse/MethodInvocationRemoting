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

import net.alastairwyse.methodinvocationremoting.*;

/**
 * Forwards operations on a contact list presenter to a remote location using the MethodInvocationRemoting framework and Apache ActiveMQ.
 * @author Alastair Wyse
 */
public class ContactListPresenterRemoteAdapter implements IContactListPresenter {

    private IMainView mainView;
    // Objects used to store and interrogate whether exceptions occurred whilst receiving method invocations
    private Object exceptionLock;
    private volatile Exception occurredException; 
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
    
    /**
     * Initialises a new instance of the ContactListPresenterRemoteAdapter class.
     * @param connectUriName     The uniform resource name of the ActiveMq broker to connect to.
     * @param outgoingQueueName  The name of the ActiveMq queue to use for outgoing method calls.
     * @param incomingQueueName  The name of the ActiveMq queue to use for incoming method calls.
     * @param requestFilter      The message filter to use for method invocation requests (i.e. calls).
     * @param responseFilter     The message file to use for method invocation responses (i.e. return values).
     */
    public ContactListPresenterRemoteAdapter(String connectUriName, String outgoingQueueName, String incomingQueueName, String requestFilter, String responseFilter) {
        exceptionLock = new Object();
        synchronized(exceptionLock) {
            occurredException = null;
        }
        
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
        methodInvocationReceiver.setReceivedEventHandler(new MessageReceivedHandler());
    }
    
    /**
     * Returns an exception that occurred whilst the presenter was operating, or null if no exception has occurred.
     * As the ContactListPresenterRemoteAdapter creates additional threads as part of the MethodInvocationRemoting framework, this method can be used to determine if any exceptions have occurred on spawned threads.
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
     * Connects and initialises the underlying MethodInvocationRemoting and ActiveMQ components.
     */
    public void Connect() throws Exception {
        outgoingSender.Connect();
        outgoingReceiver.Connect();
        incomingSender.Connect();
        incomingReceiver.Connect();
        methodInvocationReceiver.Receive();
    }
    
    /**
     * Disconnects and cleans up the underlying MethodInvocationRemoting and ActiveMQ components.
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

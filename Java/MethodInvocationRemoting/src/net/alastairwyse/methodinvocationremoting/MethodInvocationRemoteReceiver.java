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

package net.alastairwyse.methodinvocationremoting;

/**
 * Receives method invocations (represented by IMethodInvocation objects) from remote locations.
 * @author Alastair Wyse
 */
public class MethodInvocationRemoteReceiver implements IMethodInvocationRemoteReceiver {

    private IMethodInvocationSerializer serializer;
    private IRemoteSender sender;
    private IRemoteReceiver receiver;
    private IMethodInvocationReceivedEventHandler receivedEventHandler;
    private Thread receiveLoopThread;
    private volatile boolean cancelRequest = false;
    
    /**
     * Initialises a new instance of the MethodInvocationRemoteReceiver class.
     * @param serializer  Object to use to deserialize method invocations.
     * @param sender      Object to use to send serialized method invocation return values.
     * @param receiver    Object to use to receive serialized method invocations.
     */
    public MethodInvocationRemoteReceiver(IMethodInvocationSerializer serializer, IRemoteSender sender, IRemoteReceiver receiver) {
        this.serializer = serializer;
        this.sender = sender;
        this.receiver = receiver;
    }
    
    @Override
    public void setReceivedEventHandler(IMethodInvocationReceivedEventHandler receivedEventHandler) {
        this.receivedEventHandler = receivedEventHandler;
    }

    @Override
    public void Receive() throws Exception {
        if(receivedEventHandler == null) {
            throw new Exception("Member 'ReceivedEventHandler' has not been set.");
        }
        
        cancelRequest = false;
        receiveLoopThread = new Thread(new ReceiveLoopHandler(this));
        receiveLoopThread.setName("MethodInvocationRemoteReceiver.ReceiveLoopHandler");
        receiveLoopThread.setDaemon(false);
        receiveLoopThread.start();
    }

    @Override
    public void SendReturnValue(Object ReturnValue) throws Exception {
        try
        {
            String serializedReturnValue = serializer.SerializeReturnValue(ReturnValue);
            sender.Send(serializedReturnValue);
        }
        catch (Exception e)
        {
            throw new Exception("Failed to send return value.", e);
        }
    }

    @Override
    public void SendVoidReturn() throws Exception {
        try
        {
            sender.Send(serializer.getVoidReturnValue());
        }
        catch (Exception e)
        {
            throw new Exception("Failed to send void return value.", e);
        }
    }

    @Override
    public void CancelReceive() throws Exception {
        cancelRequest = true;
        receiver.CancelReceive();
        receiveLoopThread.join();
    }

    /**
     * Encapsulates a worker thread operation to receive method invocations.
     */
    private class ReceiveLoopHandler implements Runnable {
        
        private MethodInvocationRemoteReceiver outerClass;
        
        public ReceiveLoopHandler(MethodInvocationRemoteReceiver outerClass) {
            this.outerClass = outerClass;
        }

        @Override
        public void run() {
            while (cancelRequest == false) {
                try {
                    String serializedMethodInvocation = receiver.Receive();
                    if (serializedMethodInvocation != "")
                    {
                        IMethodInvocation receivedMethodInvocation = serializer.Deserialize(serializedMethodInvocation);
                        receivedEventHandler.MethodInvocationReceived(outerClass, receivedMethodInvocation);
                    }
                }
                catch (Exception e) {
                    receivedEventHandler.MethodInvocationReceiveException(outerClass, new RuntimeException("Failed to invoke method.", e));
                    break;
                }
            }
        }
    }
}

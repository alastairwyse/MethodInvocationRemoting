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

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.net.*;

/**
 * Method Invocation Remoting framework second sample application.
 * @author Alastair Wyse
 */
public class Program {

    private static volatile boolean cancelRequest = false;
    private static Thread cancelRequestReceiverThread;

    public static void main(String[] args) {

        // Uncomment one of the 3 presenter declarations below to select between ActiveMQ, file, and TCP as the underlying transport mechanism
        ContactListPresenterRemoteAdapterActiveMQ presenter = null;
        // ContactListPresenterRemoteAdapterFile presenter = null;
        // ContactListPresenterRemoteAdapterTcp presenter = null;
        
        MainView mainView = null;
        
        try {
            // Setup presenter and view
            
            // Uncomment one of the 3 presenter constructors below to select between ActiveMQ, file, and TCP as the underlying transport mechanism
            presenter = new ContactListPresenterRemoteAdapterActiveMQ("tcp://localhost:61616?wireFormat.maxInactivityDuration=0", "FromJava", "FromC#", "Request", "Response");
            // presenter = new ContactListPresenterRemoteAdapterFile("C:\\Temp\\FromJavaRequest.txt", "C:\\Temp\\FromJavaRequest.lck", "C:\\Temp\\FromJavaResponse.txt", "C:\\Temp\\FromJavaResponse.lck", "C:\\Temp\\FromC#Response.txt", "C:\\Temp\\FromC#Response.lck", "C:\\Temp\\FromC#Request.txt", "C:\\Temp\\FromC#Request.lck");
            // presenter = new ContactListPresenterRemoteAdapterTcp(InetAddress.getLoopbackAddress(), 55003, 55002, 55001, 55000, 15, 2000, 30000, 50, 25);
            
        	mainView = new MainView();
            presenter.setMainView(mainView);
            mainView.SetPresenter(presenter);
            presenter.Connect();
            // Start thread to wait for user cancel
            cancelRequestReceiverThread = new Thread(new CancelRequestReceiver());
            cancelRequestReceiverThread.setDaemon(true);
            cancelRequestReceiverThread.start();
            
            // Spin until either an exception occurs or the user cancels
            while((presenter.getOccurredException() == null) && (cancelRequest == false)) {
                Thread.sleep(1000);
            }
            if (presenter.getOccurredException() != null) {
                System.out.println("Exception occured during operation...");
                presenter.getOccurredException().printStackTrace(System.out);
            }
        }
        catch (Exception e) {
            System.out.println("Exception occured during operation...");
            e.printStackTrace(System.out);
        }
        finally {
            mainView.dispose();
            // Disconnect the presenter from the transport layer
            try {
                presenter.Disconnect();
            }
            catch (Exception e) {
                System.out.println("Exception occured during disconnection...");
                e.printStackTrace(System.out);
            }
        }
    }

    /**
     * Thread which sets the cancel request flag when the user presses the Enter key
     */
    private static class CancelRequestReceiver implements Runnable {

        @Override
        public void run() {
            BufferedReader inputReader = new BufferedReader(new InputStreamReader(System.in));
            try {
                System.out.println("Press [ENTER] to stop the application.");
                inputReader.readLine();
            } catch (Exception e) {
                e.printStackTrace();
            }
            cancelRequest = true;
        }
    }
}

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

import java.io.*;
import java.net.*;
import java.util.*;
import net.alastairwyse.applicationlogging.*;
import net.alastairwyse.applicationmetrics.*;
import net.alastairwyse.methodinvocationremoting.*;

/**
 * Method Invocation Remoting framework fourth sample application.
 * @author Alastair Wyse
 */
public class Program {

    public static void main(String[] args) throws Exception {

        // Setup logger and methodinvocationremoting objects
        try(BufferedReader inputReader = new BufferedReader(new InputStreamReader(System.in));
            FileApplicationLogger remoteReceiverLog = new FileApplicationLogger(LogLevel.Debug, '|', "  ", "C:\\Temp\\JavaReceiver.log");
            TcpRemoteReceiver tcpReceiver = new TcpRemoteReceiver(55000, 10, 1000, 25, 1024, remoteReceiverLog);
            TcpRemoteSender tcpSender = new TcpRemoteSender(InetAddress.getLoopbackAddress(), 55001, 10, 1000, 30000, 25, remoteReceiverLog)) {
            RemoteSenderCompressor remoteSenderCompressor = new RemoteSenderCompressor(tcpSender, remoteReceiverLog);
            MethodInvocationSerializer serializer = new MethodInvocationSerializer(new SerializerOperationMap(), remoteReceiverLog);
            MethodInvocationRemoteReceiver methodInvocationRemoteReceiver = new MethodInvocationRemoteReceiver(serializer, remoteSenderCompressor, tcpReceiver, remoteReceiverLog);
            methodInvocationRemoteReceiver.setReceivedEventHandler(new MessageReceivedHandler());

            // Connect to the MethodInvocationRemoteSender
            tcpReceiver.Connect();
            tcpSender.Connect();
            methodInvocationRemoteReceiver.Receive();

            System.out.println("Press [ENTER] to stop the application.");
            inputReader.readLine();
            
            // Disconnect from the MethodInvocationRemoteReceiver
            methodInvocationRemoteReceiver.CancelReceive();
            tcpSender.Disconnect();
            tcpReceiver.Disconnect();
        }
    }

    /**
     * Handles when a remote method invocation is received.
     */
    private static class MessageReceivedHandler implements IMethodInvocationReceivedEventHandler {
        
        @Override
        public void MethodInvocationReceived(IMethodInvocationRemoteReceiver source, IMethodInvocation receivedMethodInvocation) {
            if (receivedMethodInvocation.getName().equals("GenerateScenarios") == true) {
                // Get the scenario count parameter
                int scenarioCount = ((int)receivedMethodInvocation.getParameters()[0]);

                // Generate the scenarios
                Double[] scenarios = GenerateScenarios(scenarioCount);
                
                // Send the return value
                try {
                    source.SendReturnValue(scenarios);
                }
                catch (Exception e) {
                    MethodInvocationReceiveException(source, e);
                }
            }
            else {
                MethodInvocationReceiveException(source, new Exception("Unexpected method invocation '" + receivedMethodInvocation.getName() + "' receieved."));
            }
        }
        
        @Override
        public void MethodInvocationReceiveException(IMethodInvocationRemoteReceiver source, Exception e) {
            e.printStackTrace(System.out);
        }
        
        /**
         * Uses Monte Carlo simulation to generate a number of scenarios used to approximate the value of Pi.
         * @param scenarioCount  The number of Monte Carlo scenarios to generate.
         * @return               The generated scenario results.
         * @throws Exception     If an error occurs during the scenario generation.
         */
        private Double[] GenerateScenarios(int scenarioCount) {
            Random randomGenerator = new Random();
            Double[] scenarios = new Double[scenarioCount];
            
            // Create the scenarios
            for(int i = 0; i < scenarioCount; i++) {
                // Generate random numbers between 0 and 1
                Double xValue = randomGenerator.nextDouble();
                Double yValue = randomGenerator.nextDouble();
                
                // Find the distance to the origin
                Double distance = Math.sqrt((xValue * xValue) + (yValue * yValue));
                
                scenarios[i] = distance;
            }
            
            return scenarios;
        }
    }
}

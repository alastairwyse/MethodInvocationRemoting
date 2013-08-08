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
import java.math.*;

/**
 * Method Invocation Remoting framework first sample application.
 * @author Alastair Wyse
 */
public class Program {

    public static void main(String[] args) {
        // Setup connection parameters for ActiveMQ
        final String connectUri = "tcp://localhost:61616?wireFormat.maxInactivityDuration=0";
        final String queueName = "FromJava";
        final String requestQueueFilter = "Request";
        final String responseQueueFilter = "Response";
        final int receiverConnectLoopTimeout = 200;
        
        // Setup method invocation sender
        ActiveMqRemoteSender activeMqSender = new ActiveMqRemoteSender(connectUri, queueName, requestQueueFilter);
        ActiveMqRemoteReceiver activeMqReceiver = new ActiveMqRemoteReceiver(connectUri, queueName, responseQueueFilter, receiverConnectLoopTimeout);
        MethodInvocationSerializer methodSerializer = new MethodInvocationSerializer(new SerializerOperationMap());
        MethodInvocationRemoteSender methodInvocationSender = new MethodInvocationRemoteSender(methodSerializer, activeMqSender, activeMqReceiver);
        
        try {
            // Connect to ActiveMQ
            activeMqSender.Connect();
            activeMqReceiver.Connect();
            
            // Create and send the method remote method invocations
            Object[] parameters = new Object[1];
            parameters[0] = (Integer)10000;
            MethodInvocation approximatePiCall = new MethodInvocation("ApproximatePi", parameters, BigDecimal.class);

            BigDecimal piApproximation = (BigDecimal)methodInvocationSender.InvokeMethod(approximatePiCall);
            System.out.println("10000 scenarios returned a value of " + piApproximation.toString());

            parameters[0] = (Integer)100000;
            piApproximation = (BigDecimal)methodInvocationSender.InvokeMethod(approximatePiCall);
            System.out.println("100000 scenarios returned a value of " + piApproximation.toString());

            parameters[0] = (Integer)1000000;
            piApproximation = (BigDecimal)methodInvocationSender.InvokeMethod(approximatePiCall);
            System.out.println("1000000 scenarios returned a value of " + piApproximation.toString());
            
            parameters[0] = (Integer)10000000;
            piApproximation = (BigDecimal)methodInvocationSender.InvokeMethod(approximatePiCall);
            System.out.println("10000000 scenarios returned a value of " + piApproximation.toString());
            
            parameters[0] = (Integer)100000000;
            piApproximation = (BigDecimal)methodInvocationSender.InvokeMethod(approximatePiCall);
            System.out.println("100000000 scenarios returned a value of " + piApproximation.toString());
        }
        catch (Exception e) {
            e.printStackTrace(System.out);
        }
        finally {
            try {
                // Stop receiving and disconnect
                activeMqReceiver.CancelReceive();
                activeMqReceiver.Disconnect();
                activeMqSender.Disconnect();
            }
            catch (Exception e) {
                System.out.println("Exception occured during disconnection...");
                e.printStackTrace(System.out);
            }
        }
    }

}

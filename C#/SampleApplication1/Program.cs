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

namespace SampleApplication1
{
    /// <summary>
    /// Method Invocation Remoting framework first sample application.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Setup connection parameters for ActiveMQ
            const string connectUri = "activemq:tcp://localhost:61616?wireFormat.maxInactivityDuration=0";
            const string queueName = "FromJava";
            const string requestQueueFilter = "Request";
            const string responseQueueFilter = "Response";
            const int receiverConnectLoopTimeout = 200;

            // Setup method invocation receiver
            ActiveMqRemoteSender activeMqSender = new ActiveMqRemoteSender(connectUri, queueName, responseQueueFilter);
            ActiveMqRemoteReceiver activeMqReceiver = new ActiveMqRemoteReceiver(connectUri, queueName, requestQueueFilter, receiverConnectLoopTimeout);
            MethodInvocationSerializer methodSerializer = new MethodInvocationSerializer(new SerializerOperationMap());
            MethodInvocationRemoteReceiver methodInvocationReceiver = new MethodInvocationRemoteReceiver(methodSerializer, activeMqSender, activeMqReceiver);
            methodInvocationReceiver.MethodInvocationReceived += new MethodInvocationReceivedEventHandler(ReceiveMethodInvocation);

            try
            {
                // Connect to ActiveMQ
                activeMqSender.Connect();
                activeMqReceiver.Connect();
                // Start receiving method invocations
                methodInvocationReceiver.Receive();
                Console.WriteLine("Waiting for incoming method calls...");
                Console.WriteLine("Press [ENTER] to cancel.");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception occurred");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                // Stop receiving, disconnect, and dispose
                methodInvocationReceiver.CancelReceive();
                activeMqReceiver.CancelReceive();
                activeMqReceiver.Disconnect();
                activeMqSender.Disconnect();
                activeMqReceiver.Dispose();
                activeMqSender.Dispose();
            }
        }

        /// <summary>
        /// Calculates an approximation of pi using Monte Carlo simulation.  Algorithm provided by http://www.chem.unl.edu/zeng/joy/mclab/mcintro.html.
        /// </summary>
        /// <param name="scenarioCount">The number of Monte Carlo scenarios to calculate.</param>
        /// <returns>The approximate value of pi.</returns>
        private static decimal ApproximatePi(int scenarioCount)
        {
            Random RandomGenerator = new Random(DateTime.Now.Millisecond);
            int hits = 0;

            if (scenarioCount <= 0)
            {
                throw new ArgumentException("The scenario count must be greater than or equal to 1.", "scenarioCount");
            }

            // Perform the simluation
            for(int i = 0; i < scenarioCount; i = i + 1)
            {
                // Generate random numbers between 1 and 0
                double xValue = RandomGenerator.NextDouble();
                double yValue = RandomGenerator.NextDouble();

                // Find the distance to the origin
                double distance = Math.Sqrt((xValue * xValue) + (yValue * yValue));

                // Increment hit counter
                if (distance <= 1.0)
                {
                    hits = hits + 1;
                }
            }

            // Calculate pi
            decimal returnValue = Decimal.Divide((Decimal)hits, (Decimal)scenarioCount);
            returnValue = Decimal.Multiply(returnValue, 4m);

            return returnValue;
        }

        /// <summary>
        /// The event that is raised when a remote method invocation is received.
        /// </summary>
        /// <param name="sender">The object which raised the event.</param>
        /// <param name="e">The method invocation which was received.</param>
        private static void ReceiveMethodInvocation(object sender, MethodInvocationReceivedEventArgs e)
        {
            if (e.MethodInvocation.Name == "ApproximatePi")
            {
                object[] parameters = e.MethodInvocation.Parameters;

                Console.WriteLine("Received invocation of method 'ApproximatePi(" + (int)parameters[0] + ")'");
                // Call the ApproximatePi method
                decimal result = ApproximatePi((int)parameters[0]);
                Console.WriteLine("Sending return value " + result.ToString());
                // Send the result
                ((IMethodInvocationRemoteReceiver)sender).SendReturnValue(result);
            }
            else
            {
                throw new NotImplementedException("Received unhandled method invocation '" + e.MethodInvocation.Name + "'.");
            }
        }
    }
}

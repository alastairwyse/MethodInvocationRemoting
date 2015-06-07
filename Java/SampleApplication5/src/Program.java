/*
 * Copyright 2015 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
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
import java.lang.Thread.*;
import java.net.*;
import java.util.*;
import java.util.concurrent.TimeUnit;

import net.alastairwyse.applicationlogging.*;
import net.alastairwyse.applicationmetrics.*;
import net.alastairwyse.methodinvocationremoting.*;
import net.alastairwyse.methodinvocationremotingmetrics.*;

/**
 * Method Invocation Remoting framework fifth sample application.
 * @author Alastair Wyse
 */
public class Program {

    public static void main(String[] args) throws Exception {

        // Setup metric logger and MethodInvocationRemoting objects
        MetricLoggerDistributor distributor = new MetricLoggerDistributor();
        ConsoleMetricLogger consoleMetricLogger = new ConsoleMetricLogger(5000, true, new MetricLoggerExceptionHandler());
        SizeLimitedBufferProcessor fileMetricLoggerBufferProcessor = new SizeLimitedBufferProcessor(50, new MetricLoggerExceptionHandler());
        
        // Setup logger and methodinvocationremoting objects
        try(BufferedReader inputReader = new BufferedReader(new InputStreamReader(System.in));
            FileApplicationLogger remoteReceiverLog = new FileApplicationLogger(LogLevel.Information, '|', "  ", "C:\\Temp\\JavaReceiver.log");
            FileMetricLogger fileMetricLogger = new FileMetricLogger('|', "C:\\Temp\\JavaMetrics.log", fileMetricLoggerBufferProcessor, true);
            TcpRemoteReceiver tcpReceiver = new TcpRemoteReceiver(55000, 10, 1000, 25, 1024, remoteReceiverLog, distributor);
            TcpRemoteSender tcpSender = new TcpRemoteSender(InetAddress.getLoopbackAddress(), 55001, 10, 1000, 30000, 25, remoteReceiverLog, distributor)) {
            RemoteSenderCompressor remoteSenderCompressor = new RemoteSenderCompressor(tcpSender, remoteReceiverLog, distributor);
            MethodInvocationSerializer serializer = new MethodInvocationSerializer(new SerializerOperationMap(), remoteReceiverLog, distributor);
            MethodInvocationRemoteReceiver methodInvocationRemoteReceiver = new MethodInvocationRemoteReceiver(serializer, remoteSenderCompressor, tcpReceiver, remoteReceiverLog, distributor);
            methodInvocationRemoteReceiver.setReceivedEventHandler(new MessageReceivedHandler());

            // Define metric aggregates to log and start the metric logger
            consoleMetricLogger.DefineMetricAggregate(new CompressedStringSize(0), new RemoteMethodReceived(), "BytesSentPerRemoteMethodReceived", "The average number of bytes sent per remote method received");
            consoleMetricLogger.DefineMetricAggregate(new ReceivedMessageSize(0), new RemoteMethodReceived(), "BytesReceivedPerRemoteMethodReceived", "The average number of bytes received per remote method received");
            consoleMetricLogger.DefineMetricAggregate(new MessageSendTime(), new RemoteMethodReceived(), "MessageSendTimePerRemoteMethodReceived", "The average message send time per remote method sent");
            consoleMetricLogger.DefineMetricAggregate(new MessageReceiveTime(), new RemoteMethodReceived(), "MessageReceiveTimePerRemoteMethodReceived", "The average message receive time per remote method sent");
            consoleMetricLogger.DefineMetricAggregate(new RemoteMethodReceiveTime(), new RemoteMethodReceived(), "AverageRemoteMethodReceiveTime", "The average time taken to receive, invoke, and send a return value for a remote method");
            consoleMetricLogger.DefineMetricAggregate(new CompressedStringSize(0), TimeUnit.SECONDS, "BytesSentPerSecond", "The average number of bytes sent per second");
            consoleMetricLogger.DefineMetricAggregate(new ReceivedMessageSize(0), TimeUnit.SECONDS, "BytesReceivedPerSecond", "The average number of bytes received per second");
            consoleMetricLogger.DefineMetricAggregate(new RemoteMethodReceived(), TimeUnit.MINUTES, "RemoteMethodsReceivedPerMinute", "The average number of remote methods received per minute");
            consoleMetricLogger.DefineMetricAggregate(new StringCompressTime(), "TimeSpentCompressingReturnValues", "The fraction of total runtime spent compressing return values");
            consoleMetricLogger.DefineMetricAggregate(new CompressedStringSize(0), new SerializedReturnValueSize(0), "CompressionRatio", "The average compression ratio of the serialized return values");
            distributor.AddLogger(consoleMetricLogger);
            distributor.AddLogger(fileMetricLogger);
            consoleMetricLogger.Start();
            fileMetricLogger.Start();
            
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
            
            // Stop the metric loggers
            consoleMetricLogger.Stop();
            fileMetricLoggerBufferProcessor.Stop();
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
    
    /**
     * Handles when an exception occurs in the metric logger class.
     */
    private static class MetricLoggerExceptionHandler implements UncaughtExceptionHandler {

        @Override
        public void uncaughtException(Thread arg0, Throwable arg1) {
            arg1.printStackTrace(System.out);
        }
    }
    
    /**
     * Distributes metric events to multiple implementations of interface IMetricLogger.
     */
    private static class MetricLoggerDistributor implements IMetricLogger {
        
        ArrayList<IMetricLogger> loggerList;
        
        public MetricLoggerDistributor() {
            loggerList = new ArrayList<IMetricLogger>();
        }

        /**
         * Adds a IMetricLogger to the distribution list.
         * @param  logger  The logger to add.
         */
        public void AddLogger(IMetricLogger logger) {
            loggerList.add(logger);
        }
        
        @Override
        public void Increment(CountMetric countMetric) {
            for (IMetricLogger currentLogger : loggerList) {
                currentLogger.Increment(countMetric);
            }
        }

        @Override
        public void Add(AmountMetric amountMetric) {
            for (IMetricLogger currentLogger : loggerList) {
                currentLogger.Add(amountMetric);
            }
        }

        @Override
        public void Set(StatusMetric statusMetric) {
            for (IMetricLogger currentLogger : loggerList) {
                currentLogger.Set(statusMetric);
            }
        }

        @Override
        public void Begin(IntervalMetric intervalMetric) {
            for (IMetricLogger currentLogger : loggerList) {
                currentLogger.Begin(intervalMetric);
            }
        }

        @Override
        public void End(IntervalMetric intervalMetric) {
            for (IMetricLogger currentLogger : loggerList) {
                currentLogger.End(intervalMetric);
            }
        }
        
        @Override
        public void CancelBegin(IntervalMetric intervalMetric) {
            for (IMetricLogger currentLogger : loggerList) {
                currentLogger.CancelBegin(intervalMetric);
            }
        }
    }
}

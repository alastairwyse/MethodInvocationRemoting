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

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OperatingSystemAbstraction;
using ApplicationLogging;
using ApplicationMetrics;
using MethodInvocationRemoting;
using MethodInvocationRemotingMetrics;
using SampleApplication4;

namespace SampleApplication5
{
    /// <summary>
    /// Method Invocation Remoting framework fifth sample application.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Setup metric logger and MethodInvocationRemoting objects
            MetricLoggerDistributor distributor = new MetricLoggerDistributor();
            ConsoleMetricLogger consoleMetricLogger = new ConsoleMetricLogger(5000, true);

            using (FileApplicationLogger logger = new FileApplicationLogger(LogLevel.Information, '|', "  ", @"C:\Temp\C#Sender.log"))
            using (FileMetricLogger fileMetricLogger = new FileMetricLogger('|', @"C:\Temp\C#Metrics.log", 5000, true))
            using (MicrosoftAccessMetricLogger accessMetricLogger = new MicrosoftAccessMetricLogger(@"C:\Temp\MetricLogger.mdb", "SampleApplication5-C#", 5000, true))
            using (PerformanceCounterMetricLogger perfmonMetricLogger = new PerformanceCounterMetricLogger("SampleApplication5Metrics", "Metrics produced by MethodInvocationRemoting sample application 5", 1000, true))
            using (TcpRemoteSender tcpSender = new TcpRemoteSender(System.Net.IPAddress.Loopback, 55000, 10, 1000, 30000, 25, logger, distributor))
            using (TcpRemoteReceiver tcpReceiver = new TcpRemoteReceiver(55001, 10, 1000, 25, logger, distributor))
            {
                RemoteReceiverDecompressor decompressorReceiver = new RemoteReceiverDecompressor(tcpReceiver, logger, distributor);
                MethodInvocationSerializer serializer = new MethodInvocationSerializer(new SerializerOperationMap(), logger, distributor);
                MethodInvocationRemoteSender methodInvocationRemoteSender = new MethodInvocationRemoteSender(serializer, tcpSender, decompressorReceiver, logger, distributor);

                // Define metric aggregates for the console logger
                DefineMetricAggregates(consoleMetricLogger);
                
                // Register base metrics and aggregates with the performance monitor logger
                perfmonMetricLogger.RegisterMetric(new RemoteMethodSendTime());
                perfmonMetricLogger.RegisterMetric(new RemoteMethodSent());
                perfmonMetricLogger.RegisterMetric(new MethodInvocationSerializeTime());
                perfmonMetricLogger.RegisterMetric(new MethodInvocationSerialized());
                perfmonMetricLogger.RegisterMetric(new SerializedMethodInvocationSize(0));
                perfmonMetricLogger.RegisterMetric(new ReturnValueDeserializeTime());
                perfmonMetricLogger.RegisterMetric(new ReturnValueDeserialized());
                perfmonMetricLogger.RegisterMetric(new StringDecompressTime());
                perfmonMetricLogger.RegisterMetric(new RemoteReceiverDecompressorReadBufferCreated());
                perfmonMetricLogger.RegisterMetric(new StringDecompressed());
                perfmonMetricLogger.RegisterMetric(new TcpRemoteReceiverReconnected());
                perfmonMetricLogger.RegisterMetric(new MessageReceiveTime());
                perfmonMetricLogger.RegisterMetric(new MessageReceived());
                perfmonMetricLogger.RegisterMetric(new ReceivedMessageSize(0));
                perfmonMetricLogger.RegisterMetric(new TcpRemoteReceiverDuplicateSequenceNumber());
                perfmonMetricLogger.RegisterMetric(new MessageSendTime());
                perfmonMetricLogger.RegisterMetric(new MessageSent());
                perfmonMetricLogger.RegisterMetric(new TcpRemoteSenderReconnected());
                DefineMetricAggregates(perfmonMetricLogger);

                // Create performance monitor counters in the operating system
                perfmonMetricLogger.CreatePerformanceCounters();

                distributor.AddLogger(consoleMetricLogger);
                distributor.AddLogger(fileMetricLogger);
                distributor.AddLogger(accessMetricLogger);
                distributor.AddLogger(perfmonMetricLogger);
                consoleMetricLogger.Start();
                fileMetricLogger.Start();
                accessMetricLogger.Connect();
                accessMetricLogger.Start();
                perfmonMetricLogger.Start();

                // Connect to the MethodInvocationRemoteReceiver
                tcpSender.Connect();
                tcpReceiver.Connect();

                // Setup the layers of the application MVP model
                MainView mainView = new MainView();
                Model model = new Model(methodInvocationRemoteSender);
                Presenter presenter = new Presenter(mainView, model);
                mainView.Presenter = presenter;

                // Start the application
                Application.Run(mainView);

                // Disconnect from the MethodInvocationRemoteReceiver
                tcpReceiver.Disconnect();
                tcpSender.Disconnect();

                // Stop the metric loggers
                consoleMetricLogger.Stop();
                fileMetricLogger.Stop();
                accessMetricLogger.Stop();
                accessMetricLogger.Disconnect();
                perfmonMetricLogger.Stop();
            }
        }

        /// <summary>
        /// Defines metric aggregates on the specified metric aggregate logger.
        /// </summary>
        /// <param name="aggregateLogger">The logger to define the aggregates on.</param>
        private static void DefineMetricAggregates(IMetricAggregateLogger aggregateLogger)
        {
            aggregateLogger.DefineMetricAggregate(new SerializedMethodInvocationSize(0), new RemoteMethodSent(), "BytesSentPerRemoteMethodSent", "The average number of bytes sent per remote method sent");
            aggregateLogger.DefineMetricAggregate(new ReceivedMessageSize(0), new RemoteMethodSent(), "BytesReceivedPerRemoteMethodSent", "The average number of bytes received per remote method sent");
            aggregateLogger.DefineMetricAggregate(new MessageSendTime(), new RemoteMethodSent(), "MessageSendTimePerRemoteMethodSent", "The average message send time per remote method sent");
            aggregateLogger.DefineMetricAggregate(new MessageReceiveTime(), new RemoteMethodSent(), "MessageReceiveTimePerRemoteMethodSent", "The average message receive time per remote method sent");
            aggregateLogger.DefineMetricAggregate(new RemoteMethodSendTime(), new RemoteMethodSent(), "AverageRemoteMethodSendTime", "The average time taken to send and invoke a remote method");
            aggregateLogger.DefineMetricAggregate(new SerializedMethodInvocationSize(0), TimeUnit.Second, "BytesSentPerSecond", "The average number of bytes sent per second");
            aggregateLogger.DefineMetricAggregate(new ReceivedMessageSize(0), TimeUnit.Second, "BytesReceivedPerSecond", "The average number of bytes received per second");
            aggregateLogger.DefineMetricAggregate(new RemoteMethodSent(), TimeUnit.Minute, "RemoteMethodsSentPerMinute", "The average number of remote methods sent per minute");
            aggregateLogger.DefineMetricAggregate(new ReturnValueDeserializeTime(), "TimeSpentDeserializingReturnValue", "The fraction of total runtime spent deserializing return values");
        }
    }
}
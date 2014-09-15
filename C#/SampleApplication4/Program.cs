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
using System.Text;
using System.Windows.Forms;
using MethodInvocationRemoting;
using ApplicationLogging;
using ApplicationMetrics;

namespace SampleApplication4
{
    /// <summary>
    /// Method Invocation Remoting framework fourth sample application.
    /// </summary>
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Setup logger and MethodInvocationRemoting objects
            using (FileApplicationLogger remoteSenderLog = new FileApplicationLogger(LogLevel.Debug, '|', "  ", @"C:\Temp\C#Sender.log"))
            using (TcpRemoteSender tcpSender = new TcpRemoteSender(System.Net.IPAddress.Loopback, 55000, 10, 1000, 30000, 25, remoteSenderLog))
            using (TcpRemoteReceiver tcpReceiver = new TcpRemoteReceiver(55001, 10, 1000, 25, remoteSenderLog))
            {
                RemoteReceiverDecompressor decompressorReceiver = new RemoteReceiverDecompressor(tcpReceiver, remoteSenderLog);
                MethodInvocationSerializer serializer = new MethodInvocationSerializer(new SerializerOperationMap(), remoteSenderLog);
                MethodInvocationRemoteSender methodInvocationRemoteSender = new MethodInvocationRemoteSender(serializer, tcpSender, decompressorReceiver, remoteSenderLog);

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
            }
        }
    }
}

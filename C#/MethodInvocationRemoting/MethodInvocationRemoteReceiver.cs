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
using System.Threading;
using ApplicationLogging;
using ApplicationMetrics;
using MethodInvocationRemotingMetrics;

namespace MethodInvocationRemoting
{
    //******************************************************************************
    //
    // Class: MethodInvocationRemoteReceiver
    //
    //******************************************************************************
    /// <summary>
    /// Receives method invocations (represented by MethodInvocationRemoting.IMethodInvocation objects) from remote locations.
    /// </summary>
    public class MethodInvocationRemoteReceiver : IMethodInvocationRemoteReceiver
    {
        private IMethodInvocationSerializer serializer;
        private IRemoteSender sender;
        private IRemoteReceiver receiver;
        private Thread receiveLoopThread;
        private volatile bool cancelRequest;
        private IApplicationLogger logger;
        private LoggingUtilities loggingUtilities;
        private MetricsUtilities metricsUtilities;

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="E:MethodInvocationRemoting.IMethodInvocationRemoteReceiver.MethodInvocationReceived"]/*'/>
        public event MethodInvocationReceivedEventHandler MethodInvocationReceived;

        //------------------------------------------------------------------------------
        //
        // Method: MethodInvocationRemoteReceiver (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MethodInvocationRemoteReceiver class.
        /// </summary>
        /// <param name="serializer">Object to use to deserialize method invocations.</param>
        /// <param name="sender">Object to use to send serialized method invocation return values.</param>
        /// <param name="receiver">Object to use to receive serialized method invocations.</param>
        public MethodInvocationRemoteReceiver(IMethodInvocationSerializer serializer, IRemoteSender sender, IRemoteReceiver receiver)
        {
            this.serializer = serializer;
            this.sender = sender;
            this.receiver = receiver;
            logger = new ConsoleApplicationLogger(LogLevel.Information, '|', "  ");
            loggingUtilities = new LoggingUtilities(logger);
            metricsUtilities = new MetricsUtilities(new NullMetricLogger());
        }

        //------------------------------------------------------------------------------
        //
        // Method: MethodInvocationRemoteReceiver (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MethodInvocationRemoteReceiver class.
        /// </summary>
        /// <param name="serializer">Object to use to deserialize method invocations.</param>
        /// <param name="sender">Object to use to send serialized method invocation return values.</param>
        /// <param name="receiver">Object to use to receive serialized method invocations.</param>
        /// <param name="logger">The logger to write log events to.</param>
        public MethodInvocationRemoteReceiver(IMethodInvocationSerializer serializer, IRemoteSender sender, IRemoteReceiver receiver, IApplicationLogger logger)
            : this(serializer, sender, receiver)
        {
            this.logger = logger;
            loggingUtilities = new LoggingUtilities(logger);
        }

        //------------------------------------------------------------------------------
        //
        // Method: MethodInvocationRemoteReceiver (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MethodInvocationRemoteReceiver class.
        /// </summary>
        /// <param name="serializer">Object to use to deserialize method invocations.</param>
        /// <param name="sender">Object to use to send serialized method invocation return values.</param>
        /// <param name="receiver">Object to use to receive serialized method invocations.</param>
        /// <param name="metricLogger">The metric logger to write metric and instrumentation events to.</param>
        public MethodInvocationRemoteReceiver(IMethodInvocationSerializer serializer, IRemoteSender sender, IRemoteReceiver receiver, IMetricLogger metricLogger)
            : this(serializer, sender, receiver)
        {
            metricsUtilities = new MetricsUtilities(metricLogger);
        }

        //------------------------------------------------------------------------------
        //
        // Method: MethodInvocationRemoteReceiver (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MethodInvocationRemoteReceiver class.
        /// </summary>
        /// <param name="serializer">Object to use to deserialize method invocations.</param>
        /// <param name="sender">Object to use to send serialized method invocation return values.</param>
        /// <param name="receiver">Object to use to receive serialized method invocations.</param>
        /// <param name="logger">The logger to write log events to.</param>
        /// <param name="metricLogger">The metric logger to write metric and instrumentation events to.</param>
        public MethodInvocationRemoteReceiver(IMethodInvocationSerializer serializer, IRemoteSender sender, IRemoteReceiver receiver, IApplicationLogger logger, IMetricLogger metricLogger)
            : this(serializer, sender, receiver)
        {
            this.logger = logger;
            loggingUtilities = new LoggingUtilities(logger);
            metricsUtilities = new MetricsUtilities(metricLogger);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationRemoteReceiver.Receive"]/*'/>
        public void Receive()
        {
            cancelRequest = false;

            receiveLoopThread = new Thread(delegate()
            {
                while (cancelRequest == false)
                {
                    try
                    {
                        string serializedMethodInvocation = receiver.Receive();
                        if (serializedMethodInvocation != "")
                        {
                            metricsUtilities.Begin(new RemoteMethodReceiveTime());

                            IMethodInvocation receivedMethodInvocation = serializer.Deserialize(serializedMethodInvocation);
                            MethodInvocationReceived(this, new MethodInvocationReceivedEventArgs(receivedMethodInvocation));
                            loggingUtilities.Log(this, LogLevel.Information, "Received method invocation '" + receivedMethodInvocation.Name + "'.");
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Failed to invoke method.", e);
                    }
                }
            });
            receiveLoopThread.Name = "MethodInvocationRemoting.MethodInvocationRemoteReceiver message receive worker thread.";
            receiveLoopThread.Start();
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationRemoteReceiver.SendReturnValue(System.Object)"]/*'/>
        public void SendReturnValue(object returnValue)
        {
            try
            {
                string serializedReturnValue = serializer.SerializeReturnValue(returnValue);
                sender.Send(serializedReturnValue);
                metricsUtilities.End(new RemoteMethodReceiveTime());
                metricsUtilities.Increment(new RemoteMethodReceived());
                loggingUtilities.Log(this, LogLevel.Information, "Sent return value.");
            }
            catch (Exception e)
            {
                throw new Exception("Failed to send return value.", e);
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationRemoteReceiver.SendVoidReturn"]/*'/>
        public void SendVoidReturn()
        {
            try
            {
                sender.Send(serializer.VoidReturnValue);
                metricsUtilities.End(new RemoteMethodReceiveTime());
                metricsUtilities.Increment(new RemoteMethodReceived());
                loggingUtilities.Log(this, LogLevel.Information, "Sent void return value.");
            }
            catch (Exception e)
            {
                throw new Exception("Failed to send void return value.", e);
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationRemoteReceiver.CancelReceive"]/*'/>
        public void CancelReceive()
        {
            cancelRequest = true;
            receiver.CancelReceive();
            receiveLoopThread.Join();

            loggingUtilities.Log(this, LogLevel.Information, "Receive operation cancelled.");
        }
    }
}

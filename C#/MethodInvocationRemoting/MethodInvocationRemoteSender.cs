/*
 * Copyright 2016 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
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
using ApplicationLogging;
using ApplicationMetrics;
using MethodInvocationRemotingMetrics;

namespace MethodInvocationRemoting
{
    //******************************************************************************
    //
    // Class: MethodInvocationRemoteSender
    //
    //******************************************************************************
    /// <summary>
    /// Sends method invocations (represented by MethodInvocationRemoting.IMethodInvocation objects) to remote locations.
    /// </summary>
    public class MethodInvocationRemoteSender : IMethodInvocationRemoteSender
    {
        private IMethodInvocationSerializer serializer;
        private IRemoteSender sender;
        private IRemoteReceiver receiver;
        private LoggingUtilities loggingUtilities;
        private MetricsUtilities metricsUtilities;

        //------------------------------------------------------------------------------
        //
        // Method: MethodInvocationRemoteSender (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MethodInvocationRemoteSender class.
        /// </summary>
        /// <param name="serializer">Object to use to serialize method invocations.</param>
        /// <param name="sender">Object to use to send serialized method invocations.</param>
        /// <param name="receiver">Object to use to receive serialized method invocation return values.</param>
        public MethodInvocationRemoteSender(IMethodInvocationSerializer serializer, IRemoteSender sender, IRemoteReceiver receiver)
        {
            this.serializer = serializer;
            this.sender = sender;
            this.receiver = receiver;
            ConsoleApplicationLogger logger = new ConsoleApplicationLogger(LogLevel.Information, '|', "  ");
            loggingUtilities = new LoggingUtilities(logger);
            metricsUtilities = new MetricsUtilities(new NullMetricLogger());
        }

        //------------------------------------------------------------------------------
        //
        // Method: MethodInvocationRemoteSender (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MethodInvocationRemoteSender class.
        /// </summary>
        /// <param name="serializer">Object to use to serialize method invocations.</param>
        /// <param name="sender">Object to use to send serialized method invocations.</param>
        /// <param name="receiver">Object to use to receive serialized method invocation return values.</param>
        /// <param name="logger">The logger to write log events to.</param>
        public MethodInvocationRemoteSender(IMethodInvocationSerializer serializer, IRemoteSender sender, IRemoteReceiver receiver, IApplicationLogger logger)
            : this(serializer, sender, receiver)
        {
            loggingUtilities = new LoggingUtilities(logger);
        }

        //------------------------------------------------------------------------------
        //
        // Method: MethodInvocationRemoteSender (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MethodInvocationRemoteSender class.
        /// </summary>
        /// <param name="serializer">Object to use to serialize method invocations.</param>
        /// <param name="sender">Object to use to send serialized method invocations.</param>
        /// <param name="receiver">Object to use to receive serialized method invocation return values.</param>
        /// <param name="metricLogger">The metric logger to write metric and instrumentation events to.</param>
        public MethodInvocationRemoteSender(IMethodInvocationSerializer serializer, IRemoteSender sender, IRemoteReceiver receiver, IMetricLogger metricLogger)
            : this(serializer, sender, receiver)
        {
            metricsUtilities = new MetricsUtilities(metricLogger);
        }

        //------------------------------------------------------------------------------
        //
        // Method: MethodInvocationRemoteSender (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.MethodInvocationRemoteSender class.
        /// </summary>
        /// <param name="serializer">Object to use to serialize method invocations.</param>
        /// <param name="sender">Object to use to send serialized method invocations.</param>
        /// <param name="receiver">Object to use to receive serialized method invocation return values.</param>
        /// <param name="logger">The logger to write log events to.</param>
        /// <param name="metricLogger">The metric logger to write metric and instrumentation events to.</param>
        public MethodInvocationRemoteSender(IMethodInvocationSerializer serializer, IRemoteSender sender, IRemoteReceiver receiver, IApplicationLogger logger, IMetricLogger metricLogger)
            : this(serializer, sender, receiver)
        {
            loggingUtilities = new LoggingUtilities(logger);
            metricsUtilities = new MetricsUtilities(metricLogger);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationRemoteSender.InvokeMethod(MethodInvocationRemoting.IMethodInvocation)"]/*'/>
        public object InvokeMethod(IMethodInvocation inputMethodInvocation)
        {
            metricsUtilities.Begin(new RemoteMethodSendTime());

            object returnValue;

            try
            {
                // Check that inputted method invocation does not have a void return type.
                if (inputMethodInvocation.ReturnType == null)
                {
                    throw new ArgumentException("Method invocation cannot have a void return type.", "inputMethodInvocation");
                }

                string serializedReturnValue = SerializeAndSend(inputMethodInvocation);
                try
                {
                    returnValue = serializer.DeserializeReturnValue(serializedReturnValue);
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to deserialize return value.", e);
                }
            }
            catch (Exception e)
            {
                metricsUtilities.CancelBegin(new RemoteMethodSendTime());
                throw;
            }

            metricsUtilities.End(new RemoteMethodSendTime());
            metricsUtilities.Increment(new RemoteMethodSent());
            loggingUtilities.Log(this, LogLevel.Information, "Invoked method '" + inputMethodInvocation.Name + "'.");

            return returnValue;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IMethodInvocationRemoteSender.InvokeVoidMethod(MethodInvocationRemoting.IMethodInvocation)"]/*'/>
        public void InvokeVoidMethod(IMethodInvocation inputMethodInvocation)
        {
            metricsUtilities.Begin(new RemoteMethodSendTime());

            try
            {
                // Check that inputted method invocation has a void return type.
                if (inputMethodInvocation.ReturnType != null)
                {
                    throw new ArgumentException("Method invocation must have a void return type.", "inputMethodInvocation");
                }

                string serializedReturnValue = SerializeAndSend(inputMethodInvocation);
                if (serializedReturnValue != serializer.VoidReturnValue)
                {
                    throw new Exception("Invocation of void method returned non-void.");
                }
            }
            catch (Exception e)
            {
                metricsUtilities.CancelBegin(new RemoteMethodSendTime());
                throw;
            }

            metricsUtilities.End(new RemoteMethodSendTime());
            metricsUtilities.Increment(new RemoteMethodSent());
            loggingUtilities.Log(this, LogLevel.Information, "Invoked void method '" + inputMethodInvocation.Name + "'.");
        }

        //------------------------------------------------------------------------------
        //
        // Method: SerializeAndSend
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Provides common method invocation serialization and sending functionality to public methods.
        /// </summary>
        /// <param name="inputMethodInvocation">The method invocation to serialize and send.</param>
        /// <returns>The serialized return value of the method invocation.</returns>
        private string SerializeAndSend(IMethodInvocation inputMethodInvocation)
        {
            try
            {
                string serializedMethodInvocation = serializer.Serialize(inputMethodInvocation);
                sender.Send(serializedMethodInvocation);
                return receiver.Receive();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to invoke method.", e);
            }
        }
    }
}

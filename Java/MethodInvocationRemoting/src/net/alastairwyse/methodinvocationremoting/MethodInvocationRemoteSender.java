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

package net.alastairwyse.methodinvocationremoting;

import net.alastairwyse.applicationlogging.*;
import net.alastairwyse.applicationmetrics.*;
import net.alastairwyse.methodinvocationremotingmetrics.*;

/**
 * Sends method invocations (represented by IMethodInvocation objects) to remote locations.
 * @author Alastair Wyse
 */
public class MethodInvocationRemoteSender implements IMethodInvocationRemoteSender {

    private IMethodInvocationSerializer serializer;
    private IRemoteSender sender;
    private IRemoteReceiver receiver;
    private IApplicationLogger logger;
    private IMetricLogger metricLogger;
    
    /**
     * Initialises a new instance of the MethodInvocationRemoteSender class.
     * @param serializer  Object to use to serialize method invocations.
     * @param sender      Object to use to send serialized method invocations.
     * @param receiver    Object to use to send serialized method invocations.
     */
    public MethodInvocationRemoteSender(IMethodInvocationSerializer serializer, IRemoteSender sender, IRemoteReceiver receiver) {
        this.serializer = serializer;
        this.sender = sender;
        this.receiver = receiver;
        logger = new ConsoleApplicationLogger(LogLevel.Information, '|', "  ");
        metricLogger = new NullMetricLogger();
    }
    
    /**
     * Initialises a new instance of the MethodInvocationRemoteSender class.
     * @param serializer  Object to use to serialize method invocations.
     * @param sender      Object to use to send serialized method invocations.
     * @param receiver    Object to use to send serialized method invocations.
     * @param logger      The logger to write log events to.
     */
    public MethodInvocationRemoteSender(IMethodInvocationSerializer serializer, IRemoteSender sender, IRemoteReceiver receiver, IApplicationLogger logger) {
        this(serializer, sender, receiver);
        this.logger = logger;
    }
    
    /**
     * Initialises a new instance of the MethodInvocationRemoteSender class.
     * @param serializer    Object to use to serialize method invocations.
     * @param sender        Object to use to send serialized method invocations.
     * @param receiver      Object to use to send serialized method invocations.
     * @param metricLogger  The metric logger to write metric and instrumentation events to.
     */
    public MethodInvocationRemoteSender(IMethodInvocationSerializer serializer, IRemoteSender sender, IRemoteReceiver receiver, IMetricLogger metricLogger) {
        this(serializer, sender, receiver);
        this.metricLogger = metricLogger;
    }
    
    /**
     * Initialises a new instance of the MethodInvocationRemoteSender class.
     * @param serializer    Object to use to serialize method invocations.
     * @param sender        Object to use to send serialized method invocations.
     * @param receiver      Object to use to send serialized method invocations.
     * @param logger        The logger to write log events to.
     * @param metricLogger  The metric logger to write metric and instrumentation events to.
     */
    public MethodInvocationRemoteSender(IMethodInvocationSerializer serializer, IRemoteSender sender, IRemoteReceiver receiver, IApplicationLogger logger, IMetricLogger metricLogger) {
        this(serializer, sender, receiver);
        this.logger = logger;
        this.metricLogger = metricLogger;
    }
    
    @Override
    public Object InvokeMethod(IMethodInvocation inputMethodInvocation) throws Exception {
        /* //[BEGIN_METRICS]
        metricLogger.Begin(new RemoteMethodSendTime());
        //[END_METRICS] */
        
        Object returnValue;

        try {
            // Check that inputted method invocation does not have a void return type.
            if (inputMethodInvocation.getReturnType() == null) {
                throw new IllegalArgumentException("Method invocation cannot have a void return type.");
            }
    
            String serializedReturnValue = SerializeAndSend(inputMethodInvocation);
            try {
                returnValue = serializer.DeserializeReturnValue(serializedReturnValue);
            }
            catch (Exception e) {
                throw new DeserializationException("Failed to deserialize return value.", e);
            }
        }
        catch (Exception e) {
            /* //[BEGIN_METRICS]
            metricLogger.CancelBegin(new RemoteMethodSendTime());
            //[END_METRICS] */
            throw e;
        }
        
        /* //[BEGIN_METRICS]
        metricLogger.End(new RemoteMethodSendTime());
        metricLogger.Increment(new RemoteMethodSent());
        //[END_METRICS] */
        /* //[BEGIN_LOGGING]
        try {
            logger.Log(this, LogLevel.Information, "Invoked method '" + inputMethodInvocation.getName() + "'.");
        }
        catch(Exception e) {
        }
        //[END_LOGGING] */
        
        return returnValue;
    }

    @Override
    public void InvokeVoidMethod(IMethodInvocation inputMethodInvocation) throws Exception {
        /* //[BEGIN_METRICS]
        metricLogger.Begin(new RemoteMethodSendTime());
        //[END_METRICS] */
        
        try {
            // Check that inputted method invocation has a void return type.
            if (inputMethodInvocation.getReturnType() != null) {
                throw new IllegalArgumentException("Method invocation must have a void return type.");
            }
    
            String serializedReturnValue = SerializeAndSend(inputMethodInvocation);
            if (serializedReturnValue.equals(serializer.getVoidReturnValue()) == false) {
                throw new Exception("Invocation of void method returned non-void.");
            }
        }
        catch (Exception e) {
            /* //[BEGIN_METRICS]
            metricLogger.CancelBegin(new RemoteMethodSendTime());
            //[END_METRICS] */
            throw e;
        }
        
        /* //[BEGIN_METRICS]
        metricLogger.End(new RemoteMethodSendTime());
        metricLogger.Increment(new RemoteMethodSent());
        //[END_METRICS] */
        /* //[BEGIN_LOGGING]
        try {
            logger.Log(this, LogLevel.Information, "Invoked void method '" + inputMethodInvocation.getName() + "'.");
        }
        catch(Exception e) {
        }
        //[END_LOGGING] */
    }
    
    /**
     * Provides common method invocation serialization and sending functionality to public methods.
     * @param inputMethodInvocation  The method invocation to serialize and send.
     * @return                       The serialized return value of the method invocation.
     * @throws Exception
     */
    private String SerializeAndSend(IMethodInvocation inputMethodInvocation) throws Exception
    {
        try {
            String serializedMethodInvocation = serializer.Serialize(inputMethodInvocation);
            sender.Send(serializedMethodInvocation);
            return receiver.Receive();
        }
        catch (Exception e) {
            throw new Exception("Failed to invoke method.", e);
        }
    }

}

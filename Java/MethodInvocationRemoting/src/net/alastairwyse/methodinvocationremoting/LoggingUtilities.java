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

package net.alastairwyse.methodinvocationremoting;

import net.alastairwyse.applicationlogging.*;

/**
 * Contains common methods used to write log events.
 * @author Alastair Wyse
 */
class LoggingUtilities {

    private IApplicationLogger logger;
    private int stringLengthLimit;
    
    /**
     * Initialises a new instance of the LoggingUtilities class.
     * @param  logger  The logger to write log events to.
     */
    public LoggingUtilities(IApplicationLogger logger) {
        stringLengthLimit = 120;
        this.logger = logger;
    }
    
    /**
     * Logs details of a received message.
     * @param   source     The object which created the log event.
     * @param   message    The received message.
     * @throws  Exception  If an error occurs during the logging process.
     */
    public void LogMessageReceived(Object source, String message) throws Exception {
        LogLongString("Received message", "Complete message content:", source, message);
    }

    /**
     * Logs details of a serialized or deserialized parameter.
     * @param   operation   The operation which was performed (should usually be set to 'Serialized' or 'Deserialized').
     * @param   parameter   The parameter that was serialized or deserialized.
     * @throws  Exception   If an error occurs during the logging process.
     */
    public void LogParameter(Object source, String operation, Object parameter) throws Exception {
        if (parameter == null) {
            logger.Log(source, LogLevel.Debug, operation + " null parameter.");
        }
        else {
            logger.Log(source, LogLevel.Debug, operation + " parameter of type '" + parameter.getClass().getName() + "'.");
        }
    }

    /**
     * Logs details of a deserialized return value.
     * @param   source       The object which created the log event.
     * @param   returnValue  The return value.
     * @throws  Exception    If an error occurs during the logging process.
     */
    public void LogDeserializedReturnValue(Object source, Object returnValue) throws Exception {
        if (returnValue == null) {
            logger.Log(source, LogLevel.Information, "Deserialized string to null return value");
        }
        else {
            logger.Log(source, LogLevel.Information, "Deserialized string to return value of type '" + returnValue.getClass().getName() + "'.");
        }
    }
    
    /**
     * Logs details of a serialized item (e.g. method invocation or return value).
     * @param   source          The object which created the log event.
     * @param   serializedItem  The serialized item.
     * @param   itemType        The type of the serialized item.
     * @throws  Exception       If an error occurs during the logging process.
     */
    public void LogSerializedItem(Object source, String serializedItem, String itemType) throws Exception {
        LogLongString("Serialized " + itemType + " to string", "Complete string content:", source, serializedItem);
    }

    /**
     * Logs details of a decompressed string.
     * @param   source       The object which created the log event.
     * @param   inputString  The decompressed string.
     * @throws  Exception    If an error occurs during the logging process.
     */
    public void LogDecompressedString(Object source, String inputString) throws Exception {
        LogLongString("Created decompressed string", "Complete string content:", source, inputString);
    } 

    /**
     * Logs details of a compressed string.
     * @param   source       The object which created the log event.
     * @param   inputString  The compressed string.
     * @throws  Exception    If an error occurs during the logging process.
     */
    public void LogCompressedString(Object source, String inputString) throws Exception {
        LogLongString("Created compressed string", "Complete string content:", source, inputString);
    }
    
    /**
     * Logs details of a large string, and truncates the information level logging if longer than the definied limit.
     * @param   informationPrefix  The string to prefix to the information level log entry.
     * @param   debugPrefix        The string to prefix to the debug level log entry.
     * @param   source             The object which created the log event.
     * @param   inputString        The large string to log.
     * @throws  Exception          If an error occurs during the logging process.
     */
    private void LogLongString(String informationPrefix, String debugPrefix, Object source, String inputString) throws Exception {
        if (inputString.length() > stringLengthLimit) {
            logger.Log(source, LogLevel.Information, informationPrefix + " '" + inputString.substring(0, stringLengthLimit) + "' (truncated).");
        }
        else {
            logger.Log(source, LogLevel.Information, informationPrefix + " '" + inputString + "'.");
        }
        logger.Log(source, LogLevel.Debug, debugPrefix + " '" + inputString + "'.");
    }
}

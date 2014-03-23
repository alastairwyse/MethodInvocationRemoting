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

package net.alastairwyse.applicationlogging;

/**
 * Defines methods to record log events and information about an application (e.g. operational information, status, error/exception information, debug, etc...).
 * @author Alastair Wyse
 */
public interface IApplicationLogger {
    
    /**
     * Writes the log information.
     * @param level       The level of importance of the log event.
     * @param text        The details of the log event.
     * @throws Exception  If an error occurs when attempting to write the log information.
     */
    void Log(LogLevel level, String text) throws Exception;

    /**
     * Writes the log information.
     * @param source      The object which created the log event.
     * @param level       The level of importance of the log event.
     * @param text        The details of the log event.
     * @throws Exception  If an error occurs when attempting to write the log information.
     */
    void Log(Object source, LogLevel level, String text) throws Exception;
    
    /**
     * Writes the log information.
     * @param eventIdentifier  An ID number which uniquely identifies the log event.
     * @param level            The level of importance of the log event.
     * @param text             The details of the log event.
     * @throws Exception       If an error occurs when attempting to write the log information.
     */
    void Log(int eventIdentifier, LogLevel level, String text) throws Exception;

    /**
     * Writes the log information.
     * @param source           The object which created the log event.
     * @param eventIdentifier  An ID number which uniquely identifies the log event.
     * @param level            The level of importance of the log event.
     * @param text             The details of the log event.
     * @throws Exception       If an error occurs when attempting to write the log information.
     */
    void Log(Object source, int eventIdentifier, LogLevel level, String text) throws Exception;
    
    /**
     * Writes the log information.
     * @param level            The level of importance of the log event.
     * @param text             The details of the log event.
     * @param sourceException  The exception which caused the log event.
     * @throws Exception       If an error occurs when attempting to write the log information.
     */
    void Log(LogLevel level, String text, Exception sourceException) throws Exception;

    /**
     * Writes the log information.
     * @param source           The object which created the log event.
     * @param level            The level of importance of the log event.
     * @param text             The details of the log event.
     * @param sourceException  The exception which caused the log event.
     * @throws Exception       If an error occurs when attempting to write the log information.
     */
    void Log(Object source, LogLevel level, String text, Exception sourceException) throws Exception;
    
    /**
     * Writes the log information.
     * @param eventIdentifier  An ID number which uniquely identifies the log event.
     * @param level            The level of importance of the log event.
     * @param text             The details of the log event.
     * @param sourceException  The exception which caused the log event.
     * @throws Exception       If an error occurs when attempting to write the log information.
     */
    void Log(int eventIdentifier, LogLevel level, String text, Exception sourceException) throws Exception;
    
    /**
     * Writes the log information.
     * @param source           The object which created the log event.
     * @param eventIdentifier  An ID number which uniquely identifies the log event.
     * @param level            The level of importance of the log event.
     * @param text             The details of the log event.
     * @param sourceException  The exception which caused the log event.
     * @throws Exception       If an error occurs when attempting to write the log information.
     */
    void Log(Object source, int eventIdentifier, LogLevel level, String text, Exception sourceException) throws Exception;
}

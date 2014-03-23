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

package net.alastairwyse.applicationlogging.adapters;

import net.alastairwyse.applicationlogging.*;
import org.apache.log4j.*;

/**
 * Adapts the net.alastairwyse.applicationlogging.IApplicationLogger interface to an implementation of the log4j.Logger class.
 * <b>Note</b> that the Logger class in log4j does not provide a property similar to the applicationlogging eventIdentifier by default (i.e. an id number for the log event).  Hence the eventIdentifier is not supported (i.e. not passed to log4j) in this adapter class.  Similarly there is not explicit property for the object creating the log event on the Logger interface.  In log4j this is usually defined when constructing the Logger class (e.g. through the getLogger() method), hence to follow the log4j pattern a separate ApplicationLoggingLog4JAdapter should be provided to each object performing logging.
 * @author Alastair Wyse
 */
public class ApplicationLoggingLog4JAdapter implements IApplicationLogger {

    private Logger logger;
    
    /**
     * Initialises a new instance of the ApplicationLoggingLog4JAdapter class.
     * @param logger  The Logger class used to log messages into the log4j framework.
     */
    public ApplicationLoggingLog4JAdapter(Logger logger) {
        this.logger = logger;
    }
    
    @Override
    public void Log(LogLevel level, String text) throws Exception {
        PerformLogging(level, text, null);
    }

    @Override
    public void Log(Object source, LogLevel level, String text) throws Exception {
        PerformLogging(level, text, null);
    }

    @Override
    public void Log(int eventIdentifier, LogLevel level, String text) throws Exception {
        PerformLogging(level, text, null);
    }

    @Override
    public void Log(Object source, int eventIdentifier, LogLevel level, String text) throws Exception {
        PerformLogging(level, text, null);
    }

    @Override
    public void Log(LogLevel level, String text, Exception sourceException) throws Exception {
        PerformLogging(level, text, sourceException);
    }

    @Override
    public void Log(Object source, LogLevel level, String text, Exception sourceException) throws Exception {
        PerformLogging(level, text, sourceException);
    }

    @Override
    public void Log(int eventIdentifier, LogLevel level, String text, Exception sourceException) throws Exception {
        PerformLogging(level, text, sourceException);
    }

    @Override
    public void Log(Object source, int eventIdentifier, LogLevel level, String text, Exception sourceException) throws Exception {
        PerformLogging(level, text, sourceException);
    }

    /**
     * Calls the underlying log4j.Logger object to log a message.
     * In ApplicationLogging the level of a log event is passed to the Log() method, but in log4j different levels have their own method.  The main purpose of this method is to translate the level parameter received in the ApplicationLogging method call, into the appropriate log4j method call.
     * @param level            The level of importance of the log event.
     * @param text             The details of the log event.
     * @param sourceException  The exception which caused the log event.
     */
    private void PerformLogging(LogLevel level, String text, Exception sourceException) {
        switch (level) {
            case Critical:
                if (sourceException == null) {
                    logger.fatal(text);
                }
                else {
                    logger.fatal(text, sourceException);
                }
                break;
                
            case Error:
                if (sourceException == null) {
                    logger.error(text);
                }
                else {
                    logger.error(text, sourceException);
                }
                break;
                
            case Warning:
                if (sourceException == null) {
                    logger.warn(text);
                }
                else {
                    logger.warn(text, sourceException);
                }
                break;
                
            case Information:
                if (sourceException == null) {
                    logger.info(text);
                }
                else {
                    logger.info(text, sourceException);
                }
                break;
                
            case Debug:
                if (sourceException == null) {
                    logger.debug(text);
                }
                else {
                    logger.debug(text, sourceException);
                }
                break;
        }
    }
}

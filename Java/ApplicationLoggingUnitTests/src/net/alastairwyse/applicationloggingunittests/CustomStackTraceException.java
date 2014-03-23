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

package net.alastairwyse.applicationloggingunittests;

import java.io.*;

/**
 * Derives from java.lang.Exception, and overrides the printStackTrace() method to print a fixed string for use in unit tests.
 * @author Alastair Wyse
 */
public class CustomStackTraceException extends Exception {

    /**
     * Initialises a new instance of the CustomStackTraceException class.
     * @param message  The message that describes the error.
     */
    public CustomStackTraceException(String message) {
        super(message);
    }
    
    @Override
    public void printStackTrace(PrintWriter s)  {
        s.println("java.lang.Exception: Error connecting to /127.0.0.1:50000.");
        s.println("\tat net.alastairwyse.methodinvocationremoting.TcpRemoteSender.AttemptConnect(TcpRemoteSender.java:187)");
        s.println("\tat net.alastairwyse.methodinvocationremoting.TcpRemoteSender.Connect(TcpRemoteSender.java:136)");
        s.println("\tat net.alastairwyse.applicationloggingunittests.FileApplicationLoggerTests.LogSuccessTests(Test.java:135)");
    }
}

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

package net.alastairwyse.operatingsystemabstraction;

import java.io.*;

/**
 * Provides an abstraction of the java.io.FileWriter class, to facilitate mocking and unit testing.
 * @author Alastair Wyse
 */
public interface IFileWriter {

    /**
     * Writes a string.
     * @param   str          String to be written.
     * @throws  IOException  If an I/O error occurs.
     */
    void write(String str) throws IOException;
    
    /**
     * Flushes the stream.
     * @throws  IOException  If an I/O error occurs.
     */
    void flush() throws IOException;
    
    /**
     * Closes the stream, flushing it first. Once the stream has been closed, further write() or flush() invocations will cause an IOException to be thrown. Closing a previously closed stream has no effect.
     * @throws  IOException  If an I/O error occurs.
     */
    void close() throws IOException;
}

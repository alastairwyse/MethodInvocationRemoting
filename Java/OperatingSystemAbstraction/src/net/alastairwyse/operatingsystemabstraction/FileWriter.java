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

import java.io.IOException;

/**
 * Provides an abstraction of the java.io.FileWriter class, to facilitate mocking and unit testing.
 * @author Alastair Wyse
 */
public class FileWriter implements IFileWriter {

    private java.io.FileWriter fileWriter;
    
    /**
     * Initialises a new instance of the FileWriter class.
     * @param fileName      The system-dependent filename.
     * @param append        boolean if true, then data will be written to the end of the file rather than the beginning.
     * @throws IOException  if the named file exists but is a directory rather than a regular file, does not exist but cannot be created, or cannot be opened for any other reason.
     */
    public FileWriter(String fileName, boolean append) throws IOException {
        fileWriter = new java.io.FileWriter(fileName, append);
    }
    
    @Override
    public void write(String str) throws IOException {
        fileWriter.write(str);
    }

    @Override
    public void flush() throws IOException {
        fileWriter.flush();
    }

    @Override
    public void close() throws IOException {
        fileWriter.close();
    }
}

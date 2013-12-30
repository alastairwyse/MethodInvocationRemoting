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

package net.alastairwyse.operatingsystemabstraction;

import java.nio.file.*;
import java.nio.charset.*;
import java.io.*;

/**
 * Provides an abstraction of common operations on a file, to facilitate mocking and unit testing.
 * @author Alastair Wyse
 */
public class File implements IFile {

    private String path;
    private final String fileEncoding = "UTF-8";
    
    /**
     * Initialises a new instance of the File class.
     * @param path  The full path to the file.
     */
    public File(String path) {
        this.path = path;
    }
    
    @Override
    public String getPath() {
        return path;
    }

    @Override
    public void setPath(String path) {
        this.path = path;
    }

    @Override
    public String ReadAll() throws Exception {
        String returnString;
        
        byte[] fileContents = Files.readAllBytes(Paths.get(path));
        returnString = new String(fileContents, fileEncoding);

        return returnString;
    }

    @Override
    public void WriteAll(String data) throws Exception {
        try (BufferedWriter writer = Files.newBufferedWriter(Paths.get(path), Charset.forName(fileEncoding), new OpenOption[] {StandardOpenOption.CREATE_NEW, StandardOpenOption.WRITE}))
        {
            writer.write(data);
            writer.close();
        }
    }
}

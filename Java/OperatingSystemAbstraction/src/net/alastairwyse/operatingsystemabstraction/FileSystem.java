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

/**
 * Provides an abstraction of common operations on the file system, to facilitate mocking and unit testing.
 * @author Alastair Wyse
 */
public class FileSystem implements IFileSystem {

    /**
     * Initializes a new instance of the FileSystem class.
     */
    public FileSystem() {
    }
    
    @Override
    public Boolean CheckFileExists(String path) {
        return Files.exists(Paths.get(path));
    }

    @Override
    public void DeleteFile(String path) throws Exception {
        Files.delete(Paths.get(path));
    }

}

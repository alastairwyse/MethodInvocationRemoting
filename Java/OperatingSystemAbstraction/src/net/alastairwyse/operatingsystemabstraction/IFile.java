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

/**
 * Represents a file within the file system.
 * @author Alastair Wyse
 */
public interface IFile {

    /**
     * @return  The full path to the file.
     */
    String getPath();
    
    /**
     * Sets the full path to the file.
     * @param path  The full path to the file.
     */
    void setPath(String path);
    
    /**
     * Reads the entire contents of the file as a string.
     * @return             The contents of the file.
     * @throws  Exception  if an error occurs when attempting to read the file.
     */
    String ReadAll() throws Exception;
    
    /**
     * Overwrites the entire contents of the file with the specified string.
     * @param   data       The data to write to the file.
     * @throws  Exception  if an error occurs when attempting to write to the file.
     */
    void WriteAll(String data) throws Exception;
}

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
 * Represents the file system.
 * @author Alastair Wyse
 */
public interface IFileSystem {

    /**
     * Checks whether a file at the given path exists in the file system.
     * @param path  The full path to the file.
     * @return      Whether the specified file exists.
     */
    public Boolean CheckFileExists(String path);
    
    /**
     * Deletes the file specified by the given path.
     * @param path        The full path to the file.
     * @throws Exception  if an error occurs when attempting to delete the file.
     */
    public void DeleteFile(String path) throws Exception;
}

/*
 * Copyright 2015 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
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

package net.alastairwyse.applicationmetrics;

/**
 * Defines methods associated with implementations of interface IBufferProcessingStrategy, which handle when the metric events stored in buffers are removed from the buffers and processed.
 * @author Alastair Wyse
 */
public interface IBufferProcessedEventHandler {

    /**
     * Handles when the metric events stored in buffers are removed from the buffers and processed.
     * @throws  Exception  if an error occurs when processing the metric events stored in the buffers.
     */
    public void BufferProcessed() throws Exception;
}

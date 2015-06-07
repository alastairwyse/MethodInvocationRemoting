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

package net.alastairwyse.applicationmetricsunittests;

import net.alastairwyse.applicationmetrics.*;

/**
 * A simple implementation of interface IBufferProcessedEventHandler, which increments a counter each time the BufferProcessed() method is called.
 * @author Alastair Wyse
 */
class BufferProcessedEventCounter implements IBufferProcessedEventHandler {

    private int bufferProcessedEventRaisedCount = 0;
    
    /**
     * @return  The number of times the BufferProcessed() method has been called.
     */
    public int getBufferProcessedEventRaisedCount() {
        return bufferProcessedEventRaisedCount;
    }
    
    @Override
    public void BufferProcessed() throws Exception {
        bufferProcessedEventRaisedCount++;
    }

}

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
 * Defines methods which interact with a strategy to process buffered metric events in the MetricLoggerBuffer class.  MetricLoggerBuffer objects utilizing the strategy should call the relevant 'Notify' methods as necessary, and implement interface 'IBufferProcessedEventHandler' to be notified of when to process the contents of their buffers.
 * @author Alastair Wyse
 */
public interface IBufferProcessingStrategy {

    /**
     * Sets the object which should handle processing the contents of the buffers.
     * @param  bufferProcessedEventHandler  The object to handle processing the contents of the buffers.
     */
    public void setBufferProcessedEventHandler(IBufferProcessedEventHandler bufferProcessedEventHandler);
    
    /**
     * Starts the buffer processing (e.g. if the implementation of the strategy uses a worker thread, this method starts the worker thread).
     */
    public void Start();

    /**
     * Processes any metric events remaining in the buffers, and stops the buffer processing (e.g. if the implementation of the strategy uses a worker thread, this method stops the worker thread).
     * @throws  InterruptedException  if an error occurs when stopping the worker thread.
     */
    public void Stop() throws InterruptedException;

    /**
     * Stops the buffer processing (e.g. if the implementation of the strategy uses a worker thread, this method stops the worker thread).
     * There may be cases where the client code does not want to process any remaining metric events, e.g. in the case that the method was called as part of an exception handling routine.  This overload of the Stop() method allows the client code to specify this behaviour.
     * @param   processRemainingBufferedMetricEvents  Whether any metric events remaining in the buffers should be processed.
     * @throws  InterruptedException                  if an error occurs when stopping the worker thread.
     */
    public void Stop(boolean processRemainingBufferedMetricEvents) throws Exception;

    /**
     * Notifies the buffer processing strategy that a count metric event was added to the buffer.
     */
    public void NotifyCountMetricEventBuffered();

    /**
     * Notifies the buffer processing strategy that an amount metric event was added to the buffer.
     */
    public void NotifyAmountMetricEventBuffered();

    /**
     * Notifies the buffer processing strategy that a status metric event was added to the buffer.
     */
    public void NotifyStatusMetricEventBuffered();

    /**
     * Notifies the buffer processing strategy that an interval metric event was added to the buffer.
     */
    public void NotifyIntervalMetricEventBuffered();

    /**
     * Notifies the buffer processing strategy that the buffer holding count metric events was cleared.
     */
    public void NotifyCountMetricEventBufferCleared();

    /**
     * Notifies the buffer processing strategy that the buffer holding amount metric events was cleared.
     */
    public void NotifyAmountMetricEventBufferCleared();

    /**
     * Notifies the buffer processing strategy that the buffer holding status metric events was cleared.
     */
    public void NotifyStatusMetricEventBufferCleared();

    /**
     * Notifies the buffer processing strategy that the buffer holding interval metric events was cleared.
     */
    public void NotifyIntervalMetricEventBufferCleared();
}

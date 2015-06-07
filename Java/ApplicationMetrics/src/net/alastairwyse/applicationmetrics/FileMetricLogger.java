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

import java.io.IOException;
import java.lang.Thread.*;
import java.text.*;
import java.util.*;

import net.alastairwyse.operatingsystemabstraction.*;

/**
 * Writes metric and instrumentation events for an application to a file.
 * @author Alastair Wyse
 */
public class FileMetricLogger extends MetricLoggerBuffer implements AutoCloseable {

    private static String dateTimeFormat = "yyyy-MM-dd HH:mm:ss.SSS";
    private SimpleDateFormat dateFormatter;
    private char separatorCharacter;
    private IFileWriter fileWriter;

    /**
     * Initialises a new instance of the FileMetricLogger class.
     * This constructor defaults to using the LoopingWorkerThreadBufferProcessor as the buffer processing strategy, and is maintained for backwards compatibility.
     * @param   separatorCharacter            The character to use to separate fields (e.g. date/time stamp, metric name) in the file.
     * @param   filePath                      The full path of the file to write the metric events to.
     * @param   dequeueOperationLoopInterval  The time to wait in between iterations of the worker thread which dequeues metric events and writes them to the file.
     * @param   intervalMetricChecking        Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).
     * @param   exceptionHandler              Handler for any uncaught exceptions occurring on the worker thread.
     * @throws  IOException                   If the file specified in the filePath parameter cannot be opened.
     */
    public FileMetricLogger(char separatorCharacter, String filePath, int dequeueOperationLoopInterval, boolean intervalMetricChecking, UncaughtExceptionHandler exceptionHandler) throws IOException {
        this(separatorCharacter, filePath, new LoopingWorkerThreadBufferProcessor(dequeueOperationLoopInterval, exceptionHandler), intervalMetricChecking);
    }
    
    /**
     * Initialises a new instance of the FileMetricLogger class.
     * This constructor defaults to using the LoopingWorkerThreadBufferProcessor as the buffer processing strategy, and is maintained for backwards compatibility.
     * @param   separatorCharacter            The character to use to separate fields (e.g. date/time stamp, metric name) in the file.
     * @param   filePath                      The full path of the file to write the metric events to.
     * @param   appendToFile                  Whether to append to an existing file (if it exists) or overwrite.  A value of true causes appending.
     * @param   dequeueOperationLoopInterval  The time to wait in between iterations of the worker thread which dequeues metric events and writes them to the file.
     * @param   intervalMetricChecking        Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).
     * @param   exceptionHandler              Handler for any uncaught exceptions occurring on the worker thread.
     * @throws  IOException                   If the file specified in the filePath parameter cannot be opened.
     */
    public FileMetricLogger(char separatorCharacter, String filePath, boolean appendToFile, int dequeueOperationLoopInterval, boolean intervalMetricChecking, UncaughtExceptionHandler exceptionHandler) throws IOException {
        this(separatorCharacter, filePath, appendToFile, new LoopingWorkerThreadBufferProcessor(dequeueOperationLoopInterval, exceptionHandler), intervalMetricChecking);
    }

    /**
     * Initialises a new instance of the FileMetricLogger class.
     * @param   separatorCharacter        The character to use to separate fields (e.g. date/time stamp, metric name) in the file.
     * @param   filePath                  The full path of the file to write the metric events to.
     * @param   bufferProcessingStrategy  Object which implements a processing strategy for the buffers (queues).
     * @param   intervalMetricChecking    Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).
     * @throws  IOException               If the file specified in the filePath parameter cannot be opened.
     */
    public FileMetricLogger(char separatorCharacter, String filePath, IBufferProcessingStrategy bufferProcessingStrategy, boolean intervalMetricChecking) throws IOException {
        super(bufferProcessingStrategy, intervalMetricChecking);
        InitialisePrivateMembers(separatorCharacter);
        fileWriter = new FileWriter(filePath, false);
    }
    
    /**
     * Initialises a new instance of the FileMetricLogger class.
     * @param   separatorCharacter        The character to use to separate fields (e.g. date/time stamp, metric name) in the file.
     * @param   filePath                  The full path of the file to write the metric events to.
     * @param   appendToFile              Whether to append to an existing file (if it exists) or overwrite.  A value of true causes appending.
     * @param   bufferProcessingStrategy  Object which implements a processing strategy for the buffers (queues).
     * @param   intervalMetricChecking    Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).
     * @throws  IOException               If the file specified in the filePath parameter cannot be opened.
     */
    public FileMetricLogger(char separatorCharacter, String filePath, boolean appendToFile, IBufferProcessingStrategy bufferProcessingStrategy, boolean intervalMetricChecking) throws IOException {
        super(bufferProcessingStrategy, intervalMetricChecking);
        InitialisePrivateMembers(separatorCharacter);
        fileWriter = new FileWriter(filePath, appendToFile);
    }
    
    /**
     * Initialises a new instance of the FileMetricLogger class.
     * <b>Note</b> this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
     * @param   separatorCharacter        The character to use to separate fields (e.g. date/time stamp, metric name) in the file.
     * @param   bufferProcessingStrategy  Object which implements a processing strategy for the buffers (queues).
     * @param   intervalMetricChecking    Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).
     * @param   calendarProvider          A test (mock) ICalendarProvider object.
     * @param   fileWriter                A test (mock) file writer.
     * @throws  IOException               If the file specified in the filePath parameter cannot be opened.
     */
    public FileMetricLogger(char separatorCharacter, IBufferProcessingStrategy bufferProcessingStrategy, boolean intervalMetricChecking, ICalendarProvider calendarProvider, IFileWriter fileWriter) throws IOException {
        super(bufferProcessingStrategy, intervalMetricChecking, calendarProvider);
        InitialisePrivateMembers(separatorCharacter);
        this.fileWriter = fileWriter;
    }
    
    @Override
    protected void ProcessCountMetricEvent(CountMetricEventInstance countMetricEvent) throws Exception {
        StringBuilder stringBuilder = InitializeStringBuilder(countMetricEvent.getEventTime());
        stringBuilder.append(countMetricEvent.getMetric().getName());
        fileWriter.write(stringBuilder.toString());
        fileWriter.write(System.lineSeparator());
        fileWriter.flush();
    }

    @Override
    protected void ProcessAmountMetricEvent(AmountMetricEventInstance amountMetricEvent) throws Exception {
        StringBuilder stringBuilder = InitializeStringBuilder(amountMetricEvent.getEventTime());
        stringBuilder.append(amountMetricEvent.getMetric().getName());
        AppendSeparatorCharacter(stringBuilder);
        stringBuilder.append(amountMetricEvent.getMetric().getAmount());
        fileWriter.write(stringBuilder.toString());
        fileWriter.write(System.lineSeparator());
        fileWriter.flush();
    }

    @Override
    protected void ProcessStatusMetricEvent(StatusMetricEventInstance statusMetricEvent) throws Exception {
        StringBuilder stringBuilder = InitializeStringBuilder(statusMetricEvent.getEventTime());
        stringBuilder.append(statusMetricEvent.getMetric().getName());
        AppendSeparatorCharacter(stringBuilder);
        stringBuilder.append(statusMetricEvent.getMetric().getValue());
        fileWriter.write(stringBuilder.toString());
        fileWriter.write(System.lineSeparator());
        fileWriter.flush();
    }

    @Override
    protected void ProcessIntervalMetricEvent(IntervalMetricEventInstance intervalMetricEvent, long duration) throws Exception {
        StringBuilder stringBuilder = InitializeStringBuilder(intervalMetricEvent.getEventTime());
        stringBuilder.append(intervalMetricEvent.getMetric().getName());
        AppendSeparatorCharacter(stringBuilder);
        stringBuilder.append(duration);
        fileWriter.write(stringBuilder.toString());
        fileWriter.write(System.lineSeparator());
        fileWriter.flush();
    }

    /**
     * Initialises private members of the class.
     * @param separatorCharacter  The character to use to separate fields (e.g. date/time stamp, metric name) in the file.
     */
    private void InitialisePrivateMembers(char separatorCharacter) {
        this.separatorCharacter = separatorCharacter;
        dateFormatter = new SimpleDateFormat(dateTimeFormat);
        dateFormatter.setTimeZone(TimeZone.getDefault());
    }

    /**
     * Creates and returns a StringBuilder class, with the specified timestamp written to it.
     * @param   timeStamp  The timestamp to write to the StringBuilder.
     * @return  The initialized string builder.
     */
    private StringBuilder InitializeStringBuilder(Calendar timeStamp) {
        StringBuilder returnStringBuilder = new StringBuilder();
        returnStringBuilder.append(dateFormatter.format(timeStamp.getTime()));
        AppendSeparatorCharacter(returnStringBuilder);
        return returnStringBuilder;
    }
    
    /**
     * Appends the separator character to a StringBuilder object.
     * @param  stringBuilder  The StringBuilder to append the separator character to.
     */
    private void AppendSeparatorCharacter(StringBuilder stringBuilder) {
        stringBuilder.append(" ");
        stringBuilder.append(separatorCharacter);
        stringBuilder.append(" ");
    }
    
    /**
     * Closes the metric log file.
     * @throws  IOException  if an error occurs when trying to close the underlying file.
     */
    public void Close() throws IOException {
        fileWriter.close();
    }
    
    @Override
    public void close() throws Exception {
        Close();
    }
}

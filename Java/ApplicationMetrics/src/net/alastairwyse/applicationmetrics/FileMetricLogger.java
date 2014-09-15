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

package net.alastairwyse.applicationmetrics;

import java.io.IOException;
import java.lang.Thread.*;
import java.text.*;
import java.util.*;
import java.util.concurrent.*;

import net.alastairwyse.operatingsystemabstraction.*;

/**
 * Writes metric and instrumentation events for an application to a file.
 * @author Alastair Wyse
 */
public class FileMetricLogger extends MetricLoggerBuffer implements IMetricLogger, AutoCloseable {

    private static String dateTimeFormat = "yyyy-MM-dd HH:mm:ss.SSS";
    private SimpleDateFormat dateFormatter;
    private char separatorCharacter;
    private IFileWriter fileWriter;

    /**
     * Initialises a new instance of the FileMetricLogger class.
     * @param   separatorCharacter            The character to use to separate fields (e.g. date/time stamp, metric name) in the file.
     * @param   filePath                      The full path of the file to write the metric events to.
     * @param   dequeueOperationLoopInterval  The time to wait in between iterations of the worker thread which dequeues metric events and writes them to the file.
     * @param   intervalMetricChecking        Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).
     * @param   exceptionHandler              Handler for any uncaught exceptions occurring on the worker thread.
     * @throws  IOException                   If the file specified in the filePath parameter cannot be opened.
     */
    public FileMetricLogger(char separatorCharacter, String filePath, int dequeueOperationLoopInterval, boolean intervalMetricChecking, UncaughtExceptionHandler exceptionHandler) throws IOException {
        super(dequeueOperationLoopInterval, intervalMetricChecking, exceptionHandler);
        InitialisePrivateMembers(separatorCharacter);
        fileWriter = new FileWriter(filePath, false);
    }
    
    /**
     * Initialises a new instance of the FileMetricLogger class.
     * @param   separatorCharacter            The character to use to separate fields (e.g. date/time stamp, metric name) in the file.
     * @param   filePath                      The full path of the file to write the metric events to.
     * @param   appendToFile                  Whether to append to an existing file (if it exists) or overwrite.  A value of true causes appending.
     * @param   dequeueOperationLoopInterval  The time to wait in between iterations of the worker thread which dequeues metric events and writes them to the file.
     * @param   intervalMetricChecking        Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).
     * @param   exceptionHandler              Handler for any uncaught exceptions occurring on the worker thread.
     * @throws  IOException                   If the file specified in the filePath parameter cannot be opened.
     */
    public FileMetricLogger(char separatorCharacter, String filePath, boolean appendToFile, int dequeueOperationLoopInterval, boolean intervalMetricChecking, UncaughtExceptionHandler exceptionHandler) throws IOException {
        super(dequeueOperationLoopInterval, intervalMetricChecking, exceptionHandler);
        InitialisePrivateMembers(separatorCharacter);
        fileWriter = new FileWriter(filePath, appendToFile);
    }

    /**
     * Initialises a new instance of the FileMetricLogger class.
     * <b>Note</b> this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
     * @param   separatorCharacter                  The character to use to separate fields (e.g. date/time stamp, metric name) in the file.
     * @param   dequeueOperationLoopInterval        The time to wait in between iterations of the worker thread which dequeues metric events and writes them to the file.
     * @param   intervalMetricChecking              Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).
     * @param   exceptionHandler                    Handler for any uncaught exceptions occurring on the worker thread.
     * @param   calendarProvider                    A test (mock) ICalendarProvider object.
     * @param   dequeueOperationLoopCompleteSignal  Notifies test code that an iteration of the worker thread which dequeues and processes metric events has completed.
     * @param   fileWriter                          A test (mock) file writer.
     * @throws  IOException                         If the file specified in the filePath parameter cannot be opened.
     */
    public FileMetricLogger(char separatorCharacter, int dequeueOperationLoopInterval, boolean intervalMetricChecking, UncaughtExceptionHandler exceptionHandler, ICalendarProvider calendarProvider, CountDownLatch dequeueOperationLoopCompleteSignal, IFileWriter fileWriter) throws IOException {
        super(dequeueOperationLoopInterval, intervalMetricChecking, exceptionHandler, calendarProvider, dequeueOperationLoopCompleteSignal);
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
     * @throws IOException
     */
    public void Close() throws IOException {
        fileWriter.close();
    }
    
    @Override
    public void close() throws Exception {
        Close();
    }
}

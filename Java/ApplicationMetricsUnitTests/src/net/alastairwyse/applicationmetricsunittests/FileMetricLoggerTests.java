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

import java.text.*;
import java.util.*;
import java.util.concurrent.*;

import org.junit.Before;
import org.junit.Test;

import static org.junit.Assert.assertNull;
import static org.mockito.Mockito.*;
import net.alastairwyse.applicationmetrics.*;
import net.alastairwyse.operatingsystemabstraction.*;

/**
 * Unit tests for class applicationmetrics.FileMetricLogger.
 * @author Alastair Wyse
 */
public class FileMetricLoggerTests {

    /* 
     * NOTE: See notes in class MicrosoftAccessMetricLoggerTests regarding testing of underlying worker threads.  The same comments apply to this test class
     */
    
    private String timeZoneId = "UTC";
    private SimpleDateFormat dateFormatter; 
    private ApplicationMetricsUnitTestUtilities utilities;
    private ExceptionStorer exceptionStorer;
    private IFileWriter mockFileWriter;
    private ICalendarProvider mockCalendarProvider;
    private CountDownLatch dequeueOperationLoopCompleteSignal;
    private FileMetricLogger testFileMetricLogger;
    
    @Before
    public void setUp() throws Exception {
        dateFormatter = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss.SSS");
        dateFormatter.setTimeZone(TimeZone.getDefault());
        utilities = new ApplicationMetricsUnitTestUtilities();
        exceptionStorer = new ExceptionStorer();
        mockFileWriter = mock(IFileWriter.class);
        mockCalendarProvider = mock(ICalendarProvider.class);
        dequeueOperationLoopCompleteSignal = new CountDownLatch(1);
        testFileMetricLogger = new FileMetricLogger('|', new LoopingWorkerThreadBufferProcessor(10, exceptionStorer, dequeueOperationLoopCompleteSignal), false, mockCalendarProvider, mockFileWriter); 
    }
    
    @Test
    public void IncrementSuccessTest() throws Exception {
        Calendar timeStamp1 = new GregorianCalendar(2014, 6, 14, 12, 45, 31);
        Calendar timeStamp2 = new GregorianCalendar(2014, 6, 14, 12, 45, 43);
        Calendar timeStamp3 = new GregorianCalendar(2014, 6, 15, 23, 58, 47);
        timeStamp1.setTimeZone(TimeZone.getTimeZone(timeZoneId));
        timeStamp2.setTimeZone(TimeZone.getTimeZone(timeZoneId));
        timeStamp3.setTimeZone(TimeZone.getTimeZone(timeZoneId));

        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(timeStamp1)
            .thenReturn(timeStamp2)
            .thenReturn(timeStamp3);
        
        testFileMetricLogger.Increment(new TestMessageReceivedMetric());
        testFileMetricLogger.Increment(new TestDiskReadOperationMetric());
        testFileMetricLogger.Increment(new TestMessageReceivedMetric());
        testFileMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();

        verify(mockCalendarProvider, times(3)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        verify(mockFileWriter).write(dateFormatter.format(timeStamp1.getTime()) + " | " + new TestMessageReceivedMetric().getName());
        verify(mockFileWriter).write(dateFormatter.format(timeStamp2.getTime()) + " | " + new TestDiskReadOperationMetric().getName());
        verify(mockFileWriter).write(dateFormatter.format(timeStamp3.getTime()) + " | " + new TestMessageReceivedMetric().getName());
        verify(mockFileWriter, times(3)).write(System.lineSeparator());
        verify(mockFileWriter, times(3)).flush();
        verifyNoMoreInteractions(mockCalendarProvider, mockFileWriter);
        assertNull(exceptionStorer.getException());
    }
    
    @Test
    public void AddSuccessTest() throws Exception {
        Calendar timeStamp1 = new GregorianCalendar(2014, 6, 14, 12, 45, 31);
        Calendar timeStamp2 = new GregorianCalendar(2014, 6, 14, 12, 45, 43);
        Calendar timeStamp3 = new GregorianCalendar(2014, 6, 15, 23, 58, 47);
        timeStamp1.setTimeZone(TimeZone.getTimeZone(timeZoneId));
        timeStamp2.setTimeZone(TimeZone.getTimeZone(timeZoneId));
        timeStamp3.setTimeZone(TimeZone.getTimeZone(timeZoneId));

        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(timeStamp1)
            .thenReturn(timeStamp2)
            .thenReturn(timeStamp3);
        
        testFileMetricLogger.Add(new TestMessageBytesReceivedMetric(12345));
        testFileMetricLogger.Add(new TestDiskBytesReadMetric(160307));
        testFileMetricLogger.Add(new TestMessageBytesReceivedMetric(12347));
        testFileMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();

        verify(mockCalendarProvider, times(3)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        verify(mockFileWriter).write(dateFormatter.format(timeStamp1.getTime()) + " | " + new TestMessageBytesReceivedMetric(0).getName() + " | " + 12345);
        verify(mockFileWriter).write(dateFormatter.format(timeStamp2.getTime()) + " | " + new TestDiskBytesReadMetric(0).getName() + " | " + 160307);
        verify(mockFileWriter).write(dateFormatter.format(timeStamp3.getTime()) + " | " + new TestMessageBytesReceivedMetric(0).getName() + " | " + 12347);
        verify(mockFileWriter, times(3)).write(System.lineSeparator());
        verify(mockFileWriter, times(3)).flush();
        verifyNoMoreInteractions(mockCalendarProvider, mockFileWriter);
        assertNull(exceptionStorer.getException());
    }
    
    @Test
    public void SetSuccessTest() throws Exception {
        Calendar timeStamp1 = new GregorianCalendar(2014, 6, 17, 23, 42, 33);
        Calendar timeStamp2 = new GregorianCalendar(2014, 6, 17, 23, 44, 35);
        Calendar timeStamp3 = new GregorianCalendar(2014, 6, 17, 23, 59, 1);
        timeStamp1.setTimeZone(TimeZone.getTimeZone(timeZoneId));
        timeStamp2.setTimeZone(TimeZone.getTimeZone(timeZoneId));
        timeStamp3.setTimeZone(TimeZone.getTimeZone(timeZoneId));

        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(timeStamp1)
            .thenReturn(timeStamp2)
            .thenReturn(timeStamp3);
        
        testFileMetricLogger.Set(new TestAvailableMemoryMetric(301156000));
        testFileMetricLogger.Set(new TestFreeWorkerThreadsMetric(12));
        testFileMetricLogger.Set(new TestAvailableMemoryMetric(301155987));
        testFileMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();

        verify(mockCalendarProvider, times(3)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        verify(mockFileWriter).write(dateFormatter.format(timeStamp1.getTime()) + " | " + new TestAvailableMemoryMetric(0).getName() + " | " + 301156000);
        verify(mockFileWriter).write(dateFormatter.format(timeStamp2.getTime()) + " | " + new TestFreeWorkerThreadsMetric(0).getName() + " | " + 12);
        verify(mockFileWriter).write(dateFormatter.format(timeStamp3.getTime()) + " | " + new TestAvailableMemoryMetric(0).getName() + " | " + 301155987);
        verify(mockFileWriter, times(3)).write(System.lineSeparator());
        verify(mockFileWriter, times(3)).flush();
        verifyNoMoreInteractions(mockCalendarProvider, mockFileWriter);
        assertNull(exceptionStorer.getException());
    }
    
    @Test
    public void BeginEndSuccessTests() throws Exception {
        Calendar timeStamp1 = utilities.CreateCalendarWithMilliseconds(2014, 6, 14, 12, 45, 31, 0);
        Calendar timeStamp2 = utilities.CreateCalendarWithMilliseconds(2014, 6, 14, 12, 45, 31, 34);
        Calendar timeStamp3 = utilities.CreateCalendarWithMilliseconds(2014, 6, 14, 12, 45, 43, 500);
        Calendar timeStamp4 = utilities.CreateCalendarWithMilliseconds(2014, 6, 14, 12, 45, 43, 499);
        Calendar timeStamp5 = utilities.CreateCalendarWithMilliseconds(2014, 6, 15, 23, 58, 47, 750);
        Calendar timeStamp6 = utilities.CreateCalendarWithMilliseconds(2014, 6, 15, 23, 58, 48, 785);
        timeStamp1.setTimeZone(TimeZone.getTimeZone(timeZoneId));
        timeStamp2.setTimeZone(TimeZone.getTimeZone(timeZoneId));
        timeStamp3.setTimeZone(TimeZone.getTimeZone(timeZoneId));
        timeStamp4.setTimeZone(TimeZone.getTimeZone(timeZoneId));
        timeStamp5.setTimeZone(TimeZone.getTimeZone(timeZoneId));
        timeStamp6.setTimeZone(TimeZone.getTimeZone(timeZoneId));

        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(timeStamp1)
            .thenReturn(timeStamp2)
            .thenReturn(timeStamp3)
            // Note below expect makes the end time before the begin time.  Class should insert the resulting milliseconds interval as 0.
            .thenReturn(timeStamp4)
            .thenReturn(timeStamp5)
            .thenReturn(timeStamp6);
        
        testFileMetricLogger.Begin(new TestDiskReadTimeMetric());
        testFileMetricLogger.End(new TestDiskReadTimeMetric());
        testFileMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testFileMetricLogger.End(new TestMessageProcessingTimeMetric());
        testFileMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testFileMetricLogger.End(new TestMessageProcessingTimeMetric());
        testFileMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();

        verify(mockCalendarProvider, times(6)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        verify(mockFileWriter).write(dateFormatter.format(timeStamp1.getTime()) + " | " + new TestDiskReadTimeMetric().getName() + " | " + 34);
        verify(mockFileWriter).write(dateFormatter.format(timeStamp3.getTime()) + " | " + new TestMessageProcessingTimeMetric().getName() + " | " + 0);
        verify(mockFileWriter).write(dateFormatter.format(timeStamp5.getTime()) + " | " + new TestMessageProcessingTimeMetric().getName() + " | " + 1035);
        verify(mockFileWriter, times(3)).write(System.lineSeparator());
        verify(mockFileWriter, times(3)).flush();
        verifyNoMoreInteractions(mockCalendarProvider, mockFileWriter);
        assertNull(exceptionStorer.getException());
    }
    
    @Test
    public void CloseSuccessTests() throws Exception {
        testFileMetricLogger.close();
        
        verify(mockFileWriter).close();
        verifyNoMoreInteractions(mockCalendarProvider, mockFileWriter);
    }
}

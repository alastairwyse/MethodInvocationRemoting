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

package net.alastairwyse.applicationmetricsunittests;

import org.junit.Before;
import org.junit.Test;

import java.text.*;
import java.util.*;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;

import static org.mockito.Mockito.*;
import net.alastairwyse.applicationmetrics.*;
import net.alastairwyse.operatingsystemabstraction.*;

/**
 * Unit tests for class applicationmetrics.ConsoleMetricLogger.
 * @author Alastair Wyse
 */
public class ConsoleMetricLoggerTests {

    /* 
     * NOTE: See notes in class MicrosoftAccessMetricLoggerTests regarding testing of underlying worker threads.  The same comments apply to this test class
     */
    
    private final String separatorString = ": ";
    private final String timeZoneId = "UTC";
    
    private IPrintStream mockPrintStream;
    private ICalendarProvider mockCalendarProvider;
    private ExceptionStorer exceptionStorer;
    private CountDownLatch dequeueOperationLoopCompleteSignal;
    private SimpleDateFormat dateFormatter; 
    private ApplicationMetricsUnitTestUtilities utilities;
    private ConsoleMetricLogger testConsoleMetricLogger;

    @Before
    public void setUp() throws Exception {
        mockPrintStream = mock(IPrintStream.class);
        mockCalendarProvider = mock(ICalendarProvider.class);
        exceptionStorer = new ExceptionStorer();
        dequeueOperationLoopCompleteSignal = new CountDownLatch(1);
        dateFormatter = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
        utilities = new ApplicationMetricsUnitTestUtilities();
        testConsoleMetricLogger = new ConsoleMetricLogger(10, true, exceptionStorer, mockPrintStream, mockCalendarProvider, dequeueOperationLoopCompleteSignal);
    }
    
    @Test
    public void LogCountMetricTotalSuccessTest() throws Exception {
        GregorianCalendar bannerTime = new GregorianCalendar(2014, 7, 12, 15, 20, 27);
        
        // Expects for calls to Increment()
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(new GregorianCalendar(2014, 7, 12, 15, 20, 21))
            .thenReturn(new GregorianCalendar(2014, 7, 12, 15, 20, 23))
            .thenReturn(new GregorianCalendar(2014, 7, 12, 15, 20, 25))
            // Expects for calls to Start()
            .thenReturn(bannerTime)
            // Expects for calls to DequeueAndProcessMetricEvents() 
            .thenReturn(bannerTime);

        testConsoleMetricLogger.Increment(new TestMessageReceivedMetric());
        testConsoleMetricLogger.Increment(new TestDiskReadOperationMetric());
        testConsoleMetricLogger.Increment(new TestMessageReceivedMetric());
        testConsoleMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(5)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        VerifyWriteTitleExpectations(bannerTime);
        verify(mockPrintStream).println(new TestMessageReceivedMetric().getName() + separatorString + "2");
        verify(mockPrintStream).println(new TestDiskReadOperationMetric().getName() + separatorString + "1");
    }

    @Test
    public void LogAmounMetricTotalSuccessTest() throws Exception {
        GregorianCalendar bannerTime = new GregorianCalendar(2014, 7, 12, 15, 20, 27);
        
        // Expects for calls to Add()
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(new GregorianCalendar(2014, 7, 12, 15, 20, 21))
            .thenReturn(new GregorianCalendar(2014, 7, 12, 15, 20, 23))
            .thenReturn(new GregorianCalendar(2014, 7, 12, 15, 20, 25))
            // Expects for calls to Start()
            .thenReturn(bannerTime)
            // Expects for calls to DequeueAndProcessMetricEvents() 
            .thenReturn(bannerTime);
        
        testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(1024));
        testConsoleMetricLogger.Add(new TestDiskBytesReadMetric(3049));
        testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(2048));
        testConsoleMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(5)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        VerifyWriteTitleExpectations(bannerTime);
        verify(mockPrintStream).println(new TestMessageBytesReceivedMetric(0).getName() + separatorString + "3072");
        verify(mockPrintStream).println(new TestDiskBytesReadMetric(0).getName() + separatorString + "3049");
    }
    
    @Test
    public void LogStatusMetricValueSuccessTest() throws Exception {
        GregorianCalendar bannerTime = new GregorianCalendar(2014, 7, 14, 22, 54, 0);
        
        // Expects for calls to Add()
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(new GregorianCalendar(2014, 7, 14, 22, 54, 1))
            .thenReturn(new GregorianCalendar(2014, 7, 14, 22, 54, 3))
            .thenReturn(new GregorianCalendar(2014, 7, 14, 22, 54, 6))
            // Expects for calls to Start()
            .thenReturn(bannerTime)
            // Expects for calls to DequeueAndProcessMetricEvents() 
            .thenReturn(bannerTime);
        
        testConsoleMetricLogger.Set(new TestAvailableMemoryMetric(80740352));
        testConsoleMetricLogger.Set(new TestFreeWorkerThreadsMetric(8));
        testConsoleMetricLogger.Set(new TestAvailableMemoryMetric(714768384));
        testConsoleMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(5)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        VerifyWriteTitleExpectations(bannerTime);
        verify(mockPrintStream).println(new TestAvailableMemoryMetric(0).getName() + separatorString + "714768384");
        verify(mockPrintStream).println(new TestFreeWorkerThreadsMetric(0).getName() + separatorString + "8");
    }
    
    @Test
    public void LogIntervalMetricTotalSuccessTest() throws Exception {
        GregorianCalendar bannerTime = new GregorianCalendar(2014, 7, 14, 22, 54, 0);
        
        // Expects for calls to Begin() and End()
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 14, 22, 54, 1, 0))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 14, 22, 54, 3, 250))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 14, 22, 54, 6, 987))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 14, 22, 54, 7, 123))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 14, 22, 56, 59, 501))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 14, 22, 58, 1, 267))
            // Expects for calls to Start()
            .thenReturn(bannerTime)
            // Expects for calls to DequeueAndProcessMetricEvents() 
            .thenReturn(bannerTime);
        
        testConsoleMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testConsoleMetricLogger.Begin(new TestDiskReadTimeMetric());
        testConsoleMetricLogger.End(new TestDiskReadTimeMetric());
        testConsoleMetricLogger.End(new TestMessageProcessingTimeMetric());
        testConsoleMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testConsoleMetricLogger.End(new TestMessageProcessingTimeMetric());
        testConsoleMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(8)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        VerifyWriteTitleExpectations(bannerTime);
        verify(mockPrintStream).println(new TestDiskReadTimeMetric().getName() + separatorString + "3737");
        verify(mockPrintStream).println(new TestMessageProcessingTimeMetric().getName() + separatorString + "67889");
    }
    
    @Test
    public void LogCountOverTimeUnitAggregateSuccessTest() throws Exception {
        GregorianCalendar bannerTime = utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 10, 0);
        
        // Expects for the calls to Increment()
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 10, 250))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 10, 500))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 10, 750))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 11, 0))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 11, 250))
            // Expects for calls to Start()
            .thenReturn(bannerTime)
            // Expects for calls to DequeueAndProcessMetricEvents() 
            .thenReturn(bannerTime)
            // Expects for calls to LogCountOverTimeUnitAggregate()
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 12, 0));
        
        
        testConsoleMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.SECONDS, "MessagesReceivedPerSecond", "The number of messages received per second");
        for (int i = 0; i < 5; i++) {
            testConsoleMetricLogger.Increment(new TestMessageReceivedMetric());
        }
        testConsoleMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(8)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        VerifyWriteTitleExpectations(bannerTime);
        verify(mockPrintStream).println(new TestMessageReceivedMetric().getName() + separatorString + "5");
        verify(mockPrintStream).println("MessagesReceivedPerSecond" + separatorString + "2.5");
    }
    
    @Test
    public void LogCountOverTimeUnitAggregateNoInstancesSuccessTest() throws Exception {
        // Tests defining a count over time unit aggregate, where no instances of the underlying count metric have been logged
        
        GregorianCalendar bannerTime = utilities.CreateCalendarWithMilliseconds(2014, 7, 11, 23, 30, 42, 0);
        
        // Expects for calls to Start()
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(bannerTime)
            // Expects for calls to DequeueAndProcessMetricEvents() 
            .thenReturn(bannerTime)
            // Expects for calls to LogCountOverTimeUnitAggregate()
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 11, 23, 30, 47, 0));
        
        testConsoleMetricLogger.DefineMetricAggregate(new TestMessageReceivedMetric(), TimeUnit.SECONDS, "MessagesReceivedPerSecond", "The number of messages received per second");
        testConsoleMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(3)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        VerifyWriteTitleExpectations(bannerTime);
        verify(mockPrintStream).println("MessagesReceivedPerSecond" + separatorString + "0.0");
    }
    
    @Test
    public void LogAmountOverCountAggregateSuccessTest() throws Exception {
        GregorianCalendar bannerTime = utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 17, 56, 18, 0);
        
        // Expects for the calls to Add() and Increment()
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 17, 56, 19, 0))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 17, 56, 20, 0))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 17, 56, 21, 0))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 17, 56, 22, 0))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 17, 56, 23, 0))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 17, 56, 24, 0))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 17, 56, 25, 0))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 17, 56, 26, 0))
            // Expects for calls to Start()
            .thenReturn(bannerTime)
            // Expects for calls to DequeueAndProcessMetricEvents() 
            .thenReturn(bannerTime);
        
        testConsoleMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestMessageReceivedMetric(), "BytesReceivedPerMessage", "The number of bytes received per message");
        testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(2));
        testConsoleMetricLogger.Increment(new TestMessageReceivedMetric());
        testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(6));
        testConsoleMetricLogger.Increment(new TestMessageReceivedMetric());
        testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(3));
        testConsoleMetricLogger.Increment(new TestMessageReceivedMetric());
        testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(7));
        testConsoleMetricLogger.Increment(new TestMessageReceivedMetric());
        testConsoleMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(10)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        VerifyWriteTitleExpectations(bannerTime);
        verify(mockPrintStream).println(new TestMessageReceivedMetric().getName() + separatorString + "4");
        verify(mockPrintStream).println(new TestMessageBytesReceivedMetric(0).getName() + separatorString + "18");
        verify(mockPrintStream).println("BytesReceivedPerMessage" + separatorString + "4.5");
    }
    
    @Test
    public void LogAmountOverCountAggregateNoInstancesSuccessTest() throws Exception {

        // Tests defining an amount over count aggregate, where no instances of the underlying count metric have been logged
        
        GregorianCalendar bannerTime = utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 17, 56, 18, 0);
        
        // Expects for the calls to Add()
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 17, 56, 19, 0))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 17, 56, 20, 0))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 17, 56, 21, 0))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 17, 56, 22, 0))
            // Expects for calls to Start()
            .thenReturn(bannerTime)
            // Expects for calls to DequeueAndProcessMetricEvents() 
            .thenReturn(bannerTime);
        
        testConsoleMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestMessageReceivedMetric(), "BytesReceivedPerMessage", "The number of bytes received per message");
        testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(2));
        testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(6));
        testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(3));
        testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(7));
        testConsoleMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(6)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        VerifyWriteTitleExpectations(bannerTime);
        verify(mockPrintStream).println(new TestMessageBytesReceivedMetric(0).getName() + separatorString + "18");
    }
    
    @Test
    public void LogAmountOverTimeUnitAggregateSuccessTest() throws Exception {
        GregorianCalendar bannerTime = utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 10, 0);
        
        // Expects for the calls to Add()
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 10, 250))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 10, 500))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 10, 750))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 11, 0))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 11, 250))
            // Expects for calls to Start()
            .thenReturn(bannerTime)
            // Expects for calls to DequeueAndProcessMetricEvents() 
            .thenReturn(bannerTime)
            // Expects for calls to LogAmountOverTimeUnitAggregate()
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 12, 0));
        
        testConsoleMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), TimeUnit.SECONDS, "MessageBytesPerSecond", "The number of message bytes received per second");
        testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(149));
        testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(257));
        testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(439));
        testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(271));
        testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(229));
        testConsoleMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(8)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        VerifyWriteTitleExpectations(bannerTime);
        verify(mockPrintStream).println(new TestMessageBytesReceivedMetric(0).getName() + separatorString + "1345");
        verify(mockPrintStream).println("MessageBytesPerSecond" + separatorString + "672.5");
    }
    
    @Test
    public void LogAmountOverTimeUnitAggregateNoInstancesSuccessTest() throws Exception {
        // Tests defining an amount over time unit aggregate, where no instances of the underlying amount metric have been logged
        
        GregorianCalendar bannerTime = utilities.CreateCalendarWithMilliseconds(2014, 7, 11, 23, 30, 42, 0);

        // Expects for calls to Start()
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(bannerTime)
            // Expects for calls to DequeueAndProcessMetricEvents() 
            .thenReturn(bannerTime)
            // Expects for calls to LogAmountOverTimeUnitAggregate()
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 11, 23, 30, 47, 0));
        
        testConsoleMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), TimeUnit.SECONDS, "MessageBytesPerSecond", "The number of message bytes received per second");
        testConsoleMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(3)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        VerifyWriteTitleExpectations(bannerTime);
        verify(mockPrintStream).println("MessageBytesPerSecond" + separatorString + "0.0");
    }
    
    @Test
    public void LogAmountOverAmountAggregateSuccessTest() throws Exception {
        GregorianCalendar bannerTime = utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 10, 0);
        
        // Expects for the calls to Add()
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 10, 250))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 10, 500))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 10, 750))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 11, 0))
            // Expects for calls to Start()
            .thenReturn(bannerTime)
            // Expects for calls to DequeueAndProcessMetricEvents() 
            .thenReturn(bannerTime);
        
        testConsoleMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestDiskBytesReadMetric(0), "MessageBytesReceivedPerDiskBytesRead", "The number of message bytes received per disk bytes read");
        testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(149));
        testConsoleMetricLogger.Add(new TestDiskBytesReadMetric(257));
        testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(439));
        testConsoleMetricLogger.Add(new TestDiskBytesReadMetric(271));
        testConsoleMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(6)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        VerifyWriteTitleExpectations(bannerTime);
        verify(mockPrintStream).println(new TestMessageBytesReceivedMetric(0).getName() + separatorString + "588");
        verify(mockPrintStream).println(new TestDiskBytesReadMetric(0).getName() + separatorString + "528");
        verify(mockPrintStream).println("MessageBytesReceivedPerDiskBytesRead" + separatorString + "1.1136363636363635");
    }
    
    @Test
    public void LogAmountOverAmountAggregateNoNumeratorInstancesSuccessTest() throws Exception {
        GregorianCalendar bannerTime = utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 10, 0);
        
        // Expects for the calls to Add()
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 10, 250))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 10, 500))
            // Expects for calls to Start()
            .thenReturn(bannerTime)
            // Expects for calls to DequeueAndProcessMetricEvents() 
            .thenReturn(bannerTime);
        
        testConsoleMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestDiskBytesReadMetric(0), "MessageBytesReceivedPerDiskBytesRead", "The number of message bytes received per disk bytes read");
        testConsoleMetricLogger.Add(new TestDiskBytesReadMetric(257));
        testConsoleMetricLogger.Add(new TestDiskBytesReadMetric(271));
        testConsoleMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(4)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        VerifyWriteTitleExpectations(bannerTime);
        verify(mockPrintStream).println(new TestDiskBytesReadMetric(0).getName() + separatorString + "528");
        verify(mockPrintStream).println("MessageBytesReceivedPerDiskBytesRead" + separatorString + "0.0");
    }
    
    @Test
    public void LogAmountOverAmountAggregateNoDenominatorInstancesSuccessTest() throws Exception {
        GregorianCalendar bannerTime = utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 10, 0);
        
        // Expects for the calls to Add()
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 10, 250))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 12, 15, 39, 10, 500))
            // Expects for calls to Start()
            .thenReturn(bannerTime)
            // Expects for calls to DequeueAndProcessMetricEvents() 
            .thenReturn(bannerTime);
        
        testConsoleMetricLogger.DefineMetricAggregate(new TestMessageBytesReceivedMetric(0), new TestDiskBytesReadMetric(0), "MessageBytesReceivedPerDiskBytesRead", "The number of message bytes received per disk bytes read");
        testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(149));
        testConsoleMetricLogger.Add(new TestMessageBytesReceivedMetric(439));
        testConsoleMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(4)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        VerifyWriteTitleExpectations(bannerTime);
        verify(mockPrintStream).println(new TestMessageBytesReceivedMetric(0).getName() + separatorString + "588");
    }
    
    @Test
    public void LogIntervalOverCountAggregateSuccessTest() throws Exception {
        GregorianCalendar bannerTime = utilities.CreateCalendarWithMilliseconds(2014, 07, 16, 23, 01, 16, 999);
        
        // Expects for the calls to Begin(), End(), and Increment()
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 16, 23, 01, 17, 0))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 16, 23, 01, 17, 120))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 16, 23, 01, 17, 120))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 16, 23, 01, 19, 850))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 16, 23, 01, 20, 975))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 16, 23, 01, 20, 980))
            // Expects for calls to Start()
            .thenReturn(bannerTime)
            // Expects for calls to DequeueAndProcessMetricEvents() 
            .thenReturn(bannerTime);
        
        testConsoleMetricLogger.DefineMetricAggregate(new TestMessageProcessingTimeMetric(), new TestMessageReceivedMetric(), "ProcessingTimePerMessage", "The average time to process each message");
        testConsoleMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testConsoleMetricLogger.End(new TestMessageProcessingTimeMetric());
        testConsoleMetricLogger.Increment(new TestMessageReceivedMetric());
        testConsoleMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testConsoleMetricLogger.End(new TestMessageProcessingTimeMetric());
        testConsoleMetricLogger.Increment(new TestMessageReceivedMetric());
        testConsoleMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(8)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        VerifyWriteTitleExpectations(bannerTime);
        verify(mockPrintStream).println(new TestMessageReceivedMetric().getName() + separatorString + "2");
        verify(mockPrintStream).println(new TestMessageProcessingTimeMetric().getName() + separatorString + "1245");
        verify(mockPrintStream).println("ProcessingTimePerMessage" + separatorString + "622.5");
    }
    
    @Test
    public void LogIntervalOverCountAggregateNoInstancesSuccessTest() throws Exception {
        
        // Tests defining an interval over count aggregate, where no instances of the underlying count metric have been logged
        
        GregorianCalendar bannerTime = utilities.CreateCalendarWithMilliseconds(2014, 7, 16, 23, 1, 16, 999);
        
        // Expects for the calls to Begin(), End(), and Increment()
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 16, 23, 01, 17, 0))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 16, 23, 01, 17, 120))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 16, 23, 01, 19, 850))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 16, 23, 01, 20, 975))
            // Expects for calls to Start()
            .thenReturn(bannerTime)
            // Expects for calls to DequeueAndProcessMetricEvents() 
            .thenReturn(bannerTime);
        
        testConsoleMetricLogger.DefineMetricAggregate(new TestMessageProcessingTimeMetric(), new TestMessageReceivedMetric(), "ProcessingTimePerMessage", "The average time to process each message");
        testConsoleMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testConsoleMetricLogger.End(new TestMessageProcessingTimeMetric());
        testConsoleMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testConsoleMetricLogger.End(new TestMessageProcessingTimeMetric());
        testConsoleMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(6)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        VerifyWriteTitleExpectations(bannerTime);
        verify(mockPrintStream).println(new TestMessageProcessingTimeMetric().getName() + separatorString + "1245");
    }
    
    @Test
    public void LogIntervalOverTotalRunTimeAggregateSuccessTest() throws Exception {
        GregorianCalendar bannerTime = utilities.CreateCalendarWithMilliseconds(2014, 7, 19, 17, 33, 50, 0);
        
        // Expects for the calls to Begin() and End()
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 19, 17, 33, 51, 0))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 19, 17, 33, 51, 789))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 19, 17, 33, 52, 58))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 19, 17, 33, 56, 32))
            // Expects for calls to Start()
            .thenReturn(bannerTime)
            // Expects for calls to DequeueAndProcessMetricEvents() 
            .thenReturn(bannerTime)
            // Expects for calls to LogIntervalOverTotalRunTimeAggregate() 
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 19, 17, 33, 56, 300));
        
        testConsoleMetricLogger.DefineMetricAggregate(new TestMessageProcessingTimeMetric(), "MessageProcessingTimePercentage", "The amount of time spent processing messages as a percentage of total run time");
        testConsoleMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testConsoleMetricLogger.End(new TestMessageProcessingTimeMetric());
        testConsoleMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testConsoleMetricLogger.End(new TestMessageProcessingTimeMetric());
        testConsoleMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(7)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        VerifyWriteTitleExpectations(bannerTime);
        verify(mockPrintStream).println("MessageProcessingTimePercentage" + separatorString + "0.756031746031746");
    }
    
    @Test
    public void LogIntervalOverTotalRunTimeAggregateZeroElapsedTimeSuccessTest() throws Exception {

        // Tests that an aggregate is not logged when no time has elapsed
        
        GregorianCalendar bannerTime = utilities.CreateCalendarWithMilliseconds(2014, 7, 19, 17, 33, 50, 0);
        
        // Expects for the calls to Begin() and End()
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 19, 17, 33, 51, 0))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 19, 17, 33, 51, 789))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 19, 17, 33, 52, 58))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 19, 17, 33, 56, 32))
            // Expects for calls to Start()
            .thenReturn(bannerTime)
            // Expects for calls to DequeueAndProcessMetricEvents() 
            .thenReturn(bannerTime)
            // Expects for calls to LogIntervalOverTotalRunTimeAggregate() 
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 7, 19, 17, 33, 50, 0));
        
        testConsoleMetricLogger.DefineMetricAggregate(new TestMessageProcessingTimeMetric(), "MessageProcessingTimePercentage", "The amount of time spent processing messages as a percentage of total run time");
        testConsoleMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testConsoleMetricLogger.End(new TestMessageProcessingTimeMetric());
        testConsoleMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testConsoleMetricLogger.End(new TestMessageProcessingTimeMetric());
        testConsoleMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(7)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        VerifyWriteTitleExpectations(bannerTime);
        verify(mockPrintStream).println(new TestMessageProcessingTimeMetric().getName() + separatorString + "4763");
    }

    /**
     * Verifies the methods which write the title banner to the console.
     * @param expectedTime  The date and time to expect to print in the title banner
     */
    private void VerifyWriteTitleExpectations(Calendar expectedTime) {
        
        verify(mockPrintStream, times(2)).println();
        verify(mockPrintStream, times(2)).println("---------------------------------------------------");
        verify(mockPrintStream).println("-- Application metrics as of " + dateFormatter.format(expectedTime.getTime()) + " --");
    }
}

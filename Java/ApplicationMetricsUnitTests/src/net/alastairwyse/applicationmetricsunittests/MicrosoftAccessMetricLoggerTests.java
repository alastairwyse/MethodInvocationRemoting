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

import static org.junit.Assert.*;

import org.junit.Before;
import org.junit.Test;

import java.sql.*;
import java.text.*;
import java.util.*;
import java.util.concurrent.CountDownLatch;

import static org.mockito.Mockito.*;
import net.alastairwyse.applicationmetrics.*;
import net.alastairwyse.operatingsystemabstraction.*;

/**
 * Unit tests for class applicationmetrics.MicrosoftAccessMetricLogger.
 * @author Alastair Wyse
 */
public class MicrosoftAccessMetricLoggerTests {

    /*
     * NOTE: As most of the work of the MicrosoftAccessMetricLogger class is done by a worker thread, many of the tests in this class rely on checking the behavior of the worker thread.
     *       2 techniques are used to support this...
     *         1) For success tests a CountDownLatch object is passed into the test constructor of the MicrosoftAccessMetricLogger class.  After calling the method under test, the test code 
     *              calls await() on the CountDownLatch.  Inside the MicrosoftAccessMetricLogger, a single iteration of the metric dequeue and process loop is called in a worker thread, 
     *              afterwhich countDown() is called on the CountDownLatch to allow the test code to proceed with verify and assert statements.  See test IncrementSuccessTest() for an 
     *              example of this.
     *         2) For exception tests, the CountDownLatch inside the class could not be used, as the exception is thrown before a single iteration of the loop in the thread has completed.
     *              I tried to simulate the NMock2.Signal.EventWaitHandle() method used in the equivalent C# tests, by creating a custom mockito Answer<Void> implementation, which would call 
     *              countDown() on a CountDownLatch passed to its constructor.  However I couldn't find a way to get mockito to throw an exception on the mock and then subsequently perform 
     *              the answer.  Hence currently this is solved by adding a short Thread.sleep() to the test code, to give time for the test to proceed and the exception be thrown before 
     *              proceeding with verify and assert statements.  This means the test results are not deterministic, but was the best solution I could find.  See test 
     *              IncrementDatabaseInsertException() for an example of this pattern.
     *         3) An instance of the ExceptionStorer class is passed to the MicrosoftAccessMetricLogger constructor, and used to verify the properties of exceptions thrown on the worker 
     *              thread.
     */
    
    private ExceptionStorer exceptionStorer;
    private Connection mockConnection;
    private Statement mockStatement;
    private ICalendarProvider mockCalendarProvider;
    private String testDbFilePath = "C:\\Temp\\TestAccessDb.mdb";
    private String testMetricCategoryName = "DefaultCategory";
    private String timeZoneId = "UTC";
    private SimpleDateFormat dateFormatter; 
    private CountDownLatch dequeueOperationLoopCompleteSignal;
    private ApplicationMetricsUnitTestUtilities utilities;
    private MicrosoftAccessMetricLogger testMicrosoftAccessMetricLogger;
    
    @Before
    public void setUp() throws Exception {
        exceptionStorer = new ExceptionStorer();
        mockConnection = mock(Connection.class);
        mockStatement = mock(Statement.class);
        mockCalendarProvider = mock(ICalendarProvider.class);
        dateFormatter = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
        dateFormatter.setTimeZone(TimeZone.getTimeZone("UTC"));
        dequeueOperationLoopCompleteSignal = new CountDownLatch(1);
        utilities = new ApplicationMetricsUnitTestUtilities();
        testMicrosoftAccessMetricLogger = new MicrosoftAccessMetricLogger(testDbFilePath, testMetricCategoryName, 10, true, exceptionStorer, mockConnection, mockStatement, mockCalendarProvider, dequeueOperationLoopCompleteSignal); 
    }
    
    @Test 
    public void InvalidDequeueOperationLoopIntervalArgument() {
        try {
            testMicrosoftAccessMetricLogger = new MicrosoftAccessMetricLogger(testDbFilePath, testMetricCategoryName, -1, true, exceptionStorer);
            fail("Exception was not thrown.");
        }
        catch (IllegalArgumentException e) {
            assertTrue(e.getMessage().contains("Argument 'dequeueOperationLoopInterval' must be greater than or equal to 0."));
        }
    }
    
    @Test
    public void InvalidMetricCategoryNameArgument() {
        try {
            testMicrosoftAccessMetricLogger = new MicrosoftAccessMetricLogger(testDbFilePath, "  ", 10, true, exceptionStorer);
            fail("Exception was not thrown.");
        }
        catch (IllegalArgumentException e) {
            assertTrue(e.getMessage().contains("Argument 'metricCategoryName' cannot be blank."));
        }
    }
    
    @Test
    public void ConnectWhenAlreadyConnected() throws Exception {
        when(mockConnection.isClosed()).thenReturn(false);
        
        try {
            testMicrosoftAccessMetricLogger.Connect();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("Connection to database has already been established."));
        }
    }
    
    @Test
    public void ConnectException() throws Exception {
        when(mockConnection.isClosed()).thenReturn(true);
        when(mockConnection.createStatement()).thenThrow(new SQLException("Mock SQLException."));
        
        try {
            testMicrosoftAccessMetricLogger.Connect();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("Failed to connect to database at path '" + testDbFilePath + "'."));
        }
    }

    // ConnectSuccessTest() test is not included, as constructing the class using the test constructor alters the behaviour of the Connect() method 
    
    @Test
    public void DisconnectWhenNotConnected() throws Exception {
        testMicrosoftAccessMetricLogger = new MicrosoftAccessMetricLogger(testDbFilePath, testMetricCategoryName, 10, true, exceptionStorer);
        testMicrosoftAccessMetricLogger.Disconnect();
        
        verifyZeroInteractions(mockConnection, mockStatement);
    }
    
    @Test
    public void DisconnectException() throws Exception {
        when(mockConnection.isClosed()).thenReturn(false);
        when(mockStatement.isClosed()).thenReturn(false);
        doThrow(new SQLException("Mock SQLException.")).when(mockStatement).close();
        
        try {
            testMicrosoftAccessMetricLogger.Disconnect();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("Failed to disconnect from database."));
        }
    }
    
    @Test
    public void DisconnectSuccessTest() throws Exception {
        when(mockConnection.isClosed()).thenReturn(false);
        
        testMicrosoftAccessMetricLogger.Disconnect();
        
        verify(mockConnection).isClosed();
        verify(mockStatement).close();
        verify(mockConnection).close();
        verifyNoMoreInteractions(mockConnection, mockStatement);
    }
    
    @Test
    public void IncrementDatabaseInsertException() throws Exception {
        String expectedSqlStatement = CreateCountMetricInsertSql(2014, 6, 14, 12, 45, 31, new TestMessageReceivedMetric().getName(), testMetricCategoryName);

        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId))).thenReturn(new GregorianCalendar(2014, 06, 14, 12, 45, 31));
        when(mockStatement.execute(expectedSqlStatement.toString())).thenThrow(new SQLException("Mock SQLException."));

        // Note that Connect() is not called in this and other similar tests on MetricLoggerBuffer abstract methods, as Connect() does not work properly when the a MicrosoftAccessMetricLogger object is instantiated using the test constructor
        testMicrosoftAccessMetricLogger.Increment(new TestMessageReceivedMetric());
        testMicrosoftAccessMetricLogger.Start();
        Thread.sleep(50);

        verify(mockCalendarProvider).getCalendar(TimeZone.getTimeZone(timeZoneId));
        verify(mockStatement).execute(expectedSqlStatement.toString());
        assertTrue(exceptionStorer.getException().getCause().getMessage().contains("Failed to insert instance of count metric 'MessageReceived'."));
        assertTrue(exceptionStorer.getException().getCause().getCause().getMessage().contains("Mock SQLException."));
        verifyNoMoreInteractions(mockCalendarProvider, mockConnection, mockStatement);
    }
    
    @Test
    public void IncrementSuccessTest() throws Exception {
        String firstExpectedSqlStatement = CreateCountMetricInsertSql(2014, 6, 14, 12, 45, 31, new TestMessageReceivedMetric().getName(), testMetricCategoryName);
        String secondExpectedSqlStatement = CreateCountMetricInsertSql(2014, 6, 14, 12, 45, 43, new TestDiskReadOperationMetric().getName(), testMetricCategoryName);
        String thirdExpectedSqlStatement = CreateCountMetricInsertSql(2014, 6, 15, 23, 58, 47, new TestMessageReceivedMetric().getName(), testMetricCategoryName);

        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(new GregorianCalendar(2014, 6, 14, 12, 45, 31))
            .thenReturn(new GregorianCalendar(2014, 6, 14, 12, 45, 43))
            .thenReturn(new GregorianCalendar(2014, 6, 15, 23, 58, 47));
        
        // Note that Connect() is not called in this and other similar tests on MetricLoggerBuffer abstract methods, as Connect() does not work properly when the a MicrosoftAccessMetricLogger object is instantiated using the test constructor
        testMicrosoftAccessMetricLogger.Increment(new TestMessageReceivedMetric());
        testMicrosoftAccessMetricLogger.Increment(new TestDiskReadOperationMetric());
        testMicrosoftAccessMetricLogger.Increment(new TestMessageReceivedMetric());
        testMicrosoftAccessMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();

        verify(mockCalendarProvider, times(3)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        verify(mockStatement).execute(firstExpectedSqlStatement.toString());
        verify(mockStatement).execute(secondExpectedSqlStatement.toString());
        verify(mockStatement).execute(thirdExpectedSqlStatement.toString());
        verifyNoMoreInteractions(mockCalendarProvider, mockConnection, mockStatement);
    }
    
    @Test
    public void AddDatabaseInsertException() throws Exception {
        String expectedSqlStatement = CreateAmountMetricInsertSql(2014, 06, 14, 12, 45, 31, 12345, new TestMessageBytesReceivedMetric(0).getName(), testMetricCategoryName);

        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId))).thenReturn(new GregorianCalendar(2014, 06, 14, 12, 45, 31));
        when(mockStatement.execute(expectedSqlStatement.toString())).thenThrow(new SQLException("Mock SQLException."));

        testMicrosoftAccessMetricLogger.Add(new TestMessageBytesReceivedMetric(12345));
        testMicrosoftAccessMetricLogger.Start();
        Thread.sleep(50);

        verify(mockCalendarProvider).getCalendar(TimeZone.getTimeZone(timeZoneId));
        verify(mockStatement).execute(expectedSqlStatement.toString());
        assertTrue(exceptionStorer.getException().getCause().getMessage().contains("Failed to insert instance of amount metric 'MessageBytesReceived'."));
        assertTrue(exceptionStorer.getException().getCause().getCause().getMessage().contains("Mock SQLException."));
        verifyNoMoreInteractions(mockCalendarProvider, mockConnection, mockStatement);
    }
    
    @Test
    public void AddSuccessTest() throws Exception {
        String firstExpectedSqlStatement = CreateAmountMetricInsertSql(2014, 6, 14, 12, 45, 31, 12345, new TestMessageBytesReceivedMetric(0).getName(), testMetricCategoryName);
        String secondExpectedSqlStatement = CreateAmountMetricInsertSql(2014, 6, 14, 12, 45, 43, 160307, new TestDiskBytesReadMetric(0).getName(), testMetricCategoryName);
        String thirdExpectedSqlStatement = CreateAmountMetricInsertSql(2014, 6, 15, 23, 58, 47, 12347, new TestMessageBytesReceivedMetric(0).getName(), testMetricCategoryName);

        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(new GregorianCalendar(2014, 6, 14, 12, 45, 31))
            .thenReturn(new GregorianCalendar(2014, 6, 14, 12, 45, 43))
            .thenReturn(new GregorianCalendar(2014, 6, 15, 23, 58, 47));
        
        testMicrosoftAccessMetricLogger.Add(new TestMessageBytesReceivedMetric(12345));
        testMicrosoftAccessMetricLogger.Add(new TestDiskBytesReadMetric(160307));
        testMicrosoftAccessMetricLogger.Add(new TestMessageBytesReceivedMetric(12347));
        testMicrosoftAccessMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(3)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        verify(mockStatement).execute(firstExpectedSqlStatement.toString());
        verify(mockStatement).execute(secondExpectedSqlStatement.toString());
        verify(mockStatement).execute(thirdExpectedSqlStatement.toString());
        verifyNoMoreInteractions(mockCalendarProvider, mockConnection, mockStatement);
    }
    
    @Test
    public void SetDatabaseInsertException() throws Exception {
        String expectedSqlStatement = CreateStatusMetricInsertSql(2014, 6, 17, 23, 42, 33, 301156000, new TestAvailableMemoryMetric(0).getName(), testMetricCategoryName);

        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId))).thenReturn(new GregorianCalendar(2014, 6, 17, 23, 42, 33));
        when(mockStatement.execute(expectedSqlStatement.toString())).thenThrow(new SQLException("Mock SQLException."));

        testMicrosoftAccessMetricLogger.Set(new TestAvailableMemoryMetric(301156000));
        testMicrosoftAccessMetricLogger.Start();
        Thread.sleep(50);

        verify(mockCalendarProvider).getCalendar(TimeZone.getTimeZone(timeZoneId));
        verify(mockStatement).execute(expectedSqlStatement.toString());
        assertTrue(exceptionStorer.getException().getCause().getMessage().contains("Failed to insert instance of status metric 'AvailableMemory'."));
        assertTrue(exceptionStorer.getException().getCause().getCause().getMessage().contains("Mock SQLException."));
        verifyNoMoreInteractions(mockCalendarProvider, mockConnection, mockStatement);
    }
    
    @Test
    public void SetSuccessTest() throws Exception {
        String firstExpectedSqlStatement = CreateStatusMetricInsertSql(2014, 6, 17, 23, 42, 33, 301156000, new TestAvailableMemoryMetric(0).getName(), testMetricCategoryName);
        String secondExpectedSqlStatement = CreateStatusMetricInsertSql(2014, 6, 17, 23, 44, 35, 12, new TestFreeWorkerThreadsMetric(0).getName(), testMetricCategoryName);
        String thirdExpectedSqlStatement = CreateStatusMetricInsertSql(2014, 6, 17, 23, 59, 01, 301155987, new TestAvailableMemoryMetric(0).getName(), testMetricCategoryName);

        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(new GregorianCalendar(2014, 6, 17, 23, 42, 33))
            .thenReturn(new GregorianCalendar(2014, 6, 17, 23, 44, 35))
            .thenReturn(new GregorianCalendar(2014, 6, 17, 23, 59, 01));
        
        testMicrosoftAccessMetricLogger.Set(new TestAvailableMemoryMetric(301156000));
        testMicrosoftAccessMetricLogger.Set(new TestFreeWorkerThreadsMetric(12));
        testMicrosoftAccessMetricLogger.Set(new TestAvailableMemoryMetric(301155987));
        testMicrosoftAccessMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(3)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        verify(mockStatement).execute(firstExpectedSqlStatement.toString());
        verify(mockStatement).execute(secondExpectedSqlStatement.toString());
        verify(mockStatement).execute(thirdExpectedSqlStatement.toString());
        verifyNoMoreInteractions(mockCalendarProvider, mockConnection, mockStatement);
    }
    
    @Test
    public void BeginEndDatabaseInsertException() throws Exception {
        String expectedSqlStatement = CreateIntervalMetricInsertSql(2014, 6, 14, 12, 45, 50, 987, new TestMessageProcessingTimeMetric().getName(), testMetricCategoryName);
        
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 6, 14, 12, 45, 50, 31))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 6, 14, 12, 45, 51, 18));
        when(mockStatement.execute(expectedSqlStatement.toString())).thenThrow(new SQLException("Mock SQLException."));
        
        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.Start();
        Thread.sleep(50);
        
        verify(mockCalendarProvider, times(2)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        verify(mockStatement).execute(expectedSqlStatement.toString());
        assertTrue(exceptionStorer.getException().getCause().getMessage().contains("Failed to insert instance of interval metric 'MessageProcessingTime'."));
        assertTrue(exceptionStorer.getException().getCause().getCause().getMessage().contains("Mock SQLException."));
        verifyNoMoreInteractions(mockCalendarProvider, mockConnection, mockStatement);
    }
    
    @Test
    public void BeginEndDuplicateBeginIntervalEvents() throws Exception {
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 6, 14, 12, 45, 50, 31))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 6, 14, 12, 45, 51, 18));

        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.Start();
        Thread.sleep(50);
        
        verify(mockCalendarProvider, times(2)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        assertTrue(exceptionStorer.getException().getCause().getMessage().contains("Received duplicate begin 'MessageProcessingTime' metrics."));
        verifyZeroInteractions(mockCalendarProvider, mockConnection, mockStatement);
    }
    
    @Test
    public void BeginEndEndIntervalEventWithNoBegin() throws Exception {
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId))).thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 6, 14, 12, 45, 50, 31));
        
        testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.Start();
        Thread.sleep(50);
        
        verify(mockCalendarProvider).getCalendar(TimeZone.getTimeZone(timeZoneId));
        assertTrue(exceptionStorer.getException().getCause().getMessage().contains("Received end 'MessageProcessingTime' with no corresponding start interval metric."));
        verifyZeroInteractions(mockCalendarProvider, mockConnection, mockStatement);
    }
    
    @Test
    public void BeginEndSuccessTests() throws Exception {
        String firstExpectedSqlStatement = CreateIntervalMetricInsertSql(2014, 6, 14, 12, 45, 31, 34, new TestDiskReadTimeMetric().getName(), testMetricCategoryName);
        String secondExpectedSqlStatement = CreateIntervalMetricInsertSql(2014, 6, 14, 12, 45, 43, 0, new TestMessageProcessingTimeMetric().getName(), testMetricCategoryName);
        String thirdExpectedSqlStatement = CreateIntervalMetricInsertSql(2014, 6, 15, 23, 58, 47, 1035, new TestMessageProcessingTimeMetric().getName(), testMetricCategoryName);

        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 6, 14, 12, 45, 31, 0))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 6, 14, 12, 45, 31, 34))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 6, 14, 12, 45, 43, 500))
            //Note below return makes the end time before the begin time.  Class should insert the resulting milliseconds interval as 0.
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 6, 14, 12, 45, 43, 499))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 6, 15, 23, 58, 47, 750))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 6, 15, 23, 58, 48, 785));

        testMicrosoftAccessMetricLogger.Begin(new TestDiskReadTimeMetric());
        testMicrosoftAccessMetricLogger.End(new TestDiskReadTimeMetric());
        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(6)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        verify(mockStatement).execute(firstExpectedSqlStatement.toString());
        verify(mockStatement).execute(secondExpectedSqlStatement.toString());
        verify(mockStatement).execute(thirdExpectedSqlStatement.toString());
        verifyNoMoreInteractions(mockCalendarProvider, mockConnection, mockStatement);
    }
    
    @Test
    public void BeginEndNestedSuccessTest() throws Exception {
        // Tests correct logging of metrics where an interval metric's begin and end events are wholly nested within the begin and end events of another type of interval metric
        String firstExpectedSqlStatement = CreateIntervalMetricInsertSql(2014, 6, 14, 22, 48, 10, 100, new TestDiskReadTimeMetric().getName(), testMetricCategoryName);
        String secondExpectedSqlStatement = CreateIntervalMetricInsertSql(2014, 6, 14, 22, 48, 9, 60005, new TestMessageProcessingTimeMetric().getName(), testMetricCategoryName);
        
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 6, 14, 22, 48, 9, 000))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 6, 14, 22, 48, 10, 250))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 6, 14, 22, 48, 10, 350))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 6, 14, 22, 49, 9, 005));
        
        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.Begin(new TestDiskReadTimeMetric());
        testMicrosoftAccessMetricLogger.End(new TestDiskReadTimeMetric());
        testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(4)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        verify(mockStatement).execute(firstExpectedSqlStatement.toString());
        verify(mockStatement).execute(secondExpectedSqlStatement.toString());
        verifyNoMoreInteractions(mockCalendarProvider, mockConnection, mockStatement);
    }
    
    @Test
    public void BeginEndNoCheckingSuccessTests() throws Exception {
        // Tests the class with parameter 'intervalMetricChecking' set to false;
        testMicrosoftAccessMetricLogger = new MicrosoftAccessMetricLogger(testDbFilePath, testMetricCategoryName, 10, false, exceptionStorer, mockConnection, mockStatement, mockCalendarProvider, dequeueOperationLoopCompleteSignal);
        
        String firstExpectedSqlStatement = CreateIntervalMetricInsertSql(2014, 6, 14, 12, 45, 31, 3600001, new TestMessageProcessingTimeMetric().getName(), testMetricCategoryName);
        String secondExpectedSqlStatement = CreateIntervalMetricInsertSql(2014, 6, 15, 23, 58, 47, 1035, new TestMessageProcessingTimeMetric().getName(), testMetricCategoryName);
        
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(new GregorianCalendar(2013, 6, 14, 12, 45, 31))
            .thenReturn(new GregorianCalendar(2014, 6, 14, 12, 45, 31))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 6, 14, 13, 45, 31, 1))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 6, 14, 13, 45, 31, 2))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 6, 15, 23, 58, 47, 750))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 6, 15, 23, 58, 48, 785));
        
        // Tests sending sequential begin events of the same type.  The first should be ignored.
        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        // Tests correct logging of an interval metric following sequential begin events
        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
        // Tests sending an end event with no corresponding begin.  This should be ignored.
        testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
        // Tests correct logging of an interval metric following and end with no begin
        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(6)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        verify(mockStatement).execute(firstExpectedSqlStatement.toString());
        verify(mockStatement).execute(secondExpectedSqlStatement.toString());
        verifyNoMoreInteractions(mockCalendarProvider, mockConnection, mockStatement);
    }
    
    @Test
    public void CloseSuccessTests() throws Exception {
        testMicrosoftAccessMetricLogger.close();
        
        verify(mockStatement).close();
        verify(mockConnection).close();
        verifyNoMoreInteractions(mockConnection, mockStatement);
    }
    
    /**
     * Creates a valid Access SQL insert statement for inserting a count metric instance, based on the inputted parameters.
     * @param   year             The year.
     * @param   month            The month.
     * @param   day              The day.
     * @param   hour             The hours.
     * @param   minute           The minutes.
     * @param   second           The seconds
     * @param   countMetricName  The name of the count metric.
     * @param   categoryName     The name of the metric category.
     * @return  The SQL statement.
     */
    private String CreateCountMetricInsertSql(int year, int month, int day, int hour, int minute, int second, String countMetricName, String categoryName) {
        StringBuilder sqlStatement = new StringBuilder();
        sqlStatement.append("INSERT ");
        sqlStatement.append("INTO    CountMetricInstances ");
        sqlStatement.append("        ( CmetId, ");
        sqlStatement.append("          CtgrId, ");
        sqlStatement.append("          [Timestamp] ");
        sqlStatement.append("          ) ");
        sqlStatement.append("SELECT  Cmet.CmetId, ");
        sqlStatement.append("        Ctgr.CtgrId, ");
        sqlStatement.append("        '" + dateFormatter.format(new GregorianCalendar(year, month, day, hour, minute, second).getTime()) + "' ");
        sqlStatement.append("FROM    CountMetrics Cmet, ");
        sqlStatement.append("        Categories Ctgr ");
        sqlStatement.append("WHERE   Cmet.Name = '" + countMetricName + "' ");
        sqlStatement.append("  AND   Ctgr.Name = '" + categoryName + "';");

        return sqlStatement.toString();
    }

    /**
     * Creates a valid Access SQL insert statement for inserting an amount metric instance, based on the inputted parameters.
     * @param   year              The year.
     * @param   month             The month.
     * @param   day               The day.
     * @param   hour              The hours.
     * @param   minute            The minutes.
     * @param   second            The seconds.
     * @param   amount            The amount value associated with the amount metric.
     * @param   amountMetricName  The name of the amount metric.
     * @param   categoryName      The name of the metric category.
     * @return  The SQL statement.
     */
    private String CreateAmountMetricInsertSql(int year, int month, int day, int hour, int minute, int second, long amount, String amountMetricName, String categoryName) {
        StringBuilder sqlStatement = new StringBuilder();
        sqlStatement.append("INSERT ");
        sqlStatement.append("INTO    AmountMetricInstances ");
        sqlStatement.append("        ( CtgrId, ");
        sqlStatement.append("          AmetId, ");
        sqlStatement.append("          Amount, ");
        sqlStatement.append("          [Timestamp] ");
        sqlStatement.append("          ) ");
        sqlStatement.append("SELECT  Ctgr.CtgrId, ");
        sqlStatement.append("        Amet.AmetId, ");
        sqlStatement.append("        " + amount + ", ");
        sqlStatement.append("        '" + dateFormatter.format(new GregorianCalendar(year, month, day, hour, minute, second).getTime()) + "' ");
        sqlStatement.append("FROM    AmountMetrics Amet, ");
        sqlStatement.append("        Categories Ctgr ");
        sqlStatement.append("WHERE   Amet.Name = '" + amountMetricName + "' ");
        sqlStatement.append("  AND   Ctgr.Name = '" + categoryName + "';");

        return sqlStatement.toString();
    }

    /**
     * Creates a valid Access SQL insert statement for inserting a status metric instance, based on the inputted parameters.
     * @param   year              The year.
     * @param   month             The month.
     * @param   day               The day.
     * @param   hour              The hours.
     * @param   minute            The minutes.
     * @param   second            The seconds.
     * @param   value             The amount value associated with the status metric.
     * @param   statusMetricName  The name of the status metric.
     * @param   categoryName      The name of the metric category.
     * @return  The SQL statement.
     */
    private String CreateStatusMetricInsertSql(int year, int month, int day, int hour, int minute, int second, long value, String statusMetricName, String categoryName) {
        StringBuilder sqlStatement = new StringBuilder();
        sqlStatement.append("INSERT ");
        sqlStatement.append("INTO    StatusMetricInstances ");
        sqlStatement.append("        ( CtgrId, ");
        sqlStatement.append("          SmetId, ");
        sqlStatement.append("          [Value], ");
        sqlStatement.append("          [Timestamp] ");
        sqlStatement.append("          ) ");
        sqlStatement.append("SELECT  Ctgr.CtgrId, ");
        sqlStatement.append("        Smet.SmetId, ");
        sqlStatement.append("        " + value + ", ");
        sqlStatement.append("        '" + dateFormatter.format(new GregorianCalendar(year, month, day, hour, minute, second).getTime()) + "' ");
        sqlStatement.append("FROM    StatusMetrics Smet, ");
        sqlStatement.append("        Categories Ctgr ");
        sqlStatement.append("WHERE   Smet.Name = '" + statusMetricName + "' ");
        sqlStatement.append("  AND   Ctgr.Name = '" + categoryName + "';");

        return sqlStatement.toString();
    }

    /**
     * Creates a valid Access SQL insert statement for inserting an interval metric instance, based on the inputted parameters.
     * @param   year                The year.
     * @param   month               The month.
     * @param   day                 The day.
     * @param   hour                The hours.
     * @param   minute              The minutes.
     * @param   second              The seconds.
     * @param   milliseconds        The number of milliseconds in the interval.
     * @param   intervalMetricName  The name of the amount metric.
     * @param   categoryName        The name of the metric category.
     * @return  The SQL statement.
     */
    private String CreateIntervalMetricInsertSql(int year, int month, int day, int hour, int minute, int second, int milliseconds, String intervalMetricName, String categoryName) {
        StringBuilder sqlStatement = new StringBuilder();
        sqlStatement.append("INSERT ");
        sqlStatement.append("INTO    IntervalMetricInstances ");
        sqlStatement.append("        ( CtgrId, ");
        sqlStatement.append("          ImetId, ");
        sqlStatement.append("          MilliSeconds, ");
        sqlStatement.append("          [Timestamp] ");
        sqlStatement.append("          ) ");
        sqlStatement.append("SELECT  Ctgr.CtgrId, ");
        sqlStatement.append("        Imet.ImetId, ");
        sqlStatement.append("        " + milliseconds + ", ");
        sqlStatement.append("        '" + dateFormatter.format(new GregorianCalendar(year, month, day, hour, minute, second).getTime()) + "' ");
        sqlStatement.append("FROM    IntervalMetrics Imet, ");
        sqlStatement.append("        Categories Ctgr ");
        sqlStatement.append("WHERE   Imet.Name = '" + intervalMetricName + "' ");
        sqlStatement.append("  AND   Ctgr.Name = '" + categoryName + "';");

        return sqlStatement.toString();
    }
    

}

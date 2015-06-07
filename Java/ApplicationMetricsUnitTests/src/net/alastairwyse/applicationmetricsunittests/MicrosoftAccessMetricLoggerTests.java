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

import static org.junit.Assert.*;

import org.junit.Before;
import org.junit.Test;
import org.mockito.invocation.InvocationOnMock;
import org.mockito.stubbing.Answer;

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
     *       3 techniques are used to support this...
     *         1) For success tests a CountDownLatch object is passed into the test constructor of the LoopingWorkerThreadBufferProcessor (which in turn is passed into the constructor of the 
     *              MicrosoftAccessMetricLogger class).
     *         2) For exception tests, the CountDownLatch inside the class could not be used, as the exception is thrown before a single iteration of the loop in the thread has completed.
     *              I tried to simulate the NMock2.Signal.EventWaitHandle() method used in the equivalent C# tests, by creating a custom mockito Answer<Void> implementation, which would call 
     *              countDown() on a CountDownLatch passed to its constructor.  However I couldn't find a way to get mockito to throw an exception on the mock and then subsequently perform 
     *              the answer.  Hence currently this is solved by adding a Thread.yield() call to the test code, to force the main test thread to be suspended, and give time for the worker 
     *              thread to proceed and the exception be thrown before executing the verify and assert statements.  This means the test results are not deterministic, but was the best 
     *              solution I could find.  See test IncrementDatabaseInsertException() for an example of this pattern.
     *         3) An instance of the ExceptionStorer class is passed to the LoopingWorkerThreadBufferProcessor constructor, and used to verify the properties of exceptions thrown on the 
     *              worker thread.
     */
    
    private IBufferProcessingStrategy mockBufferProcessingStrategy;
    private BufferProcessedEventHandlerCapturer bufferProcessedEventHandlerCapturer;
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
        mockBufferProcessingStrategy = mock(IBufferProcessingStrategy.class);
        bufferProcessedEventHandlerCapturer = new BufferProcessedEventHandlerCapturer();
        exceptionStorer = new ExceptionStorer();
        mockConnection = mock(Connection.class);
        mockStatement = mock(Statement.class);
        mockCalendarProvider = mock(ICalendarProvider.class);
        dateFormatter = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
        dateFormatter.setTimeZone(TimeZone.getTimeZone("UTC"));
        dequeueOperationLoopCompleteSignal = new CountDownLatch(1);
        utilities = new ApplicationMetricsUnitTestUtilities();
        testMicrosoftAccessMetricLogger = new MicrosoftAccessMetricLogger(testDbFilePath, testMetricCategoryName, new LoopingWorkerThreadBufferProcessor(10, exceptionStorer, dequeueOperationLoopCompleteSignal), true, mockConnection, mockStatement, mockCalendarProvider); 
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
        Thread.yield();

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
        assertNull(exceptionStorer.getException());
    }
    
    @Test
    public void AddDatabaseInsertException() throws Exception {
        String expectedSqlStatement = CreateAmountMetricInsertSql(2014, 06, 14, 12, 45, 31, 12345, new TestMessageBytesReceivedMetric(0).getName(), testMetricCategoryName);

        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId))).thenReturn(new GregorianCalendar(2014, 06, 14, 12, 45, 31));
        when(mockStatement.execute(expectedSqlStatement.toString())).thenThrow(new SQLException("Mock SQLException."));

        testMicrosoftAccessMetricLogger.Add(new TestMessageBytesReceivedMetric(12345));
        testMicrosoftAccessMetricLogger.Start();
        Thread.yield();

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
        assertNull(exceptionStorer.getException());
    }
    
    @Test
    public void SetDatabaseInsertException() throws Exception {
        String expectedSqlStatement = CreateStatusMetricInsertSql(2014, 6, 17, 23, 42, 33, 301156000, new TestAvailableMemoryMetric(0).getName(), testMetricCategoryName);

        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId))).thenReturn(new GregorianCalendar(2014, 6, 17, 23, 42, 33));
        when(mockStatement.execute(expectedSqlStatement.toString())).thenThrow(new SQLException("Mock SQLException."));

        testMicrosoftAccessMetricLogger.Set(new TestAvailableMemoryMetric(301156000));
        testMicrosoftAccessMetricLogger.Start();
        Thread.yield();

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
        assertNull(exceptionStorer.getException());
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
        Thread.yield();
        
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
        Thread.yield();
        
        verify(mockCalendarProvider, times(2)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        assertEquals(IllegalStateException.class, exceptionStorer.getException().getCause().getClass());
        assertTrue(exceptionStorer.getException().getCause().getMessage().contains("Received duplicate begin 'MessageProcessingTime' metrics."));
        verifyZeroInteractions(mockCalendarProvider, mockConnection, mockStatement);
    }
    
    @Test
    public void BeginEndEndIntervalEventWithNoBegin() throws Exception {
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId))).thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 6, 14, 12, 45, 50, 31));
        
        testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.Start();
        Thread.yield();
        
        verify(mockCalendarProvider).getCalendar(TimeZone.getTimeZone(timeZoneId));
        assertEquals(IllegalStateException.class, exceptionStorer.getException().getCause().getClass());
        assertTrue(exceptionStorer.getException().getCause().getMessage().contains("Received end 'MessageProcessingTime' with no corresponding start interval metric."));
        verifyZeroInteractions(mockCalendarProvider, mockConnection, mockStatement);
    }
    
    @Test
    public void CancelBeginWithNoBeginAndQueuedMetrics() throws Exception {
        
        // Tests that an exception is thrown if the CancelBegin() method is called without a preceding Begin() method having been called for the same interval metric event
        String expectedSqlStatement = CreateIntervalMetricInsertSql(2015, 5, 6, 19, 36, 7, 567, new TestMessageProcessingTimeMetric().getName(), testMetricCategoryName);
        
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 06, 19, 36, 7, 12))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 06, 19, 36, 7, 579))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 06, 19, 36, 7, 731));

        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.CancelBegin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.Start();
        Thread.yield();

        verify(mockCalendarProvider, times(3)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        verify(mockStatement).execute(expectedSqlStatement.toString());
        assertEquals(IllegalStateException.class, exceptionStorer.getException().getCause().getClass());
        assertTrue(exceptionStorer.getException().getCause().getMessage().contains("Received cancel 'MessageProcessingTime' with no corresponding start interval metric."));
        verifyNoMoreInteractions(mockCalendarProvider, mockConnection, mockStatement);
    }
    
    @Test
    public void CancelBeginWithNoBeginAndNoQueuedMetrics() throws Exception {
        
        // Tests that an exception is thrown if the CancelBegin() method is called without a preceding Begin() where no metric events are currently queued
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2014, 6, 14, 12, 45, 50, 31));
        
        testMicrosoftAccessMetricLogger.CancelBegin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.Start();
        Thread.yield();
        
        verify(mockCalendarProvider).getCalendar(TimeZone.getTimeZone(timeZoneId));
        assertEquals(IllegalStateException.class, exceptionStorer.getException().getCause().getClass());
        assertTrue(exceptionStorer.getException().getCause().getMessage().contains("Received cancel 'MessageProcessingTime' with no corresponding start interval metric."));
        verifyNoMoreInteractions(mockCalendarProvider, mockConnection, mockStatement);
    }
    
    @Test
    public void BeginEndBufferProcessingBetweenBeginAndEndSuccessTests() throws Exception {

        // Tests that interval metrics are processed correctly when the buffers/queues are processed in between calls to Begin() and End().
        //   This test is actually testing functionality in class MetricLoggerBuffer
        String firstExpectedSqlStatement = CreateIntervalMetricInsertSql(2015, 4, 25, 17, 32, 14, 68, new TestDiskReadTimeMetric().getName(), testMetricCategoryName);
        String secondExpectedSqlStatement = CreateIntervalMetricInsertSql(2015, 4, 25, 17, 32, 14, 69, new TestMessageProcessingTimeMetric().getName(), testMetricCategoryName);
        
        doAnswer(bufferProcessedEventHandlerCapturer).when(mockBufferProcessingStrategy).setBufferProcessedEventHandler(any(IBufferProcessedEventHandler.class));
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 4, 25, 17, 32, 14, 0))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 4, 25, 17, 32, 14, 34))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 4, 25, 17, 32, 14, 68))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 4, 25, 17, 32, 14, 103));

        testMicrosoftAccessMetricLogger = new MicrosoftAccessMetricLogger(testDbFilePath, testMetricCategoryName, mockBufferProcessingStrategy, true, mockConnection, mockStatement, mockCalendarProvider);
        IBufferProcessedEventHandler capturedBufferProcessedEventHandler = bufferProcessedEventHandlerCapturer.getCapturedBufferProcessedEventHandler();
        testMicrosoftAccessMetricLogger.Begin(new TestDiskReadTimeMetric());
        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        capturedBufferProcessedEventHandler.BufferProcessed();
        testMicrosoftAccessMetricLogger.End(new TestDiskReadTimeMetric());
        testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
        capturedBufferProcessedEventHandler.BufferProcessed();

        verify(mockCalendarProvider, times(4)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        // 'Cleared' methods are expected twice each, corresponding to 2 callbacks of method BufferProcessed().
        //   NotifyIntervalMetricEventBuffered() is expected twice, corresponding to logging of 2 interval metrics.
        verify(mockBufferProcessingStrategy, times(2)).NotifyCountMetricEventBufferCleared();
        verify(mockBufferProcessingStrategy, times(2)).NotifyAmountMetricEventBufferCleared();
        verify(mockBufferProcessingStrategy, times(2)).NotifyStatusMetricEventBufferCleared();
        verify(mockBufferProcessingStrategy, times(2)).NotifyIntervalMetricEventBufferCleared();
        verify(mockBufferProcessingStrategy, times(2)).NotifyIntervalMetricEventBuffered();
        verify(mockStatement).execute(firstExpectedSqlStatement.toString());
        verify(mockStatement).execute(secondExpectedSqlStatement.toString());

        // Note - mock mockBufferProcessingStrategy should really be included in the below verifyNoMoreInteractions() call, but doing so causes a NoInteractionsWanted exception, 
        //   because the testMicrosoftAccessMetricLogger constructor is called in the test.  Tried moving this constructor to the setUp() method, but then 
        //   IBufferProcessedEventHandler object can't be captured.  Compromise is to remove mockBufferProcessingStrategy.
        verifyNoMoreInteractions(mockCalendarProvider, mockConnection, mockStatement);
        assertNull(exceptionStorer.getException());
    }
    
    @Test
    public void BeginCancelBeginBufferProcessingBetweenBeginAndCancelBeginSuccessTests() throws Exception {
        // Tests that CancelBegin() method works correctly when the interval metric event being cancelled has been moved to the start interval metric event HashMap object as the result of the IBufferProcessedEventHandler.BufferProcessed() callback.
        //   This test is actually testing functionality in class MetricLoggerBuffer
        String expectedSqlStatement = CreateIntervalMetricInsertSql(2015, 5, 06, 19, 36, 07, 567, new TestMessageProcessingTimeMetric().getName(), testMetricCategoryName);

        doAnswer(bufferProcessedEventHandlerCapturer).when(mockBufferProcessingStrategy).setBufferProcessedEventHandler(any(IBufferProcessedEventHandler.class));
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 06, 19, 36, 7, 2))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 06, 19, 36, 7, 7))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 06, 19, 36, 7, 12))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 06, 19, 36, 7, 579));
        
        testMicrosoftAccessMetricLogger = new MicrosoftAccessMetricLogger(testDbFilePath, testMetricCategoryName, mockBufferProcessingStrategy, true, mockConnection, mockStatement, mockCalendarProvider);
        IBufferProcessedEventHandler capturedBufferProcessedEventHandler = bufferProcessedEventHandlerCapturer.getCapturedBufferProcessedEventHandler();
        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        capturedBufferProcessedEventHandler.BufferProcessed();
        testMicrosoftAccessMetricLogger.CancelBegin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
        capturedBufferProcessedEventHandler.BufferProcessed();
        
        verify(mockCalendarProvider, times(4)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        // 'Cleared' methods are expected twice each, corresponding to 2 callbacks of method BufferProcessed().
        //   NotifyIntervalMetricEventBuffered() is expected once, corresponding to logging of 1 interval metric.
        verify(mockBufferProcessingStrategy, times(2)).NotifyCountMetricEventBufferCleared();
        verify(mockBufferProcessingStrategy, times(2)).NotifyAmountMetricEventBufferCleared();
        verify(mockBufferProcessingStrategy, times(2)).NotifyStatusMetricEventBufferCleared();
        verify(mockBufferProcessingStrategy, times(2)).NotifyIntervalMetricEventBufferCleared();
        verify(mockBufferProcessingStrategy).NotifyIntervalMetricEventBuffered();
        
        verify(mockStatement).execute(expectedSqlStatement.toString());

        // See notes on this same method call in test BeginEndBufferProcessingBetweenBeginAndEndSuccessTests().  The same applies here.
        verifyNoMoreInteractions(mockCalendarProvider, mockConnection, mockStatement);
        assertNull(exceptionStorer.getException());
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
            // Note below return makes the end time before the begin time.  Class should insert the resulting milliseconds interval as 0.
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
        assertNull(exceptionStorer.getException());
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
        assertNull(exceptionStorer.getException());
    }
    
    @Test
    public void BeginEndNoCheckingSuccessTests() throws Exception {
        // Tests the Begin() and End() methods of the class with parameter 'intervalMetricChecking' set to false
        testMicrosoftAccessMetricLogger = new MicrosoftAccessMetricLogger(testDbFilePath, testMetricCategoryName, new LoopingWorkerThreadBufferProcessor(10, exceptionStorer, dequeueOperationLoopCompleteSignal), false, mockConnection, mockStatement, mockCalendarProvider);
        
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
        assertNull(exceptionStorer.getException());
    }
    
    @Test
    public void CancelBeginSuccessTests() throws Exception {
        String expectedSqlStatement = CreateIntervalMetricInsertSql(2015, 5, 6, 19, 36, 7, 567, new TestMessageProcessingTimeMetric().getName(), testMetricCategoryName);
        
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 06, 19, 36, 7, 2))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 06, 19, 36, 7, 5))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 06, 19, 36, 7, 12))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 06, 19, 36, 7, 579));

        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.CancelBegin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(4)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        verify(mockStatement).execute(expectedSqlStatement.toString());
        verifyNoMoreInteractions(mockCalendarProvider, mockConnection, mockStatement);
        assertNull(exceptionStorer.getException());
    }
    
    @Test
    public void CancelBeginQueueMaintenanceSuccessTests() throws Exception {
        // Tests that the rebuilding of the interval metric queue performed by the CancelBegin() method preserves the queue order for the interval metrics that are not cancelled
        //   Note this test was created specifically to test a previous implementation of MetricLoggerBuffer where cancelling of an interval metric was performed by the main thread.  
        //   In the current implementation of MetricLoggerBuffer, cancelling is performed by the buffer processing strategy worker thread, and hence this test is equivalent to test CancelBeginSuccessTests().
        //   However, it will be kept for extra thoroughness of testing.
        String firstExpectedSqlStatement = CreateIntervalMetricInsertSql(2015, 5, 6, 8, 9, 56, 121002, new TestDiskReadTimeMetric().getName(), testMetricCategoryName);
        String secondExpectedSqlStatement = CreateIntervalMetricInsertSql(2015, 5, 6, 8, 10, 5, 121109, new TestDiskWriteTimeMetric().getName(), testMetricCategoryName);

        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 6, 8, 9, 56, 100))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 6, 8, 10, 1, 200))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 6, 8, 10, 5, 300))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 6, 8, 10, 6, 301))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 6, 8, 11, 57, 102))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 6, 8, 12, 6, 409));

        testMicrosoftAccessMetricLogger.Begin(new TestDiskReadTimeMetric());
        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.Begin(new TestDiskWriteTimeMetric());
        testMicrosoftAccessMetricLogger.CancelBegin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.End(new TestDiskReadTimeMetric());
        testMicrosoftAccessMetricLogger.End(new TestDiskWriteTimeMetric());
        testMicrosoftAccessMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        
        verify(mockCalendarProvider, times(6)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        verify(mockStatement).execute(firstExpectedSqlStatement.toString());
        verify(mockStatement).execute(secondExpectedSqlStatement.toString());
        verifyNoMoreInteractions(mockCalendarProvider, mockConnection, mockStatement);
        assertNull(exceptionStorer.getException());
    }
    
    @Test
    public void CancelBeginLongQueueSuccessTests() throws Exception {
        // Tests the case where several successive start and end interval metric events exist in the interval metric queue when CancelBegin() is called
        //   Ensures only the most recent end interval metric is removed from the queue
        //   Note this test was created specifically to test a previous implementation of MetricLoggerBuffer where cancelling of an interval metric was performed by the main thread.  
        //   In the current implementation of MetricLoggerBuffer, cancelling is performed by the buffer processing strategy worker thread, and hence this test is equivalent to test CancelBeginSuccessTests().
        //   However, it will be kept for extra thoroughness of testing.
        String firstExpectedSqlStatement = CreateIntervalMetricInsertSql(2015, 5, 12, 22, 49, 01, 203, new TestMessageProcessingTimeMetric().getName(), testMetricCategoryName);
        String secondExpectedSqlStatement = CreateIntervalMetricInsertSql(2015, 5, 12, 22, 49, 52, 304, new TestMessageProcessingTimeMetric().getName(), testMetricCategoryName);

        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 12, 22, 49, 1, 100))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 12, 22, 49, 1, 303))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 12, 22, 49, 52, 400))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 12, 22, 49, 52, 704))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 12, 22, 49, 59, 800))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 12, 22, 49, 59, 905));
        
        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.CancelBegin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();

        verify(mockCalendarProvider, times(6)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        verify(mockStatement).execute(firstExpectedSqlStatement.toString());
        verify(mockStatement).execute(secondExpectedSqlStatement.toString());
        verifyNoMoreInteractions(mockCalendarProvider, mockConnection, mockStatement);
        assertNull(exceptionStorer.getException());
    }
    
    @Test
    public void CancelBeginStartIntervalMetricInEventStoreSuccessTests() throws Exception {
        // Tests the case where CancelBegin() is called, and the start interval metric to cancel is stored in the start interval metric event store
        //   Expects that the start interval metric is correctly removed from the start interval metric event store
        //   Note this test was created specifically to test a previous implementation of MetricLoggerBuffer where cancelling of an interval metric was performed by the main thread.  
        //   In the current implementation of MetricLoggerBuffer, cancelling is performed by the buffer processing strategy worker thread, and hence this test is equivalent to test CancelBeginSuccessTests().
        //   However, it will be kept for extra thoroughness of testing.
        String expectedSqlStatement = CreateIntervalMetricInsertSql(2015, 5, 12, 22, 57, 01, 506, new TestMessageProcessingTimeMetric().getName(), testMetricCategoryName);
        
        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 12, 22, 57, 1, 100))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 12, 22, 57, 1, 606))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 12, 22, 57, 2, 400))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 12, 22, 57, 3, 513))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 12, 22, 57, 3, 704));
        
        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        // Due to calling Start() the preceding start interval metric should be moved to the start interval metric event store
        testMicrosoftAccessMetricLogger.Start();
        dequeueOperationLoopCompleteSignal.await();
        testMicrosoftAccessMetricLogger.CancelBegin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        
        verify(mockCalendarProvider, times(5)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        verify(mockStatement).execute(expectedSqlStatement.toString());
        verifyNoMoreInteractions(mockCalendarProvider, mockConnection, mockStatement);
        assertNull(exceptionStorer.getException());
    }
    
    @Test
    public void CancelBeginNoCheckingSuccessTests() throws Exception {
        // Tests that a call to CancelBegin() with no preceding call to Begin() for the same metric with 'intervalMetricChecking' set to false, will not throw an exception
        testMicrosoftAccessMetricLogger = new MicrosoftAccessMetricLogger(testDbFilePath, testMetricCategoryName, new LoopingWorkerThreadBufferProcessor(10, exceptionStorer, dequeueOperationLoopCompleteSignal), false, mockConnection, mockStatement, mockCalendarProvider);

        when(mockCalendarProvider.getCalendar(TimeZone.getTimeZone(timeZoneId)))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 6, 19, 36, 7, 012))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 6, 19, 36, 7, 579))
            .thenReturn(utilities.CreateCalendarWithMilliseconds(2015, 5, 6, 19, 36, 7, 645));
        
        testMicrosoftAccessMetricLogger.Begin(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.End(new TestMessageProcessingTimeMetric());
        testMicrosoftAccessMetricLogger.CancelBegin(new TestDiskReadTimeMetric());
        
        verify(mockCalendarProvider, times(3)).getCalendar(TimeZone.getTimeZone(timeZoneId));
        verifyNoMoreInteractions(mockCalendarProvider, mockConnection, mockStatement);
        assertNull(exceptionStorer.getException());
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
    
    /**
     * Mock answer for the IBufferProcessingStrategy.setBufferProcessedEventHandler() method.  Captures the IBufferProcessedEventHandler object passed to this method, to allow the BufferProcessed() method to be explicitly called on the object in unit test code.
     * @author Alastair Wyse
     */
    private class BufferProcessedEventHandlerCapturer implements Answer<Void> {

        private IBufferProcessedEventHandler capturedBufferProcessedEventHandler;
        
        /**
         * @return  The IBufferProcessedEventHandler object set using the setBufferProcessedEventHandler() method.
         */
        public IBufferProcessedEventHandler getCapturedBufferProcessedEventHandler() {
            return capturedBufferProcessedEventHandler;
        }
        
        @Override
        public Void answer(InvocationOnMock invocation) throws Throwable {
            capturedBufferProcessedEventHandler = (IBufferProcessedEventHandler)invocation.getArguments()[0];
            return null;
        }
    }
}

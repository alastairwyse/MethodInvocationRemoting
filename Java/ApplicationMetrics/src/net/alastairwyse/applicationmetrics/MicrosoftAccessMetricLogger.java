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

import java.lang.Thread.*;
import java.sql.*;
import java.text.*;
import java.util.*;

import net.alastairwyse.operatingsystemabstraction.*;

/**
 * Writes metric and instrumentation events for an application to a Microsoft Access database.
 * <b>Note</b> as of Java 8, the JDBC-ODBC bridge has been removed, which means this class no longer functions.  It seems to work using 3rd part library UCanAccess (http://ucanaccess.sourceforge.net/site.html), but this library does not support the MS Access 97 format of the included blank Access database.  The blank database could be upgraded to a later format, however as per the applicationmetrics documentation, this class is really meant to serve as just an example of how the ApplicationMetrics interfaces could be implemented to support relational databases.
 * @author Alastair Wyse
 */
public class MicrosoftAccessMetricLogger extends MetricLoggerBuffer implements AutoCloseable {

    private final String accessConnectionStringPrefix = "jdbc:odbc:;Driver={Microsoft Access Driver (*.mdb)};DBQ=";
    protected final String timeZoneId = "UTC";
    
    private String databaseFilePath;
    private String metricCategoryName;
    private Connection dbConnection;
    private Statement dbStatement;
    private SimpleDateFormat dateFormatter;

    /**
     * Initialises a new instance of the MicrosoftAccessMetricLogger class.
     * This constructor defaults to using the LoopingWorkerThreadBufferProcessor as the buffer processing strategy, and is maintained for backwards compatibility. 
     * @param  databaseFilePath              The full path to the Microsoft Access data file.
     * @param  metricCategoryName            The name of the category which the metric events should be logged under in the database.
     * @param  dequeueOperationLoopInterval  The time to wait in between iterations of the worker thread which dequeues metric events and writes them to the Access database.
     * @param  intervalMetricChecking        Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).
     * @param  exceptionHandler              Handler for any uncaught exceptions occurring on the worker thread.
     */
    public MicrosoftAccessMetricLogger(String databaseFilePath, String metricCategoryName, int dequeueOperationLoopInterval, boolean intervalMetricChecking, UncaughtExceptionHandler exceptionHandler) {
        this(databaseFilePath, metricCategoryName, new LoopingWorkerThreadBufferProcessor(dequeueOperationLoopInterval, exceptionHandler), intervalMetricChecking);
    }
    
    /**
     * Initialises a new instance of the MicrosoftAccessMetricLogger class.
     * @param  databaseFilePath          The full path to the Microsoft Access data file.
     * @param  metricCategoryName        The name of the category which the metric events should be logged under in the database.
     * @param  bufferProcessingStrategy  Object which implements a processing strategy for the buffers (queues).
     * @param  intervalMetricChecking    Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).
     */
    public MicrosoftAccessMetricLogger(String databaseFilePath, String metricCategoryName, IBufferProcessingStrategy bufferProcessingStrategy, boolean intervalMetricChecking) {
        super(bufferProcessingStrategy, intervalMetricChecking);
        InitialisePrivateMembers(databaseFilePath, metricCategoryName);
    }
    
    /**
     * Initialises a new instance of the MicrosoftAccessMetricLogger class.  
     * <b>Note</b> this is an additional constructor to facilitate unit tests, and should not be used to instantiate the class under normal conditions.
     * @param  databaseFilePath          The full path to the Microsoft Access data file.
     * @param  metricCategoryName        The name of the category which the metric events should be logged under in the database.
     * @param  bufferProcessingStrategy  Object which implements a processing strategy for the buffers (queues).
     * @param  intervalMetricChecking    Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).
     * @param  dbConnection              A test (mock) connection object.
     * @param  dbStatement               A test (mock) statement object.
     * @param  calendarProvider          A test (mock) ICalendarProvider object.
     */
    public MicrosoftAccessMetricLogger(String databaseFilePath, String metricCategoryName, IBufferProcessingStrategy bufferProcessingStrategy, boolean intervalMetricChecking, Connection dbConnection, Statement dbStatement, ICalendarProvider calendarProvider) {
        super(bufferProcessingStrategy, intervalMetricChecking, calendarProvider);
        InitialisePrivateMembers(databaseFilePath, metricCategoryName);
        this.dbConnection = dbConnection;
        this.dbStatement = dbStatement;
    }
    
    /**
     * Connects to the configured database.
     * @throws Exception  if an error occurs attempting to connect to the database.
     */
    public void Connect() throws Exception {
        if (CheckConnected() == true) {
            throw new Exception("Connection to database has already been established.");
        }
        try {
            dbConnection = DriverManager.getConnection(accessConnectionStringPrefix + databaseFilePath + ";", "", "");
            dbConnection.setAutoCommit(true);
            dbStatement = dbConnection.createStatement();
        }
        catch (Exception e) {
            throw new Exception("Failed to connect to database at path '" + databaseFilePath + "'.", e);
        }
    }
    
    /**
     * Disconnects from the database.
     * @throws  SQLException  if an error occurs accessing the database connection.
     * @throws  Exception     if an error occurs attempting to disconnect from the database.
     */
    public void Disconnect() throws SQLException, Exception {
        if (CheckConnected() == true) {
            try {
                dbStatement.close();
                dbConnection.close();
            }
            catch (Exception e) {
                throw new Exception("Failed to disconnect from database.", e);
            }
        }
    }

    @Override
    protected void ProcessCountMetricEvent(CountMetricEventInstance countMetricEvent) throws Exception {
        StringBuilder sqlInsertStatement = new StringBuilder();
        sqlInsertStatement.append("INSERT ");
        sqlInsertStatement.append("INTO    CountMetricInstances ");
        sqlInsertStatement.append("        ( CmetId, ");
        sqlInsertStatement.append("          CtgrId, ");
        sqlInsertStatement.append("          [Timestamp] ");
        sqlInsertStatement.append("          ) ");
        sqlInsertStatement.append("SELECT  Cmet.CmetId, ");
        sqlInsertStatement.append("        Ctgr.CtgrId, ");
        sqlInsertStatement.append("        '" + dateFormatter.format(countMetricEvent.getEventTime().getTime()) + "' ");
        sqlInsertStatement.append("FROM    CountMetrics Cmet, ");
        sqlInsertStatement.append("        Categories Ctgr ");
        sqlInsertStatement.append("WHERE   Cmet.Name = '" + countMetricEvent.getMetric().getName() + "' ");
        sqlInsertStatement.append("  AND   Ctgr.Name = '" + metricCategoryName + "';");

        try {
            dbStatement.execute(sqlInsertStatement.toString());
        }
        catch (Exception e) {
            throw new Exception("Failed to insert instance of count metric '" + countMetricEvent.getMetric().getName() + "'.", e);
        }
    }

    @Override
    protected void ProcessAmountMetricEvent(AmountMetricEventInstance amountMetricEvent) throws Exception {
        StringBuilder sqlInsertStatement = new StringBuilder();
        sqlInsertStatement.append("INSERT ");
        sqlInsertStatement.append("INTO    AmountMetricInstances ");
        sqlInsertStatement.append("        ( CtgrId, ");
        sqlInsertStatement.append("          AmetId, ");
        sqlInsertStatement.append("          Amount, ");
        sqlInsertStatement.append("          [Timestamp] ");
        sqlInsertStatement.append("          ) ");
        sqlInsertStatement.append("SELECT  Ctgr.CtgrId, ");
        sqlInsertStatement.append("        Amet.AmetId, ");
        sqlInsertStatement.append("        " + amountMetricEvent.getMetric().getAmount() + ", ");
        sqlInsertStatement.append("        '" + dateFormatter.format(amountMetricEvent.getEventTime().getTime()) + "' ");
        sqlInsertStatement.append("FROM    AmountMetrics Amet, ");
        sqlInsertStatement.append("        Categories Ctgr ");
        sqlInsertStatement.append("WHERE   Amet.Name = '" + amountMetricEvent.getMetric().getName() + "' ");
        sqlInsertStatement.append("  AND   Ctgr.Name = '" + metricCategoryName + "';");

        try {
            dbStatement.execute(sqlInsertStatement.toString());
        }
        catch (Exception e) {
            throw new Exception("Failed to insert instance of amount metric '" + amountMetricEvent.getMetric().getName() + "'.", e);
        }
    }

    @Override
    protected void ProcessStatusMetricEvent(StatusMetricEventInstance statusMetricEvent) throws Exception {
        StringBuilder sqlInsertStatement = new StringBuilder();
        sqlInsertStatement.append("INSERT ");
        sqlInsertStatement.append("INTO    StatusMetricInstances ");
        sqlInsertStatement.append("        ( CtgrId, ");
        sqlInsertStatement.append("          SmetId, ");
        sqlInsertStatement.append("          [Value], ");
        sqlInsertStatement.append("          [Timestamp] ");
        sqlInsertStatement.append("          ) ");
        sqlInsertStatement.append("SELECT  Ctgr.CtgrId, ");
        sqlInsertStatement.append("        Smet.SmetId, ");
        sqlInsertStatement.append("        " + statusMetricEvent.getMetric().getValue() + ", ");
        sqlInsertStatement.append("        '" + dateFormatter.format(statusMetricEvent.getEventTime().getTime()) + "' ");
        sqlInsertStatement.append("FROM    StatusMetrics Smet, ");
        sqlInsertStatement.append("        Categories Ctgr ");
        sqlInsertStatement.append("WHERE   Smet.Name = '" + statusMetricEvent.getMetric().getName() + "' ");
        sqlInsertStatement.append("  AND   Ctgr.Name = '" + metricCategoryName + "';");

        try {
            dbStatement.execute(sqlInsertStatement.toString());
        }
        catch (Exception e) {
            throw new Exception("Failed to insert instance of status metric '" + statusMetricEvent.getMetric().getName() + "'.", e);
        }
    }

    @Override
    protected void ProcessIntervalMetricEvent(IntervalMetricEventInstance intervalMetricEvent, long duration) throws Exception {
        StringBuilder sqlInsertStatement = new StringBuilder();
        sqlInsertStatement.append("INSERT ");
        sqlInsertStatement.append("INTO    IntervalMetricInstances ");
        sqlInsertStatement.append("        ( CtgrId, ");
        sqlInsertStatement.append("          ImetId, ");
        sqlInsertStatement.append("          MilliSeconds, ");
        sqlInsertStatement.append("          [Timestamp] ");
        sqlInsertStatement.append("          ) ");
        sqlInsertStatement.append("SELECT  Ctgr.CtgrId, ");
        sqlInsertStatement.append("        Imet.ImetId, ");
        sqlInsertStatement.append("        " + duration + ", ");
        sqlInsertStatement.append("        '" + dateFormatter.format(intervalMetricEvent.getEventTime().getTime()) + "' ");
        sqlInsertStatement.append("FROM    IntervalMetrics Imet, ");
        sqlInsertStatement.append("        Categories Ctgr ");
        sqlInsertStatement.append("WHERE   Imet.Name = '" + intervalMetricEvent.getMetric().getName() + "' ");
        sqlInsertStatement.append("  AND   Ctgr.Name = '" + metricCategoryName + "';");

        try {
            dbStatement.execute(sqlInsertStatement.toString());
        }
        catch (Exception e) {
            throw new Exception("Failed to insert instance of interval metric '" + intervalMetricEvent.getMetric().getName() + "'.", e);
        }
    }

    /**
     * Initialises private members of the class.
     * @param databaseFilePath    The full path to the Microsoft Access data file.
     * @param metricCategoryName  The name of the category which the metric events should be logged under in the database.
     */
    private void InitialisePrivateMembers(String databaseFilePath, String metricCategoryName) {
        this.databaseFilePath = databaseFilePath;
        if (metricCategoryName.trim().equals("") == false) {
            this.metricCategoryName = metricCategoryName;
        }
        else {
            throw new IllegalArgumentException("Argument 'metricCategoryName' cannot be blank.");
        }
        dateFormatter = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
        dateFormatter.setTimeZone(TimeZone.getTimeZone(timeZoneId));
    }

    /**
     * Checks whether a connection to the database has been established.
     * @return               True if currently connected.
     * @throws SQLException  if an error occurs accessing the database connection.
     */
    private boolean CheckConnected() throws SQLException {
        if(dbConnection == null || dbStatement == null) {
            return false;
        }
        if(dbConnection.isClosed() == true) {
            return false;
        }
        
        return true;
    }
    
    @Override
    public void close() throws SQLException {
        if(dbStatement != null) {
            dbStatement.close();
        }
        if(dbConnection != null) {
            dbConnection.close();
        }
    }
}

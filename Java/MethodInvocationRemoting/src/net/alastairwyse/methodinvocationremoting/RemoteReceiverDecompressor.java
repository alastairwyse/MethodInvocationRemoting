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

package net.alastairwyse.methodinvocationremoting;

import java.util.Vector;
import java.io.*;
import java.util.zip.*;
import org.apache.commons.codec.binary.Base64;
import net.alastairwyse.applicationlogging.*;
import net.alastairwyse.applicationmetrics.*;
import net.alastairwyse.methodinvocationremotingmetrics.*;

/**
 * Decompresses a message, after receiving it from a remote location via an underlying IRemoteReceiver implementation.
 * @author Alastair Wyse
 */
public class RemoteReceiverDecompressor implements IRemoteReceiver {

    private IRemoteReceiver remoteReceiver;
    private int decompressionBufferSize;
    private String stringEncodingCharset = "UTF-8";
    private volatile boolean decompressing = false;
    private IApplicationLogger logger;
    private LoggingUtilities loggingUtilities;
    private IMetricLogger metricLogger;
    
    /**
     * Initialises a new instance of the RemoteReceiverDecompressor class.
     * @param underlyingRemoteReceiver  The remote receiver to receive the message from before decompressing.
     */
    public RemoteReceiverDecompressor(IRemoteReceiver underlyingRemoteReceiver) {
        this.remoteReceiver = underlyingRemoteReceiver;
        this.decompressionBufferSize = 1024;
        logger = new ConsoleApplicationLogger(LogLevel.Information, '|', "  ");
        loggingUtilities = new LoggingUtilities(logger);
        metricLogger = new NullMetricLogger();
    }
    
    /**
     * Initialises a new instance of the RemoteReceiverDecompressor class.
     * @param underlyingRemoteReceiver  The remote receiver to receive the message from before decompressing.
     * @param logger                    The logger to write log events to.
     */
    public RemoteReceiverDecompressor(IRemoteReceiver underlyingRemoteReceiver, IApplicationLogger logger) {
        this(underlyingRemoteReceiver);
        this.logger = logger;
        loggingUtilities = new LoggingUtilities(logger);
    }
    
    /**
     * Initialises a new instance of the RemoteReceiverDecompressor class.
     * @param underlyingRemoteReceiver  The remote receiver to receive the message from before decompressing.
     * @param metricLogger              The metric logger to write metric and instrumentation events to.
     */
    public RemoteReceiverDecompressor(IRemoteReceiver underlyingRemoteReceiver, IMetricLogger metricLogger) {
        this(underlyingRemoteReceiver);
        this.metricLogger = metricLogger;
    }
    
    /**
     * Initialises a new instance of the RemoteReceiverDecompressor class.
     * @param underlyingRemoteReceiver  The remote receiver to receive the message from before decompressing.
     * @param logger                    The logger to write log events to.
     * @param metricLogger              The metric logger to write metric and instrumentation events to.
     */
    public RemoteReceiverDecompressor(IRemoteReceiver underlyingRemoteReceiver, IApplicationLogger logger, IMetricLogger metricLogger) {
        this(underlyingRemoteReceiver);
        this.logger = logger;
        loggingUtilities = new LoggingUtilities(logger);
        this.metricLogger = metricLogger;
    }
    
    /**
     * Initialises a new instance of the RemoteReceiverDecompressor class.
     * @param underlyingRemoteReceiver   The remote receiver to receive the message from before compressing.
     * @param decompressionBufferSize    The size of the buffer to use when decompressing the message in bytes.  Denotes how much data will be read from the internal stream decompressor class in each read operation.  Should be set to match the expected decompressed message size as closely as possible.
     * @throws IllegalArgumentException  If the specified decompressionBufferSize is less than 1.
     */
    public RemoteReceiverDecompressor(IRemoteReceiver underlyingRemoteReceiver, int decompressionBufferSize) {
        this(underlyingRemoteReceiver);
        if (decompressionBufferSize < 1) {
            throw new IllegalArgumentException("Argument 'decompressionBufferSize' must be greater than 0.");
        }
        this.decompressionBufferSize = decompressionBufferSize;
    }
    
    /**
     * Initialises a new instance of the RemoteReceiverDecompressor class.
     * @param underlyingRemoteReceiver   The remote receiver to receive the message from before compressing.
     * @param decompressionBufferSize    The size of the buffer to use when decompressing the message in bytes.  Denotes how much data will be read from the internal stream decompressor class in each read operation.  Should be set to match the expected decompressed message size as closely as possible.
     * @param logger                     The logger to write log events to.
     * @throws IllegalArgumentException  If the specified decompressionBufferSize is less than 1.
     */
    public RemoteReceiverDecompressor(IRemoteReceiver underlyingRemoteReceiver, int decompressionBufferSize, IApplicationLogger logger) {
        this(underlyingRemoteReceiver, decompressionBufferSize);
        this.logger = logger;
        loggingUtilities = new LoggingUtilities(logger);
    }
    
    /**
     * Initialises a new instance of the RemoteReceiverDecompressor class.
     * @param underlyingRemoteReceiver   The remote receiver to receive the message from before compressing.
     * @param decompressionBufferSize    The size of the buffer to use when decompressing the message in bytes.  Denotes how much data will be read from the internal stream decompressor class in each read operation.  Should be set to match the expected decompressed message size as closely as possible.
     * @param metricLogger               The metric logger to write metric and instrumentation events to.
     * @throws IllegalArgumentException  If the specified decompressionBufferSize is less than 1.
     */
    public RemoteReceiverDecompressor(IRemoteReceiver underlyingRemoteReceiver, int decompressionBufferSize, IMetricLogger metricLogger) {
        this(underlyingRemoteReceiver, decompressionBufferSize);
        this.metricLogger = metricLogger;
    }
    
    /**
     * Initialises a new instance of the RemoteReceiverDecompressor class.
     * @param underlyingRemoteReceiver   The remote receiver to receive the message from before compressing.
     * @param decompressionBufferSize    The size of the buffer to use when decompressing the message in bytes.  Denotes how much data will be read from the internal stream decompressor class in each read operation.  Should be set to match the expected decompressed message size as closely as possible.
     * @param logger                     The logger to write log events to.
     * @param metricLogger               The metric logger to write metric and instrumentation events to.
     * @throws IllegalArgumentException  If the specified decompressionBufferSize is less than 1.
     */
    public RemoteReceiverDecompressor(IRemoteReceiver underlyingRemoteReceiver, int decompressionBufferSize, IApplicationLogger logger, IMetricLogger metricLogger) {
        this(underlyingRemoteReceiver, decompressionBufferSize);
        this.logger = logger;
        loggingUtilities = new LoggingUtilities(logger);
        this.metricLogger = metricLogger;
    }
    
    @Override
    public String Receive() throws Exception {
        return DecompressString(remoteReceiver.Receive());
    }

    @Override
    public void CancelReceive() {
        remoteReceiver.CancelReceive();
        while (decompressing == true);
        
        /* //[BEGIN_LOGGING]
        try {
            logger.Log(this, LogLevel.Information, "Receive operation cancelled.");
        }
        catch(Exception e) {
        }
        //[END_LOGGING] */
    }

    /**
     * Decompresses a string.
     * @param inputString  The string to decompress.
     * @return             The decompressed string.
     */
    private String DecompressString(String inputString) throws Exception {
        /* //[BEGIN_METRICS]
        metricLogger.Begin(new StringDecompressTime());
        //[END_METRICS] */
        
        decompressing = true;
        
        Vector<byte[]> readBuffers = new Vector<byte[]>();
        int currentReadBufferPosition = 0;
        String returnString = "";
        
        // If the inputted string is blank, then skip decompression and return a blank string.  The C# RemoteSenderCompressor class will compress a blank string as blank.
        if (inputString.equals("") == false) {
            byte[] encodedBytes = Base64.decodeBase64(inputString);
            try (ByteArrayInputStream decompressedStringStream = new ByteArrayInputStream(encodedBytes);
                 GZIPInputStream decompressor = new GZIPInputStream(decompressedStringStream)) {
                int bytesRead = 0;
                while (bytesRead != -1) {
                    // If the vector of buffers is empty, or the read position in the current (last) buffer is at the end of the buffer, then create a new read buffer
                    if ((readBuffers.size() == 0) || (currentReadBufferPosition == decompressionBufferSize)) {
                        readBuffers.add(new byte[decompressionBufferSize]);
                        currentReadBufferPosition = 0;
                        
                        /* //[BEGIN_METRICS]
                        metricLogger.Increment(new RemoteReceiverDecompressorReadBufferCreated());
                        //[END_METRICS] */
                    }
                    currentReadBufferPosition = currentReadBufferPosition + bytesRead;
                    bytesRead = decompressor.read(readBuffers.lastElement(), currentReadBufferPosition, decompressionBufferSize - currentReadBufferPosition);
                }
                
                // Create decompressed byte array with size as buffer size times the number of buffers (except the last buffer), plus the position within the last buffer
                byte[] decompressedBytes = new byte[((readBuffers.size() - 1) * decompressionBufferSize) + currentReadBufferPosition];
                // Copy the contents of the read buffers into the decompressed byte array
                int decompressedBytesPosition = 0;
                for (byte[] currentReadBuffer : readBuffers) {
                    if(currentReadBuffer != readBuffers.lastElement()) {
                        System.arraycopy(currentReadBuffer, 0, decompressedBytes, decompressedBytesPosition, decompressionBufferSize);
                        decompressedBytesPosition = decompressedBytesPosition + decompressionBufferSize;
                    }
                    else {
                        System.arraycopy(currentReadBuffer, 0, decompressedBytes, decompressedBytesPosition, currentReadBufferPosition);
                    }
                }
                
                returnString = new String(decompressedBytes, stringEncodingCharset);
            }
            catch (Exception e) {
                /* //[BEGIN_METRICS]
                metricLogger.CancelBegin(new StringDecompressTime());
                //[END_METRICS] */
                throw new Exception("Error decompressing message.", e);
            }
        }
        
        /* //[BEGIN_METRICS]
        metricLogger.End(new StringDecompressTime());
        metricLogger.Increment(new StringDecompressed());
        //[END_METRICS] */
        /* //[BEGIN_LOGGING]
        loggingUtilities.LogDecompressedString(this, returnString);
        //[END_LOGGING] */
        
        decompressing = false;
        return returnString;
    }
}

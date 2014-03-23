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

package net.alastairwyse.methodinvocationremoting;

import java.io.*;
import java.util.zip.*;
import org.apache.commons.codec.binary.*;
import net.alastairwyse.applicationlogging.*;

public class RemoteSenderCompressor implements IRemoteSender {

    private IRemoteSender remoteSender;
    private String stringEncodingCharset = "UTF-8";
    private IApplicationLogger logger;
    private LoggingUtilities loggingUtilities;
    
    /**
     * Initialises a new instance of the RemoteSenderCompressor class.
     * @param underlyingRemoteSender  The remote sender to send the message to after compressing.
     */
    public RemoteSenderCompressor(IRemoteSender underlyingRemoteSender) {
        remoteSender = underlyingRemoteSender;
        logger = new ConsoleApplicationLogger(LogLevel.Information, '|', "  ");
        loggingUtilities = new LoggingUtilities(logger);
    }
    
    /**
     * Initialises a new instance of the RemoteSenderCompressor class.
     * @param underlyingRemoteSender  The remote sender to send the message to after compressing.
     * @param logger                  The logger to write log events to.
     */
    public RemoteSenderCompressor(IRemoteSender underlyingRemoteSender, IApplicationLogger logger) {
        this(underlyingRemoteSender);
        this.logger = logger;
        loggingUtilities = new LoggingUtilities(logger);
    }
    
    @Override
    public void Send(String message) throws Exception {
        remoteSender.Send(CompressString(message));
    }

    /**
     * Compresses a string.
     * @param inputString  The string to compress.
     * @return             The compressed string.
     * @throws Exception   If an error occurs whilst compressing the string.
     */
    private String CompressString(String inputString) throws Exception {
        byte[] compressedByteArray;
        
        try (ByteArrayOutputStream compressedStringStream = new ByteArrayOutputStream();
             GZIPOutputStream compressor = new GZIPOutputStream(compressedStringStream)) {
            byte[] inputStringBytes = inputString.getBytes(stringEncodingCharset);
            compressor.write(inputStringBytes);
            // Although close() is called as part of this try-with-resources statement, the stream is not written correctly unless it is closed before reading 
            compressor.close();
            compressedByteArray = compressedStringStream.toByteArray();
        }
        catch (Exception e) {
            throw new Exception ("Error compressing message.", e);
        }
        
        String returnString = Base64.encodeBase64String(compressedByteArray);
        
        /* //[BEGIN_LOGGING]
        loggingUtilities.LogCompressedString(this, returnString);
        //[END_LOGGING] */

        return returnString;
    }
}

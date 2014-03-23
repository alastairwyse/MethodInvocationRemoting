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

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using ApplicationLogging;

namespace MethodInvocationRemoting
{
    //******************************************************************************
    //
    // Class: RemoteSenderCompressor
    //
    //******************************************************************************
    /// <summary>
    /// Compresses a message, before passing to an underlying IRemoteSender implementation to send to a remote location.
    /// </summary>
    public class RemoteSenderCompressor : IRemoteSender
    {
        private IRemoteSender remoteSender;
        private Encoding stringEncoding = Encoding.UTF8;
        private IApplicationLogger logger;
        private LoggingUtilities loggingUtilities;

        //******************************************************************************
        //
        // Method: RemoteSenderCompressor (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.RemoteSenderCompressor class.
        /// </summary>
        /// <param name="underlyingRemoteSender">The remote sender to send the message to after compressing.</param>
        public RemoteSenderCompressor(IRemoteSender underlyingRemoteSender)
        {
            remoteSender = underlyingRemoteSender;
            logger = new ConsoleApplicationLogger(LogLevel.Information, '|', "  ");
            loggingUtilities = new LoggingUtilities(logger);
        }

        //******************************************************************************
        //
        // Method: RemoteSenderCompressor (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.RemoteSenderCompressor class.
        /// </summary>
        /// <param name="underlyingRemoteSender">The remote sender to send the message to after compressing.</param>
        /// <param name="logger">The logger to write log events to.</param>
        public RemoteSenderCompressor(IRemoteSender underlyingRemoteSender, IApplicationLogger logger)
            : this(underlyingRemoteSender)
        {
            this.logger = logger;
            loggingUtilities = new LoggingUtilities(logger);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IRemoteSender.Send(System.String)"]/*'/>
        public void Send(string message)
        {
            remoteSender.Send(CompressString(message));
        }

        //******************************************************************************
        //
        // Method: CompressString
        //
        //******************************************************************************
        /// <summary>
        /// Compresses a string.
        /// </summary>
        /// <param name="inputString">The string to compress.</param>
        /// <returns>The compressed string.</returns>
        private string CompressString(string inputString)
        {
            // Note that no try/catch block is included in this method.  According to the .NET documentation, most methods here will only cause exceptions in cases like null parameters being passed (e.g. in the case if UTF8Encoding.GetBytes() and Convert.ToBase64String()), which would be very unlikely to occur if these classes are used in the intended context.

            byte[] inputStringBytes = stringEncoding.GetBytes(inputString);
            byte[] compressedByteArray;

            // Use the GZipStream class to compress the bytes of the string
            using (MemoryStream compressedStringStream = new MemoryStream())
            using (GZipStream compressor = new GZipStream(compressedStringStream, CompressionMode.Compress))
            {
                compressor.Write(inputStringBytes, 0, inputStringBytes.Length);
                compressor.Close();
                compressedByteArray = compressedStringStream.ToArray();
                compressedStringStream.Close();
            }

            // Convert the compressed bytes to a base 64 string
            string returnString = Convert.ToBase64String(compressedByteArray);

            loggingUtilities.LogCompressedString(this, returnString);

            return returnString;
        }
    }
}

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
using ApplicationMetrics;
using MethodInvocationRemotingMetrics;

namespace MethodInvocationRemoting
{
    //******************************************************************************
    //
    // Class: RemoteReceiverDecompressor
    //
    //******************************************************************************
    /// <summary>
    /// Decompresses a message, after receiving it from a remote location via an underlying IRemoteReceiver implementation.
    /// </summary>
    public class RemoteReceiverDecompressor : IRemoteReceiver
    {
        private IRemoteReceiver remoteReceiver;
        private int decompressionBufferSize;
        private Encoding stringEncoding = Encoding.UTF8;
        private volatile bool decompressing = false;
        private IApplicationLogger logger;
        private LoggingUtilities loggingUtilities;
        private MetricsUtilities metricsUtilities;

        //------------------------------------------------------------------------------
        //
        // Method: RemoteReceiverDecompressor (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.RemoteReceiverDecompressor class.
        /// </summary>
        /// <param name="underlyingRemoteReceiver">The remote receiver to receive the message from before decompressing.</param>
        public RemoteReceiverDecompressor(IRemoteReceiver underlyingRemoteReceiver)
        {
            this.remoteReceiver = underlyingRemoteReceiver;
            this.decompressionBufferSize = 1024;
            logger = new ConsoleApplicationLogger(LogLevel.Information, '|', "  ");
            loggingUtilities = new LoggingUtilities(logger);
            metricsUtilities = new MetricsUtilities(new NullMetricLogger());
        }

        //------------------------------------------------------------------------------
        //
        // Method: RemoteReceiverDecompressor (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.RemoteReceiverDecompressor class.
        /// </summary>
        /// <param name="underlyingRemoteReceiver">The remote receiver to receive the message from before decompressing.</param>
        /// <param name="logger">The logger to write log events to.</param>
        public RemoteReceiverDecompressor(IRemoteReceiver underlyingRemoteReceiver, IApplicationLogger logger)
            : this(underlyingRemoteReceiver)
        {
            this.logger = logger;
            loggingUtilities = new LoggingUtilities(logger);
        }

        //------------------------------------------------------------------------------
        //
        // Method: RemoteReceiverDecompressor (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.RemoteReceiverDecompressor class.
        /// </summary>
        /// <param name="underlyingRemoteReceiver">The remote receiver to receive the message from before decompressing.</param>
        /// <param name="metricLogger">The metric logger to write metric and instrumentation events to.</param>
        public RemoteReceiverDecompressor(IRemoteReceiver underlyingRemoteReceiver, IMetricLogger metricLogger)
            : this(underlyingRemoteReceiver)
        {
            metricsUtilities = new MetricsUtilities(metricLogger);
        }

        //------------------------------------------------------------------------------
        //
        // Method: RemoteReceiverDecompressor (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.RemoteReceiverDecompressor class.
        /// </summary>
        /// <param name="underlyingRemoteReceiver">The remote receiver to receive the message from before decompressing.</param>
        /// <param name="logger">The logger to write log events to.</param>
        /// <param name="metricLogger">The metric logger to write metric and instrumentation events to.</param>
        public RemoteReceiverDecompressor(IRemoteReceiver underlyingRemoteReceiver, IApplicationLogger logger, IMetricLogger metricLogger)
            : this(underlyingRemoteReceiver)
        {
            this.logger = logger;
            loggingUtilities = new LoggingUtilities(logger);
            metricsUtilities = new MetricsUtilities(metricLogger);
        }

        //------------------------------------------------------------------------------
        //
        // Method: RemoteReceiverDecompressor (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.RemoteReceiverDecompressor class.
        /// </summary>
        /// <param name="underlyingRemoteReceiver">The remote receiver to receive the message from before compressing.</param>
        /// <param name="decompressionBufferSize">The size of the buffer to use when decompressing the message in bytes.  Denotes how much data will be read from the internal stream decompressor class in each read operation.  Should be set to match the expected decompressed message size as closely as possible.</param>
        public RemoteReceiverDecompressor(IRemoteReceiver underlyingRemoteReceiver, int decompressionBufferSize)
            : this(underlyingRemoteReceiver)
        {
            if (decompressionBufferSize < 1)
            {
                throw new ArgumentOutOfRangeException("decompressionBufferSize", "Argument 'decompressionBufferSize' must be greater than 0.");
            }
            this.decompressionBufferSize = decompressionBufferSize;
        }

        //------------------------------------------------------------------------------
        //
        // Method: RemoteReceiverDecompressor (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.RemoteReceiverDecompressor class.
        /// </summary>
        /// <param name="underlyingRemoteReceiver">The remote receiver to receive the message from before compressing.</param>
        /// <param name="decompressionBufferSize">The size of the buffer to use when decompressing the message in bytes.  Denotes how much data will be read from the internal stream decompressor class in each read operation.  Should be set to match the expected decompressed message size as closely as possible.</param>
        /// <param name="logger">The logger to write log events to.</param>
        public RemoteReceiverDecompressor(IRemoteReceiver underlyingRemoteReceiver, int decompressionBufferSize, IApplicationLogger logger)
            : this(underlyingRemoteReceiver, decompressionBufferSize)
        {
            this.logger = logger;
            loggingUtilities = new LoggingUtilities(logger);
        }

        //------------------------------------------------------------------------------
        //
        // Method: RemoteReceiverDecompressor (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.RemoteReceiverDecompressor class.
        /// </summary>
        /// <param name="underlyingRemoteReceiver">The remote receiver to receive the message from before compressing.</param>
        /// <param name="decompressionBufferSize">The size of the buffer to use when decompressing the message in bytes.  Denotes how much data will be read from the internal stream decompressor class in each read operation.  Should be set to match the expected decompressed message size as closely as possible.</param>
        /// <param name="metricLogger">The metric logger to write metric and instrumentation events to.</param>
        public RemoteReceiverDecompressor(IRemoteReceiver underlyingRemoteReceiver, int decompressionBufferSize, IMetricLogger metricLogger)
            : this(underlyingRemoteReceiver, decompressionBufferSize)
        {
            metricsUtilities = new MetricsUtilities(metricLogger);
        }

        //------------------------------------------------------------------------------
        //
        // Method: RemoteReceiverDecompressor (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemoting.RemoteReceiverDecompressor class.
        /// </summary>
        /// <param name="underlyingRemoteReceiver">The remote receiver to receive the message from before compressing.</param>
        /// <param name="decompressionBufferSize">The size of the buffer to use when decompressing the message in bytes.  Denotes how much data will be read from the internal stream decompressor class in each read operation.  Should be set to match the expected decompressed message size as closely as possible.</param>
        /// <param name="logger">The logger to write log events to.</param>
        /// <param name="metricLogger">The metric logger to write metric and instrumentation events to.</param>
        public RemoteReceiverDecompressor(IRemoteReceiver underlyingRemoteReceiver, int decompressionBufferSize, IApplicationLogger logger, IMetricLogger metricLogger)
            : this(underlyingRemoteReceiver, decompressionBufferSize)
        {
            this.logger = logger;
            loggingUtilities = new LoggingUtilities(logger);
            metricsUtilities = new MetricsUtilities(metricLogger);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IRemoteReceiver.Receive"]/*'/>
        public string Receive()
        {
            return DecompressString(remoteReceiver.Receive());
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:MethodInvocationRemoting.IRemoteReceiver.CancelReceive"]/*'/>
        public void CancelReceive()
        {
            remoteReceiver.CancelReceive();
            while (decompressing == true) ;

            loggingUtilities.Log(this, LogLevel.Information, "Receive operation cancelled.");
        }

        //------------------------------------------------------------------------------
        //
        // Method: DecompressString
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Decompresses a string.
        /// </summary>
        /// <param name="inputString">The string to decompress.</param>
        /// <returns>The decompressed string.</returns>
        private string DecompressString(string inputString)
        {
            metricsUtilities.Begin(new StringDecompressTime());

            decompressing = true;

            List<byte[]> readBuffers = new List<byte[]>();
            int currentReadBufferPosition = 0;
            string returnString = "";

            try
            {
                // Decode from base 64
                byte[] encodedBytes = Convert.FromBase64String(inputString);
                using (MemoryStream decompressedStringStream = new MemoryStream(encodedBytes))
                using (GZipStream decompressor = new GZipStream(decompressedStringStream, CompressionMode.Decompress))
                {
                    int bytesRead = -1;
                    while (bytesRead != 0)
                    {
                        // If the list of buffers is empty, or the read position in the current (last) buffer is at the end of the buffer, then create a new read buffer
                        if ((readBuffers.Count == 0) || (currentReadBufferPosition == decompressionBufferSize))
                        {
                            readBuffers.Add(new byte[decompressionBufferSize]);
                            currentReadBufferPosition = 0;

                            metricsUtilities.Increment(new RemoteReceiverDecompressorReadBufferCreated());
                        }
                        bytesRead = decompressor.Read(readBuffers[readBuffers.Count - 1], currentReadBufferPosition, decompressionBufferSize - currentReadBufferPosition);
                        currentReadBufferPosition = currentReadBufferPosition + bytesRead;
                    }
                    decompressor.Close();
                    decompressedStringStream.Close();
                }

                // Create decompressed byte array with size as buffer size times the number of buffers (except the last buffer), plus the position within the last buffer
                byte[] decompressedBytes = new byte[((readBuffers.Count - 1) * decompressionBufferSize) + currentReadBufferPosition];
                // Copy the contents of the read buffers into the decompressed byte array
                int decompressedBytesPosition = 0;
                foreach (byte[] currentReadBuffer in readBuffers)
                {
                    if (currentReadBuffer != readBuffers[readBuffers.Count - 1])
                    {
                        Array.Copy(currentReadBuffer, 0, decompressedBytes, decompressedBytesPosition, decompressionBufferSize);
                        decompressedBytesPosition = decompressedBytesPosition + decompressionBufferSize;
                    }
                    else
                    {
                        Array.Copy(currentReadBuffer, 0, decompressedBytes, decompressedBytesPosition, currentReadBufferPosition);
                    }
                }

                returnString = stringEncoding.GetString(decompressedBytes);
            }
            catch (Exception e)
            {
                throw new Exception("Error decompressing message.", e);
            }

            metricsUtilities.End(new StringDecompressTime());
            metricsUtilities.Increment(new StringDecompressed());
            loggingUtilities.LogDecompressedString(this, returnString);

            decompressing = false;
            return returnString;
        }
    }
}

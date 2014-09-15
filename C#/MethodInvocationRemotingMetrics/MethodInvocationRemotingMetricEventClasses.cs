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
using ApplicationMetrics;

namespace MethodInvocationRemotingMetrics
{
    /// <summary>
    /// Metric representing the number of remote method calls sent.
    /// </summary>
    public class RemoteMethodSent : CountMetric
    {
        public RemoteMethodSent()
        {
            base.name = "RemoteMethodSent";
            base.description = "The number of remote method calls sent";
        }
    }

    /// <summary>
    /// Metric representing the time taken to send a remote method call.
    /// </summary>
    public class RemoteMethodSendTime : IntervalMetric
    {
        public RemoteMethodSendTime()
        {
            base.name = "RemoteMethodSendTime";
            base.description = "The time taken to send and execute a remote method call";
        }
    }

    /// <summary>
    /// Metric representing the number of remote method calls received.
    /// </summary>
    public class RemoteMethodReceived : CountMetric
    {
        public RemoteMethodReceived()
        {
            base.name = "RemoteMethodReceived";
            base.description = "The number of remote method calls received";
        }
    }

    /// <summary>
    /// Metric representing the time taken to receive a remote method call.
    /// </summary>
    public class RemoteMethodReceiveTime : IntervalMetric
    {
        public RemoteMethodReceiveTime()
        {
            base.name = "RemoteMethodReceiveTime";
            base.description = "The time taken to receive and execute a remote method call";
        }
    }

    /// <summary>
    /// Metric representing the number of times a string was compressed.
    /// </summary>
    public class StringCompressed : CountMetric
    {
        public StringCompressed()
        {
            base.name = "StringCompressed";
            base.description = "The number of strings compressed";
        }
    }

    /// <summary>
    /// Metric representing the time taken to compress a string.
    /// </summary>
    public class StringCompressTime : IntervalMetric
    {
        public StringCompressTime()
        {
            base.name = "StringCompressTime";
            base.description = "The time taken to compress a string";
        }
    }

    /// <summary>
    /// Metric representing the size of a string after compressing it.
    /// </summary>
    public class CompressedStringSize : AmountMetric
    {
        public CompressedStringSize(long stringSize)
        {
            base.name = "CompressedStringSize";
            base.description = "The size of a string after compressing";
            base.amount = stringSize;
        }
    }

    /// <summary>
    /// Metric representing the number of times a string was decompressed.
    /// </summary>
    public class StringDecompressed : CountMetric
    {
        public StringDecompressed()
        {
            base.name = "StringDecompressed";
            base.description = "The number of strings decompressed";
        }
    }

    /// <summary>
    /// Metric representing the time taken to decompress a string.
    /// </summary>
    public class StringDecompressTime : IntervalMetric
    {
        public StringDecompressTime()
        {
            base.name = "StringDecompressTime";
            base.description = "The time taken to decompress a string";
        }
    }

    /// <summary>
    /// Metric representing the number of times a read buffer was created in RemoteReceiverDecompressor objects.
    /// </summary>
    /// <remarks>For maximum efficiency one read buffer should be created per decompressed string.  If multiple buffers are created each time a string is decompressed, it indicates that the string size is larger than the buffer size.</remarks>
    public class RemoteReceiverDecompressorReadBufferCreated : CountMetric
    {
        public RemoteReceiverDecompressorReadBufferCreated()
        {
            base.name = "RemoteReceiverDecompressorReadBufferCreated";
            base.description = "The number of read buffers created in RemoteReceiverDecompressor objects";
        }
    }

    /// <summary>
    /// Metric representing the number of messages sent.
    /// </summary>
    public class MessageSent : CountMetric
    {
        public MessageSent()
        {
            base.name = "MessageSent";
            base.description = "The number of messages sent";
        }
    }

    /// <summary>
    /// Metric representing the time taken to send a message.
    /// </summary>
    public class MessageSendTime : IntervalMetric
    {
        public MessageSendTime()
        {
            base.name = "MessageSendTime";
            base.description = "The time taken to send a message";
        }
    }

    /// <summary>
    /// Metric representing the number of messages received.
    /// </summary>
    public class MessageReceived : CountMetric
    {
        public MessageReceived()
        {
            base.name = "MessageReceived";
            base.description = "The number of messages received";
        }
    }

    /// <summary>
    /// Metric representing the time taken to receive a message.
    /// </summary>
    public class MessageReceiveTime : IntervalMetric
    {
        public MessageReceiveTime()
        {
            base.name = "MessageReceiveTime";
            base.description = "The time taken to receive a message";
        }
    }

    /// <summary>
    /// Metric representing the size of a received message.
    /// </summary>
    public class ReceivedMessageSize : AmountMetric
    {
        public ReceivedMessageSize(long messageSize)
        {
            base.name = "ReceivedMessageSize";
            base.description = "The size of a received message";
            base.amount = messageSize;
        }
    }

    /// <summary>
    /// Metric representing the number of times a TcpRemoteSender object reconnected.
    /// </summary>
    public class TcpRemoteSenderReconnected : CountMetric
    {
        public TcpRemoteSenderReconnected()
        {
            base.name = "TcpRemoteSenderReconnected";
            base.description = "The number of times a TcpRemoteSender object reconnected";
        }
    }

    /// <summary>
    /// Metric representing the number of times a TcpRemoteReceiver object reconnected.
    /// </summary>
    public class TcpRemoteReceiverReconnected : CountMetric
    {
        public TcpRemoteReceiverReconnected()
        {
            base.name = "TcpRemoteReceiverReconnected";
            base.description = "The number of times a TcpRemoteReceiver object reconnected";
        }
    }

    /// <summary>
    /// Metric representing the number of times a TcpRemoteReceiver object received a message with a duplicate sequence number.
    /// </summary>
    public class TcpRemoteReceiverDuplicateSequenceNumber : CountMetric
    {
        public TcpRemoteReceiverDuplicateSequenceNumber()
        {
            base.name = "TcpRemoteReceiverDuplicateSequenceNumber";
            base.description = "The number of times a TcpRemoteReceiver object received a message with a duplicate sequence number";
        }
    }

    /// <summary>
    /// Metric representing the number of times a method invocation was serialized.
    /// </summary>
    public class MethodInvocationSerialized : CountMetric
    {
        public MethodInvocationSerialized()
        {
            base.name = "MethodInvocationSerialized";
            base.description = "The number of method invocations serialized";
        }
    }

    /// <summary>
    /// Metric representing the number of times a method invocation was deserialized.
    /// </summary>
    public class MethodInvocationDeserialized : CountMetric
    {
        public MethodInvocationDeserialized()
        {
            base.name = "MethodInvocationDeserialized";
            base.description = "The number of method invocations deserialized";
        }
    }

    /// <summary>
    /// Metric representing the time taken to serialize a method invocation.
    /// </summary>
    public class MethodInvocationSerializeTime : IntervalMetric
    {
        public MethodInvocationSerializeTime()
        {
            base.name = "MethodInvocationSerializeTime";
            base.description = "The time taken to serialize a method invocation";
        }
    }

    /// <summary>
    /// Metric representing the time taken to deserialize a method invocation.
    /// </summary>
    public class MethodInvocationDeserializeTime : IntervalMetric
    {
        public MethodInvocationDeserializeTime()
        {
            base.name = "MethodInvocationDeserializeTime";
            base.description = "The time taken to deserialize a method invocation";
        }
    }

    /// <summary>
    /// Metric representing the size of a serialized method invocation.
    /// </summary>
    public class SerializedMethodInvocationSize : AmountMetric
    {
        public SerializedMethodInvocationSize(long stringSize)
        {
            base.name = "SerializedMethodInvocationSize";
            base.description = "The size of a serialized method invocation";
            base.amount = stringSize;
        }
    }

    /// <summary>
    /// Metric representing the number of times a return value was serialized.
    /// </summary>
    public class ReturnValueSerialized : CountMetric
    {
        public ReturnValueSerialized()
        {
            base.name = "ReturnValueSerialized";
            base.description = "The number of return values serialized";
        }
    }

    /// <summary>
    /// Metric representing the number of times a return value was deserialized.
    /// </summary>
    public class ReturnValueDeserialized : CountMetric
    {
        public ReturnValueDeserialized()
        {
            base.name = "ReturnValueDeserialized";
            base.description = "The number of return values deserialized";
        }
    }

    /// <summary>
    /// Metric representing the time taken to serialize a return value.
    /// </summary>
    public class ReturnValueSerializeTime : IntervalMetric
    {
        public ReturnValueSerializeTime()
        {
            base.name = "ReturnValueSerializeTime";
            base.description = "The time taken to serialize a return value";
        }
    }

    /// <summary>
    /// Metric representing the time taken to deserialize a return value.
    /// </summary>
    public class ReturnValueDeserializeTime : IntervalMetric
    {
        public ReturnValueDeserializeTime()
        {
            base.name = "ReturnValueDeserializeTime";
            base.description = "The time taken to deserialize a return value";
        }
    }

    /// <summary>
    /// Metric representing the size of a serialized return value.
    /// </summary>
    public class SerializedReturnValueSize : AmountMetric
    {
        public SerializedReturnValueSize(long stringSize)
        {
            base.name = "SerializedReturnValueSize";
            base.description = "The size of a serialized return value";
            base.amount = stringSize;
        }
    }
}
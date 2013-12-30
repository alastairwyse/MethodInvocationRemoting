/*
 * Copyright 2013 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
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

namespace OperatingSystemAbstraction
{
    //******************************************************************************
    //
    // Class: NetworkStream
    //
    //******************************************************************************
    /// <summary>
    /// Provides an abstraction of the System.Net.Sockets.NetworkStream class, to facilitate mocking and unit testing.
    /// </summary>
    /// <remarks>Although instances of this class compose a System.Net.Sockets.NetworkStream object which implements IDisposable, this class does not implement IDisposable due to the fact that its use-case is to return a mockable NetworkStream from a call to TcpClient.GetStream().  The underlying NetworkStream is created by the TcpClient class, and hence the TcpClient should also dispose/finalize it.</remarks>
    public class NetworkStream : INetworkStream
    {
        private System.Net.Sockets.NetworkStream networkStream;

        //******************************************************************************
        //
        // Method: NetworkStream (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the OperatingSystemAbstraction.NetworkStream class.
        /// </summary>
        /// <param name="underlyingStream">The System.Net.Sockets.NetworkStream underlying the instance of the class.</param>
        public NetworkStream(System.Net.Sockets.NetworkStream underlyingStream)
        {
            networkStream = underlyingStream;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="P:OperatingSystemAbstraction.INetworkStream.CanRead"]/*'/>
        public bool CanRead
        {
            get
            {
                return networkStream.CanRead;
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:OperatingSystemAbstraction.INetworkStream.Read(System.Byte[]@,System.Int32,System.Int32)"]/*'/>
        public int Read(ref byte[] buffer, int offset, int size)
        {
            return networkStream.Read(buffer, offset, size);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:OperatingSystemAbstraction.INetworkStream.ReadByte"]/*'/>
        public int ReadByte()
        {
            return networkStream.ReadByte();
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:OperatingSystemAbstraction.INetworkStream.Write(System.Byte[],System.Int32,System.Int32)"]/*'/>
        public void Write(byte[] buffer, int offset, int size)
        {
            networkStream.Write(buffer, offset, size);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:OperatingSystemAbstraction.INetworkStream.WriteByte(System.Byte)"]/*'/>
        public void WriteByte(byte value)
        {
            networkStream.WriteByte(value);
        }
    }
}

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
    // Class: TcpClient
    //
    //******************************************************************************
    /// <summary>
    /// Provides an abstraction of the System.Net.Sockets.TcpClient class, to facilitate mocking and unit testing.
    /// </summary>
    /// <remarks>As per http://msdn.microsoft.com/en-us/library/ych8bz3x.aspx, a System.Net.Sockets.Socket object cannot be used to perform a synchronous reconnect after having previously been connected.  Hence the Close() method disposes the underlying TcpClient class, and the Connect() method subsequently creates a new instance.</remarks>
    public class TcpClient : ITcpClient
    {
        protected bool disposed;
        private System.Net.Sockets.TcpClient tcpClient;
        // Following boolean is used to denote whether the overloaded constructor is used which injects the underlying System.Net.Sockets.TcpClient
        private bool injectedClient = false;

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="P:OperatingSystemAbstraction.ITcpClient.Connected"]/*'/>
        public bool Connected
        {
            get
            {
                if (tcpClient == null)
                {
                    return false;
                }
                else
                {
                    return tcpClient.Connected;
                }
            }
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="P:OperatingSystemAbstraction.ITcpClient.Available"]/*'/>
        public int Available
        {
            get
            {
                return tcpClient.Available;
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: TcpClient (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the OperatingSystemAbstraction.TcpClient class.
        /// </summary>
        public TcpClient()
        {
            disposed = false;
        }

        //------------------------------------------------------------------------------
        //
        // Method: TcpClient (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the OperatingSystemAbstraction.TcpClient class.
        /// </summary>
        /// <param name="underlyingClient">The System.Net.Sockets.TcpClient underlying the instance of the class.</param>
        /// <remarks>Typically a System.Net.Sockets.TcpClient object would be instantiated either by calling the constructor, or being returned from a call to the System.Net.Sockets.TcpListener.AcceptTcpClient() method.  To allow for mocking, this additional constructor is provided.  In the real world case where a TcpClient is returned from the TcpListener.AcceptTcpClient() method, the OperatingSystemAbstraction.TcpListener can call the underlying real TcpListener, and use this constructor to wrap and return the socket channel as an OperatingSystemAbstraction.ITcpClient.  In the mocked case, a call to OperatingSystemAbstraction.TcpListener.AcceptTcpClient() returns a mocked ITcpClient.  The parameterless constructor would be used to create an outbound TCP connection via the Connect() method.  This constructor would be used to accept an inbound TCP connection from a TcpListener object (in which case there is no need to call the Connect() method).</remarks>
        public TcpClient(System.Net.Sockets.TcpClient underlyingClient)
        {
            this.tcpClient = underlyingClient;
            injectedClient = true;
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:OperatingSystemAbstraction.ITcpClient.Connect(System.Net.IPAddress,System.Int32)"]/*'/>
        public void Connect(System.Net.IPAddress ipAddress, int port)
        {
            // Throw an exception if this method is called after instantiating the class with an injected underlying TcpClient, as this would either override the injected mock object, or lose the reference to the TcpClient which this class wraps
            //   See full explanation in constructor comments above.
            if (injectedClient == true)
            {
                throw new InvalidOperationException("The 'Connect' method cannot be called after instantiating the class using the constructor which injects the underlying System.Net.Sockets.TcpClient object.");
            }
            tcpClient = new System.Net.Sockets.TcpClient();
            tcpClient.Connect(ipAddress, port);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:OperatingSystemAbstraction.ITcpClient.GetStream"]/*'/>
        public INetworkStream GetStream()
        {
            // Returning System.Net.Sockets.NetworkStream wrapped in an OperatingSystemAbstraction.NetworkStream (which implements INetworkStream), allows this method to be mocked
            System.Net.Sockets.NetworkStream networkStream = tcpClient.GetStream();
            return new NetworkStream(networkStream);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:OperatingSystemAbstraction.ITcpClient.Close"]/*'/>
        public void Close()
        {
            if (tcpClient.Connected == true)
            {
                tcpClient.Client.Shutdown(System.Net.Sockets.SocketShutdown.Send);
                tcpClient.Client.Disconnect(false);
            }
            tcpClient.Close();
            tcpClient = null;
        }

        #region Finalize / Dispose Methods

        /// <summary>
        /// Releases the unmanaged resources used by the TcpClient.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #pragma warning disable 1591
        ~TcpClient()
        {
            Dispose(false);
        }
        #pragma warning restore 1591

        //------------------------------------------------------------------------------
        //
        // Method: Dispose
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Provides a method to free unmanaged resources used by this class.
        /// </summary>
        /// <param name="disposing">Whether the method is being called as part of an explicit Dispose routine, and hence whether managed resources should also be freed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                }
                // Free your own state (unmanaged objects).
                // Note that if the instance of the class was created with an injected TcpClient, we do not close and free resources here.  It is up to the class that created the injected TcpClient to dispose of it.
                if ((injectedClient == false) && (tcpClient != null))
                {
                    if (tcpClient.Connected == true)
                    {
                        tcpClient.Client.Shutdown(System.Net.Sockets.SocketShutdown.Send);
                        tcpClient.Client.Disconnect(false);
                    }
                    tcpClient.Close();
                }
                // Set large fields to null.
                tcpClient = null;

                disposed = true;
            }
        }

        #endregion
    }
}

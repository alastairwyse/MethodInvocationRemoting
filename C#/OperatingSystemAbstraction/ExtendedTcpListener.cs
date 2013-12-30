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
    // Class: ExtendedTcpListener
    //
    //******************************************************************************
    /// <summary>
    /// An extended version of the System.Net.Sockets.TcpListener class, which exposes protected methods on that class.
    /// </summary>
    public class ExtendedTcpListener : System.Net.Sockets.TcpListener
    {
        //******************************************************************************
        //
        // Method: ExtendedTcpListener (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the OperatingSystemAbstraction.ExtendedTcpListener class.
        /// </summary>
        /// <param name="ipAddress">An IPAddress that represents the local IP address.</param>
        /// <param name="port">The port on which to listen for incoming connection attempts. </param>
        public ExtendedTcpListener(System.Net.IPAddress ipAddress, int port)
            : base(ipAddress, port)
        {
        }

        /// <summary>
        /// Gets a value that indicates whether TcpListener is actively listening for client connections.
        /// </summary>
        public bool Active
        {
            get
            {
                return base.Active;
            }
        }
    }
}

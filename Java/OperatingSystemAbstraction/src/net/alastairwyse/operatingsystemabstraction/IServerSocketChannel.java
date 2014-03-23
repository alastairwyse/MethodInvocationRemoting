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

package net.alastairwyse.operatingsystemabstraction;

import java.io.*;
import java.net.*;
import java.nio.channels.*;

/**
 * Provides an abstraction of the java.nio.channels.ServerSocketChannel class, to facilitate mocking and unit testing.
 * @author Alastair Wyse
 */
public interface IServerSocketChannel {

    /**
     * Tells whether or not this channel is open.
     * @return  true if, and only if, this channel is open.
     */
    boolean isOpen();
    
    /**
     * Binds the channel's socket to a local address and configures the socket to listen for connections. 
     * @param local                             The address to bind the socket, or null to bind to an automatically assigned socket address.
     * @param backlog                           The maximum number of pending connections.
     * @throws AlreadyBoundException            if the socket is already bound.
     * @throws UnsupportedAddressTypeException  if the type of the given address is not supported.
     * @throws ClosedChannelException           if this channel is closed.
     * @throws IOException                      if some other I/O error occurs.
     * @throws SecurityException                if a security manager has been installed and its checkListen method denies the operation.
     */
    void bind (SocketAddress local, int backlog) throws AlreadyBoundException, UnsupportedAddressTypeException, ClosedChannelException, IOException, SecurityException;
    
    /**
     * Opens a server-socket channel. 
     * @throws IOException  if an I/O error occurs.
     */
    void open() throws IOException;

    /**
     * Closes this channel.
     * @throws IOException  if an I/O error occurs.
     */
    void close() throws IOException;
    
    /**
     * Accepts a connection made to this channel's socket. 
     * @return  The socket channel for the new connection, or null if this channel is in non-blocking mode and no connection is available to be accepted.
     * @throws ClosedChannelException      if this channel is closed.
     * @throws AsynchronousCloseException  if another thread closes this channel while the accept operation is in progress.
     * @throws ClosedByInterruptException  if another thread interrupts the current thread while the accept operation is in progress, thereby closing the channel and setting the current thread's interrupt status.
     * @throws NotYetBoundException        if this channel's socket has not yet been bound.
     * @throws SecurityException           if a security manager has been installed and it does not permit access to the remote endpoint of the new connection.
     * @throws IOException                 if some other I/O error occurs.
     */
    ISocketChannel accept() throws ClosedChannelException, AsynchronousCloseException, ClosedByInterruptException, NotYetBoundException, SecurityException, IOException;
    
    /**
     * Adjusts this channel's blocking mode. 
     * @param block                    If true then this channel will be placed in blocking mode; if false then it will be placed non-blocking mode.
     * @return                         This selectable channel.
     * @throws ClosedChannelException  if this channel is closed.
     * @throws IOException             if an I/O error occurs.
     */
    SelectableChannel configureBlocking(boolean block) throws ClosedChannelException, IOException;
}

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
import java.nio.*;
import java.nio.channels.*;

/**
 * Provides an abstraction of the java.nio.channels.SocketChannel class, to facilitate mocking and unit testing.
 * @author Alastair Wyse
 */
public interface ISocketChannel {

    /**
     * Tells whether or not this channel's network socket is connected.
     * @return  true if, and only if, this channel's network socket is open and connected
     */
    public boolean isConnected();
    
    /**
     * Opens a socket channel.  Note this differs from the java.nio.channels.SocketChannel.open() method, as it assigns a new socket channel inside the class, rather than returning it.
     * @throws IOException  if an I/O error occurs.
     */
    public void open() throws IOException;
    
    /**
     * Closes this channel.
     * @throws IOException  if an I/O error occurs.
     */
    public void close() throws IOException;
    
    /**
     * Connects this channel's socket. 
     * @param remote                            The remote address to which this channel is to be connected.
     * @return                                  true if a connection was established, false if this channel is in non-blocking mode and the connection operation is in progress.
     * @throws AlreadyConnectedException        if this channel is already connected.
     * @throws ConnectionPendingException       if a non-blocking connection operation is already in progress on this channel.
     * @throws ClosedChannelException           if this channel is closed.
     * @throws AsynchronousCloseException       if another thread closes this channel while the connect operation is in progress.
     * @throws ClosedByInterruptException       if another thread interrupts the current thread while the connect operation is in progress, thereby closing the channel and setting the current thread's interrupt status.
     * @throws UnresolvedAddressException       if the given remote address is not fully resolved.
     * @throws UnsupportedAddressTypeException  if the type of the given remote address is not supported.
     * @throws SecurityException                if a security manager has been installed and it does not permit access to the given remote endpoint.
     * @throws IOException                      if some other I/O error occurs.
     */
    public boolean connect(SocketAddress remote) throws AlreadyConnectedException, ConnectionPendingException, ClosedChannelException, AsynchronousCloseException, ClosedByInterruptException, UnresolvedAddressException, UnsupportedAddressTypeException, SecurityException, IOException;
    
    /**
     * Writes a sequence of bytes to this channel from the given buffer. 
     * @param src                          The buffer from which bytes are to be retrieved.
     * @return                             The number of bytes written, possibly zero.
     * @throws NotYetConnectedException    if this channel is not yet connected.
     * @throws ClosedChannelException      if this channel is closed.
     * @throws AsynchronousCloseException  if another thread closes this channel while the write operation is in progress.
     * @throws ClosedByInterruptException  if another thread interrupts the current thread while the write operation is in progress, thereby closing the channel and setting the current thread's interrupt status.
     * @throws IOException                 if some other I/O error occurs.
     */
    public int write(ByteBuffer src) throws NotYetConnectedException, ClosedChannelException, AsynchronousCloseException, ClosedByInterruptException, IOException;
    
    /**
     * Reads a sequence of bytes from this channel into the given buffer. 
     * @param dst                          The buffer into which bytes are to be transferred.
     * @return                             The number of bytes read, possibly zero, or -1 if the channel has reached end-of-stream.
     * @throws NotYetConnectedException    if this channel is not yet connected.
     * @throws ClosedChannelException      if this channel is closed.
     * @throws AsynchronousCloseException  if another thread closes this channel while the read operation is in progress.
     * @throws ClosedByInterruptException  if another thread interrupts the current thread while the read operation is in progress, thereby closing the channel and setting the current thread's interrupt status.
     * @throws IOException                 if some other I/O error occurs.
     */
    public int read(ByteBuffer dst) throws NotYetConnectedException, ClosedChannelException, AsynchronousCloseException, ClosedByInterruptException, IOException;
    
    /**
     * Adjusts this channel's blocking mode. 
     * @param block                    If true then this channel will be placed in blocking mode; if false then it will be placed non-blocking mode.
     * @return                         This selectable channel.
     * @throws ClosedChannelException  if this channel is closed.
     * @throws IOException             if an I/O error occurs.
     */
    public SelectableChannel configureBlocking(boolean block) throws ClosedChannelException, IOException;
}

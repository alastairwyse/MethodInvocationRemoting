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
public class SocketChannel implements ISocketChannel, AutoCloseable {
    private java.nio.channels.SocketChannel socketChannel;
    // Following boolean is used to denote whether the overloaded constructor is used which injects the underlying java.nio.channels.SocketChannel
    private boolean injectedClient = false;
    
    /**
     * Initialises a new instance of the SocketChannel class.
     */
    public SocketChannel() {
    }

    /**
     * Initialises a new instance of the SocketChannel class.
     * <b>Note</b> Typically a java.nio.channels.SocketChannel object would be instantiated either by calling the constructor, or being returned from a call to the java.nio.channels.ServerSocketChannel.accept() method.
     * To allow for mocking, this additional constructor is provided.  In the real world case where a SocketChannel is returned from the serverSocketChannel.accept() method, the operatingsystemabstraction.ServerSocketChannel can call the underlying real ServerSocketChannel, and use this constructor to wrap and return the socket channel as an operatingsystemabstraction.ISocketChannel.
     * In the mocked case, a call to operatingsystemabstraction.IServerSocketChannel.accept() returns a mocked ISocketChannel.
     * The parameterless constructor would be used to create an outbound TCP connection via the connect() method.
     * This constructor would be used to accept an inbound TCP connection from a ServerSocketChannel object (in which case there is no need to call the connect() method).
     * @param underlyingSocketChannel  The java.nio.channels.SocketChannel underlying the instance of the class.
     */
    public SocketChannel(java.nio.channels.SocketChannel underlyingSocketChannel)
    {
        this.socketChannel = underlyingSocketChannel;
        injectedClient = true;
    }
    
    @Override
    public boolean isConnected() {
        if(socketChannel == null) {
            return false;
        }
        else {
            return socketChannel.isConnected();
        }
    }
    
    @Override
    public void open() throws IOException {
        if(socketChannel != null) {
            socketChannel.close();
        }
        socketChannel = java.nio.channels.SocketChannel.open();
    }
    
    @Override
    public void close() throws IOException {
        socketChannel.close();
    }
    
    @Override
    public boolean connect(SocketAddress remote) throws AlreadyConnectedException, ConnectionPendingException, ClosedChannelException, AsynchronousCloseException, ClosedByInterruptException, UnresolvedAddressException, UnsupportedAddressTypeException, SecurityException, IOException {
        // Throw an exception if this method is called after instantiating the class with an injected underlying SocketChannel, as this would either override the injected mock object, or lose the reference to the SocketChannel which this class wraps
        //   See full explanation in constructor comments above.
        if (injectedClient == true)
        {
            throw new IllegalStateException("The 'Connect' method cannot be called after instantiating the class using the constructor which injects the underlying java.nio.channels.SocketChannel object.");
        }
        return socketChannel.connect(remote);
    }
    
    @Override
    public int write(ByteBuffer src) throws NotYetConnectedException, ClosedChannelException, AsynchronousCloseException, ClosedByInterruptException, IOException {
        return socketChannel.write(src);
    }
    
    @Override
    public int read(ByteBuffer dst) throws NotYetConnectedException, ClosedChannelException, AsynchronousCloseException, ClosedByInterruptException, IOException {
        return socketChannel.read(dst);
    }
    
    @Override
    public SelectableChannel configureBlocking(boolean block) throws ClosedChannelException, IOException {
        return socketChannel.configureBlocking(block);
    }
}

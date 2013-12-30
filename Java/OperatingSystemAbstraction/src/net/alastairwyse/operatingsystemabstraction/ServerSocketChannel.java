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
 * @author User
 */
public class ServerSocketChannel implements IServerSocketChannel {
    private java.nio.channels.ServerSocketChannel serverSocketChannel;
    
    /**
     * Initialises a new instance of the ServerSocketChannel class.
     */
    public ServerSocketChannel() {
    }
    
    @Override
    public boolean isOpen() {
        if(serverSocketChannel == null) {
            return false;
        }
        else {
            return serverSocketChannel.isOpen();
        }
    }
    
    @Override
    public void bind (SocketAddress local, int backlog) throws AlreadyBoundException, UnsupportedAddressTypeException, ClosedChannelException, IOException, SecurityException {
        serverSocketChannel.bind(local, backlog);
    }
    
    @Override
    public void open() throws IOException {
        if(serverSocketChannel != null) {
            serverSocketChannel.close();
        }
        serverSocketChannel = java.nio.channels.ServerSocketChannel.open();
    }
    
    @Override
    public void close() throws IOException {
        serverSocketChannel.close();
    }
    
    @Override
    public ISocketChannel accept() throws ClosedChannelException, AsynchronousCloseException, ClosedByInterruptException, NotYetBoundException, SecurityException, IOException {
        // Returning java.nio.channels.SocketChannel wrapped in an operatingsystemabstraction.SocketChannel and implementing ISocketChannel, allows this method to be mocked
        java.nio.channels.SocketChannel socketChannel = serverSocketChannel.accept();
        if (socketChannel != null) {
            return new SocketChannel(socketChannel);
        }
        else {
            return null;
        }
    }
    
    @Override
    public SelectableChannel configureBlocking(boolean block) throws ClosedChannelException, IOException {
        return serverSocketChannel.configureBlocking(block);
    }
}

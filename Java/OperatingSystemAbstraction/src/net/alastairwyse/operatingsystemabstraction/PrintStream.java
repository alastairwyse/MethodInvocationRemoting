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

package net.alastairwyse.operatingsystemabstraction;

/**
 * Provides an abstraction of the java.io.PrintStream class, to facilitate mocking and unit testing.
 * @author Alastair Wyse
 */
public class PrintStream implements IPrintStream {

    /**
     * Initializes a new instance of the PrintStream class.
     */
    public PrintStream() {
    }
    
    @Override
    public void println(String x) {
        System.out.println(x);
    }

    @Override
    public void println() {
        System.out.println();
    }
}

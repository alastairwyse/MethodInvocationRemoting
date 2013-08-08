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

package net.alastairwyse.methodinvocationremotingunittests;

import static org.junit.Assert.*;

import org.junit.Test;
import net.alastairwyse.methodinvocationremoting.*;

/**
 * Unit tests for class methodinvocationremoting.MethodInvocation.
 * @author Alastair Wyse
 */
public class MethodInvocationTests {

    private MethodInvocation testMethodInvocation;
    
    @Test
    public void FullMethodConstructorSuccessTest() {
        Object[] parameters = new Object[2];
        parameters[0] = 1;
        parameters[1] = 2;
        testMethodInvocation = new MethodInvocation("AddNumbers", parameters, int.class);
        assertEquals("AddNumbers", testMethodInvocation.getName());
        assertEquals(1, testMethodInvocation.getParameters()[0]);
        assertEquals(2, testMethodInvocation.getParameters()[1]);
        assertEquals(int.class, testMethodInvocation.getReturnType());
    }

    @Test
    public void VoidMethodConstructorSuccessTest()
    {
        Object[] parameters = new Object[2];
        parameters[0] = 1;
        parameters[1] = 2;
        testMethodInvocation = new MethodInvocation("AddNumbers", parameters);
        assertEquals("AddNumbers", testMethodInvocation.getName());
        assertEquals(1, testMethodInvocation.getParameters()[0]);
        assertEquals(2, testMethodInvocation.getParameters()[1]);
        assertNull(testMethodInvocation.getReturnType());
    }
    
    @Test
    public void ParameterlessMethodConstructorSuccessTest() {
        testMethodInvocation = new MethodInvocation("AddNumbers", int.class);
        assertEquals("AddNumbers", testMethodInvocation.getName());
        assertEquals(int.class, testMethodInvocation.getReturnType());
        assertNull(testMethodInvocation.getParameters());
    }
    
    @Test
    public void ParameterlessVoidMethodConstructorSuccessTest() {
        testMethodInvocation = new MethodInvocation("AddNumbers");
        assertEquals("AddNumbers", testMethodInvocation.getName());
        assertNull(testMethodInvocation.getParameters());
        assertNull(testMethodInvocation.getReturnType());
    }
    
    @Test
    public void FullMethodConstructorZeroLengthParameters() {
        try {
            testMethodInvocation = new MethodInvocation("AddNumbers", new Object[0], int.class);
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("The method invocation parameters cannot be empty."));
        }
    }
    
    @Test
    public void VoidMethodConstructorZeroLengthParameters() {
        try {
            testMethodInvocation = new MethodInvocation("AddNumbers", new Object[0]);
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("The method invocation parameters cannot be empty."));
        }
    }
    
    @Test
    public void FullMethodConstructorBlankName() {
        try {
            Object[] parameters = new Object[2];
            parameters[0] = 1;
            parameters[1] = 2;
            testMethodInvocation = new MethodInvocation("   ", parameters, Integer.class);
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("The method name cannot be blank."));
        }
    }

    @Test
    public void VoidMethodConstructorBlankName() {
        try {
            Object[] parameters = new Object[2];
            parameters[0] = 1;
            parameters[1] = 2;
            testMethodInvocation = new MethodInvocation("   ", parameters);
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("The method name cannot be blank."));
        }
    }

    @Test
    public void ParameterlessMethodConstructorBlankName() {
        try {
            testMethodInvocation = new MethodInvocation("   ", Integer.class);
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("The method name cannot be blank."));
        }
    }

    @Test
    public void ParameterlessVoidMethodConstructorBlankName() {
        try {
            testMethodInvocation = new MethodInvocation("   ");
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("The method name cannot be blank."));
        }
    }
}

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

package net.alastairwyse.methodinvocationremotingloggingtests;

import org.junit.Before;
import org.junit.Test;
import static org.mockito.Mockito.*;
import net.alastairwyse.methodinvocationremoting.*;
import net.alastairwyse.applicationlogging.*;

/**
 * Unit tests for the logging functionality in class methodinvocationremoting.MethodInvocationSerializer.
 * @author Alastair Wyse
 */
public class MethodInvocationSerializerLoggingTests {

    private IApplicationLogger mockApplicationLogger;
    private MethodInvocationSerializer testMethodInvocationSerializer;
    
    @Before
    public void setUp() throws Exception {
        mockApplicationLogger = mock(IApplicationLogger.class);
        testMethodInvocationSerializer = new MethodInvocationSerializer(new SerializerOperationMap(), mockApplicationLogger);
    }
    
    @Test
    public void SerializeLoggingTest() throws Exception {
        MethodInvocation testMethodInvocation = new MethodInvocation("TestMethod", new Object[] { "abc", ((int)123), null, ((double)456.789) });

        testMethodInvocationSerializer.Serialize(testMethodInvocation);
        
        verify(mockApplicationLogger).Log(testMethodInvocationSerializer, LogLevel.Debug, "Serialized parameter of type '" + String.class.getName() + "'.");
        verify(mockApplicationLogger).Log(testMethodInvocationSerializer, LogLevel.Debug, "Serialized parameter of type '" + Integer.class.getName() + "'.");
        verify(mockApplicationLogger).Log(testMethodInvocationSerializer, LogLevel.Debug, "Serialized null parameter.");
        verify(mockApplicationLogger).Log(testMethodInvocationSerializer, LogLevel.Debug, "Serialized parameter of type '" + Double.class.getName() + "'.");
        verify(mockApplicationLogger).Log(testMethodInvocationSerializer, LogLevel.Information, "Serialized method invocation to string '<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataT' (truncated).");
        verify(mockApplicationLogger).Log(testMethodInvocationSerializer, LogLevel.Debug, "Complete string content: '<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><Data>abc</Data></Parameter><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter></Parameter><Parameter><DataType>double</DataType><Data>4.56789E2</Data></Parameter></Parameters><ReturnType></ReturnType></MethodInvocation>'.");
        verifyNoMoreInteractions(mockApplicationLogger);
    }
    
    @Test
    public void DeserializeLoggingTest() throws Exception {
        testMethodInvocationSerializer.Deserialize("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><Data>abc</Data></Parameter><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter /><Parameter><DataType>double</DataType><Data>4.5678899999999999e+002</Data></Parameter></Parameters><ReturnType /></MethodInvocation>");

        verify(mockApplicationLogger).Log(testMethodInvocationSerializer, LogLevel.Debug, "Deserialized parameter of type '" + String.class.getName() + "'.");
        verify(mockApplicationLogger).Log(testMethodInvocationSerializer, LogLevel.Debug, "Deserialized parameter of type '" + Integer.class.getName() + "'.");
        verify(mockApplicationLogger).Log(testMethodInvocationSerializer, LogLevel.Debug, "Deserialized null parameter.");
        verify(mockApplicationLogger).Log(testMethodInvocationSerializer, LogLevel.Debug, "Deserialized parameter of type '" + Double.class.getName() + "'.");
        verify(mockApplicationLogger).Log(testMethodInvocationSerializer, LogLevel.Information, "Deserialized string to method invocation 'TestMethod'.");
        verifyNoMoreInteractions(mockApplicationLogger);
    }
    
    @Test
    public void SerializeReturnValueLoggingTest() throws Exception {
        testMethodInvocationSerializer.SerializeReturnValue("ReturnString");
        testMethodInvocationSerializer.SerializeReturnValue("0123456789001234567890012345678900123456789001234567890012345678900123456789001234567890012345678900123456789001234567890012345678901");
        
        verify(mockApplicationLogger).Log(testMethodInvocationSerializer, LogLevel.Information, "Serialized return value to string '<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data>ReturnString</Data></ReturnValue>'.");
        verify(mockApplicationLogger).Log(testMethodInvocationSerializer, LogLevel.Debug, "Complete string content: '<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data>ReturnString</Data></ReturnValue>'.");
        verify(mockApplicationLogger).Log(testMethodInvocationSerializer, LogLevel.Information, "Serialized return value to string '<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data>012345678900123456789001234567890012' (truncated).");
        verify(mockApplicationLogger).Log(testMethodInvocationSerializer, LogLevel.Debug, "Complete string content: '<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data>0123456789001234567890012345678900123456789001234567890012345678900123456789001234567890012345678900123456789001234567890012345678901</Data></ReturnValue>'.");
        verifyNoMoreInteractions(mockApplicationLogger);
    }
    
    @Test
    public void DeserializeReturnValueLoggingTest() throws Exception {
        testMethodInvocationSerializer.DeserializeReturnValue("<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data>ReturnString</Data></ReturnValue>");
        testMethodInvocationSerializer.DeserializeReturnValue("<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue />");
        
        verify(mockApplicationLogger).Log(testMethodInvocationSerializer, LogLevel.Information, "Deserialized string to return value of type '" + String.class.getName() + "'.");
        verify(mockApplicationLogger).Log(testMethodInvocationSerializer, LogLevel.Information, "Deserialized string to null return value");
        verifyNoMoreInteractions(mockApplicationLogger);
    }
}

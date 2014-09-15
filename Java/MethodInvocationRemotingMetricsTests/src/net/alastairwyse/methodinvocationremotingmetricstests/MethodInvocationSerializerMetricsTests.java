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

package net.alastairwyse.methodinvocationremotingmetricstests;

import org.junit.Before;
import org.junit.Test;
import static org.mockito.Mockito.*;
import net.alastairwyse.applicationmetrics.*;
import net.alastairwyse.methodinvocationremoting.*;
import net.alastairwyse.methodinvocationremotingmetrics.*;

/**
 * Unit tests for the metrics logging functionality in class methodinvocationremoting.MethodInvocationSerializer.
 * @author Alastair Wyse
 */
public class MethodInvocationSerializerMetricsTests {

    private IMetricLogger mockMetricLogger;
    private MethodInvocationSerializer testMethodInvocationSerializer;
    
    @Before
    public void setUp() throws Exception {
        mockMetricLogger = mock(IMetricLogger.class);
        testMethodInvocationSerializer = new MethodInvocationSerializer(new SerializerOperationMap(), mockMetricLogger);
    }
    
    @Test
    public void SerializeMetricsTest() throws Exception {
        MethodInvocation testMethodInvocation = new MethodInvocation("TestMethod", new Object[] { "abc", ((int)123), null, ((double)456.789) });

        testMethodInvocationSerializer.Serialize(testMethodInvocation);
        
        verify(mockMetricLogger).Begin(isA(MethodInvocationSerializeTime.class));
        verify(mockMetricLogger).End(isA(MethodInvocationSerializeTime.class));
        verify(mockMetricLogger).Increment(isA(MethodInvocationSerialized.class));
        verify(mockMetricLogger).Add((argThat(new IsAmountMetric(new SerializedMethodInvocationSize(388)))));
        verifyNoMoreInteractions(mockMetricLogger);
    }
    
    @Test
    public void DeserializeMetricsTest() throws Exception {
        testMethodInvocationSerializer.Deserialize("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><Data>abc</Data></Parameter><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter /><Parameter><DataType>double</DataType><Data>4.5678899999999999e+002</Data></Parameter></Parameters><ReturnType /></MethodInvocation>");

        verify(mockMetricLogger).Begin(isA(MethodInvocationDeserializeTime.class));
        verify(mockMetricLogger).End(isA(MethodInvocationDeserializeTime.class));
        verify(mockMetricLogger).Increment(isA(MethodInvocationDeserialized.class));
        verifyNoMoreInteractions(mockMetricLogger);
    }
    
    @Test
    public void SerializeReturnValueMetricsTest() throws Exception {
        testMethodInvocationSerializer.SerializeReturnValue("ReturnString");
        
        verify(mockMetricLogger).Begin(isA(ReturnValueSerializeTime.class));
        verify(mockMetricLogger).End(isA(ReturnValueSerializeTime.class));
        verify(mockMetricLogger).Increment(isA(ReturnValueSerialized.class));
        verify(mockMetricLogger).Add((argThat(new IsAmountMetric(new SerializedReturnValueSize(117)))));
        verifyNoMoreInteractions(mockMetricLogger);
    }
    
    @Test
    public void DeserializeReturnValueMetricsTest() throws Exception {
        testMethodInvocationSerializer.DeserializeReturnValue("<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data>ReturnString</Data></ReturnValue>");
        testMethodInvocationSerializer.DeserializeReturnValue("<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue />");
        
        verify(mockMetricLogger, times(2)).Begin(isA(ReturnValueDeserializeTime.class));
        verify(mockMetricLogger, times(2)).End(isA(ReturnValueDeserializeTime.class));
        verify(mockMetricLogger, times(2)).Increment(isA(ReturnValueDeserialized.class));
        verifyNoMoreInteractions(mockMetricLogger);
    }
}

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

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NMock2;
using ApplicationLogging;
using ApplicationMetrics;
using MethodInvocationRemoting;
using MethodInvocationRemotingMetrics;

namespace MethodInvocationRemotingMetricsTests
{
    //******************************************************************************
    //
    // Class: MethodInvocationSerializerMetricsTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for the metrics logging functionality in class MethodInvocationRemoting.MethodInvocationSerializer.
    /// </summary>
    [TestFixture]
    class MethodInvocationSerializerMetricsTests
    {
        private Mockery mocks;
        private IMetricLogger mockMetricLogger;
        private MethodInvocationSerializer testMethodInvocationSerializer;

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockMetricLogger = mocks.NewMock<IMetricLogger>();
            testMethodInvocationSerializer = new MethodInvocationSerializer(new SerializerOperationMap(), new ConsoleApplicationLogger(LogLevel.Critical, '|', "  "), mockMetricLogger);
        }

        [Test]
        public void SerializeMetricsTest()
        {
            MethodInvocation testMethodInvocation = new MethodInvocation("TestMethod", new object[] { "abc", ((int)123), null, ((double)456.789) });

            using (mocks.Ordered)
            {
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new MethodInvocationSerializeTime()));
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new MethodInvocationSerializeTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new MethodInvocationSerialized()));
                Expect.Once.On(mockMetricLogger).Method("Add").With(IsAmountMetric.Equal(new SerializedMethodInvocationSize(381)));
            }
            testMethodInvocationSerializer.Serialize(testMethodInvocation);

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void DeserializeMetricsTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new MethodInvocationDeserializeTime()));
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new MethodInvocationDeserializeTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new MethodInvocationDeserialized()));
            }

            testMethodInvocationSerializer.Deserialize("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><Data>abc</Data></Parameter><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter /><Parameter><DataType>double</DataType><Data>4.5678899999999999e+002</Data></Parameter></Parameters><ReturnType /></MethodInvocation>");

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SerializeReturnValueMetricsTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new ReturnValueSerializeTime()));
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new ReturnValueSerializeTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new ReturnValueSerialized()));
                Expect.Once.On(mockMetricLogger).Method("Add").With(IsAmountMetric.Equal(new SerializedReturnValueSize(117)));
            }

            testMethodInvocationSerializer.SerializeReturnValue("ReturnString");

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void DeserializeReturnValueMetricsTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new ReturnValueDeserializeTime()));
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new ReturnValueDeserializeTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new ReturnValueDeserialized()));
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new ReturnValueDeserializeTime()));
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new ReturnValueDeserializeTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new ReturnValueDeserialized()));
            }

            testMethodInvocationSerializer.DeserializeReturnValue("<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data>ReturnString</Data></ReturnValue>");
            testMethodInvocationSerializer.DeserializeReturnValue("<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue />");

            mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}

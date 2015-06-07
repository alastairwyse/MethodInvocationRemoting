/*
 * Copyright 2015 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
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

#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NMock2;
using NMock2.Matchers;
using ApplicationMetrics;
using MethodInvocationRemotingMetrics;
using MethodInvocationRemoting;

namespace MethodInvocationRemotingMetricsTests
{
    //******************************************************************************
    //
    // Class: SoapMethodInvocationSerializerMetricsTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for the metrics logging functionality in class MethodInvocationRemoting.SoapMethodInvocationSerializer.
    /// </summary>
    [TestFixture]
    class SoapMethodInvocationSerializerMetricsTests
    {
        private Mockery mocks;
        private IMetricLogger mockMetricLogger;
        private SoapMethodInvocationSerializer testSoapMethodInvocationSerializer;

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockMetricLogger = mocks.NewMock<IMetricLogger>();
            testSoapMethodInvocationSerializer = new SoapMethodInvocationSerializer(mockMetricLogger);
        }

        [Test]
        public void SerializeDeserializeMetricsTest()
        {
            MethodInvocation testMethodInvocation = new MethodInvocation("TestMethod", new object[] { 1, "abc", true }, typeof(string));

            using (mocks.Ordered)
            {
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new MethodInvocationSerializeTime()));
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new MethodInvocationSerializeTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new MethodInvocationSerialized()));
                Expect.Once.On(mockMetricLogger).Method("Add").With(new TypeMatcher(typeof(SerializedMethodInvocationSize)));
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new MethodInvocationDeserializeTime()));
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new MethodInvocationDeserializeTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new MethodInvocationDeserialized()));
            }

            string serializedMethodInvocation = testSoapMethodInvocationSerializer.Serialize(testMethodInvocation);
            testSoapMethodInvocationSerializer.Deserialize(serializedMethodInvocation);

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SerializeDeserializeReturnValueMetricsTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new ReturnValueSerializeTime()));
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new ReturnValueSerializeTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new ReturnValueSerialized()));
                Expect.Once.On(mockMetricLogger).Method("Add").With(new TypeMatcher(typeof(SerializedReturnValueSize)));
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new ReturnValueDeserializeTime()));
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new ReturnValueDeserializeTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new ReturnValueDeserialized()));
            }

            string serializedReturnValue = testSoapMethodInvocationSerializer.SerializeReturnValue(new Int32[] { 1, 2, 3, 4, 5 });
            testSoapMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        // No unit test defined for throwing an exception (and calling method CancelBegin()) from methods Serialize() and SerializeReturnValue().
        //   This is because the underlying SoapFormatter class inside the SoapMethodInvocationSerializer cannot be made to throw an exception using the public methods available on the SoapMethodInvocationSerializer class.

        [Test]
        public void DeserializeExceptionMetricsTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new MethodInvocationDeserializeTime()));
                Expect.Once.On(mockMetricLogger).Method("CancelBegin").With(IsMetric.Equal(new MethodInvocationDeserializeTime()));
            }

            DeserializationException e = Assert.Throws<DeserializationException>(delegate
            {
                testSoapMethodInvocationSerializer.Deserialize("InvalidSerializedMethodInvocation");
            });
        }

        [Test]
        public void DeserializeReturnValueExceptionMetricsTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new ReturnValueDeserializeTime()));
                Expect.Once.On(mockMetricLogger).Method("CancelBegin").With(IsMetric.Equal(new ReturnValueDeserializeTime()));
            }

            DeserializationException e = Assert.Throws<DeserializationException>(delegate
            {
                testSoapMethodInvocationSerializer.DeserializeReturnValue("InvalidSerializedMethodInvocation");
            });
        }
    }
}

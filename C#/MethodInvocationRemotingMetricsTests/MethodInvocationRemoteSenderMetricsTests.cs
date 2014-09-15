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
    // Class: MethodInvocationRemoteSenderMetricsTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for the metrics logging functionality in class MethodInvocationRemoting.MethodInvocationRemoteSender.
    /// </summary>
    [TestFixture]
    class MethodInvocationRemoteSenderMetricsTests
    {
        private Mockery mocks;
        private IMethodInvocationSerializer mockMethodInvocationSerializer;
        private IRemoteSender mockRemoteSender;
        private IRemoteReceiver mockRemoteReceiver;
        private IMetricLogger mockMetricLogger;
        private MethodInvocationRemoteSender testMethodInvocationRemoteSender;
        private IMethodInvocation testMethodInvocation;
        private string testSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><string>ABC</string></Parameter><Parameter><DataType>integer</DataType><int>12345</int></Parameter><Parameter><DataType>boolean</DataType><boolean>true</boolean></Parameter></Parameters><ReturnType>string</ReturnType></MethodInvocation>";
        private string testSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><DataType>string</DataType><string>Return Data</string>";
        private string testVoidSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><void/>";

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockMethodInvocationSerializer = mocks.NewMock<IMethodInvocationSerializer>();
            mockRemoteSender = mocks.NewMock<IRemoteSender>();
            mockRemoteReceiver = mocks.NewMock<IRemoteReceiver>();
            mockMetricLogger = mocks.NewMock<IMetricLogger>();
            testMethodInvocationRemoteSender = new MethodInvocationRemoteSender(mockMethodInvocationSerializer, mockRemoteSender, mockRemoteReceiver, mockMetricLogger);
            testMethodInvocation = new MethodInvocation("TestMethod", typeof(string));
        }

        [Test]
        public void InvokeMethodMetricsTest()
        {
            Expect.Once.On(mockMethodInvocationSerializer).Method("Serialize").WithAnyArguments().Will(Return.Value(testSerializedMethodInvocation));
            Expect.Once.On(mockRemoteReceiver).Method("Receive").WithAnyArguments().Will(Return.Value(testSerializedReturnValue));
            Expect.Once.On(mockMethodInvocationSerializer).Method("DeserializeReturnValue").WithAnyArguments().Will(Return.Value("Return Data"));
            Expect.AtLeastOnce.On(mockRemoteSender);
            using (mocks.Ordered)
            {
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new RemoteMethodSendTime()));
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new RemoteMethodSendTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new RemoteMethodSent()));
            }

            testMethodInvocationRemoteSender.InvokeMethod(testMethodInvocation);

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void InvokeVoidMethodMetricsTest()
        {
            testMethodInvocation = new MethodInvocation("TestMethod");

            Expect.Once.On(mockMethodInvocationSerializer).Method("Serialize").WithAnyArguments().Will(Return.Value(testSerializedMethodInvocation));
            Expect.Once.On(mockRemoteReceiver).Method("Receive").WithAnyArguments().Will(Return.Value(testVoidSerializedReturnValue));
            Expect.Once.On(mockMethodInvocationSerializer).GetProperty("VoidReturnValue").Will(Return.Value(testVoidSerializedReturnValue));
            Expect.AtLeastOnce.On(mockRemoteSender);
            using (mocks.Ordered)
            {
                using (mocks.Ordered)
                {
                    Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new RemoteMethodSendTime()));
                    Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new RemoteMethodSendTime()));
                    Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new RemoteMethodSent()));
                }
            }

            testMethodInvocationRemoteSender.InvokeVoidMethod(testMethodInvocation);

            mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}

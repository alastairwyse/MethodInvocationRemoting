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
    // Class: MethodInvocationRemoteReceiverMetricsTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for the metrics logging functionality in class MethodInvocationRemoting.MethodInvocationRemoteReceiver.
    /// </summary>
    [TestFixture]
    class MethodInvocationRemoteReceiverMetricsTests
    {
        private Mockery mocks;
        private IMethodInvocationSerializer mockMethodInvocationSerializer;
        private IRemoteSender mockRemoteSender;
        private IRemoteReceiver mockRemoteReceiver;
        private IMetricLogger mockMetricLogger;
        private MethodInvocationRemoteReceiver testMethodInvocationRemoteReceiver;
        private string testSerializedReturnValue;
        private string testVoidReturnValue;
        private string testSerializedMethodInvocation;

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockMethodInvocationSerializer = mocks.NewMock<IMethodInvocationSerializer>();
            mockRemoteSender = mocks.NewMock<IRemoteSender>();
            mockRemoteReceiver = mocks.NewMock<IRemoteReceiver>();
            mockMetricLogger = mocks.NewMock<IMetricLogger>();
            testMethodInvocationRemoteReceiver = new MethodInvocationRemoteReceiver(mockMethodInvocationSerializer, mockRemoteSender, mockRemoteReceiver, mockMetricLogger);
            testSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><string>TestReturnValue</string></ReturnValue>";
            testVoidReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnType>void</ReturnType>";
            testSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><string>ABC</string></Parameter><Parameter><DataType>integer</DataType><int>12345</int></Parameter><Parameter><DataType>boolean</DataType><boolean>true</boolean></Parameter></Parameters><ReturnType>string</ReturnType></MethodInvocation>";
        }

        [Test]
        public void ReceiveMetricsTest()
        {
            testMethodInvocationRemoteReceiver.MethodInvocationReceived += new MethodInvocationReceivedEventHandler(SimulateMethodInvocationReceive);

            Expect.AtLeastOnce.On(mockRemoteReceiver).Method("Receive").WithNoArguments().Will(Return.Value(testSerializedMethodInvocation));
            Expect.AtLeastOnce.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new RemoteMethodReceiveTime()));
            Expect.AtLeastOnce.On(mockMethodInvocationSerializer).Method("Deserialize").Will(Return.Value(new MethodInvocation("TestMethod")));
            Expect.Once.On(mockRemoteReceiver).Method("CancelReceive").WithNoArguments();

            testMethodInvocationRemoteReceiver.Receive();
            // Need to pause so that receiveLoopThread has time to iterate before CancelReceive() is sent.
            //   Unfortunately this is still not a deterministic way to test, but best that can be done given that the Receive() spawns off a new thread.
            System.Threading.Thread.Sleep(50);
            testMethodInvocationRemoteReceiver.CancelReceive();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SendReturnValueMetricsTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockMethodInvocationSerializer).Method("SerializeReturnValue").Will(Return.Value(testSerializedReturnValue));
                Expect.AtLeastOnce.On(mockRemoteSender);
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new RemoteMethodReceiveTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new RemoteMethodReceived()));
            }

            testMethodInvocationRemoteReceiver.SendReturnValue(new object());

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SendVoidReturnValueMetricsTest()
        {
            Expect.Once.On(mockMethodInvocationSerializer).GetProperty("VoidReturnValue").Will(Return.Value(testVoidReturnValue));
            Expect.AtLeastOnce.On(mockRemoteSender);
            Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new RemoteMethodReceiveTime()));
            Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new RemoteMethodReceived()));

            testMethodInvocationRemoteReceiver.SendVoidReturn();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        private void SimulateMethodInvocationReceive(object sender, MethodInvocationReceivedEventArgs e)
        {
        }
    }
}

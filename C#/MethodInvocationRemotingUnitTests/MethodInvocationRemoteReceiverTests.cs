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

#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NMock2;
using MethodInvocationRemoting;

namespace MethodInvocationRemotingUnitTests
{
    //******************************************************************************
    //
    // Class: MethodInvocationRemoteReceiverTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for class MethodInvocationRemoting.MethodInvocationRemoteReceiver.
    /// </summary>
    [TestFixture]
    public class MethodInvocationRemoteReceiverTests
    {
        private Mockery mocks;
        private IMethodInvocationSerializer mockMethodInvocationSerializer;
        private IRemoteSender mockRemoteSender;
        private IRemoteReceiver mockRemoteReceiver;
        private IMethodInvocationRemoteReceiver testMethodInvocationRemoteReceiver;
        private IMethodInvocation testReceivedMethodInvocation;
        private IMethodInvocation testMethodInvocation;

        private string testSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><string>ABC</string></Parameter><Parameter><DataType>integer</DataType><int>12345</int></Parameter><Parameter><DataType>boolean</DataType><boolean>true</boolean></Parameter></Parameters><ReturnType>string</ReturnType></MethodInvocation>";

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockMethodInvocationSerializer = mocks.NewMock<IMethodInvocationSerializer>();
            mockRemoteSender = mocks.NewMock<IRemoteSender>();
            mockRemoteReceiver = mocks.NewMock<IRemoteReceiver>();
            testMethodInvocationRemoteReceiver = new MethodInvocationRemoteReceiver(mockMethodInvocationSerializer, mockRemoteSender, mockRemoteReceiver);
            object[] testMethodInvocationParameters = new object[3];
            testMethodInvocationParameters[0] = "ABC";
            testMethodInvocationParameters[1] = 12345;
            testMethodInvocationParameters[2] = true;
            testMethodInvocation = new MethodInvocation("TestMethod", testMethodInvocationParameters, typeof(string));
        }

        // Note - Exception in Receive() method is not tested, as NUnit ignores exceptions on non-test threads.

        [Test]
        public void ReceiveSuccessTests()
        {

            testMethodInvocationRemoteReceiver.MethodInvocationReceived += new MethodInvocationReceivedEventHandler(SimulateMethodInvocationReceive);

            Expect.AtLeastOnce.On(mockRemoteReceiver).Method("Receive").WithNoArguments().Will(Return.Value(testSerializedMethodInvocation));
            Expect.AtLeastOnce.On(mockMethodInvocationSerializer).Method("Deserialize").With(testSerializedMethodInvocation).Will(Return.Value(testMethodInvocation));
            Expect.Once.On(mockRemoteReceiver).Method("CancelReceive").WithNoArguments();

            testMethodInvocationRemoteReceiver.Receive();
            // Need to pause so that receiveLoopThread has time to iterate before CancelReceive() is sent.
            //   Unfortunately this is still not a deterministic way to test, but best that can be done given that the Receive() spawns off a new thread.
            System.Threading.Thread.Sleep(50);
            testMethodInvocationRemoteReceiver.CancelReceive();
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.AreSame(testMethodInvocation, testReceivedMethodInvocation);
        }

        [Test]
        public void SendReturnValueException()
        {
            string testReturnValue = "TestReturnValue";
            string testSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><string>TestReturnValue</string></ReturnValue>";

            using (mocks.Ordered)
            {
                Expect.Once.On(mockMethodInvocationSerializer).Method("SerializeReturnValue").With(testReturnValue).Will(Return.Value(testSerializedReturnValue));
                Expect.Once.On(mockRemoteSender).Method("Send").With(testSerializedReturnValue).Will(Throw.Exception(new Exception("Mock Send Failure."))); ;
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testMethodInvocationRemoteReceiver.SendReturnValue(testReturnValue);
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to send return value."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("Mock Send Failure."));
        }

        [Test]
        public void SendReturnValueSuccessTests()
        {
            string testReturnValue = "TestReturnValue";
            string testSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><string>TestReturnValue</string></ReturnValue>";

            using (mocks.Ordered)
            {
                Expect.Once.On(mockMethodInvocationSerializer).Method("SerializeReturnValue").With(testReturnValue).Will(Return.Value(testSerializedReturnValue));
                Expect.Once.On(mockRemoteSender).Method("Send").With(testSerializedReturnValue);
            }

            testMethodInvocationRemoteReceiver.SendReturnValue(testReturnValue);
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SendVoidReturnException()
        {
            string testVoidReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnType>void</ReturnType>";

            using (mocks.Ordered)
            {
                Expect.Once.On(mockMethodInvocationSerializer).GetProperty("VoidReturnValue").Will(Return.Value(testVoidReturnValue));
                Expect.Once.On(mockRemoteSender).Method("Send").With(testVoidReturnValue).Will(Throw.Exception(new Exception("Mock Send Failure.")));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testMethodInvocationRemoteReceiver.SendVoidReturn();
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to send void return value."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("Mock Send Failure."));
        }

        [Test]
        public void SendVoidReturnSuccessTests()
        {
            string testVoidReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnType>void</ReturnType>";

            using (mocks.Ordered)
            {
                Expect.Once.On(mockMethodInvocationSerializer).GetProperty("VoidReturnValue").Will(Return.Value(testVoidReturnValue));
                Expect.Once.On(mockRemoteSender).Method("Send").With(testVoidReturnValue);
            }

            testMethodInvocationRemoteReceiver.SendVoidReturn();
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        private void SimulateMethodInvocationReceive(object sender, MethodInvocationReceivedEventArgs e)
        {
            testReceivedMethodInvocation = e.MethodInvocation;
        }
    }
}

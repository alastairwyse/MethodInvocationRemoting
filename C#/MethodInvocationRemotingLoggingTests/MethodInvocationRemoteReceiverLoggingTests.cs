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

#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NMock2;
using MethodInvocationRemoting;
using ApplicationLogging;

namespace MethodInvocationRemotingLoggingTests
{
    //******************************************************************************
    //
    // Class: MethodInvocationRemoteReceiverLoggingTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for the logging functionality in class MethodInvocationRemoting.MethodInvocationRemoteReceiver.
    /// </summary>
    [TestFixture]
    public class MethodInvocationRemoteReceiverLoggingTests
    {
        private Mockery mocks;
        private IMethodInvocationSerializer mockMethodInvocationSerializer;
        private IRemoteSender mockRemoteSender;
        private IRemoteReceiver mockRemoteReceiver;
        private IApplicationLogger mockApplicationLogger;
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
            mockApplicationLogger = mocks.NewMock<IApplicationLogger>();
            testMethodInvocationRemoteReceiver = new MethodInvocationRemoteReceiver(mockMethodInvocationSerializer, mockRemoteSender, mockRemoteReceiver, mockApplicationLogger);
            testSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><string>TestReturnValue</string></ReturnValue>";
            testVoidReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnType>void</ReturnType>";
            testSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><string>ABC</string></Parameter><Parameter><DataType>integer</DataType><int>12345</int></Parameter><Parameter><DataType>boolean</DataType><boolean>true</boolean></Parameter></Parameters><ReturnType>string</ReturnType></MethodInvocation>";
        }

        [Test]
        public void ReceiveLoggingTest()
        {
            testMethodInvocationRemoteReceiver.MethodInvocationReceived += new MethodInvocationReceivedEventHandler(SimulateMethodInvocationReceive);

            Expect.AtLeastOnce.On(mockRemoteReceiver).Method("Receive").WithNoArguments().Will(Return.Value(testSerializedMethodInvocation));
            Expect.AtLeastOnce.On(mockMethodInvocationSerializer).Method("Deserialize").Will(Return.Value(new MethodInvocation("TestMethod")));
            Expect.AtLeastOnce.On(mockApplicationLogger).Method("Log").With(testMethodInvocationRemoteReceiver, LogLevel.Information, "Received method invocation 'TestMethod'.");
            Expect.Once.On(mockRemoteReceiver).Method("CancelReceive").WithNoArguments();
            Expect.Once.On(mockApplicationLogger).Method("Log").With(testMethodInvocationRemoteReceiver, LogLevel.Information, "Receive operation cancelled.");

            testMethodInvocationRemoteReceiver.Receive();
            // Need to pause so that receiveLoopThread has time to iterate before CancelReceive() is sent.
            //   Unfortunately this is still not a deterministic way to test, but best that can be done given that the Receive() spawns off a new thread.
            System.Threading.Thread.Sleep(50);
            testMethodInvocationRemoteReceiver.CancelReceive();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SendReturnValueLoggingTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockMethodInvocationSerializer).Method("SerializeReturnValue").Will(Return.Value(testSerializedReturnValue));
                Expect.AtLeastOnce.On(mockRemoteSender);
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testMethodInvocationRemoteReceiver, LogLevel.Information, "Sent return value.");
            }

            testMethodInvocationRemoteReceiver.SendReturnValue(new object());

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SendVoidReturnValueLoggingTest()
        {
            Expect.Once.On(mockMethodInvocationSerializer).GetProperty("VoidReturnValue").Will(Return.Value(testVoidReturnValue));
            Expect.AtLeastOnce.On(mockRemoteSender);
            Expect.Once.On(mockApplicationLogger).Method("Log").With(testMethodInvocationRemoteReceiver, LogLevel.Information, "Sent void return value.");

            testMethodInvocationRemoteReceiver.SendVoidReturn();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void CancelReceiveLoggingTest()
        {
            Expect.AtLeastOnce.On(mockRemoteReceiver).Method("Receive").Will(Return.Value(""));
            Expect.AtLeastOnce.On(mockRemoteReceiver);
            Expect.Once.On(mockApplicationLogger).Method("Log").With(testMethodInvocationRemoteReceiver, LogLevel.Information, "Receive operation cancelled.");

            testMethodInvocationRemoteReceiver.Receive();
            testMethodInvocationRemoteReceiver.CancelReceive();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        private void SimulateMethodInvocationReceive(object sender, MethodInvocationReceivedEventArgs e)
        {
        }
    }
}

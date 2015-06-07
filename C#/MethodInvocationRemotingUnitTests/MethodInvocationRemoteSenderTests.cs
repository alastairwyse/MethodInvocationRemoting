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
    // Class: MethodInvocationRemoteSenderTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for class MethodInvocationRemoting.MethodInvocationRemoteSender.
    /// </summary>
    [TestFixture]
    public class MethodInvocationRemoteSenderTests
    {
        private Mockery mocks;
        private IMethodInvocationSerializer mockMethodInvocationSerializer;
        private IRemoteSender mockRemoteSender;
        private IRemoteReceiver mockRemoteReceiver;
        private IMethodInvocationRemoteSender testMethodInvocationRemoteSender;
        private IMethodInvocation testMethodInvocation;
        private string testSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><string>ABC</string></Parameter><Parameter><DataType>integer</DataType><int>12345</int></Parameter><Parameter><DataType>boolean</DataType><boolean>true</boolean></Parameter></Parameters><ReturnType>string</ReturnType></MethodInvocation>";
        private string testSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><DataType>string</DataType><string>Return Data</string>";
        private string testReturnValue = "Return Data";
        private IMethodInvocation testVoidMethodInvocation;
        private string testVoidSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><string>ABC</string></Parameter><Parameter><DataType>integer</DataType><int>12345</int></Parameter><Parameter><DataType>boolean</DataType><boolean>true</boolean></Parameter></Parameters><ReturnType/></MethodInvocation>";
        private string testVoidSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><void/>";

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockMethodInvocationSerializer = mocks.NewMock<IMethodInvocationSerializer>();
            mockRemoteSender = mocks.NewMock<IRemoteSender>();
            mockRemoteReceiver = mocks.NewMock<IRemoteReceiver>();
            testMethodInvocationRemoteSender = new MethodInvocationRemoteSender(mockMethodInvocationSerializer, mockRemoteSender, mockRemoteReceiver);
            object[] testMethodInvocationParameters = new object[3];
            testMethodInvocationParameters[0] = "ABC";
            testMethodInvocationParameters[1] = 12345;
            testMethodInvocationParameters[2] = true;
            testMethodInvocation = new MethodInvocation("TestMethod", testMethodInvocationParameters, typeof(string));
            testVoidMethodInvocation = new MethodInvocation("TestMethod", testMethodInvocationParameters);
        }

        [Test]
        public void InvokeMethodVoidMethodInvocation()
        {
            ArgumentException e = Assert.Throws<ArgumentException>(delegate
            {
                testMethodInvocationRemoteSender.InvokeMethod(testVoidMethodInvocation);
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Method invocation cannot have a void return type."));
            Assert.AreEqual("inputMethodInvocation", e.ParamName);
        }

        [Test]
        public void InvokeMethodException()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockMethodInvocationSerializer).Method("Serialize").With(testMethodInvocation).Will(Return.Value(testSerializedMethodInvocation));
                Expect.Once.On(mockRemoteSender).Method("Send").With(testSerializedMethodInvocation);
                Expect.Once.On(mockRemoteReceiver).Method("Receive").WithNoArguments().Will(Throw.Exception(new Exception("Mock Receive Failure.")));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testMethodInvocationRemoteSender.InvokeMethod(testMethodInvocation);
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to invoke method."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("Mock Receive Failure."));
        }

        [Test]
        public void InvokeMethodReturnValueDeserializationException()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockMethodInvocationSerializer).Method("Serialize").With(testMethodInvocation).Will(Return.Value(testSerializedMethodInvocation));
                Expect.Once.On(mockRemoteSender).Method("Send").With(testSerializedMethodInvocation);
                Expect.Once.On(mockRemoteReceiver).Method("Receive").WithNoArguments().Will(Return.Value(testSerializedReturnValue));
                Expect.Once.On(mockMethodInvocationSerializer).Method("DeserializeReturnValue").With(testSerializedReturnValue).Will(Throw.Exception(new Exception("Mock Deserialize Failure.")));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testMethodInvocationRemoteSender.InvokeMethod(testMethodInvocation);
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to deserialize return value."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("Mock Deserialize Failure."));
        }

        [Test]
        public void InvokeMethodSuccessTests()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockMethodInvocationSerializer).Method("Serialize").With(testMethodInvocation).Will(Return.Value(testSerializedMethodInvocation));
                Expect.Once.On(mockRemoteSender).Method("Send").With(testSerializedMethodInvocation);
                Expect.Once.On(mockRemoteReceiver).Method("Receive").WithNoArguments().Will(Return.Value(testSerializedReturnValue));
                Expect.Once.On(mockMethodInvocationSerializer).Method("DeserializeReturnValue").With(testSerializedReturnValue).Will(Return.Value(testReturnValue));
            }

            string returnValue = (string)testMethodInvocationRemoteSender.InvokeMethod(testMethodInvocation);
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.AreEqual(testReturnValue, returnValue);
        }

        [Test]
        public void InvokeVoidMethodNonVoidMethodInvocation()
        {
            IMethodInvocation testMethodInvocation = new MethodInvocation("TestMethod", typeof(string));

            ArgumentException e = Assert.Throws<ArgumentException>(delegate
            {
                testMethodInvocationRemoteSender.InvokeVoidMethod(testMethodInvocation);
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Method invocation must have a void return type."));
            Assert.AreEqual("inputMethodInvocation", e.ParamName);
        }

        [Test]
        public void InvokeVoidMethodException()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockMethodInvocationSerializer).Method("Serialize").With(testVoidMethodInvocation).Will(Return.Value(testVoidSerializedMethodInvocation));
                Expect.Once.On(mockRemoteSender).Method("Send").With(testVoidSerializedMethodInvocation);
                Expect.Once.On(mockRemoteReceiver).Method("Receive").WithNoArguments().Will(Throw.Exception(new Exception("Mock Receive Failure.")));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testMethodInvocationRemoteSender.InvokeVoidMethod(testVoidMethodInvocation);
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to invoke method."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("Mock Receive Failure."));
        }

        [Test]
        public void InvokeVoidMethodNonVoidReturnType()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockMethodInvocationSerializer).Method("Serialize").With(testVoidMethodInvocation).Will(Return.Value(testVoidSerializedMethodInvocation));
                Expect.Once.On(mockRemoteSender).Method("Send").With(testVoidSerializedMethodInvocation);
                Expect.Once.On(mockRemoteReceiver).Method("Receive").WithNoArguments().Will(Return.Value(testSerializedReturnValue));
                Expect.Once.On(mockMethodInvocationSerializer).GetProperty("VoidReturnValue").Will(Return.Value(testVoidSerializedReturnValue));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testMethodInvocationRemoteSender.InvokeVoidMethod(testVoidMethodInvocation);
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Invocation of void method returned non-void."));
        }

        [Test]
        public void InvokeVoidMethodSuccessTests()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockMethodInvocationSerializer).Method("Serialize").With(testVoidMethodInvocation).Will(Return.Value(testVoidSerializedMethodInvocation));
                Expect.Once.On(mockRemoteSender).Method("Send").With(testVoidSerializedMethodInvocation);
                Expect.Once.On(mockRemoteReceiver).Method("Receive").WithNoArguments().Will(Return.Value(testVoidSerializedReturnValue));
                Expect.Once.On(mockMethodInvocationSerializer).GetProperty("VoidReturnValue").Will(Return.Value(testVoidSerializedReturnValue));
            }

            testMethodInvocationRemoteSender.InvokeVoidMethod(testVoidMethodInvocation);
            mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}

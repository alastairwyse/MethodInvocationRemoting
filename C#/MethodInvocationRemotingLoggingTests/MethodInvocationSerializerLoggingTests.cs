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
using MethodInvocationRemoting;
using ApplicationLogging;

namespace MethodInvocationRemotingLoggingTests
{
    //******************************************************************************
    //
    // Class: MethodInvocationSerializerLoggingTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for the logging functionality in class MethodInvocationRemoting.MethodInvocationSerializer.
    /// </summary>
    [TestFixture]
    public class MethodInvocationSerializerLoggingTests
    {
        private Mockery mocks;
        private IApplicationLogger mockApplicationLogger;
        private MethodInvocationSerializer testMethodInvocationSerializer;

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockApplicationLogger = mocks.NewMock<IApplicationLogger>();
            testMethodInvocationSerializer = new MethodInvocationSerializer(new SerializerOperationMap(), mockApplicationLogger);
        }

        [Test]
        public void SerializeLoggingTest()
        {
            MethodInvocation testMethodInvocation = new MethodInvocation("TestMethod", new object[] { "abc", ((int)123), null, ((double)456.789) });

            using(mocks.Ordered)
            {
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testMethodInvocationSerializer, LogLevel.Debug, "Serialized parameter of type '" + typeof(string).FullName + "'.");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testMethodInvocationSerializer, LogLevel.Debug, "Serialized parameter of type '" + typeof(int).FullName + "'.");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testMethodInvocationSerializer, LogLevel.Debug, "Serialized null parameter.");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testMethodInvocationSerializer, LogLevel.Debug, "Serialized parameter of type '" + typeof(double).FullName + "'.");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testMethodInvocationSerializer, LogLevel.Information, "Serialized method invocation to string '<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataT' (truncated).");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testMethodInvocationSerializer, LogLevel.Debug, "Complete string content: '<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><Data>abc</Data></Parameter><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter /><Parameter><DataType>double</DataType><Data>4.5678899999999999e+002</Data></Parameter></Parameters><ReturnType /></MethodInvocation>'.");
            }
            testMethodInvocationSerializer.Serialize(testMethodInvocation);

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void DeserializeLoggingTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testMethodInvocationSerializer, LogLevel.Debug, "Deserialized parameter of type '" + typeof(string).FullName + "'.");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testMethodInvocationSerializer, LogLevel.Debug, "Deserialized parameter of type '" + typeof(int).FullName + "'.");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testMethodInvocationSerializer, LogLevel.Debug, "Deserialized null parameter.");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testMethodInvocationSerializer, LogLevel.Debug, "Deserialized parameter of type '" + typeof(double).FullName + "'.");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testMethodInvocationSerializer, LogLevel.Information, "Deserialized string to method invocation 'TestMethod'.");
            }

            testMethodInvocationSerializer.Deserialize("<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><Data>abc</Data></Parameter><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter /><Parameter><DataType>double</DataType><Data>4.5678899999999999e+002</Data></Parameter></Parameters><ReturnType /></MethodInvocation>");

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SerializeReturnValueLoggingTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testMethodInvocationSerializer, LogLevel.Information, "Serialized return value to string '<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data>ReturnString</Data></ReturnValue>'.");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testMethodInvocationSerializer, LogLevel.Debug, "Complete string content: '<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data>ReturnString</Data></ReturnValue>'.");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testMethodInvocationSerializer, LogLevel.Information, "Serialized return value to string '<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data>012345678900123456789001234567890012' (truncated).");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testMethodInvocationSerializer, LogLevel.Debug, "Complete string content: '<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data>0123456789001234567890012345678900123456789001234567890012345678900123456789001234567890012345678900123456789001234567890012345678901</Data></ReturnValue>'.");
            }

            testMethodInvocationSerializer.SerializeReturnValue("ReturnString");
            testMethodInvocationSerializer.SerializeReturnValue("0123456789001234567890012345678900123456789001234567890012345678900123456789001234567890012345678900123456789001234567890012345678901");
            
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void DeserializeReturnValueLoggingTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testMethodInvocationSerializer, LogLevel.Information, "Deserialized string to return value of type '" + typeof(string).FullName + "'.");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testMethodInvocationSerializer, LogLevel.Information, "Deserialized string to null return value");
            }

            testMethodInvocationSerializer.DeserializeReturnValue("<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data>ReturnString</Data></ReturnValue>");
            testMethodInvocationSerializer.DeserializeReturnValue("<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue />");

            mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}

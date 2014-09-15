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
using NMock2.Matchers;
using ApplicationLogging;
using MethodInvocationRemoting;

namespace MethodInvocationRemotingLoggingTests
{
    //******************************************************************************
    //
    // Class: SoapMethodInvocationSerializerLoggingTests
    //
    //******************************************************************************
    /// <summary>
    ///  Unit tests for the logging functionality in class MethodInvocationRemoting.SoapMethodInvocationSerializer.
    /// </summary>
    [TestFixture]
    class SoapMethodInvocationSerializerLoggingTests
    {
        private Mockery mocks;
        private IApplicationLogger mockApplicationLogger;
        private SoapMethodInvocationSerializer testSoapMethodInvocationSerializer;

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockApplicationLogger = mocks.NewMock<IApplicationLogger>();
            testSoapMethodInvocationSerializer = new SoapMethodInvocationSerializer(mockApplicationLogger);
        }

        [Test]
        public void SerializeDeserializeSuccessTest()
        {
            MethodInvocation testMethodInvocation = new MethodInvocation("TestMethod", new object[] { 1, "abc", true }, typeof(string)); 

            using (mocks.Ordered)
            {
                // In testing of the SoapMethodInvocationSerializer class, we are not testing the actual content of the resulting serialized string (as this is actually created by the SoapFormatter class)
                //   Hence we just expect the starting text of the log entry
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testSoapMethodInvocationSerializer, LogLevel.Information, new StringContainsMatcher("Serialized method invocation to string "));
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testSoapMethodInvocationSerializer, LogLevel.Debug, new StringContainsMatcher("Complete string content: "));
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testSoapMethodInvocationSerializer, LogLevel.Information, "Deserialized string to method invocation 'TestMethod'.");
            }

            string serializedMethodInvocation = testSoapMethodInvocationSerializer.Serialize(testMethodInvocation);
            testSoapMethodInvocationSerializer.Deserialize(serializedMethodInvocation);

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SerializeDeserializeReturnValueSuccessTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testSoapMethodInvocationSerializer, LogLevel.Information, new StringContainsMatcher("Serialized return value to string "));
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testSoapMethodInvocationSerializer, LogLevel.Debug, new StringContainsMatcher("Complete string content: "));
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testSoapMethodInvocationSerializer, LogLevel.Information, "Deserialized string to return value of type '" + typeof(Int32[]).FullName.ToString() + "'.");
            }

            string serializedReturnValue = testSoapMethodInvocationSerializer.SerializeReturnValue(new Int32[] { 1, 2, 3, 4, 5 });
            testSoapMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);

            mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}

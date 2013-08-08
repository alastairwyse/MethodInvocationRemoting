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

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MethodInvocationRemoting;


namespace MethodInvocationRemotingUnitTests
{
    //******************************************************************************
    //
    // Class: SoapMethodInvocationSerializerTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for class MethodInvocationRemoting.SoapMethodInvocationSerializer.
    /// </summary>
    [TestFixture]
    public class SoapMethodInvocationSerializerTests
    {
        private SoapMethodInvocationSerializer testSoapMethodInvocationSerializer;

        [SetUp]
        protected void SetUp()
        {
            testSoapMethodInvocationSerializer = new SoapMethodInvocationSerializer();
        }

        [Test]
        public void SerializeDeserializeSuccessTests()
        {
            // Create a MethodInvocation object to serialize
            object[] parameters = new object[6];
            parameters[0] = (string)"ABC";
            parameters[1] = (int)54321;
            parameters[2] = new DateTime(2013, 1, 5, 10, 27, 30);
            parameters[3] = (decimal)1234.5678;
            parameters[4] = new TimeSpan(4, 3, 2, 1);
            parameters[5] = (bool)true;
            MethodInvocation testMethodInvocation = new MethodInvocation("MyMethod", parameters, typeof(bool));

            // Serialize and deserialize MethodInvocation the object
            string serializedMethodInvocation = testSoapMethodInvocationSerializer.Serialize(testMethodInvocation);
            IMethodInvocation deserializedMethodInvocation = testSoapMethodInvocationSerializer.Deserialize(serializedMethodInvocation);

            // Test resulting object
            Assert.AreEqual("MyMethod", deserializedMethodInvocation.Name);
            Assert.AreEqual(typeof(bool), deserializedMethodInvocation.ReturnType);
            Assert.AreEqual("ABC", (string)deserializedMethodInvocation.Parameters[0]);
            Assert.AreEqual(54321, (int)deserializedMethodInvocation.Parameters[1]);
            Assert.AreEqual(2013, ((DateTime)deserializedMethodInvocation.Parameters[2]).Year);
            Assert.AreEqual(1, ((DateTime)deserializedMethodInvocation.Parameters[2]).Month);
            Assert.AreEqual(5, ((DateTime)deserializedMethodInvocation.Parameters[2]).Day);
            Assert.AreEqual(10, ((DateTime)deserializedMethodInvocation.Parameters[2]).Hour);
            Assert.AreEqual(27, ((DateTime)deserializedMethodInvocation.Parameters[2]).Minute);
            Assert.AreEqual(30, ((DateTime)deserializedMethodInvocation.Parameters[2]).Second);
            Assert.AreEqual(1234.5678, (decimal)deserializedMethodInvocation.Parameters[3]);
            Assert.AreEqual(4, ((TimeSpan)deserializedMethodInvocation.Parameters[4]).Days);
            Assert.AreEqual(3, ((TimeSpan)deserializedMethodInvocation.Parameters[4]).Hours);
            Assert.AreEqual(2, ((TimeSpan)deserializedMethodInvocation.Parameters[4]).Minutes);
            Assert.AreEqual(1, ((TimeSpan)deserializedMethodInvocation.Parameters[4]).Seconds);
            Assert.AreEqual(true, (bool)deserializedMethodInvocation.Parameters[5]);
        }

        [Test]
        public void SerializeUnserializableParameter()
        {
            // The SoapFormatter object does not support serializing generic types, hence test that this is correctly handled.
            List<string> listParameter = new List<string>();
            listParameter.Add("Apple");
            listParameter.Add("Orange");
            listParameter.Add("Peach");
            object[] parameters = new object[1] {listParameter};
            MethodInvocation testMethodInvocation = new MethodInvocation("MyMethod", parameters);

            SerializationException e = Assert.Throws<SerializationException>(delegate
            {
                string serializedMethodInvocation = testSoapMethodInvocationSerializer.Serialize(testMethodInvocation);
            });
            Assert.That(e.Message, Is.StringStarting("Failed to serialize method invocation 'MyMethod'."));
        }

        [Test]
        public void DeserializeInvalidString()
        {
            DeserializationException e = Assert.Throws<DeserializationException>(delegate
            {
                IMethodInvocation deserializedMethodInvocation = testSoapMethodInvocationSerializer.Deserialize("<InvalidXml><WithoutProperClosingTag>");
            });
            Assert.That(e.Message, Is.StringStarting("Failed to deserialize method invocation."));
        }

        [Test]
        public void VoidReturnValueSuccessTests()
        {
            string expectedVoidReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnType>void</ReturnType>";

            Assert.AreEqual(expectedVoidReturnValue, testSoapMethodInvocationSerializer.VoidReturnValue);
        }
    }
}

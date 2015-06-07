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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MethodInvocationRemotingUnitTests
{
    //******************************************************************************
    //
    // Class: MethodInvocationSerializerTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for class MethodInvocationRemoting.MethodInvocationSerializer.
    /// </summary>
    [TestFixture]
    public class MethodInvocationSerializerTests
    {
        private MethodInvocationSerializer testMethodInvocationSerializer;

        [SetUp]
        protected void SetUp()
        {
            testMethodInvocationSerializer = new MethodInvocationSerializer(new SerializerOperationMap());
        }

        [Test]
        public void AddIXmlSerializableOperationsInvalidType()
        {


            ArgumentException e = Assert.Throws<ArgumentException>(delegate
            {
                testMethodInvocationSerializer.AddIXmlSerializableOperations(typeof(String), "string");
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("The native type 'System.String' does not implement interface System.Xml.Serialization.IXmlSerializable."));
            Assert.AreEqual("nativeType", e.ParamName);
        }

        [Test]
        public void DeserializeMissingStartTag()
        {
            string testXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocationX></MethodInvocationX>";
            
            DeserializationException e = Assert.Throws<DeserializationException>(delegate
            {
                testMethodInvocationSerializer.Deserialize(testXml);
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to deserialize method invocation."));
        }

        [Test]
        public void DeserializeMissingMethodNameTag()
        {
            string testXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodNameX></MethodNameX></MethodInvocation>";

            DeserializationException e = Assert.Throws<DeserializationException>(delegate
            {
                testMethodInvocationSerializer.Deserialize(testXml);
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to deserialize method invocation."));
        }

        [Test]
        public void DeserializeBlankMethodName()
        {
            string testXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName /><Parameters /><ReturnType /></MethodInvocation>";

            Exception e = Assert.Throws<Exception>(delegate
            {
                testMethodInvocationSerializer.Deserialize(testXml);
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to build method invocation object."));
        }

        [Test]
        public void DeserializeUnsupportedParameterDataType()
        {
            string testXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>falseDataType</DataType><Data>546</Data></Parameter></Parameters><ReturnType/></MethodInvocation>";

            DeserializationException e = Assert.Throws<DeserializationException>(delegate
            {
                testMethodInvocationSerializer.Deserialize(testXml);
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to deserialize method invocation."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("Serialized type 'falseDataType' does not exist in the operation map."));
        }

        [Test]
        public void SerializeUnsupportedParameterDataType()
        {
            UInt16 unsupportedParameter = 123;
            MethodInvocation testMethodInvocation = new MethodInvocation("MyMethod", new object[1] { unsupportedParameter });

            SerializationException e = Assert.Throws<SerializationException>(delegate
            {
                testMethodInvocationSerializer.Serialize(testMethodInvocation);
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to serialize invocation of method 'MyMethod'."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("Native type 'System.UInt16' does not exist in the operation map."));
        }

        [Test]
        public void SerializeMethodSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter><DataType>integer</DataType><Data>-456</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)123, (Int32)(-456) }, typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeMethodSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter><DataType>integer</DataType><Data>-456</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(2, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(123, (Int32)returnedMethodInvocation.Parameters[0]);
            Assert.AreEqual(-456, (Int32)returnedMethodInvocation.Parameters[1]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);
        }

        [Test]
        public void SerializeReturnValueSuccessTests()
        {
            string expectedSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>integer</DataType><Data>789</Data></ReturnValue>";

            string actualSerializedReturnValue = testMethodInvocationSerializer.SerializeReturnValue(789);
            Assert.AreEqual(expectedSerializedReturnValue, actualSerializedReturnValue);
        }

        [Test]
        public void DeserializeReturnValueSuccessTests()
        {
            string serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>integer</DataType><Data>789</Data></ReturnValue>";

            Int32 returnValue = (Int32)testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
            Assert.AreEqual(789, returnValue);
        }

        #region SerializationException and DeserializationException Field Tests

        [Test]
        public void DeserializeExceptionFieldTest()
        {
            string testXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>falseDataType</DataType><Data>546</Data></Parameter><InvalidTag></InvalidTag></Parameters><ReturnType/></MethodInvocation>";

            DeserializationException e = Assert.Throws<DeserializationException>(delegate
            {
                testMethodInvocationSerializer.Deserialize(testXml);
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to deserialize method invocation."));
            Assert.AreEqual(testXml, e.SerializedObject);
        }

        [Test]
        public void DeserializeReturnValueExceptionFieldTest()
        {
            string serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><InvalidTag></InvalidTag></ReturnValue>";

            DeserializationException e = Assert.Throws<DeserializationException>(delegate
            {
                testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to deserialize return value."));
            Assert.AreEqual(serializedReturnValue, e.SerializedObject);
        }


        [Test]
        public void SerializeExceptionFieldTest()
        {
            object[] methodParameters = new object[1] { (UInt16)123 };
            MethodInvocation testMethodInvocation = new MethodInvocation("TestMethod", methodParameters);

            SerializationException e = Assert.Throws<SerializationException>(delegate
            {
                testMethodInvocationSerializer.Serialize(testMethodInvocation);
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to serialize invocation of method 'TestMethod'."));
            Assert.AreSame(testMethodInvocation, e.TargetObject);
        }

        [Test]
        public void SerializeReturnValueExceptionFieldTest()
        {
            Dictionary<int, string> returnValue = new Dictionary<int, string>();

            SerializationException e = Assert.Throws<SerializationException>(delegate
            {
                testMethodInvocationSerializer.SerializeReturnValue(returnValue);
            });
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Failed to serialize return value."));
            Assert.AreSame(returnValue, e.TargetObject);
        }

        #endregion

        #region MethodInvocation Permutation Success Tests

        [Test]
        public void VoidReturnValueSuccessTests()
        {
            string expectedVoidReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnType>void</ReturnType>";

            Assert.AreEqual(expectedVoidReturnValue, testMethodInvocationSerializer.VoidReturnValue);
        }

        [Test]
        public void SerializeParameterlessVoidMethodSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters /><ReturnType /></MethodInvocation>";

            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod"));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeParameterlessVoidMethodSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters /><ReturnType /></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.IsNull(returnedMethodInvocation.Parameters);
            Assert.IsNull(returnedMethodInvocation.ReturnType);

            // Test again without self closing tags for parameters and return type
            serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters></Parameters><ReturnType></ReturnType></MethodInvocation>";

            returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.IsNull(returnedMethodInvocation.Parameters);
            Assert.IsNull(returnedMethodInvocation.ReturnType);
        }

        [Test]
        public void SerializeVoidMethodSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter><DataType>integer</DataType><Data>-456</Data></Parameter></Parameters><ReturnType /></MethodInvocation>";

            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)123, (Int32)(-456) }));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeVoidMethodSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter><DataType>integer</DataType><Data>-456</Data></Parameter></Parameters><ReturnType /></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(2, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(123, (Int32)returnedMethodInvocation.Parameters[0]);
            Assert.AreEqual(-456, (Int32)returnedMethodInvocation.Parameters[1]);
            Assert.IsNull(returnedMethodInvocation.ReturnType);

            // Test again without self closing tags for return type
            serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter><DataType>integer</DataType><Data>-456</Data></Parameter></Parameters><ReturnType></ReturnType></MethodInvocation>";

            returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(2, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(123, (Int32)returnedMethodInvocation.Parameters[0]);
            Assert.AreEqual(-456, (Int32)returnedMethodInvocation.Parameters[1]);
            Assert.IsNull(returnedMethodInvocation.ReturnType);
        }

        [Test]
        public void SerializeParameterlessMethodSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters /><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeParameterlessMethodSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters /><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.IsNull(returnedMethodInvocation.Parameters);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);

            // Test again without self closing tags for parameters
            serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.IsNull(returnedMethodInvocation.Parameters);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);
        }

        #endregion

        #region Null Parameter Tests

        [Test]
        public void SerializeNullParameterMethodSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter /><Parameter><DataType>integer</DataType><Data>-456</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)123, null, (Int32)(-456) }, typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeNullParameterMethodSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter /><Parameter><DataType>integer</DataType><Data>-456</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(3, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(123, (Int32)returnedMethodInvocation.Parameters[0]);
            Assert.IsNull(returnedMethodInvocation.Parameters[1]);
            Assert.AreEqual(-456, (Int32)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);

            // Test again without self closing tags for null parameter
            serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter></Parameter><Parameter><DataType>integer</DataType><Data>-456</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";
            returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(3, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(123, (Int32)returnedMethodInvocation.Parameters[0]);
            Assert.IsNull(returnedMethodInvocation.Parameters[1]);
            Assert.AreEqual(-456, (Int32)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);
        }

        [Test]
        public void SerializeNullReturnValueSuccessTests()
        {
            string expectedSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue />";
            String returnValue = null;

            string actualSerializedReturnValue = testMethodInvocationSerializer.SerializeReturnValue(returnValue);
            Assert.AreEqual(expectedSerializedReturnValue, actualSerializedReturnValue);
        }

        [Test]
        public void DeserializeNullReturnValueSuccessTests()
        {
            string serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue />";

            object returnValue = testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
            Assert.IsNull(returnValue);

            // Test again without self closing tag for return type
            serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue></ReturnValue>";

            returnValue = testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
            Assert.IsNull(returnValue);
        }

        #endregion

        #region Array Parameter Tests

        [Test]
        public void SerializeEmptyArrayParameterSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType></Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)9, new Int32[] { }, (Int32)(8) }, typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeEmptyArrayParameterSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType></Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(3, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(9, (Int32)returnedMethodInvocation.Parameters[0]);
            Int32[] arrayParameter = (Int32[])returnedMethodInvocation.Parameters[1];
            Assert.AreEqual(0, arrayParameter.Length);
            Assert.AreEqual(8, (Int32)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);
        }

        [Test]
        public void SerializeEmptyArrayReturnValueSuccessTests()
        {
            string expectedSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType></Data></ReturnValue>";

            string actualSerializedReturnValue = testMethodInvocationSerializer.SerializeReturnValue(new Int32[] { });
            Assert.AreEqual(expectedSerializedReturnValue, actualSerializedReturnValue);
        }

        [Test]
        public void DeserializeEmptyArrayReturnValueSuccessTests()
        {
            string serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType></Data></ReturnValue>";

            Int32[] returnValue = (Int32[])testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
            Assert.AreEqual(0, returnValue.Length);
        }

        [Test]
        public void SerializeArrayParameterSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType><Element><DataType>integer</DataType><Data>123</Data></Element><Element><DataType>integer</DataType><Data>-456</Data></Element></Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)9, new Int32[] { 123, -456 }, (Int32)(8) }, typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeArrayParameterSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType><Element><DataType>integer</DataType><Data>123</Data></Element><Element><DataType>integer</DataType><Data>-456</Data></Element></Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(3, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(9, (Int32)returnedMethodInvocation.Parameters[0]);
            Int32[] arrayParameter = (Int32[])returnedMethodInvocation.Parameters[1];
            Assert.AreEqual(123, arrayParameter[0]);
            Assert.AreEqual(-456, arrayParameter[1]);
            Assert.AreEqual(8, (Int32)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);
        }

        [Test]
        public void SerializeArrayReturnValueSuccessTests()
        {
            string expectedSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType><Element><DataType>integer</DataType><Data>123</Data></Element><Element><DataType>integer</DataType><Data>-456</Data></Element></Data></ReturnValue>";

            string actualSerializedReturnValue = testMethodInvocationSerializer.SerializeReturnValue(new Int32[] { 123, -456 });
            Assert.AreEqual(expectedSerializedReturnValue, actualSerializedReturnValue);
        }

        [Test]
        public void DeserializeArrayReturnValueSuccessTests()
        {
            string serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType><Element><DataType>integer</DataType><Data>123</Data></Element><Element><DataType>integer</DataType><Data>-456</Data></Element></Data></ReturnValue>";

            Int32[] returnValue = (Int32[])testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
            Assert.AreEqual(2, returnValue.Length);
            Assert.AreEqual(123, returnValue[0]);
            Assert.AreEqual(-456, returnValue[1]);
        }

        [Test]
        public void SerializeNullElementArrayParameterSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>101</Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>String 1</Data></Element><Element /><Element><DataType>string</DataType><Data>String 3</Data></Element></Data></Parameter><Parameter><DataType>integer</DataType><Data>202</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)101, new String[] { "String 1", null, "String 3" }, (Int32)202 }, typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeNullElementArrayParameterSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>101</Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>String 1</Data></Element><Element /><Element><DataType>string</DataType><Data>String 3</Data></Element></Data></Parameter><Parameter><DataType>integer</DataType><Data>202</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(3, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(101, (Int32)returnedMethodInvocation.Parameters[0]);
            String[] arrayParameter = (String[])returnedMethodInvocation.Parameters[1];
            Assert.AreEqual("String 1", arrayParameter[0]);
            Assert.IsNull(arrayParameter[1]);
            Assert.AreEqual("String 3", arrayParameter[2]);
            Assert.AreEqual(3, arrayParameter.Length);
            Assert.AreEqual(202, (Int32)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);

            // Test again without self closing tags for null element
            serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>101</Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>String 1</Data></Element><Element></Element><Element><DataType>string</DataType><Data>String 3</Data></Element></Data></Parameter><Parameter><DataType>integer</DataType><Data>202</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";
            returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(3, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(101, (Int32)returnedMethodInvocation.Parameters[0]);
            arrayParameter = (String[])returnedMethodInvocation.Parameters[1];
            Assert.AreEqual("String 1", arrayParameter[0]);
            Assert.IsNull(arrayParameter[1]);
            Assert.AreEqual("String 3", arrayParameter[2]);
            Assert.AreEqual(3, arrayParameter.Length);
            Assert.AreEqual(202, (Int32)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);
        }

        [Test]
        public void SerializeNullElementArrayReturnValueSuccessTests()
        {
            string expectedSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>String 1</Data></Element><Element /><Element><DataType>string</DataType><Data>String 3</Data></Element></Data></ReturnValue>";

            string actualSerializedReturnValue = testMethodInvocationSerializer.SerializeReturnValue(new String[] { "String 1", null, "String 3" });
            Assert.AreEqual(expectedSerializedReturnValue, actualSerializedReturnValue);
        }

        [Test]
        public void DeserializeNullElementArrayReturnValueSuccessTests()
        {
            string serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>String 1</Data></Element><Element /><Element><DataType>string</DataType><Data>String 3</Data></Element></Data></ReturnValue>";

            String[] returnValue = (String[])testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
            Assert.AreEqual("String 1", returnValue[0]);
            Assert.IsNull(returnValue[1]);
            Assert.AreEqual("String 3", returnValue[2]);
            Assert.AreEqual(3, returnValue.Length);

            // Test again without self closing tags for null element
            serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>String 1</Data></Element><Element></Element><Element><DataType>string</DataType><Data>String 3</Data></Element></Data></ReturnValue>";
            returnValue = (String[])testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
            Assert.AreEqual("String 1", returnValue[0]);
            Assert.IsNull(returnValue[1]);
            Assert.AreEqual("String 3", returnValue[2]);
            Assert.AreEqual(3, returnValue.Length);
        }

        #endregion

        #region Integer Parameter Tests

        [Test]
        public void SerializeIntegerParameterSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>-2147483648</Data></Parameter><Parameter><DataType>integer</DataType><Data>2147483647</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)(-2147483648), (Int32)2147483647 }, typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeIntegerParameterSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>-2147483648</Data></Parameter><Parameter><DataType>integer</DataType><Data>2147483647</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(2, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(-2147483648, (Int32)returnedMethodInvocation.Parameters[0]);
            Assert.AreEqual(2147483647, (Int32)returnedMethodInvocation.Parameters[1]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);
        }

        #endregion

        #region String Parameter Tests

        [Test]
        public void SerializeEmptyStringParameterSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>string</DataType><Data><Empty /></Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)9, (String)"", (Int32)(8) }, typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeEmptyStringParameterSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>string</DataType><Data><Empty /></Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(3, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(9, (Int32)returnedMethodInvocation.Parameters[0]);
            Assert.AreEqual("", (String)returnedMethodInvocation.Parameters[1]);
            Assert.AreEqual(8, (Int32)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);

            // Test again without self closing tags for blank string
            serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>string</DataType><Data><Empty></Empty></Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";
            returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(3, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(9, (Int32)returnedMethodInvocation.Parameters[0]);
            Assert.AreEqual("", (String)returnedMethodInvocation.Parameters[1]);
            Assert.AreEqual(8, (Int32)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);
        }

        [Test]
        public void SerializeEmptyStringReturnValueSuccessTests()
        {
            string expectedSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data><Empty /></Data></ReturnValue>";

            string actualSerializedReturnValue = testMethodInvocationSerializer.SerializeReturnValue("");
            Assert.AreEqual(expectedSerializedReturnValue, actualSerializedReturnValue);
        }

        [Test]
        public void DeserializeEmptyStringReturnValueSuccessTests()
        {
            string serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data><Empty /></Data></ReturnValue>";

            string returnValue = (string)testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
            Assert.AreEqual("", returnValue);

            // Test again without self closing tags for blank string
            serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data><Empty></Empty></Data></ReturnValue>";

            returnValue = (string)testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
            Assert.AreEqual("", returnValue);
        }

        [Test]
        public void SerializeStringParameterSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>string</DataType><Data>Test string with &lt;embedded&gt;XML tag&lt;/embedded&gt;</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>string</DataType></ReturnType></MethodInvocation>";

            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)9, (String)"Test string with <embedded>XML tag</embedded>", (Int32)(8) }, typeof(String)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeStringParameterSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>string</DataType><Data>Test string with &lt;embedded&gt;XML tag&lt;/embedded&gt;</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>string</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(3, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(9, (Int32)returnedMethodInvocation.Parameters[0]);
            Assert.AreEqual("Test string with <embedded>XML tag</embedded>", (String)returnedMethodInvocation.Parameters[1]);
            Assert.AreEqual(8, (Int32)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(typeof(String), returnedMethodInvocation.ReturnType);
        }

        [Test]
        public void SerializeStringReturnValueSuccessTests()
        {
            string expectedSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data>Test string with &lt;embedded&gt;XML tag&lt;/embedded&gt;</Data></ReturnValue>";

            string actualSerializedReturnValue = testMethodInvocationSerializer.SerializeReturnValue("Test string with <embedded>XML tag</embedded>");
            Assert.AreEqual(expectedSerializedReturnValue, actualSerializedReturnValue);
        }

        [Test]
        public void DeserializeStringReturnValueSuccessTests()
        {
            string serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>string</DataType><Data>Test string with &lt;embedded&gt;XML tag&lt;/embedded&gt;</Data></ReturnValue>";

            string returnValue = (string)testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
            Assert.AreEqual("Test string with <embedded>XML tag</embedded>", returnValue);
        }

        #endregion

        #region SByte Parameter Tests

        [Test]
        public void SerializeSByteParameterSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>signedByte</DataType><Data>-128</Data></Parameter><Parameter><DataType>signedByte</DataType><Data>127</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)9, (SByte)(-128), (SByte)(127), (Int32)8 }, typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeSByteParameterSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>signedByte</DataType><Data>-128</Data></Parameter><Parameter><DataType>signedByte</DataType><Data>127</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(4, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(9, (Int32)returnedMethodInvocation.Parameters[0]);
            Assert.AreEqual(-128, (SByte)returnedMethodInvocation.Parameters[1]);
            Assert.AreEqual(127, (SByte)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(8, (Int32)returnedMethodInvocation.Parameters[3]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);
        }

        #endregion

        #region Short Integer Parameter Tests

        [Test]
        public void SerializeShortParameterSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>shortInteger</DataType><Data>-32768</Data></Parameter><Parameter><DataType>shortInteger</DataType><Data>32767</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)9, (short)(-32768), (short)32767, (Int32)8 }, typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeShortParameterSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>shortInteger</DataType><Data>-32768</Data></Parameter><Parameter><DataType>shortInteger</DataType><Data>32767</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(4, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(9, (Int32)returnedMethodInvocation.Parameters[0]);
            Assert.AreEqual(-32768, (short)returnedMethodInvocation.Parameters[1]);
            Assert.AreEqual(32767, (short)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(8, (Int32)returnedMethodInvocation.Parameters[3]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);
        }

        #endregion

        #region Long Integer Parameter Tests

        [Test]
        public void SerializeLongParameterSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>longInteger</DataType><Data>-9223372036854775808</Data></Parameter><Parameter><DataType>longInteger</DataType><Data>9223372036854775807</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)9, (long)(-9223372036854775808), (long)9223372036854775807, (Int32)8 }, typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeLongParameterSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>longInteger</DataType><Data>-9223372036854775808</Data></Parameter><Parameter><DataType>longInteger</DataType><Data>9223372036854775807</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(4, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(9, (Int32)returnedMethodInvocation.Parameters[0]);
            Assert.AreEqual(-9223372036854775808, (long)returnedMethodInvocation.Parameters[1]);
            Assert.AreEqual(9223372036854775807, (long)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(8, (Int32)returnedMethodInvocation.Parameters[3]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);
        }

        #endregion

        #region Float Parameter Tests

        [Test]
        public void SerializeFloatParameterSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>float</DataType><Data>-3.14159261e-038</Data></Parameter><Parameter><DataType>float</DataType><Data>3.14159272e+038</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)9, (float)(-3.14159265358979323846264338327E-38F), (float)3.14159265358979323846264338327E+38F, (Int32)8 }, typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeFloatParameterSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>float</DataType><Data>-3.14159261e-038</Data></Parameter><Parameter><DataType>float</DataType><Data>3.14159272e+038</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(4, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(9, (Int32)returnedMethodInvocation.Parameters[0]);
            Assert.AreEqual(-3.14159265358979323846264338327E-38F, (float)returnedMethodInvocation.Parameters[1]);
            Assert.AreEqual(3.14159265358979323846264338327E+38F, (float)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(8, (Int32)returnedMethodInvocation.Parameters[3]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);

            // Test again with values serialized in Java format
            serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>float</DataType><Data>-3.14159261E-38</Data></Parameter><Parameter><DataType>float</DataType><Data>3.14159272E38</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";
            returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(4, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(9, (Int32)returnedMethodInvocation.Parameters[0]);
            Assert.AreEqual(-3.14159265358979323846264338327E-38F, (float)returnedMethodInvocation.Parameters[1]);
            Assert.AreEqual(3.14159265358979323846264338327E+38F, (float)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(8, (Int32)returnedMethodInvocation.Parameters[3]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);
        }

        [Test]
        public void SerializeInfinityFloatParameterSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>float</DataType><Data>-Infinity</Data></Parameter><Parameter><DataType>float</DataType><Data>Infinity</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)9, Single.NegativeInfinity, Single.PositiveInfinity, (Int32)8 }, typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeInfinityFloatParameterSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>float</DataType><Data>-Infinity</Data></Parameter><Parameter><DataType>float</DataType><Data>Infinity</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(4, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(9, (Int32)returnedMethodInvocation.Parameters[0]);
            Assert.AreEqual(Single.NegativeInfinity, (Single)returnedMethodInvocation.Parameters[1]);
            Assert.AreEqual(Single.PositiveInfinity, (Single)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(8, (Int32)returnedMethodInvocation.Parameters[3]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);
        }

        #endregion
        
        #region Double Parameter Tests

        [Test]
        public void SerializeDoubleParameterSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>double</DataType><Data>-1.6976931348623213e-308</Data></Parameter><Parameter><DataType>double</DataType><Data>1.6976931348623213e+308</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)9, (double)(-1.6976931348623214159265E-308D), (double)1.6976931348623214159265E+308D, (Int32)8 }, typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeDoubleParameterSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>double</DataType><Data>-1.6976931348623213e-308</Data></Parameter><Parameter><DataType>double</DataType><Data>1.6976931348623213e+308</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(4, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(9, (Int32)returnedMethodInvocation.Parameters[0]);
            Assert.AreEqual(-1.6976931348623214159265E-308D, (double)returnedMethodInvocation.Parameters[1]);
            Assert.AreEqual(1.6976931348623214159265E+308D, (double)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(8, (Int32)returnedMethodInvocation.Parameters[3]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);

            // Test again with values serialized in Java format
            serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>double</DataType><Data>-1.6976931348623213E-308</Data></Parameter><Parameter><DataType>double</DataType><Data>1.6976931348623213E308</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";
            returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(4, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(9, (Int32)returnedMethodInvocation.Parameters[0]);
            Assert.AreEqual(-1.6976931348623214159265E-308D, (double)returnedMethodInvocation.Parameters[1]);
            Assert.AreEqual(1.6976931348623214159265E+308D, (double)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(8, (Int32)returnedMethodInvocation.Parameters[3]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);
        }

        [Test]
        public void SerializeInfinityDoubleParameterSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>double</DataType><Data>-Infinity</Data></Parameter><Parameter><DataType>double</DataType><Data>Infinity</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)9, Double.NegativeInfinity, Double.PositiveInfinity, (Int32)8 }, typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeInfinityDoubleParameterSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>double</DataType><Data>-Infinity</Data></Parameter><Parameter><DataType>double</DataType><Data>Infinity</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(4, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(9, (Int32)returnedMethodInvocation.Parameters[0]);
            Assert.AreEqual(Double.NegativeInfinity, (double)returnedMethodInvocation.Parameters[1]);
            Assert.AreEqual(Double.PositiveInfinity, (double)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(8, (Int32)returnedMethodInvocation.Parameters[3]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);
        }

        #endregion

        #region Char Parameter Tests

        [Test]
        public void SerializeCharParameterSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>char</DataType><Data>A</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)9, (Char)'A', (Int32)8 }, typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeCharParameterSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>char</DataType><Data>A</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(3, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(9, (Int32)returnedMethodInvocation.Parameters[0]);
            Assert.AreEqual('A', (Char)returnedMethodInvocation.Parameters[1]);
            Assert.AreEqual(8, (Int32)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);
        }

        #endregion

        #region Boolean Parameter Tests

        [Test]
        public void SerializeBooleanParameterSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>bool</DataType><Data>true</Data></Parameter><Parameter><DataType>bool</DataType><Data>false</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)9, true, false, (Int32)8 }, typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeBooleanParameterSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>bool</DataType><Data>true</Data></Parameter><Parameter><DataType>bool</DataType><Data>false</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(4, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(9, (Int32)returnedMethodInvocation.Parameters[0]);
            Assert.AreEqual(true, (bool)returnedMethodInvocation.Parameters[1]);
            Assert.AreEqual(false, (bool)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(8, (Int32)returnedMethodInvocation.Parameters[3]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);
        }

        #endregion

        #region Decimal Parameter Tests

        [Test]
        public void SerializeDecimalParameterSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>decimal</DataType><Data>-79228162514264337593543950335</Data></Parameter><Parameter><DataType>decimal</DataType><Data>79228162514264337593543950335</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)9, (Decimal)(-79228162514264337593543950335m), (Decimal)79228162514264337593543950335m, (Int32)8 }, typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeDecimalParameterSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>decimal</DataType><Data>-79228162514264337593543950335</Data></Parameter><Parameter><DataType>decimal</DataType><Data>79228162514264337593543950335</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(4, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(9, (Int32)returnedMethodInvocation.Parameters[0]);
            Assert.AreEqual(-79228162514264337593543950335m, (Decimal)returnedMethodInvocation.Parameters[1]);
            Assert.AreEqual(79228162514264337593543950335m, (Decimal)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(8, (Int32)returnedMethodInvocation.Parameters[3]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);
        }

        #endregion

        #region DateTime Parameter Tests

        [Test]
        public void SerializeDateTimeParameterSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>dateTime</DataType><Data>2013-04-05T06:07:08.009</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            DateTime dataTimeParameter = DateTime.ParseExact("2013-04-05T06:07:08.009", "yyyy-MM-ddTHH:mm:ss.FFF", System.Globalization.CultureInfo.InvariantCulture);
            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)9, dataTimeParameter, (Int32)8 }, typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);

            // Test again with 24 hour hour
            expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>dateTime</DataType><Data>2013-04-05T23:07:08.009</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            dataTimeParameter = DateTime.ParseExact("2013-04-05T23:07:08.009", "yyyy-MM-ddTHH:mm:ss.FFF", System.Globalization.CultureInfo.InvariantCulture);
            actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)9, dataTimeParameter, (Int32)8 }, typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeDateTimeParameterSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>dateTime</DataType><Data>2013-04-05T06:07:08.009</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(3, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(9, (Int32)returnedMethodInvocation.Parameters[0]);
            DateTime dataTimeParameter = (DateTime)returnedMethodInvocation.Parameters[1];
            Assert.AreEqual(2013, dataTimeParameter.Year);
            Assert.AreEqual(4, dataTimeParameter.Month);
            Assert.AreEqual(5, dataTimeParameter.Day);
            Assert.AreEqual(6, dataTimeParameter.Hour);
            Assert.AreEqual(7, dataTimeParameter.Minute);
            Assert.AreEqual(8, dataTimeParameter.Second);
            Assert.AreEqual(9, dataTimeParameter.Millisecond);
            Assert.AreEqual(8, (Int32)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);

            // Test again with 24 hour hour
            serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>dateTime</DataType><Data>2013-04-05T23:07:08.009</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";
            returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(3, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(9, (Int32)returnedMethodInvocation.Parameters[0]);
            dataTimeParameter = (DateTime)returnedMethodInvocation.Parameters[1];
            Assert.AreEqual(2013, dataTimeParameter.Year);
            Assert.AreEqual(4, dataTimeParameter.Month);
            Assert.AreEqual(5, dataTimeParameter.Day);
            Assert.AreEqual(23, dataTimeParameter.Hour);
            Assert.AreEqual(7, dataTimeParameter.Minute);
            Assert.AreEqual(8, dataTimeParameter.Second);
            Assert.AreEqual(9, dataTimeParameter.Millisecond);
            Assert.AreEqual(8, (Int32)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);
        }

        [Test]
        public void SerializeZeroMillisecondDateTimeParameterSuccessTests()
        {
            // For DateTimes with a 0 millisecond component, the ".000" is added explicitly to the string
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>dateTime</DataType><Data>2013-04-05T06:07:08.000</Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            DateTime dataTimeParameter = DateTime.ParseExact("2013-04-05T06:07:08.000", "yyyy-MM-ddTHH:mm:ss.FFF", System.Globalization.CultureInfo.InvariantCulture);
            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)9, dataTimeParameter, (Int32)8 }, typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        #endregion

        #region Multi Parameter Tests

        [Test]
        public void SerializeMultiParameterSuccessTests()
        {
            String expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>MethodWithAllDataTypesAsParameters</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter><DataType>dateTimeArray</DataType><Data><ElementDataType>dateTime</ElementDataType><Element><DataType>dateTime</DataType><Data>0001-01-01T00:00:00.000</Data></Element><Element><DataType>dateTime</DataType><Data>9999-12-31T23:59:59.999</Data></Element></Data></Parameter><Parameter><DataType>string</DataType><Data>&lt;TestString&gt;This is a test string &lt;&gt;?/:\";''[]{}+=_-)(*&amp;^%$#@!|\\&lt;/TestString&gt;</Data></Parameter><Parameter><DataType>decimalArray</DataType><Data><ElementDataType>decimal</ElementDataType><Element><DataType>decimal</DataType><Data>-79228162514264337593543950335</Data></Element><Element><DataType>decimal</DataType><Data>79228162514264337593543950335</Data></Element></Data></Parameter><Parameter><DataType>signedByte</DataType><Data>8</Data></Parameter><Parameter><DataType>boolArray</DataType><Data><ElementDataType>bool</ElementDataType><Element><DataType>bool</DataType><Data>false</Data></Element><Element><DataType>bool</DataType><Data>true</Data></Element></Data></Parameter><Parameter><DataType>shortInteger</DataType><Data>-16343</Data></Parameter><Parameter><DataType>charArray</DataType><Data><ElementDataType>char</ElementDataType><Element><DataType>char</DataType><Data>M</Data></Element><Element><DataType>char</DataType><Data>&lt;</Data></Element></Data></Parameter><Parameter><DataType>longInteger</DataType><Data>76543</Data></Parameter><Parameter><DataType>doubleArray</DataType><Data><ElementDataType>double</ElementDataType><Element><DataType>double</DataType><Data>-1.6976931348623213e-308</Data></Element><Element><DataType>double</DataType><Data>1.6976931348623213e+308</Data></Element></Data></Parameter><Parameter><DataType>float</DataType><Data>-Infinity</Data></Parameter><Parameter><DataType>floatArray</DataType><Data><ElementDataType>float</ElementDataType><Element><DataType>float</DataType><Data>-3.14159261e-038</Data></Element><Element><DataType>float</DataType><Data>3.14159272e+038</Data></Element></Data></Parameter><Parameter><DataType>double</DataType><Data>Infinity</Data></Parameter><Parameter><DataType>longIntegerArray</DataType><Data><ElementDataType>longInteger</ElementDataType><Element><DataType>longInteger</DataType><Data>-9223372036854775808</Data></Element><Element><DataType>longInteger</DataType><Data>9223372036854775807</Data></Element></Data></Parameter><Parameter><DataType>char</DataType><Data>!</Data></Parameter><Parameter><DataType>shortIntegerArray</DataType><Data><ElementDataType>shortInteger</ElementDataType><Element><DataType>shortInteger</DataType><Data>-32768</Data></Element><Element><DataType>shortInteger</DataType><Data>32767</Data></Element></Data></Parameter><Parameter><DataType>bool</DataType><Data>true</Data></Parameter><Parameter><DataType>signedByteArray</DataType><Data><ElementDataType>signedByte</ElementDataType><Element><DataType>signedByte</DataType><Data>-128</Data></Element><Element><DataType>signedByte</DataType><Data>127</Data></Element></Data></Parameter><Parameter><DataType>decimal</DataType><Data>40958609456.39898479845</Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus in nulla magna, non cursus elit. Vivamus velit dolor, aliquet a.</Data></Element><Element><DataType>string</DataType><Data><Empty /></Data></Element></Data></Parameter><Parameter><DataType>dateTime</DataType><Data>2013-05-01T12:43:56.654</Data></Parameter><Parameter><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType><Element><DataType>integer</DataType><Data>-2147483648</Data></Element><Element><DataType>integer</DataType><Data>2147483647</Data></Element></Data></Parameter><Parameter><DataType>decimalArray</DataType><Data><ElementDataType>decimal</ElementDataType></Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut aliquet quam sit amet massa tincidunt a interdum purus interdum. Cras.</Data></Element><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc vitae velit tellus, non interdum urna. Quisque vehicula augue eu mauris.</Data></Element><Element /></Data></Parameter><Parameter /></Parameters><ReturnType><DataType>dateTime</DataType></ReturnType></MethodInvocation>";

            Object[] parameters = new Object[25];
            // Integer parameter
            parameters[0] = (Int32)123;
            // DateTime array parameter
            DateTime[] dateTimeArrayParameter = new DateTime[2];
            DateTime minDate = DateTime.ParseExact("0001-01-01T00:00:00.000", "yyyy-MM-ddTHH:mm:ss.FFF", System.Globalization.CultureInfo.InvariantCulture);
            dateTimeArrayParameter[0] = minDate;
            DateTime maxDate = DateTime.ParseExact("9999-12-31T23:59:59.999", "yyyy-MM-ddTHH:mm:ss.FFF", System.Globalization.CultureInfo.InvariantCulture);
            dateTimeArrayParameter[1] = maxDate;
            parameters[1] = dateTimeArrayParameter;
            // String parameter
            parameters[2] = "<TestString>This is a test string <>?/:\";''[]{}+=_-)(*&^%$#@!|\\</TestString>";
            // Decimal array parameter
            decimal[] decimalArrayParameter = new decimal[2];
            decimalArrayParameter[0] = -79228162514264337593543950335m;
            decimalArrayParameter[1] = 79228162514264337593543950335m;
            parameters[3] = decimalArrayParameter;
            // Sbyte parameter
            parameters[4] = (sbyte)8;
            // Boolean array parameter
            Boolean[] booleanArrayParameter = new Boolean[2];
            booleanArrayParameter[0] = false;
            booleanArrayParameter[1] = true;
            parameters[5] = booleanArrayParameter;
            // Short parameter
            parameters[6] = (short)-16343;
            // Char array parameter
            Char[] charArrayParameter = new Char[2];
            charArrayParameter[0] = 'M';
            charArrayParameter[1] = '<';
            parameters[7] = charArrayParameter;
            // Long parameter
            parameters[8] = (long)76543;
            // Double array parameter
            Double[] doubleArrayParameter = new Double[2];
            doubleArrayParameter[0] = -1.6976931348623214159265E-308D;
            doubleArrayParameter[1] = 1.6976931348623214159265E+308D;
            parameters[9] = doubleArrayParameter;
            // Float parameter
            parameters[10] = float.NegativeInfinity;
            // Float array parameter
            float[] floatArrayParameter = new float[2];
            floatArrayParameter[0] = -3.14159265358979323846264338327E-38F;
            floatArrayParameter[1] = 3.14159265358979323846264338327E+38F;
            parameters[11] = floatArrayParameter;
            // Double parameter
            parameters[12] = Double.PositiveInfinity;
            // Long array parameter
            long[] longArrayParameter = new long[2];
            longArrayParameter[0] = -9223372036854775808L;
            longArrayParameter[1] = 9223372036854775807L;
            parameters[13] = longArrayParameter;
            // Character parameter
            parameters[14] = (char)'!';
            // Short array parameter
            short[] shortArrayParameter = new short[2];
            shortArrayParameter[0] = -32768;
            shortArrayParameter[1] = 32767;
            parameters[15] = shortArrayParameter;
            // Boolean parameter
            parameters[16] = true;
            // Syte array parameter
            sbyte[] sbyteArrayParameter = new sbyte[2];
            sbyteArrayParameter[0] = -128;
            sbyteArrayParameter[1] = 127;
            parameters[17] = sbyteArrayParameter;
            // Decimal parameter
            parameters[18] = (decimal)40958609456.39898479845m;
            // String array parameter
            String[] stringArrayParameter = new String[2];
            stringArrayParameter[0] = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus in nulla magna, non cursus elit. Vivamus velit dolor, aliquet a.";
            stringArrayParameter[1] = "";
            parameters[19] = stringArrayParameter;
            // DateTime parameter
            parameters[20] = DateTime.ParseExact("2013-05-01T12:43:56.654", "yyyy-MM-ddTHH:mm:ss.FFF", System.Globalization.CultureInfo.InvariantCulture);
            // Integer array parameter
            Int32[] integerParameter = new Int32[2];
            integerParameter[0] = -2147483648;
            integerParameter[1] = 2147483647;
            parameters[21] = integerParameter;
            // Empty array
            parameters[22] = new decimal[0];
            // Array with null parameter
            String[] nullStringArrayParameter = new String[3];
            nullStringArrayParameter[0] = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut aliquet quam sit amet massa tincidunt a interdum purus interdum. Cras.";
            nullStringArrayParameter[1] = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc vitae velit tellus, non interdum urna. Quisque vehicula augue eu mauris.";
            nullStringArrayParameter[2] = null;
            parameters[23] = nullStringArrayParameter;
            // Null parameter
            parameters[24] = null;
            String actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("MethodWithAllDataTypesAsParameters", parameters , typeof(DateTime)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeMultiParameterSuccessTests()
        {
            String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>MethodWithAllDataTypesAsParameters</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter><DataType>dateTimeArray</DataType><Data><ElementDataType>dateTime</ElementDataType><Element><DataType>dateTime</DataType><Data>0001-01-01T00:00:00.000</Data></Element><Element><DataType>dateTime</DataType><Data>9999-12-31T23:59:59.999</Data></Element></Data></Parameter><Parameter><DataType>string</DataType><Data>&lt;TestString&gt;This is a test string &lt;&gt;?/:\";''[]{}+=_-)(*&amp;^%$#@!|\\&lt;/TestString&gt;</Data></Parameter><Parameter><DataType>decimalArray</DataType><Data><ElementDataType>decimal</ElementDataType><Element><DataType>decimal</DataType><Data>-79228162514264337593543950335</Data></Element><Element><DataType>decimal</DataType><Data>79228162514264337593543950335</Data></Element></Data></Parameter><Parameter><DataType>signedByte</DataType><Data>8</Data></Parameter><Parameter><DataType>boolArray</DataType><Data><ElementDataType>bool</ElementDataType><Element><DataType>bool</DataType><Data>false</Data></Element><Element><DataType>bool</DataType><Data>true</Data></Element></Data></Parameter><Parameter><DataType>shortInteger</DataType><Data>-16343</Data></Parameter><Parameter><DataType>charArray</DataType><Data><ElementDataType>char</ElementDataType><Element><DataType>char</DataType><Data>M</Data></Element><Element><DataType>char</DataType><Data>&lt;</Data></Element></Data></Parameter><Parameter><DataType>longInteger</DataType><Data>76543</Data></Parameter><Parameter><DataType>doubleArray</DataType><Data><ElementDataType>double</ElementDataType><Element><DataType>double</DataType><Data>-1.6976931348623213e-308</Data></Element><Element><DataType>double</DataType><Data>1.6976931348623213e+308</Data></Element></Data></Parameter><Parameter><DataType>float</DataType><Data>-Infinity</Data></Parameter><Parameter><DataType>floatArray</DataType><Data><ElementDataType>float</ElementDataType><Element><DataType>float</DataType><Data>-3.14159261e-038</Data></Element><Element><DataType>float</DataType><Data>3.14159272e+038</Data></Element></Data></Parameter><Parameter><DataType>double</DataType><Data>Infinity</Data></Parameter><Parameter><DataType>longIntegerArray</DataType><Data><ElementDataType>longInteger</ElementDataType><Element><DataType>longInteger</DataType><Data>-9223372036854775808</Data></Element><Element><DataType>longInteger</DataType><Data>9223372036854775807</Data></Element></Data></Parameter><Parameter><DataType>char</DataType><Data>!</Data></Parameter><Parameter><DataType>shortIntegerArray</DataType><Data><ElementDataType>shortInteger</ElementDataType><Element><DataType>shortInteger</DataType><Data>-32768</Data></Element><Element><DataType>shortInteger</DataType><Data>32767</Data></Element></Data></Parameter><Parameter><DataType>bool</DataType><Data>true</Data></Parameter><Parameter><DataType>signedByteArray</DataType><Data><ElementDataType>signedByte</ElementDataType><Element><DataType>signedByte</DataType><Data>-128</Data></Element><Element><DataType>signedByte</DataType><Data>127</Data></Element></Data></Parameter><Parameter><DataType>decimal</DataType><Data>40958609456.39898479845</Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus in nulla magna, non cursus elit. Vivamus velit dolor, aliquet a.</Data></Element><Element><DataType>string</DataType><Data><Empty /></Data></Element></Data></Parameter><Parameter><DataType>dateTime</DataType><Data>2013-05-01T12:43:56.654</Data></Parameter><Parameter><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType><Element><DataType>integer</DataType><Data>-2147483648</Data></Element><Element><DataType>integer</DataType><Data>2147483647</Data></Element></Data></Parameter><Parameter><DataType>decimalArray</DataType><Data><ElementDataType>decimal</ElementDataType></Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut aliquet quam sit amet massa tincidunt a interdum purus interdum. Cras.</Data></Element><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc vitae velit tellus, non interdum urna. Quisque vehicula augue eu mauris.</Data></Element><Element /></Data></Parameter><Parameter /></Parameters><ReturnType><DataType>dateTime</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("MethodWithAllDataTypesAsParameters", returnedMethodInvocation.Name);
            Object[] parameters = returnedMethodInvocation.Parameters;
            Assert.AreEqual(25, parameters.Length);

            // Test parameters
            // Integer parameter
            Assert.AreEqual((int)123, (int)parameters[0]);
            // DateTime array parameter
            DateTime[] dateTimeArrayParameter = (DateTime[])parameters[1];
            DateTime minDate = DateTime.ParseExact("0001-01-01T00:00:00.000", "yyyy-MM-ddTHH:mm:ss.FFF", System.Globalization.CultureInfo.InvariantCulture);
            DateTime maxDate = DateTime.ParseExact("9999-12-31T23:59:59.999", "yyyy-MM-ddTHH:mm:ss.FFF", System.Globalization.CultureInfo.InvariantCulture);
            Assert.AreEqual(minDate, dateTimeArrayParameter[0]);
            Assert.AreEqual(maxDate, dateTimeArrayParameter[1]);
            // String parameter
            Assert.AreEqual("<TestString>This is a test string <>?/:\";''[]{}+=_-)(*&^%$#@!|\\</TestString>", (String)parameters[2]);
            // Decimal array parameter
            Decimal[] decimalArrayParameter = (Decimal[])parameters[3];
            Assert.AreEqual(-79228162514264337593543950335d, decimalArrayParameter[0]);
            Assert.AreEqual(79228162514264337593543950335d, decimalArrayParameter[1]);
            // Sbyte parameter
            Assert.AreEqual((sbyte)8, (sbyte)parameters[4]);
            // Boolean array parameter
            Boolean[] booleanArrayParameter = (Boolean[])parameters[5];
            Assert.AreEqual(false, booleanArrayParameter[0]);
            Assert.AreEqual(true, booleanArrayParameter[1]);
            // Short parameter
            Assert.AreEqual((short)-16343, (short)parameters[6]);
            // Char array parameter
            Char[] charArrayParameter = (Char[])parameters[7];
            Assert.AreEqual('M', (char)charArrayParameter[0]);
            Assert.AreEqual('<', (char)charArrayParameter[1]);
            // Long parameter
            Assert.AreEqual((long)76543, (long)parameters[8]);
            // Double array parameter
            Double[] doubleArrayParameter = (Double[])parameters[9];
            Assert.AreEqual(-1.6976931348623214159265E-308D, (double)doubleArrayParameter[0]);
            Assert.AreEqual(1.6976931348623214159265E+308D, (double)doubleArrayParameter[1]);
            // Float parameter
            Assert.AreEqual(float.NegativeInfinity, (float)parameters[10]);
            // Float array parameter
            float[] floatArrayParameter = (float[])parameters[11];
            floatArrayParameter[0] = -3.14159265358979323846264338327E-38F;
            floatArrayParameter[1] = 3.14159265358979323846264338327E+38F;
            Assert.AreEqual(-3.14159265358979323846264338327E-38F, (float)floatArrayParameter[0]);
            Assert.AreEqual(3.14159265358979323846264338327E+38F, (float)floatArrayParameter[1]);
            // Double parameter
            Assert.AreEqual(Double.PositiveInfinity, (double)parameters[12]);
            // Long array parameter
            long[] longArrayParameter = (long[])parameters[13];
            Assert.AreEqual(-9223372036854775808L, (long)longArrayParameter[0]);
            Assert.AreEqual(9223372036854775807L, (long)longArrayParameter[1]);
            // Character parameter
            Assert.AreEqual('!', (char)parameters[14]);
            // Short array parameter
            short[] shortArrayParameter = (short[])parameters[15];
            Assert.AreEqual(-32768, (short)shortArrayParameter[0]);
            Assert.AreEqual(32767, (short)shortArrayParameter[1]);
            // Boolean parameter
            Assert.AreEqual(true, (bool)parameters[16]);
            // Sbyte array parameter
            sbyte[] sbyteArrayParameter = (sbyte[])parameters[17];
            Assert.AreEqual(-128, (sbyte)sbyteArrayParameter[0]);
            Assert.AreEqual(127, (sbyte)sbyteArrayParameter[1]);
            // Decimal parameter
            Assert.AreEqual(40958609456.39898479845d, (decimal)parameters[18]);
            // String array parameter
            String[] stringArrayParameter = (String[])parameters[19];
            Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus in nulla magna, non cursus elit. Vivamus velit dolor, aliquet a.", (String)stringArrayParameter[0]);
            Assert.AreEqual("", (String)stringArrayParameter[1]);
            // DateTime parameter
            DateTime expectedDateParameter = DateTime.ParseExact("2013-05-01T12:43:56.654", "yyyy-MM-ddTHH:mm:ss.FFF", System.Globalization.CultureInfo.InvariantCulture);
            Assert.AreEqual(expectedDateParameter, (DateTime)parameters[20]);
            // Integer array parameter
            Int32[] integerParameter = (Int32[])parameters[21];
            Assert.AreEqual(-2147483648, (Int32)integerParameter[0]);
            Assert.AreEqual(2147483647, (Int32)integerParameter[1]);
            // Empty array
            Decimal[] decimalParameter = (Decimal[])parameters[22];
            Assert.AreEqual(0, decimalParameter.Length);
            // Array with null parameter
            String[] nullStringArrayParameter = (String[])parameters[23];
            Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut aliquet quam sit amet massa tincidunt a interdum purus interdum. Cras.", (String)nullStringArrayParameter[0]);
            Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc vitae velit tellus, non interdum urna. Quisque vehicula augue eu mauris.", (String)nullStringArrayParameter[1]);
            Assert.IsNull((String)nullStringArrayParameter[2]);
            // Null parameter
            Assert.IsNull(parameters[24]);
         
            // Test return type
            Assert.AreEqual(typeof(DateTime), returnedMethodInvocation.ReturnType);


            // Test again with Java version of the same method invocation
            serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>MethodWithAllDataTypesAsParameters</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter><DataType>dateTimeArray</DataType><Data><ElementDataType>dateTime</ElementDataType><Element><DataType>dateTime</DataType><Data>0001-01-01T00:00:00.000</Data></Element><Element><DataType>dateTime</DataType><Data>9999-12-31T23:59:59.999</Data></Element></Data></Parameter><Parameter><DataType>string</DataType><Data>&lt;TestString&gt;This is a test string &lt;&gt;?/:\";''[]{}+=_-)(*&amp;^%$#@!|\\&lt;/TestString&gt;</Data></Parameter><Parameter><DataType>decimalArray</DataType><Data><ElementDataType>decimal</ElementDataType><Element><DataType>decimal</DataType><Data>-79228162514264337593543950335</Data></Element><Element><DataType>decimal</DataType><Data>79228162514264337593543950335</Data></Element></Data></Parameter><Parameter><DataType>signedByte</DataType><Data>8</Data></Parameter><Parameter><DataType>boolArray</DataType><Data><ElementDataType>bool</ElementDataType><Element><DataType>bool</DataType><Data>false</Data></Element><Element><DataType>bool</DataType><Data>true</Data></Element></Data></Parameter><Parameter><DataType>shortInteger</DataType><Data>-16343</Data></Parameter><Parameter><DataType>charArray</DataType><Data><ElementDataType>char</ElementDataType><Element><DataType>char</DataType><Data>M</Data></Element><Element><DataType>char</DataType><Data>&lt;</Data></Element></Data></Parameter><Parameter><DataType>longInteger</DataType><Data>76543</Data></Parameter><Parameter><DataType>doubleArray</DataType><Data><ElementDataType>double</ElementDataType><Element><DataType>double</DataType><Data>-1.6976931348623213E-308</Data></Element><Element><DataType>double</DataType><Data>1.6976931348623213E308</Data></Element></Data></Parameter><Parameter><DataType>float</DataType><Data>-Infinity</Data></Parameter><Parameter><DataType>floatArray</DataType><Data><ElementDataType>float</ElementDataType><Element><DataType>float</DataType><Data>-3.14159261E-38</Data></Element><Element><DataType>float</DataType><Data>3.14159272E38</Data></Element></Data></Parameter><Parameter><DataType>double</DataType><Data>Infinity</Data></Parameter><Parameter><DataType>longIntegerArray</DataType><Data><ElementDataType>longInteger</ElementDataType><Element><DataType>longInteger</DataType><Data>-9223372036854775808</Data></Element><Element><DataType>longInteger</DataType><Data>9223372036854775807</Data></Element></Data></Parameter><Parameter><DataType>char</DataType><Data>!</Data></Parameter><Parameter><DataType>shortIntegerArray</DataType><Data><ElementDataType>shortInteger</ElementDataType><Element><DataType>shortInteger</DataType><Data>-32768</Data></Element><Element><DataType>shortInteger</DataType><Data>32767</Data></Element></Data></Parameter><Parameter><DataType>bool</DataType><Data>true</Data></Parameter><Parameter><DataType>signedByteArray</DataType><Data><ElementDataType>signedByte</ElementDataType><Element><DataType>signedByte</DataType><Data>-128</Data></Element><Element><DataType>signedByte</DataType><Data>127</Data></Element></Data></Parameter><Parameter><DataType>decimal</DataType><Data>40958609456.39898479845</Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus in nulla magna, non cursus elit. Vivamus velit dolor, aliquet a.</Data></Element><Element><DataType>string</DataType><Data><Empty></Empty></Data></Element></Data></Parameter><Parameter><DataType>dateTime</DataType><Data>2013-05-01T12:43:56.654</Data></Parameter><Parameter><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType><Element><DataType>integer</DataType><Data>-2147483648</Data></Element><Element><DataType>integer</DataType><Data>2147483647</Data></Element></Data></Parameter><Parameter><DataType>decimalArray</DataType><Data><ElementDataType>decimal</ElementDataType></Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut aliquet quam sit amet massa tincidunt a interdum purus interdum. Cras.</Data></Element><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc vitae velit tellus, non interdum urna. Quisque vehicula augue eu mauris.</Data></Element><Element></Element></Data></Parameter><Parameter></Parameter></Parameters><ReturnType><DataType>dateTime</DataType></ReturnType></MethodInvocation>";

            returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("MethodWithAllDataTypesAsParameters", returnedMethodInvocation.Name);
            parameters = returnedMethodInvocation.Parameters;
            Assert.AreEqual(25, parameters.Length);

            // Test parameters
            // Integer parameter
            Assert.AreEqual((int)123, (int)parameters[0]);
            // DateTime array parameter
            dateTimeArrayParameter = (DateTime[])parameters[1];
            minDate = DateTime.ParseExact("0001-01-01T00:00:00.000", "yyyy-MM-ddTHH:mm:ss.FFF", System.Globalization.CultureInfo.InvariantCulture);
            maxDate = DateTime.ParseExact("9999-12-31T23:59:59.999", "yyyy-MM-ddTHH:mm:ss.FFF", System.Globalization.CultureInfo.InvariantCulture);
            Assert.AreEqual(minDate, dateTimeArrayParameter[0]);
            Assert.AreEqual(maxDate, dateTimeArrayParameter[1]);
            // String parameter
            Assert.AreEqual("<TestString>This is a test string <>?/:\";''[]{}+=_-)(*&^%$#@!|\\</TestString>", (String)parameters[2]);
            // Decimal array parameter
            decimalArrayParameter = (Decimal[])parameters[3];
            Assert.AreEqual(-79228162514264337593543950335d, decimalArrayParameter[0]);
            Assert.AreEqual(79228162514264337593543950335d, decimalArrayParameter[1]);
            // Sbyte parameter
            Assert.AreEqual((sbyte)8, (sbyte)parameters[4]);
            // Boolean array parameter
            booleanArrayParameter = (Boolean[])parameters[5];
            Assert.AreEqual(false, booleanArrayParameter[0]);
            Assert.AreEqual(true, booleanArrayParameter[1]);
            // Short parameter
            Assert.AreEqual((short)-16343, (short)parameters[6]);
            // Char array parameter
            charArrayParameter = (Char[])parameters[7];
            Assert.AreEqual('M', (char)charArrayParameter[0]);
            Assert.AreEqual('<', (char)charArrayParameter[1]);
            // Long parameter
            Assert.AreEqual((long)76543, (long)parameters[8]);
            // Double array parameter
            doubleArrayParameter = (Double[])parameters[9];
            Assert.AreEqual(-1.6976931348623214159265E-308D, (double)doubleArrayParameter[0]);
            Assert.AreEqual(1.6976931348623214159265E+308D, (double)doubleArrayParameter[1]);
            // Float parameter
            Assert.AreEqual(float.NegativeInfinity, (float)parameters[10]);
            // Float array parameter
            floatArrayParameter = (float[])parameters[11];
            floatArrayParameter[0] = -3.14159265358979323846264338327E-38F;
            floatArrayParameter[1] = 3.14159265358979323846264338327E+38F;
            Assert.AreEqual(-3.14159265358979323846264338327E-38F, (float)floatArrayParameter[0]);
            Assert.AreEqual(3.14159265358979323846264338327E+38F, (float)floatArrayParameter[1]);
            // Double parameter
            Assert.AreEqual(Double.PositiveInfinity, (double)parameters[12]);
            // Long array parameter
            longArrayParameter = (long[])parameters[13];
            Assert.AreEqual(-9223372036854775808L, (long)longArrayParameter[0]);
            Assert.AreEqual(9223372036854775807L, (long)longArrayParameter[1]);
            // Character parameter
            Assert.AreEqual('!', (char)parameters[14]);
            // Short array parameter
            shortArrayParameter = (short[])parameters[15];
            Assert.AreEqual(-32768, (short)shortArrayParameter[0]);
            Assert.AreEqual(32767, (short)shortArrayParameter[1]);
            // Boolean parameter
            Assert.AreEqual(true, (bool)parameters[16]);
            // Sbyte array parameter
            sbyteArrayParameter = (sbyte[])parameters[17];
            Assert.AreEqual(-128, (sbyte)sbyteArrayParameter[0]);
            Assert.AreEqual(127, (sbyte)sbyteArrayParameter[1]);
            // Decimal parameter
            Assert.AreEqual(40958609456.39898479845d, (decimal)parameters[18]);
            // String array parameter
            stringArrayParameter = (String[])parameters[19];
            Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus in nulla magna, non cursus elit. Vivamus velit dolor, aliquet a.", (String)stringArrayParameter[0]);
            Assert.AreEqual("", (String)stringArrayParameter[1]);
            // DateTime parameter
            expectedDateParameter = DateTime.ParseExact("2013-05-01T12:43:56.654", "yyyy-MM-ddTHH:mm:ss.FFF", System.Globalization.CultureInfo.InvariantCulture);
            Assert.AreEqual(expectedDateParameter, (DateTime)parameters[20]);
            // Integer array parameter
            integerParameter = (Int32[])parameters[21];
            Assert.AreEqual(-2147483648, (Int32)integerParameter[0]);
            Assert.AreEqual(2147483647, (Int32)integerParameter[1]);
            // Empty array
            decimalParameter = (Decimal[])parameters[22];
            Assert.AreEqual(0, decimalParameter.Length);
            // Array with null parameter
            nullStringArrayParameter = (String[])parameters[23];
            Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut aliquet quam sit amet massa tincidunt a interdum purus interdum. Cras.", (String)nullStringArrayParameter[0]);
            Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc vitae velit tellus, non interdum urna. Quisque vehicula augue eu mauris.", (String)nullStringArrayParameter[1]);
            Assert.IsNull((String)nullStringArrayParameter[2]);
            // Null parameter
            Assert.IsNull(parameters[24]);

            // Test return type
            Assert.AreEqual(typeof(DateTime), returnedMethodInvocation.ReturnType);
        }

        #endregion

        #region IXmlSerializable Parameter Tests

        [Test]
        public void SerializeIXmlSerializableParameterSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>987</Data></Parameter><Parameter><DataType>interestRateCurve</DataType><Data><DataType>interestRateCurve</DataType><interestRateCurve><Currency>AUD</Currency><Curve><Point><Term>1</Term><Rate>0.0437</Rate></Point><Point><Term>3</Term><Rate>0.0436</Rate></Point><Point><Term>6</Term><Rate>0.0433</Rate></Point><Point><Term>9</Term><Rate>0.0406</Rate></Point><Point><Term>12</Term><Rate>0.0378</Rate></Point></Curve></interestRateCurve></Data></Parameter><Parameter><DataType>integer</DataType><Data>654</Data></Parameter></Parameters><ReturnType><DataType>interestRateCurve</DataType></ReturnType></MethodInvocation>";

            testMethodInvocationSerializer.AddIXmlSerializableOperations(typeof(InterestRateCurve), "interestRateCurve");
            InterestRateCurve testInterestRateCurve = new InterestRateCurve();
            testInterestRateCurve.Currency = "AUD";
            testInterestRateCurve.AddPoint(1, 0.0437);
            testInterestRateCurve.AddPoint(3, 0.0436);
            testInterestRateCurve.AddPoint(6, 0.0433);
            testInterestRateCurve.AddPoint(9, 0.0406);
            testInterestRateCurve.AddPoint(12, 0.0378);
            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)987, testInterestRateCurve, (Int32)(654) }, typeof(InterestRateCurve)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeIXmlSerializableParameterSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>987</Data></Parameter><Parameter><DataType>interestRateCurve</DataType><Data><DataType>interestRateCurve</DataType><interestRateCurve><Currency>AUD</Currency><Curve><Point><Term>1</Term><Rate>0.0437</Rate></Point><Point><Term>3</Term><Rate>0.0436</Rate></Point><Point><Term>6</Term><Rate>0.0433</Rate></Point><Point><Term>9</Term><Rate>0.0406</Rate></Point><Point><Term>12</Term><Rate>0.0378</Rate></Point></Curve></interestRateCurve></Data></Parameter><Parameter><DataType>integer</DataType><Data>654</Data></Parameter></Parameters><ReturnType><DataType>interestRateCurve</DataType></ReturnType></MethodInvocation>";

            testMethodInvocationSerializer.AddIXmlSerializableOperations(typeof(InterestRateCurve), "interestRateCurve");
            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(3, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(987, (Int32)returnedMethodInvocation.Parameters[0]);
            InterestRateCurve returnedInterestRateCurve = (InterestRateCurve)returnedMethodInvocation.Parameters[1];
            Assert.AreEqual("AUD", returnedInterestRateCurve.Currency);
            Assert.AreEqual(0.0437, returnedInterestRateCurve.Curve[1]);
            Assert.AreEqual(0.0436, returnedInterestRateCurve.Curve[3]);
            Assert.AreEqual(0.0433, returnedInterestRateCurve.Curve[6]);
            Assert.AreEqual(0.0406, returnedInterestRateCurve.Curve[9]);
            Assert.AreEqual(0.0378, returnedInterestRateCurve.Curve[12]);
            Assert.AreEqual(5, returnedInterestRateCurve.Curve.Count);
            Assert.AreEqual(654, (Int32)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(typeof(InterestRateCurve), returnedMethodInvocation.ReturnType);
        }

        [Test]
        public void SerializeIXmlSerializableReturnValueSuccessTests()
        {
            string expectedSerializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>interestRateCurve</DataType><Data><DataType>interestRateCurve</DataType><interestRateCurve><Currency>AUD</Currency><Curve><Point><Term>1</Term><Rate>0.0437</Rate></Point><Point><Term>3</Term><Rate>0.0436</Rate></Point><Point><Term>6</Term><Rate>0.0433</Rate></Point><Point><Term>9</Term><Rate>0.0406</Rate></Point><Point><Term>12</Term><Rate>0.0378</Rate></Point></Curve></interestRateCurve></Data></ReturnValue>";

            testMethodInvocationSerializer.AddIXmlSerializableOperations(typeof(InterestRateCurve), "interestRateCurve");
            InterestRateCurve testInterestRateCurve = new InterestRateCurve();
            testInterestRateCurve.Currency = "AUD";
            testInterestRateCurve.AddPoint(1, 0.0437);
            testInterestRateCurve.AddPoint(3, 0.0436);
            testInterestRateCurve.AddPoint(6, 0.0433);
            testInterestRateCurve.AddPoint(9, 0.0406);
            testInterestRateCurve.AddPoint(12, 0.0378);
            string actualSerializedReturnValue = testMethodInvocationSerializer.SerializeReturnValue(testInterestRateCurve);
            Assert.AreEqual(expectedSerializedReturnValue, actualSerializedReturnValue);
        }

        [Test]
        public void DeserializeIXmlSerializableReturnValueSuccessTests()
        {
            string serializedReturnValue = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReturnValue><DataType>interestRateCurve</DataType><Data><DataType>interestRateCurve</DataType><interestRateCurve><Currency>AUD</Currency><Curve><Point><Term>1</Term><Rate>0.0437</Rate></Point><Point><Term>3</Term><Rate>0.0436</Rate></Point><Point><Term>6</Term><Rate>0.0433</Rate></Point><Point><Term>9</Term><Rate>0.0406</Rate></Point><Point><Term>12</Term><Rate>0.0378</Rate></Point></Curve></interestRateCurve></Data></ReturnValue>";

            testMethodInvocationSerializer.AddIXmlSerializableOperations(typeof(InterestRateCurve), "interestRateCurve");
            InterestRateCurve returnValue = (InterestRateCurve)testMethodInvocationSerializer.DeserializeReturnValue(serializedReturnValue);
            Assert.AreEqual("AUD", returnValue.Currency);
            Assert.AreEqual(0.0437, returnValue.Curve[1]);
            Assert.AreEqual(0.0436, returnValue.Curve[3]);
            Assert.AreEqual(0.0433, returnValue.Curve[6]);
            Assert.AreEqual(0.0406, returnValue.Curve[9]);
            Assert.AreEqual(0.0378, returnValue.Curve[12]);
            Assert.AreEqual(5, returnValue.Curve.Count);
        }

        #endregion

        #region List Parameter Tests
        /* Support for generics will be enabled when supported on the Java side
        [Test]
        public void SerializeListParameterSuccessTests()
        {
            string expectedSerializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>integerList</DataType><Data><ElementDataType>integer</ElementDataType><Element><DataType>integer</DataType><Data>123</Data></Element><Element><DataType>integer</DataType><Data>-456</Data></Element></Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            List<Int32> listParameter = new List<Int32>();
            listParameter.Add(123);
            listParameter.Add(-456);
            string actualSerializedMethodInvocation = testMethodInvocationSerializer.Serialize(new MethodInvocation("TestMethod", new Object[] { (Int32)9, listParameter, (Int32)8 }, typeof(Int32)));
            Assert.AreEqual(expectedSerializedMethodInvocation, actualSerializedMethodInvocation);
        }

        [Test]
        public void DeserializeListParameterSuccessTests()
        {
            string serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>9</Data></Parameter><Parameter><DataType>integerList</DataType><Data><ElementDataType>integer</ElementDataType><Element><DataType>integer</DataType><Data>123</Data></Element><Element><DataType>integer</DataType><Data>-456</Data></Element></Data></Parameter><Parameter><DataType>integer</DataType><Data>8</Data></Parameter></Parameters><ReturnType><DataType>integer</DataType></ReturnType></MethodInvocation>";

            IMethodInvocation returnedMethodInvocation = testMethodInvocationSerializer.Deserialize(serializedMethodInvocation);
            Assert.AreEqual("TestMethod", returnedMethodInvocation.Name);
            Assert.AreEqual(3, returnedMethodInvocation.Parameters.Length);
            Assert.AreEqual(9, (Int32)returnedMethodInvocation.Parameters[0]);
            List<Int32> listParameter = (List<Int32>)returnedMethodInvocation.Parameters[1];
            Assert.AreEqual(123, listParameter[0]);
            Assert.AreEqual(-456, listParameter[1]);
            Assert.AreEqual(2, listParameter.Count);
            Assert.AreEqual(8, (Int32)returnedMethodInvocation.Parameters[2]);
            Assert.AreEqual(typeof(Int32), returnedMethodInvocation.ReturnType);
        }
        */
        #endregion

    }

    //******************************************************************************
    //
    // Class: InterestRateCurve
    //
    //******************************************************************************
    /// <summary>
    /// Represents an interest rate curve.  Used for testing serialization of classes that implement IXmlSerializable.
    /// </summary>
    public class InterestRateCurve : IXmlSerializable
    {
        private string currency;
        private Dictionary<int, double> curve;

        //------------------------------------------------------------------------------
        //
        // Method: InterestRateCurve (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the MethodInvocationRemotingUnitTests.InterestRateCurve class.
        /// </summary>
        public InterestRateCurve()
        {
            curve = new Dictionary<int, double>();
        }

        /// <summary>
        /// The currency of the interest rate curve.
        /// </summary>
        public string Currency
        {
            get
            {
                return currency;
            }
            set
            {
                currency = value;
            }
        }

        /// <summary>
        /// The interest rate curve.
        /// </summary>
        public Dictionary<int, double> Curve
        {
            get
            {
                return curve;
            }
        }

        /// <summary>
        /// Adds a data point to the curve.
        /// </summary>
        /// <param name="term">The term or time dimension of the data point in months.</param>
        /// <param name="rate">The interest rate of the data point.</param>
        public void AddPoint(int term, double rate)
        {
            curve.Add(term, rate);
        }

        #region IXmlSerializable Members

        public XmlSchema GetSchema()
        {
            // Return null as recommended by http://msdn.microsoft.com/en-us/library/system.xml.serialization.ixmlserializable.aspx
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            // Consume the wrapping element
            reader.Read();

            Currency = reader.ReadElementString("Currency");
            reader.ReadStartElement("Curve");
            // If the next node is not an end node there are no points in the curve.
            if (reader.NodeType != XmlNodeType.EndElement)
            {
                int baseDepth = reader.Depth;

                while (reader.Depth >= baseDepth)
                {
                    int currentTerm;
                    double currentRate;

                    reader.ReadStartElement("Point");
                    currentTerm = Convert.ToInt32(reader.ReadElementString("Term"));
                    currentRate = Convert.ToDouble(reader.ReadElementString("Rate"));
                    reader.ReadEndElement();

                    AddPoint(currentTerm, currentRate);
                }
            }
            reader.ReadEndElement();

            // Consume the end wrapping element
            reader.Read();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("Currency", currency);
            writer.WriteStartElement("Curve");
            foreach (KeyValuePair<int, double> currPoint in curve)
            {
                writer.WriteStartElement("Point");
                writer.WriteElementString("Term", currPoint.Key.ToString());
                writer.WriteElementString("Rate", currPoint.Value.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        #endregion
    }
}

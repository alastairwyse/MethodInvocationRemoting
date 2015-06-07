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
using System.Xml;
using NUnit.Framework;
using MethodInvocationRemoting;

namespace MethodInvocationRemotingUnitTests
{
    //******************************************************************************
    //
    // Class: SerializerOperationMapTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for class MethodInvocationRemoting.SerializerOperationMap.
    /// </summary>
    [TestFixture]
    public class SerializerOperationMapTests
    {
        private SerializerOperationMap testSerializerOperationMap;
        // Setup fake delegates to pass as XML serialization and deserialization operations
        XmlSerializationOperation TestXmlSerializationOperation1;
        XmlDeserializationOperation TestXmlDeserializationOperation1;
        XmlSerializationOperation TestXmlSerializationOperation2;
        XmlDeserializationOperation TestXmlDeserializationOperation2;

        [SetUp]
        protected void SetUp()
        {
            testSerializerOperationMap = new SerializerOperationMap();
            TestXmlSerializationOperation1 = XmlSerialize1;
            TestXmlDeserializationOperation1 = XmlDeserialize1;
            TestXmlSerializationOperation2 = XmlSerialize2;
            TestXmlDeserializationOperation2 = XmlDeserialize2;
        }

        [Test]
        public void AddMappingNativeTypeNull()
        {
            ArgumentNullException e = Assert.Throws<ArgumentNullException>(delegate
            {
                testSerializerOperationMap.AddMapping(null, "string", TestXmlSerializationOperation1, TestXmlDeserializationOperation1);
            });
            Assert.That(e.Message, Is.StringStarting("Parameter 'nativeType' cannot be null."));
            Assert.AreEqual("nativeType", e.ParamName);
        }

        [Test]
        public void AddMappingSerializedTypeNull()
        {
            ArgumentNullException e = Assert.Throws<ArgumentNullException>(delegate
            {
                testSerializerOperationMap.AddMapping(typeof(System.String), null, TestXmlSerializationOperation1, TestXmlDeserializationOperation1);
            });
            Assert.That(e.Message, Is.StringStarting("Parameter 'serializedType' cannot be null."));
            Assert.AreEqual("serializedType", e.ParamName);
        }

        [Test]
        public void AddMappingSerializationOperationNull()
        {
            ArgumentNullException e = Assert.Throws<ArgumentNullException>(delegate
            {
                testSerializerOperationMap.AddMapping(typeof(System.String), "string", null, TestXmlDeserializationOperation1);
            });
            Assert.That(e.Message, Is.StringStarting("Parameter 'serializationOperation' cannot be null."));
            Assert.AreEqual("serializationOperation", e.ParamName);
        }

        [Test]
        public void AddMappingDeserializationOperationNull()
        {
            ArgumentNullException e = Assert.Throws<ArgumentNullException>(delegate
            {
                testSerializerOperationMap.AddMapping(typeof(System.String), "string", TestXmlSerializationOperation1, null);
            });
            Assert.That(e.Message, Is.StringStarting("Parameter 'deserializationOperation' cannot be null."));
            Assert.AreEqual("deserializationOperation", e.ParamName);
        }

        [Test]
        public void AddMappingNativeTypeExists()
        {
            testSerializerOperationMap.AddMapping(typeof(System.String), "string", TestXmlSerializationOperation1, TestXmlDeserializationOperation1);
            ArgumentException e = Assert.Throws<ArgumentException>(delegate
            {
                testSerializerOperationMap.AddMapping(typeof(System.String), "string2", TestXmlSerializationOperation2, TestXmlDeserializationOperation2);
            });
            Assert.That(e.Message, Is.StringStarting("The native type 'System.String' already exists in the map."));
            Assert.AreEqual("nativeType", e.ParamName.ToString());
        }

        [Test]
        public void AddMappingSerializedTypeExists()
        {
            testSerializerOperationMap.AddMapping(typeof(System.Int16), "integer", TestXmlSerializationOperation1, TestXmlDeserializationOperation1);
            ArgumentException e = Assert.Throws<ArgumentException>(delegate
            {
                testSerializerOperationMap.AddMapping(typeof(System.Int32), "integer", TestXmlSerializationOperation2, TestXmlDeserializationOperation2);
            });
            Assert.That(e.Message, Is.StringStarting("The serialized type 'integer' already exists in the map."));
            Assert.AreEqual("serializedType", e.ParamName.ToString());
        }

        [Test]
        public void UpdateMappingNativeTypeNull()
        {
            testSerializerOperationMap.AddMapping(typeof(System.String), "string", TestXmlSerializationOperation1, TestXmlDeserializationOperation1);

            ArgumentNullException e = Assert.Throws<ArgumentNullException>(delegate
            {
                testSerializerOperationMap.UpdateMapping(null, "string", TestXmlSerializationOperation1, TestXmlDeserializationOperation1);
            });
            Assert.That(e.Message, Is.StringStarting("Parameter 'nativeType' cannot be null."));
            Assert.AreEqual("nativeType", e.ParamName);
        }

        [Test]
        public void UpdateMappingSerializedTypeNull()
        {
            testSerializerOperationMap.AddMapping(typeof(System.String), "string", TestXmlSerializationOperation1, TestXmlDeserializationOperation1);

            ArgumentNullException e = Assert.Throws<ArgumentNullException>(delegate
            {
                testSerializerOperationMap.UpdateMapping(typeof(System.String), null, TestXmlSerializationOperation1, TestXmlDeserializationOperation1);
            });
            Assert.That(e.Message, Is.StringStarting("Parameter 'serializedType' cannot be null."));
            Assert.AreEqual("serializedType", e.ParamName);
        }

        [Test]
        public void UpdateMappingSerializationOperationNull()
        {
            testSerializerOperationMap.AddMapping(typeof(System.String), "string", TestXmlSerializationOperation1, TestXmlDeserializationOperation1);

            ArgumentNullException e = Assert.Throws<ArgumentNullException>(delegate
            {
                testSerializerOperationMap.UpdateMapping(typeof(System.String), "string", null, TestXmlDeserializationOperation1);
            });
            Assert.That(e.Message, Is.StringStarting("Parameter 'serializationOperation' cannot be null."));
            Assert.AreEqual("serializationOperation", e.ParamName);
        }

        [Test]
        public void UpdateMappingDeserializationOperationNull()
        {
            testSerializerOperationMap.AddMapping(typeof(System.String), "string", TestXmlSerializationOperation1, TestXmlDeserializationOperation1);

            ArgumentNullException e = Assert.Throws<ArgumentNullException>(delegate
            {
                testSerializerOperationMap.UpdateMapping(typeof(System.String), "string", TestXmlSerializationOperation1, null);
            });
            Assert.That(e.Message, Is.StringStarting("Parameter 'deserializationOperation' cannot be null."));
            Assert.AreEqual("deserializationOperation", e.ParamName);
        }

        [Test]
        public void UpdateMappingNativeTypeDoesNotExist()
        {
            testSerializerOperationMap.AddMapping(typeof(System.String), "string", TestXmlSerializationOperation1, TestXmlDeserializationOperation1);

            ArgumentException e = Assert.Throws<ArgumentException>(delegate
            {
                testSerializerOperationMap.UpdateMapping(typeof(System.Int16), "smallInteger", TestXmlSerializationOperation1, TestXmlDeserializationOperation1);
            });
            Assert.That(e.Message, Is.StringStarting("The native type 'System.Int16' does not exist in the map."));
            Assert.AreEqual("nativeType", e.ParamName);
        }

        [Test]
        public void UpdateMappingSuccessTests()
        {
            testSerializerOperationMap.AddMapping(typeof(System.String), "string", TestXmlSerializationOperation1, TestXmlDeserializationOperation1);
            testSerializerOperationMap.AddMapping(typeof(System.Int32), "integer", TestXmlSerializationOperation2, TestXmlDeserializationOperation2);
            testSerializerOperationMap.UpdateMapping(typeof(System.String), "UpdatedString", TestXmlSerializationOperation2, TestXmlDeserializationOperation2);
            XmlSerializationOperation returnedSerializationOperation = testSerializerOperationMap.GetSerializationOperation(typeof(System.String));
            XmlDeserializationOperation returnedDeserializationOperation = testSerializerOperationMap.GetDeserializationOperation("UpdatedString");
            Assert.AreEqual("UpdatedString", testSerializerOperationMap.GetSerializedType(typeof(System.String)));
            Assert.AreEqual(TestXmlSerializationOperation2, returnedSerializationOperation);
            Assert.AreEqual(TestXmlDeserializationOperation2, returnedDeserializationOperation);
        }

        [Test]
        public void GetSerializedTypeTypeDoesNotExist()
        {
            testSerializerOperationMap.AddMapping(typeof(System.String), "string", TestXmlSerializationOperation1, TestXmlDeserializationOperation1);
            string returnedString = testSerializerOperationMap.GetSerializedType(typeof(System.DateTime));
            Assert.IsNull(returnedString);
        }

        [Test]
        public void GetSerializedTypeTypeExists()
        {
            testSerializerOperationMap.AddMapping(typeof(System.String), "string", TestXmlSerializationOperation1, TestXmlDeserializationOperation1);
            testSerializerOperationMap.AddMapping(typeof(System.Int32), "integer", TestXmlSerializationOperation2, TestXmlDeserializationOperation2);
            string returnedString = testSerializerOperationMap.GetSerializedType(typeof(System.String));
            Assert.AreEqual("string", returnedString);
        }

        [Test]
        public void GetNativeTypeTypeDoesNotExist()
        {
            testSerializerOperationMap.AddMapping(typeof(System.String), "string", TestXmlSerializationOperation1, TestXmlDeserializationOperation1);
            testSerializerOperationMap.AddMapping(typeof(System.Decimal), "long", TestXmlSerializationOperation2, TestXmlDeserializationOperation2);
            Type returnedType = testSerializerOperationMap.GetNativeType("double");
            Assert.IsNull(returnedType);
        }

        [Test]
        public void GetNativeTypeTypeExists()
        {
            testSerializerOperationMap.AddMapping(typeof(System.Double), "Double", TestXmlSerializationOperation1, TestXmlDeserializationOperation1);
            testSerializerOperationMap.AddMapping(typeof(System.DateTime), "Date", TestXmlSerializationOperation2, TestXmlDeserializationOperation2);
            testSerializerOperationMap.AddMapping(typeof(System.String), "String", TestXmlSerializationOperation1, TestXmlDeserializationOperation1);
            Type returnedType = testSerializerOperationMap.GetNativeType("Date");
            Assert.AreEqual(typeof(DateTime), returnedType);
        }

        [Test]
        public void GetSerializationOperationTypeDoesNotExist()
        {
            testSerializerOperationMap.AddMapping(typeof(System.String), "string", TestXmlSerializationOperation1, TestXmlDeserializationOperation1);
            testSerializerOperationMap.AddMapping(typeof(System.Decimal), "long", TestXmlSerializationOperation2, TestXmlDeserializationOperation2);
            XmlSerializationOperation returnedOperation = testSerializerOperationMap.GetSerializationOperation(typeof(System.DateTime));
            Assert.IsNull(returnedOperation);
        }

        [Test]
        public void GetSerializationOperationTypeExists()
        {
            testSerializerOperationMap.AddMapping(typeof(System.String), "string", TestXmlSerializationOperation1, TestXmlDeserializationOperation1);
            testSerializerOperationMap.AddMapping(typeof(System.Decimal), "long", TestXmlSerializationOperation2, TestXmlDeserializationOperation2);
            XmlSerializationOperation returnedOperation = testSerializerOperationMap.GetSerializationOperation(typeof(System.String));
            Assert.AreEqual(TestXmlSerializationOperation1, returnedOperation);
        }

        [Test]
        public void GetDeserializationOperationTypeDoesNotExist()
        {
            testSerializerOperationMap.AddMapping(typeof(System.String), "string", TestXmlSerializationOperation1, TestXmlDeserializationOperation1);
            testSerializerOperationMap.AddMapping(typeof(System.Decimal), "long", TestXmlSerializationOperation2, TestXmlDeserializationOperation2);
            XmlDeserializationOperation returnedOperation = testSerializerOperationMap.GetDeserializationOperation("Date");
            Assert.IsNull(returnedOperation);
        }

        [Test]
        public void GetDeserializationOperationTypeExists()
        {
            testSerializerOperationMap.AddMapping(typeof(System.Double), "Double", TestXmlSerializationOperation1, TestXmlDeserializationOperation1);
            testSerializerOperationMap.AddMapping(typeof(System.DateTime), "Date", TestXmlSerializationOperation2, TestXmlDeserializationOperation2);
            testSerializerOperationMap.AddMapping(typeof(System.String), "String", TestXmlSerializationOperation1, TestXmlDeserializationOperation1);
            XmlDeserializationOperation returnedOperation = testSerializerOperationMap.GetDeserializationOperation("Double");
            Assert.AreEqual(TestXmlDeserializationOperation1, returnedOperation);
        }

        private void XmlSerialize1(object inputObject, XmlWriter writer)
        {
        }

        private object XmlDeserialize1(XmlReader reader)
        {
            return null;
        }

        private void XmlSerialize2(object inputObject, XmlWriter writer)
        {
        }

        private object XmlDeserialize2(XmlReader reader)
        {
            return null;
        }
    }
}

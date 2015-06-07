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
using MethodInvocationRemoting;

namespace MethodInvocationRemotingUnitTests
{
    //******************************************************************************
    //
    // Class: MethodInvocationTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for class MethodInvocationRemoting.MethodInvocation.
    /// </summary>
    [TestFixture]
    public class MethodInvocationTests
    {
        private MethodInvocation testMethodInvocation;

        [Test]
        public void FullMethodConstructorSuccessTest()
        {
            object[] parameters = new object[2];
            parameters[0] = 1;
            parameters[1] = 2;
            testMethodInvocation = new MethodInvocation("AddNumbers", parameters, typeof(int));
            Assert.AreEqual("AddNumbers", testMethodInvocation.Name);
            Assert.AreEqual(1, testMethodInvocation.Parameters[0]);
            Assert.AreEqual(2, testMethodInvocation.Parameters[1]);
            Assert.AreEqual(typeof(int), testMethodInvocation.ReturnType);
        }

        [Test]
        public void VoidMethodConstructorSuccessTest()
        {
            object[] parameters = new object[2];
            parameters[0] = 1;
            parameters[1] = 2;
            testMethodInvocation = new MethodInvocation("AddNumbers", parameters);
            Assert.AreEqual("AddNumbers", testMethodInvocation.Name);
            Assert.AreEqual(1, testMethodInvocation.Parameters[0]);
            Assert.AreEqual(2, testMethodInvocation.Parameters[1]);
            Assert.IsNull(testMethodInvocation.ReturnType);
        }

        [Test]
        public void ParameterlessMethodConstructorSuccessTest()
        {
            testMethodInvocation = new MethodInvocation("AddNumbers", typeof(int));
            Assert.AreEqual("AddNumbers", testMethodInvocation.Name);
            Assert.IsNull(testMethodInvocation.Parameters);
            Assert.AreEqual(typeof(int), testMethodInvocation.ReturnType);
        }

        [Test]
        public void ParameterlessVoidMethodConstructorSuccessTest()
        {
            testMethodInvocation = new MethodInvocation("AddNumbers");
            Assert.AreEqual("AddNumbers", testMethodInvocation.Name);
            Assert.IsNull(testMethodInvocation.Parameters);
            Assert.IsNull(testMethodInvocation.ReturnType);
        }

        [Test]
        public void FullMethodConstructorZeroLengthParameters()
        {
            ArgumentException e = Assert.Throws<ArgumentException>(delegate
            {
                testMethodInvocation = new MethodInvocation("AddNumbers", new object[0], typeof(int));
            });
            Assert.That(e.Message, Is.StringStarting("The method invocation parameters cannot be empty."));
            Assert.AreEqual(e.ParamName, "methodParameters");
        }

        [Test]
        public void VoidMethodConstructorZeroLengthParameters()
        {
            ArgumentException e = Assert.Throws<ArgumentException>(delegate
            {
                testMethodInvocation = new MethodInvocation("AddNumbers", new object[0]);
            });
            Assert.That(e.Message, Is.StringStarting("The method invocation parameters cannot be empty."));
            Assert.AreEqual(e.ParamName, "methodParameters");
        }

        [Test]
        public void FullMethodConstructorBlankName()
        {
            ArgumentException e = Assert.Throws<ArgumentException>(delegate
            {
                object[] parameters = new object[2];
                parameters[0] = 1;
                parameters[1] = 2;
                testMethodInvocation = new MethodInvocation("   ", parameters, typeof(int));
            });
            Assert.That(e.Message, Is.StringStarting("The method name cannot be blank."));
            Assert.AreEqual(e.ParamName, "methodName");
        }

        [Test]
        public void VoidMethodConstructorBlankName()
        {
            ArgumentException e = Assert.Throws<ArgumentException>(delegate
            {
                object[] parameters = new object[2];
                parameters[0] = 1;
                parameters[1] = 2;
                testMethodInvocation = new MethodInvocation("   ", parameters);
            });
            Assert.That(e.Message, Is.StringStarting("The method name cannot be blank."));
            Assert.AreEqual(e.ParamName, "methodName");
        }

        [Test]
        public void ParameterlessMethodConstructorBlankName()
        {
            ArgumentException e = Assert.Throws<ArgumentException>(delegate
            {
                testMethodInvocation = new MethodInvocation("   ", typeof(int));
            });
            Assert.That(e.Message, Is.StringStarting("The method name cannot be blank."));
            Assert.AreEqual(e.ParamName, "methodName");
        }

        [Test]
        public void ParameterlessVoidMethodConstructorBlankName()
        {
            ArgumentException e = Assert.Throws<ArgumentException>(delegate
            {
                testMethodInvocation = new MethodInvocation("   ");
            });
            Assert.That(e.Message, Is.StringStarting("The method name cannot be blank."));
            Assert.AreEqual(e.ParamName, "methodName");
        }
    }
}

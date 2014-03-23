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
using OperatingSystemAbstraction;

namespace ApplicationLoggingUnitTests
{
    //******************************************************************************
    //
    // Class: FileApplicationLoggerTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for class ApplicationLogging.FileApplicationLogger.
    /// </summary>
    [TestFixture]
    public class FileApplicationLoggerTests
    {
        private Mockery mocks;
        private IStreamWriter mockStreamWriter;
        private FileApplicationLogger testFileApplicationLogger;

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockStreamWriter = mocks.NewMock<IStreamWriter>();
            testFileApplicationLogger = new FileApplicationLogger(LogLevel.Debug, ':', "  ", mockStreamWriter);
        }

        [Test]
        public void LogSuccessTests()
        {
            string expectedStackTraceText = Environment.NewLine + @"     at ApplicationLoggingUnitTests.LogSuccessTests() in C:\MethodInvocationRemoting\C#\ApplicationLoggingUnitTests\FileApplicationLoggerTests.cs:line 123" + Environment.NewLine + @"     at ApplicationLoggingUnitTests.LogSuccessTests() in C:\MethodInvocationRemoting\C#\ApplicationLoggingUnitTests\FileApplicationLoggerTests.cs:line 456";

            using (mocks.Ordered)
            {
                Expect.Once.On(mockStreamWriter).Method("WriteLine").With(new StringContainsMatcher(" CRITICAL : Log Text 1."));
                Expect.Once.On(mockStreamWriter).Method("Flush").WithNoArguments();
                Expect.Once.On(mockStreamWriter).Method("WriteLine").With(new StringContainsMatcher(" Log Event Id = 123 : ERROR : Log Text 2."));
                Expect.Once.On(mockStreamWriter).Method("Flush").WithNoArguments();
                Expect.Once.On(mockStreamWriter).Method("WriteLine").With(new StringContainsMatcher(" WARNING : Log Text 3." + Environment.NewLine + "  ApplicationLoggingUnitTests.CustomStackTraceException: Mocked Exception." + expectedStackTraceText));
                Expect.Once.On(mockStreamWriter).Method("Flush").WithNoArguments();
                Expect.Once.On(mockStreamWriter).Method("WriteLine").With(new StringContainsMatcher(" Log Event Id = 124 : Log Text 4." + Environment.NewLine + "  ApplicationLoggingUnitTests.CustomStackTraceException: Mocked Exception 2." + expectedStackTraceText));
                Expect.Once.On(mockStreamWriter).Method("Flush").WithNoArguments();
                Expect.Once.On(mockStreamWriter).Method("WriteLine").With(new StringContainsMatcher(" Source = FileApplicationLoggerTests : CRITICAL : Log Text 5."));
                Expect.Once.On(mockStreamWriter).Method("Flush").WithNoArguments();
                Expect.Once.On(mockStreamWriter).Method("WriteLine").With(new StringContainsMatcher(" Source = FileApplicationLoggerTests : Log Event Id = 123 : ERROR : Log Text 6."));
                Expect.Once.On(mockStreamWriter).Method("Flush").WithNoArguments();
                Expect.Once.On(mockStreamWriter).Method("WriteLine").With(new StringContainsMatcher(" Source = FileApplicationLoggerTests : WARNING : Log Text 7." + Environment.NewLine + "  ApplicationLoggingUnitTests.CustomStackTraceException: Mocked Exception 3." + expectedStackTraceText));
                Expect.Once.On(mockStreamWriter).Method("Flush").WithNoArguments();
                Expect.Once.On(mockStreamWriter).Method("WriteLine").With(new StringContainsMatcher(" Source = FileApplicationLoggerTests : Log Event Id = 124 : Log Text 8." + Environment.NewLine + "  ApplicationLoggingUnitTests.CustomStackTraceException: Mocked Exception 4." + expectedStackTraceText));
                Expect.Once.On(mockStreamWriter).Method("Flush").WithNoArguments();
            }
            testFileApplicationLogger.Log(LogLevel.Critical, "Log Text 1.");
            testFileApplicationLogger.Log(123, LogLevel.Error, "Log Text 2.");
            testFileApplicationLogger.Log(LogLevel.Warning, "Log Text 3.", new CustomStackTraceException("Mocked Exception."));
            testFileApplicationLogger.Log(124, LogLevel.Information, "Log Text 4.", new CustomStackTraceException("Mocked Exception 2."));
            testFileApplicationLogger.Log(this, LogLevel.Critical, "Log Text 5.");
            testFileApplicationLogger.Log(this, 123, LogLevel.Error, "Log Text 6.");
            testFileApplicationLogger.Log(this, LogLevel.Warning, "Log Text 7.", new CustomStackTraceException("Mocked Exception 3."));
            testFileApplicationLogger.Log(this, 124, LogLevel.Information, "Log Text 8.", new CustomStackTraceException("Mocked Exception 4."));

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void LogBelowMinimumLogLevelNotLogged()
        {
            testFileApplicationLogger = new FileApplicationLogger(LogLevel.Warning, ':', "  ", mockStreamWriter);

            Expect.Never.On(mockStreamWriter);

            testFileApplicationLogger.Log(LogLevel.Information, "Log Text 1.");
            testFileApplicationLogger.Log(123, LogLevel.Information, "Log Text 2.");
            testFileApplicationLogger.Log(LogLevel.Information, "Log Text 3.", new CustomStackTraceException("Mocked Exception."));
            testFileApplicationLogger.Log(124, LogLevel.Information, "Log Text 4.", new CustomStackTraceException("Mocked Exception 2."));
            testFileApplicationLogger.Log(LogLevel.Debug, "Log Text 1.");
            testFileApplicationLogger.Log(123, LogLevel.Debug, "Log Text 2.");
            testFileApplicationLogger.Log(LogLevel.Debug, "Log Text 3.", new CustomStackTraceException("Mocked Exception."));
            testFileApplicationLogger.Log(124, LogLevel.Debug, "Log Text 4.", new CustomStackTraceException("Mocked Exception 2."));
            testFileApplicationLogger.Log(this, LogLevel.Information, "Log Text 5.");
            testFileApplicationLogger.Log(this, 123, LogLevel.Information, "Log Text 6.");
            testFileApplicationLogger.Log(this, LogLevel.Information, "Log Text 7.", new CustomStackTraceException("Mocked Exception 3."));
            testFileApplicationLogger.Log(this, 124, LogLevel.Information, "Log Text 8.", new CustomStackTraceException("Mocked Exception 4."));
            testFileApplicationLogger.Log(this, LogLevel.Debug, "Log Text 5.");
            testFileApplicationLogger.Log(this, 123, LogLevel.Debug, "Log Text 6.");
            testFileApplicationLogger.Log(this, LogLevel.Debug, "Log Text 7.", new CustomStackTraceException("Mocked Exception 3."));
            testFileApplicationLogger.Log(this, 124, LogLevel.Debug, "Log Text 8.", new CustomStackTraceException("Mocked Exception 4."));
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void CloseSuccessTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockStreamWriter).Method("Close").WithNoArguments();
            }

            testFileApplicationLogger.Close();
            mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}

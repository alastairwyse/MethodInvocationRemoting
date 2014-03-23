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
using OperatingSystemAbstraction;
using ApplicationLogging;

namespace MethodInvocationRemotingLoggingTests
{
    //******************************************************************************
    //
    // Class: FileRemoteSenderLoggingTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for the logging functionality in class MethodInvocationRemoting.FileRemoteSender.
    /// </summary>
    [TestFixture]
    public class FileRemoteSenderLoggingTests
    {
        private Mockery mocks;
        private IFile mockMessageFile;
        private IFile mockLockFile;
        private IFileSystem mockFileSystem;
        private IApplicationLogger mockApplicationLogger;
        private FileRemoteSender testFileRemoteSender;

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockMessageFile = mocks.NewMock<IFile>();
            mockLockFile = mocks.NewMock<IFile>();
            mockFileSystem = mocks.NewMock<IFileSystem>();
            mockApplicationLogger = mocks.NewMock<IApplicationLogger>();
            testFileRemoteSender = new FileRemoteSender(@"C:\Temp\TestFilePath.txt", @"C:\Temp\TestFilePath.lck", mockApplicationLogger, mockMessageFile, mockLockFile, mockFileSystem);
        }

        [Test]
        public void SendLoggingTest()
        {
            Expect.AtLeastOnce.On(mockMessageFile);
            Expect.AtLeastOnce.On(mockLockFile);
            Expect.AtLeastOnce.On(mockFileSystem);
            Expect.Once.On(mockApplicationLogger).Method("Log").With(testFileRemoteSender, LogLevel.Information, "Message sent.");

            testFileRemoteSender.Send("<TestMessage>Test message content</TestMessage>");

            mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}

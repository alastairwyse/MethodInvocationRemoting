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
using NMock2;
using MethodInvocationRemoting;
using OperatingSystemAbstraction;
using ApplicationLogging;

namespace MethodInvocationRemotingUnitTests
{
    //******************************************************************************
    //
    // Class: FileRemoteSenderTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for class MethodInvocationRemoting.FileRemoteSender.
    /// </summary>
    [TestFixture]
    public class FileRemoteSenderTests
    {
        private Mockery mocks;
        private IFile mockMessageFile;
        private IFile mockLockFile;
        private IFileSystem mockFileSystem;
        private FileRemoteSender testFileRemoteSender;

        private const string messageFilePath = @"C:\Temp\TestFilePath.txt";
        private const string lockFilePath = @"C:\Temp\TestFilePath.lck";
        private const string testMessage = "<TestMessage>Test message content</TestMessage>";

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockMessageFile = mocks.NewMock<IFile>();
            mockLockFile = mocks.NewMock<IFile>();
            mockFileSystem = mocks.NewMock<IFileSystem>();
            testFileRemoteSender = new FileRemoteSender(messageFilePath, lockFilePath, new ConsoleApplicationLogger(LogLevel.Warning, '|', "  "), mockMessageFile, mockLockFile, mockFileSystem);
        }

        [Test]
        public void SendAfterDispose()
        {
            SetDisposeExpectations();
            testFileRemoteSender.Dispose();
            ObjectDisposedException e = Assert.Throws<ObjectDisposedException>(delegate
            {
                testFileRemoteSender.Send(testMessage);
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.AreEqual(e.ObjectName, "FileRemoteSender");
        }

        [Test]
        public void SendException()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockLockFile).Method("WriteAll").With("");
                Expect.Once.On(mockMessageFile).Method("WriteAll").With(testMessage).Will(Throw.Exception(new Exception("Mock Write Failure")));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testFileRemoteSender.Send(testMessage);
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Error sending message."));
        }

        [Test]
        public void DisposeSuccessTest()
        {
            using (mocks.Ordered)
            {
                SetDisposeExpectations();
            }

            testFileRemoteSender.Dispose();
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SendSuccessTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockLockFile).Method("WriteAll").With("");
                Expect.Once.On(mockMessageFile).Method("WriteAll").With(testMessage);
                Expect.Once.On(mockFileSystem).Method("DeleteFile").With(lockFilePath);
            }

            testFileRemoteSender.Send(testMessage);
            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        //------------------------------------------------------------------------------
        //
        // Method: SetDisposeExpectations
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Sets mock expectations for the Dispose method.
        /// </summary>
        private void SetDisposeExpectations()
        {
            Expect.Once.On(mockMessageFile).Method("Dispose").WithNoArguments();
            Expect.Once.On(mockLockFile).Method("Dispose").WithNoArguments();
        }
    }
}

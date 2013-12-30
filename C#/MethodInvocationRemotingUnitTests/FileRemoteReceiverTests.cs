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

namespace MethodInvocationRemotingUnitTests
{
    //******************************************************************************
    //
    // Class: FileRemoteReceiverTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for class MethodInvocationRemoting.FileRemoteReceiver.
    /// </summary>
    [TestFixture]
    public class FileRemoteReceiverTests
    {
        private Mockery mocks;
        private IFile mockMessageFile;
        private IFileSystem mockFileSystem;
        private FileRemoteReceiver testFileRemoteReceiver;

        private const string messageFilePath = @"C:\Temp\TestFilePath.txt";
        private const string lockFilePath = @"C:\Temp\TestFilePath.lck";
        private const string testMessage = "<TestMessage>Test message content</TestMessage>";

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockMessageFile = mocks.NewMock<IFile>();
            mockFileSystem = mocks.NewMock<IFileSystem>();
            testFileRemoteReceiver = new FileRemoteReceiver(messageFilePath, lockFilePath, 1000, mockMessageFile, mockFileSystem);
        }

        [Test]
        public void ReceiveAfterDispose()
        {
            using (mocks.Ordered)
            {
                SetDisposeExpectations();
            }

            testFileRemoteReceiver.Dispose();
            ObjectDisposedException e = Assert.Throws<ObjectDisposedException>(delegate
            {
                testFileRemoteReceiver.Receive();
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.AreEqual(e.ObjectName, "FileRemoteReceiver");
        }

        [Test]
        public void ReceiveException()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockFileSystem).Method("CheckFileExists").With(messageFilePath).Will(Return.Value(true));
                Expect.Once.On(mockFileSystem).Method("CheckFileExists").With(lockFilePath).Will(Return.Value(false));
                Expect.Once.On(mockMessageFile).Method("ReadAll").WithNoArguments().Will(Throw.Exception(new Exception("Mock Read Failure")));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testFileRemoteReceiver.Receive();
            });
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.That(e.Message, NUnit.Framework.Is.StringStarting("Error receiving message."));
            Assert.That(e.InnerException.Message, NUnit.Framework.Is.StringStarting("Mock Read Failure"));
        }

        [Test]
        public void ReceiveSuccessTests()
        {
            string receivedMessage;

            using (mocks.Ordered)
            {
                Expect.Once.On(mockFileSystem).Method("CheckFileExists").With(messageFilePath).Will(Return.Value(true));
                Expect.Once.On(mockFileSystem).Method("CheckFileExists").With(lockFilePath).Will(Return.Value(false));
                Expect.Once.On(mockMessageFile).Method("ReadAll").WithNoArguments().Will(Return.Value(testMessage));
                Expect.Once.On(mockFileSystem).Method("DeleteFile").With(messageFilePath);
            }

            receivedMessage = testFileRemoteReceiver.Receive();
            mocks.VerifyAllExpectationsHaveBeenMet();
            Assert.AreEqual(testMessage, receivedMessage);
        }

        [Test]
        public void DisposeSuccessTest()
        {
            using (mocks.Ordered)
            {
                SetDisposeExpectations();
            }

            testFileRemoteReceiver.Dispose();
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
        }
    }
}

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
    // Class: FileRemoteReceiverLoggingTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for the logging functionality in class MethodInvocationRemoting.FileRemoteReceiver.
    /// </summary>
    [TestFixture]
    public class FileRemoteReceiverLoggingTests
    {
        private Mockery mocks;
        private IFile mockMessageFile;
        private IFileSystem mockFileSystem;
        private IApplicationLogger mockApplicationLogger;
        private FileRemoteReceiver testFileRemoteReceiver;
        private const string messageFilePath = @"C:\Temp\TestFilePath.txt";
        private const string lockFilePath = @"C:\Temp\TestFilePath.lck";

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockMessageFile = mocks.NewMock<IFile>();
            mockFileSystem = mocks.NewMock<IFileSystem>();
            mockApplicationLogger = mocks.NewMock<IApplicationLogger>();
            testFileRemoteReceiver = new FileRemoteReceiver(messageFilePath, lockFilePath, 1000, mockApplicationLogger, mockMessageFile, mockFileSystem);
        }

        [Test]
        public void ReceiveLoggingTest()
        {
            string receivedMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><Data>abc</Data></Parameter><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter /><Parameter><DataType>double</DataType><Data>4.5678899999999999e+002</Data></Parameter></Parameters><ReturnType /></MethodInvocation>";
            string smallMessage = "<TestMessage>Test message content</TestMessage>";

            using (mocks.Ordered)
            {
                Expect.Once.On(mockFileSystem).Method("CheckFileExists").With(messageFilePath).Will(Return.Value(true));
                Expect.Once.On(mockFileSystem).Method("CheckFileExists").With(lockFilePath).Will(Return.Value(false));
                Expect.Once.On(mockMessageFile).Method("ReadAll").WithNoArguments().Will(Return.Value(receivedMessage));
                Expect.AtLeastOnce.On(mockFileSystem);
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testFileRemoteReceiver, LogLevel.Information, "Received message '<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataT' (truncated).");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testFileRemoteReceiver, LogLevel.Debug, "Complete message content: '" + receivedMessage + "'.");
                Expect.Once.On(mockFileSystem).Method("CheckFileExists").With(messageFilePath).Will(Return.Value(true));
                Expect.Once.On(mockFileSystem).Method("CheckFileExists").With(lockFilePath).Will(Return.Value(false));
                Expect.Once.On(mockMessageFile).Method("ReadAll").WithNoArguments().Will(Return.Value(smallMessage));
                Expect.AtLeastOnce.On(mockFileSystem);
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testFileRemoteReceiver, LogLevel.Information, "Received message '" + smallMessage + "'.");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testFileRemoteReceiver, LogLevel.Debug, "Complete message content: '" + smallMessage + "'.");
            }

            testFileRemoteReceiver.Receive();
            testFileRemoteReceiver.Receive();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void CancelReceiveLoggingTest()
        {
            Expect.Once.On(mockApplicationLogger).Method("Log").With(testFileRemoteReceiver, LogLevel.Information, "Receive operation cancelled.");

            testFileRemoteReceiver.CancelReceive();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}

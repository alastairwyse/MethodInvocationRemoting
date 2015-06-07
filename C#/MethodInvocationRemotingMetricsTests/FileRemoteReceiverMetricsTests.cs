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

#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NMock2;
using MethodInvocationRemoting;
using OperatingSystemAbstraction;
using ApplicationLogging;
using ApplicationMetrics;
using MethodInvocationRemotingMetrics;

namespace MethodInvocationRemotingMetricsTests
{
    //******************************************************************************
    //
    // Class: FileRemoteReceiverMetricsTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for the metrics logging functionality in class MethodInvocationRemoting.FileRemoteReceiver.
    /// </summary>
    [TestFixture]
    class FileRemoteReceiverMetricsTests
    {
        private Mockery mocks;
        private IFile mockMessageFile;
        private IFileSystem mockFileSystem;
        private IMetricLogger mockMetricLogger;
        private FileRemoteReceiver testFileRemoteReceiver;
        private const string messageFilePath = @"C:\Temp\TestFilePath.txt";
        private const string lockFilePath = @"C:\Temp\TestFilePath.lck";

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockMessageFile = mocks.NewMock<IFile>();
            mockFileSystem = mocks.NewMock<IFileSystem>();
            mockMetricLogger = mocks.NewMock<IMetricLogger>();
            testFileRemoteReceiver = new FileRemoteReceiver(messageFilePath, lockFilePath, 1000, new ConsoleApplicationLogger(LogLevel.Critical, '|', "  "), mockMetricLogger, mockMessageFile, mockFileSystem);
        }

        [Test]
        public void ReceiveMetricsTest()
        {
            string receivedMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>TestMethod</MethodName><Parameters><Parameter><DataType>string</DataType><Data>abc</Data></Parameter><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter /><Parameter><DataType>double</DataType><Data>4.5678899999999999e+002</Data></Parameter></Parameters><ReturnType /></MethodInvocation>";

            using (mocks.Ordered)
            {
                Expect.Once.On(mockFileSystem).Method("CheckFileExists").With(messageFilePath).Will(Return.Value(true));
                Expect.Once.On(mockFileSystem).Method("CheckFileExists").With(lockFilePath).Will(Return.Value(false));
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new MessageReceiveTime()));
                Expect.Once.On(mockMessageFile).Method("ReadAll").WithNoArguments().Will(Return.Value(receivedMessage)); 
                Expect.AtLeastOnce.On(mockFileSystem);
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new MessageReceiveTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new MessageReceived()));
                Expect.Once.On(mockMetricLogger).Method("Add").With(IsAmountMetric.Equal(new ReceivedMessageSize(381)));
            }

            testFileRemoteReceiver.Receive();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}

/*
 * Copyright 2015 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
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
    // Class: FileRemoteSenderMetricsTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for the metrics logging functionality in class MethodInvocationRemoting.FileRemoteSender.
    /// </summary>
    [TestFixture]
    class FileRemoteSenderMetricsTests
    {
        private Mockery mocks;
        private IFile mockMessageFile;
        private IFile mockLockFile;
        private IFileSystem mockFileSystem;
        private IMetricLogger mockMetricLogger;
        private FileRemoteSender testFileRemoteSender;

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockMessageFile = mocks.NewMock<IFile>();
            mockLockFile = mocks.NewMock<IFile>();
            mockFileSystem = mocks.NewMock<IFileSystem>();
            mockMetricLogger = mocks.NewMock<IMetricLogger>();
            testFileRemoteSender = new FileRemoteSender(@"C:\Temp\TestFilePath.txt", @"C:\Temp\TestFilePath.lck", new ConsoleApplicationLogger(LogLevel.Critical, '|', "  "), mockMetricLogger, mockMessageFile, mockLockFile, mockFileSystem);
        }

        [Test]
        public void SendMetricsTest()
        {
            Expect.AtLeastOnce.On(mockMessageFile);
            Expect.AtLeastOnce.On(mockLockFile);
            Expect.AtLeastOnce.On(mockFileSystem);
            using (mocks.Ordered)
            {
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new MessageSendTime()));
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new MessageSendTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new MessageSent()));
            }

            testFileRemoteSender.Send("<TestMessage>Test message content</TestMessage>");

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SendExceptionMetricsTest()
        {
            Expect.AtLeastOnce.On(mockLockFile);
            Expect.Once.On(mockMessageFile).Method("WriteAll").WithAnyArguments().Will(Throw.Exception(new Exception("Mock File Write Failure")));
            using (mocks.Ordered)
            {
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new MessageSendTime()));
                Expect.Once.On(mockMetricLogger).Method("CancelBegin").With(IsMetric.Equal(new MessageSendTime()));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testFileRemoteSender.Send("<TestMessage>Test message content</TestMessage>");
            });
        }
    }
}

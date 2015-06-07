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
using ApplicationLogging;
using ApplicationMetrics;
using MethodInvocationRemoting;
using MethodInvocationRemotingMetrics;

namespace MethodInvocationRemotingMetricsTests
{
    //******************************************************************************
    //
    // Class: RemoteReceiverDecompressorMetricsTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for the metrics logging functionality in class MethodInvocationRemoting.RemoteReceiverDecompressor.
    /// </summary>
    [TestFixture]
    class RemoteReceiverDecompressorMetricsTests
    {
        private Mockery mocks;
        private IRemoteReceiver mockUnderlyingRemoteReceiver;
        private IMetricLogger mockMetricLogger;
        private RemoteReceiverDecompressor testRemoteReceiverDecompressor;

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockUnderlyingRemoteReceiver = mocks.NewMock<IRemoteReceiver>();
            mockMetricLogger = mocks.NewMock<IMetricLogger>();
            testRemoteReceiverDecompressor = new RemoteReceiverDecompressor(mockUnderlyingRemoteReceiver, mockMetricLogger);
        }

        [Test]
        public void ReceiveMetricsTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockUnderlyingRemoteReceiver).Method("Receive").Will(Return.Value("H4sIAAAAAAAEAO29B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/Ih7/Hu8WZXqZ101RLT/7aHe881GaL6fVrFhefPbRuj3fPvjo9zh6/EXezqvZ2fKymmYttTSfvMgW+ZH8+t2inR+X5dOszd5cr/Lj5mVW07ctgX5812v92H3u/X702Lx4VCzb/CKvH9+1n/B3R7t79+Szo8d3vfdiIGZZm78pFvlxXWfXPUCPT8t8kS/bXvPHd7vfmKYR2D2wOzs7u9v8vzc7O4/4f2P6zOJsQb0HzIf0bO/ubd/bfbN379H9h/S/MX3Uh3lLwjRtTRPb6+YXlu3hm7xpX/PXv/CC/poXTUr/y9KWPk/lvRTt8O3vcffRR4cff/y97//iX/LJZ7//9p2tb/3CbLE6/P1+99/tx3/P3/Vnfl80uxvCuy2Cs3xaLLLythMnrW83b6ZtB+j2g4d7ewe7n+7d393f+3T/3r0H9x/eu79/7+H9nXv37t9q9gYgvyfgW1KoKS6W+ezJddvnloPbwphU1W1JjKa3oq807IA7z8pG8dxMw+jbbb2OvHzLMTbzqm7PBnTJ9u6n9/ZvrU6m86y+JbnQ9FbkkoYdcF/chlTRNyFyX5tUZbW8GKLUg0/v355Qs2o9KW+tdbnxrYhlmvZmcfzpwwefPry3e2//4NO9e3u790637+04MdgktHGQEYhRgLckyHlZZW0f77PlebEsWiXSLcHckqza5S2oOoDcvfHu/u79h3uf7hIxb0XLOCAD58HeaQzMLYc+ME/vS0GPxW9Jx0AobkHNTUK0TZaA9P/ezr1PD+7vP3hw/+B2TLoJZh/kg69N5KhK+V1v+7avaW9J3FA534K6m7X5vb0Hn96KoBvBAMrXp+HNFuwmMlrDflsiep7AbUg47DiQi3k78g2D2N37+qQbcqD2dx7eP/h05+H+/U/H9x4ePDzYf0D/d67TTQRl7/O2xFTP+DaEjDvRz6s6X6TFqlkv0llVVnXaFG0KrEbptFo2+bTN23WdZrNiVTRT+NN5WbTj9OU8a/KyXJPDvUyX67LM0kV2scxG6bJaptN13dBX0vQni8tsQX9d4k/pZZRmZfGL1jl1Nb7VHMaxf3y6WLXXeFV/ft25HApl9nZ2723v3Ed4tLv3aJ+imU/H5FvcFq5GhbecTRtD3mI6h+LN7b3d/Qf7B/c+3b+VbAyBsVA+WD5uOfbhoOiW3f1/RW6+ai3r/6J1trCvkfQ0DQWuxXJazNZL+jDF5NQz6mC1rlnO5M9xelJnzYeIzddG/sV6OU0vizbLVZhbVgEi8xbddb3MxulPrIuGhkkN58V0TeohW1/Qn/maRrqui40DuC2z+R+73ylF8wr4L92QB6X8rt/y7hfdjNH/A4VP6g5qEgAA"));
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new StringDecompressTime()));
                Expect.Exactly(5).On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new RemoteReceiverDecompressorReadBufferCreated()));
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new StringDecompressTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new StringDecompressed()));
                Expect.Once.On(mockUnderlyingRemoteReceiver).Method("Receive").Will(Return.Value(""));
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new StringDecompressTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new RemoteReceiverDecompressorReadBufferCreated()));
                Expect.Once.On(mockMetricLogger).Method("End").With(IsMetric.Equal(new StringDecompressTime()));
                Expect.Once.On(mockMetricLogger).Method("Increment").With(IsMetric.Equal(new StringDecompressed()));
            }

            testRemoteReceiverDecompressor.Receive();
            testRemoteReceiverDecompressor.Receive();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void ReceiveExceptionMetricsTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockUnderlyingRemoteReceiver).Method("Receive").Will(Return.Value("InvalidCompressedString"));
                Expect.Once.On(mockMetricLogger).Method("Begin").With(IsMetric.Equal(new StringDecompressTime()));
                Expect.Once.On(mockMetricLogger).Method("CancelBegin").With(IsMetric.Equal(new StringDecompressTime()));
            }

            Exception e = Assert.Throws<Exception>(delegate
            {
                testRemoteReceiverDecompressor.Receive();
            });
        }
    }
}

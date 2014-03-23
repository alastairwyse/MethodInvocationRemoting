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
using ApplicationLogging;

namespace MethodInvocationRemotingLoggingTests
{
    //******************************************************************************
    //
    // Class: RemoteReceiverDecompressorLoggingTests
    //
    //******************************************************************************
    /// <summary>
    /// Unit tests for the logging functionality in class MethodInvocationRemoting.RemoteReceiverDecompressor.
    /// </summary>
    [TestFixture]
    public class RemoteReceiverDecompressorLoggingTests
    {
        private Mockery mocks;
        private IRemoteReceiver mockUnderlyingRemoteReceiver;
        private IApplicationLogger mockApplicationLogger;
        private RemoteReceiverDecompressor testRemoteReceiverDecompressor;

        [SetUp]
        protected void SetUp()
        {
            mocks = new Mockery();
            mockUnderlyingRemoteReceiver = mocks.NewMock<IRemoteReceiver>();
            mockApplicationLogger = mocks.NewMock<IApplicationLogger>();
            testRemoteReceiverDecompressor = new RemoteReceiverDecompressor(mockUnderlyingRemoteReceiver, mockApplicationLogger);
        }

        [Test]
        public void ReceiveLoggingTest()
        {
            using (mocks.Ordered)
            {
                Expect.Once.On(mockUnderlyingRemoteReceiver).Method("Receive").Will(Return.Value("H4sIAAAAAAAEAO29B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/Ih7/Hu8WZXqZ101RLT/7aHe881GaL6fVrFhefPbRuj3fPvjo9zh6/EXezqvZ2fKymmYttTSfvMgW+ZH8+t2inR+X5dOszd5cr/Lj5mVW07ctgX5812v92H3u/X702Lx4VCzb/CKvH9+1n/B3R7t79+Szo8d3vfdiIGZZm78pFvlxXWfXPUCPT8t8kS/bXvPHd7vfmKYR2D2wOzs7u9v8vzc7O4/4f2P6zOJsQb0HzIf0bO/ubd/bfbN379H9h/S/MX3Uh3lLwjRtTRPb6+YXlu3hm7xpX/PXv/CC/poXTUr/y9KWPk/lvRTt8O3vcffRR4cff/y97//iX/LJZ7//9p2tb/3CbLE6/P1+99/tx3/P3/Vnfl80uxvCuy2Cs3xaLLLythMnrW83b6ZtB+j2g4d7ewe7n+7d393f+3T/3r0H9x/eu79/7+H9nXv37t9q9gYgvyfgW1KoKS6W+ezJddvnloPbwphU1W1JjKa3oq807IA7z8pG8dxMw+jbbb2OvHzLMTbzqm7PBnTJ9u6n9/ZvrU6m86y+JbnQ9FbkkoYdcF/chlTRNyFyX5tUZbW8GKLUg0/v355Qs2o9KW+tdbnxrYhlmvZmcfzpwwefPry3e2//4NO9e3u790637+04MdgktHGQEYhRgLckyHlZZW0f77PlebEsWiXSLcHckqza5S2oOoDcvfHu/u79h3uf7hIxb0XLOCAD58HeaQzMLYc+ME/vS0GPxW9Jx0AobkHNTUK0TZaA9P/ezr1PD+7vP3hw/+B2TLoJZh/kg69N5KhK+V1v+7avaW9J3FA534K6m7X5vb0Hn96KoBvBAMrXp+HNFuwmMlrDflsiep7AbUg47DiQi3k78g2D2N37+qQbcqD2dx7eP/h05+H+/U/H9x4ePDzYf0D/d67TTQRl7/O2xFTP+DaEjDvRz6s6X6TFqlkv0llVVnXaFG0KrEbptFo2+bTN23WdZrNiVTRT+NN5WbTj9OU8a/KyXJPDvUyX67LM0kV2scxG6bJaptN13dBX0vQni8tsQX9d4k/pZZRmZfGL1jl1Nb7VHMaxf3y6WLXXeFV/ft25HApl9nZ2723v3Ed4tLv3aJ+imU/H5FvcFq5GhbecTRtD3mI6h+LN7b3d/Qf7B/c+3b+VbAyBsVA+WD5uOfbhoOiW3f1/RW6+ai3r/6J1trCvkfQ0DQWuxXJazNZL+jDF5NQz6mC1rlnO5M9xelJnzYeIzddG/sV6OU0vizbLVZhbVgEi8xbddb3MxulPrIuGhkkN58V0TeohW1/Qn/maRrqui40DuC2z+R+73ylF8wr4L92QB6X8rt/y7hfdjNH/A4VP6g5qEgAA"));
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testRemoteReceiverDecompressor, LogLevel.Information, "Created decompressed string '<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>MethodWithAllDataTypeAsParameters</MethodName><Param' (truncated).");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testRemoteReceiverDecompressor, LogLevel.Debug, "Complete string content: '<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>MethodWithAllDataTypeAsParameters</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter><DataType>dateTimeArray</DataType><Data><ElementDataType>dateTime</ElementDataType><Element><DataType>dateTime</DataType><Data>0001-01-01T00:00:00.000</Data></Element><Element><DataType>dateTime</DataType><Data>9999-12-31T23:59:59.999</Data></Element></Data></Parameter><Parameter><DataType>string</DataType><Data>&lt;TestString&gt;This is a test string &lt;&gt;?/:\";''[]{}+=_-)(*&amp;^%$#@!|\\&lt;/TestString&gt;</Data></Parameter><Parameter><DataType>decimalArray</DataType><Data><ElementDataType>decimal</ElementDataType><Element><DataType>decimal</DataType><Data>-79228162514264337593543950335</Data></Element><Element><DataType>decimal</DataType><Data>79228162514264337593543950335</Data></Element></Data></Parameter><Parameter><DataType>signedByte</DataType><Data>8</Data></Parameter><Parameter><DataType>boolArray</DataType><Data><ElementDataType>bool</ElementDataType><Element><DataType>bool</DataType><Data>false</Data></Element><Element><DataType>bool</DataType><Data>true</Data></Element></Data></Parameter><Parameter><DataType>shortInteger</DataType><Data>-16343</Data></Parameter><Parameter><DataType>charArray</DataType><Data><ElementDataType>char</ElementDataType><Element><DataType>char</DataType><Data>M</Data></Element><Element><DataType>char</DataType><Data>&lt;</Data></Element></Data></Parameter><Parameter><DataType>longInteger</DataType><Data>76543</Data></Parameter><Parameter><DataType>doubleArray</DataType><Data><ElementDataType>double</ElementDataType><Element><DataType>double</DataType><Data>-1.6976931348623213E-308</Data></Element><Element><DataType>double</DataType><Data>1.6976931348623213E308</Data></Element></Data></Parameter><Parameter><DataType>float</DataType><Data>-Infinity</Data></Parameter><Parameter><DataType>floatArray</DataType><Data><ElementDataType>float</ElementDataType><Element><DataType>float</DataType><Data>-3.14159261E-38</Data></Element><Element><DataType>float</DataType><Data>3.14159272E38</Data></Element></Data></Parameter><Parameter><DataType>double</DataType><Data>Infinity</Data></Parameter><Parameter><DataType>longIntegerArray</DataType><Data><ElementDataType>longInteger</ElementDataType><Element><DataType>longInteger</DataType><Data>-9223372036854775808</Data></Element><Element><DataType>longInteger</DataType><Data>9223372036854775807</Data></Element></Data></Parameter><Parameter><DataType>char</DataType><Data>!</Data></Parameter><Parameter><DataType>shortIntegerArray</DataType><Data><ElementDataType>shortInteger</ElementDataType><Element><DataType>shortInteger</DataType><Data>-32768</Data></Element><Element><DataType>shortInteger</DataType><Data>32767</Data></Element></Data></Parameter><Parameter><DataType>bool</DataType><Data>true</Data></Parameter><Parameter><DataType>signedByteArray</DataType><Data><ElementDataType>signedByte</ElementDataType><Element><DataType>signedByte</DataType><Data>-128</Data></Element><Element><DataType>signedByte</DataType><Data>127</Data></Element></Data></Parameter><Parameter><DataType>decimal</DataType><Data>40958609456.39898479845</Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus in nulla magna, non cursus elit. Vivamus velit dolor, aliquet a.</Data></Element><Element><DataType>string</DataType><Data><Empty></Empty></Data></Element></Data></Parameter><Parameter><DataType>dateTime</DataType><Data>2013-05-01T12:43:56.654</Data></Parameter><Parameter><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType><Element><DataType>integer</DataType><Data>-2147483648</Data></Element><Element><DataType>integer</DataType><Data>2147483647</Data></Element></Data></Parameter><Parameter><DataType>decimalArray</DataType><Data><ElementDataType>decimal</ElementDataType></Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut aliquet quam sit amet massa tincidunt a interdum purus interdum. Cras.</Data></Element><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc vitae velit tellus, non interdum urna. Quisque vehicula augue eu mauris.</Data></Element><Element></Element></Data></Parameter><Parameter></Parameter></Parameters><ReturnType><DataType>dateTime</DataType></ReturnType></MethodInvocation>'.");
                Expect.Once.On(mockUnderlyingRemoteReceiver).Method("Receive").Will(Return.Value(""));
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testRemoteReceiverDecompressor, LogLevel.Information, "Created decompressed string ''.");
                Expect.Once.On(mockApplicationLogger).Method("Log").With(testRemoteReceiverDecompressor, LogLevel.Debug, "Complete string content: ''.");
            }

            testRemoteReceiverDecompressor.Receive();
            testRemoteReceiverDecompressor.Receive();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void CancelReceiveLoggingTest()
        {
            Expect.AtLeastOnce.On(mockUnderlyingRemoteReceiver);
            Expect.Once.On(mockApplicationLogger).Method("Log").With(testRemoteReceiverDecompressor, LogLevel.Information, "Receive operation cancelled.");

            testRemoteReceiverDecompressor.CancelReceive();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}

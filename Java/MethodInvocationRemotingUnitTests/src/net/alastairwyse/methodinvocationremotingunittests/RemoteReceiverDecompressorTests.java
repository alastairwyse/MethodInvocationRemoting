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

package net.alastairwyse.methodinvocationremotingunittests;

import static org.junit.Assert.*;

import java.util.zip.ZipException;

import org.junit.Before;
import org.junit.Test;

import static org.mockito.Mockito.*;
import net.alastairwyse.methodinvocationremoting.*;

public class RemoteReceiverDecompressorTests {
    private IRemoteReceiver mockRemoteReceiver;
    private RemoteReceiverDecompressor testRemoteReceiverDecompressor;
    private String compressedString;
    private String expectedUncompressedString;
    
    @Before
    public void setUp() throws Exception {
        mockRemoteReceiver = mock(IRemoteReceiver.class);
        testRemoteReceiverDecompressor = new RemoteReceiverDecompressor(mockRemoteReceiver);
        
        compressedString = "H4sIAAAAAAAAAN1Y23LbNhD9FURJnTQ1bwCvNi3XTfzgmSSTtkr70NsgJCxhhgQVENBEk+TfA4qSIpukspLfqoFGBHBwAJzFLhdKLz+WBVowWfNKXIw82x0hJrIq52J6MdLq1opHl+P0NVOzKr8RiyqjyiA3LW9oycbt459cza6K4iVVdLKcs6v6LZWmVxnq1NlBp9/ad57H6WbgmAvFpkymzrZl1Tf2MGnbxqmzM66PIqeKTXjJrqSkyw5Rel2wkgnVgafO/Z4NtIe7Q+u6rmetysR1z1bFNm3bNW+pDuBMzMfysEW8CSZnQWKKbZq6nEBhaiWNYTvTnBTqfMJq9fuq+2RqajNeI1MoUqYdteNQg2t6L52z0fnTp3/98+nLTxf/WT8+e35Cy/n5vz88efzzo89/NzDnLh/YcizjJS2ghmvRMLttsPdIrSjBOPZCHHg+Dn1CoiAhgU+SwCUkAFlvgPlAYqgJ+VSw/Jel6p6WGMrxvqqgEjdQkL4t8B7dLS1qBtGwd7SSumcwVKdZJdXNQCyxvJD44HCSzagEytVAQXK1wHt0ryFS9Y5sXO5oqYpKTIeUisIALlRe6fcFOOquwDDfXUM7VrTDJAoT4hE/DjHBHrm2iBuDnLafsoexlxAoyG1RUdVd94245YKr5UE0QFnXUwJUHVgcsT3fCxIcekZMkJb9RBueCF/30Rx0pDrkhyq4c8SBOt5xCoCa+5zIMm8CE/+xS8I48KMoiGGHdB9nlzI6WuTekPLomEgLFPducAaouz+aExyFIEH30jQsx2v4/TcY+MUOFXEnE4BIOJw4mBQTJt8whYePl24ogfLdJIhDN/GD0CZJnMR+ZL4BWNBV9gkVc50ZQ4TsT6JfVZKViM9rXaK8KiqJaq5Qs6pTlFWiZpliSktEcz7nddbk06zgykZvZ7RmRaFNwi2Q0EVBUUmngp4iUQmUaVmbrhb6B1/Q0tQWTbWd5RTRgn/QzExlg2zYv/r0upyrZTN0/XusLYeuMtj1iOUGzfXIw2e+uc2EtsktoLz8oPjCDwgtQ/dNC3t+5Mck9EG+MUSzZXmwfzz4UvQ/85t3anv0P2habocZ76lrc3HlIuO5FqYRNcaRuZlgruXKz9qqjV5IWj/EbY5e/BstMrTgirK1M6tVCGh9frtcLQW10a+a12abBjjjmTbhgeqpqTJtdqol37sB6GFzep/rcfpbs37xbcuDXu7sIp3OP0ZfAYVP6g5qEgAA";
        expectedUncompressedString = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>MethodWithAllDataTypeAsParameters</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter><DataType>dateTimeArray</DataType><Data><ElementDataType>dateTime</ElementDataType><Element><DataType>dateTime</DataType><Data>0001-01-01T00:00:00.000</Data></Element><Element><DataType>dateTime</DataType><Data>9999-12-31T23:59:59.999</Data></Element></Data></Parameter><Parameter><DataType>string</DataType><Data>&lt;TestString&gt;This is a test string &lt;&gt;?/:\";''[]{}+=_-)(*&amp;^%$#@!|\\&lt;/TestString&gt;</Data></Parameter><Parameter><DataType>decimalArray</DataType><Data><ElementDataType>decimal</ElementDataType><Element><DataType>decimal</DataType><Data>-79228162514264337593543950335</Data></Element><Element><DataType>decimal</DataType><Data>79228162514264337593543950335</Data></Element></Data></Parameter><Parameter><DataType>signedByte</DataType><Data>8</Data></Parameter><Parameter><DataType>boolArray</DataType><Data><ElementDataType>bool</ElementDataType><Element><DataType>bool</DataType><Data>false</Data></Element><Element><DataType>bool</DataType><Data>true</Data></Element></Data></Parameter><Parameter><DataType>shortInteger</DataType><Data>-16343</Data></Parameter><Parameter><DataType>charArray</DataType><Data><ElementDataType>char</ElementDataType><Element><DataType>char</DataType><Data>M</Data></Element><Element><DataType>char</DataType><Data>&lt;</Data></Element></Data></Parameter><Parameter><DataType>longInteger</DataType><Data>76543</Data></Parameter><Parameter><DataType>doubleArray</DataType><Data><ElementDataType>double</ElementDataType><Element><DataType>double</DataType><Data>-1.6976931348623213E-308</Data></Element><Element><DataType>double</DataType><Data>1.6976931348623213E308</Data></Element></Data></Parameter><Parameter><DataType>float</DataType><Data>-Infinity</Data></Parameter><Parameter><DataType>floatArray</DataType><Data><ElementDataType>float</ElementDataType><Element><DataType>float</DataType><Data>-3.14159261E-38</Data></Element><Element><DataType>float</DataType><Data>3.14159272E38</Data></Element></Data></Parameter><Parameter><DataType>double</DataType><Data>Infinity</Data></Parameter><Parameter><DataType>longIntegerArray</DataType><Data><ElementDataType>longInteger</ElementDataType><Element><DataType>longInteger</DataType><Data>-9223372036854775808</Data></Element><Element><DataType>longInteger</DataType><Data>9223372036854775807</Data></Element></Data></Parameter><Parameter><DataType>char</DataType><Data>!</Data></Parameter><Parameter><DataType>shortIntegerArray</DataType><Data><ElementDataType>shortInteger</ElementDataType><Element><DataType>shortInteger</DataType><Data>-32768</Data></Element><Element><DataType>shortInteger</DataType><Data>32767</Data></Element></Data></Parameter><Parameter><DataType>bool</DataType><Data>true</Data></Parameter><Parameter><DataType>signedByteArray</DataType><Data><ElementDataType>signedByte</ElementDataType><Element><DataType>signedByte</DataType><Data>-128</Data></Element><Element><DataType>signedByte</DataType><Data>127</Data></Element></Data></Parameter><Parameter><DataType>decimal</DataType><Data>40958609456.39898479845</Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus in nulla magna, non cursus elit. Vivamus velit dolor, aliquet a.</Data></Element><Element><DataType>string</DataType><Data><Empty></Empty></Data></Element></Data></Parameter><Parameter><DataType>dateTime</DataType><Data>2013-05-01T12:43:56.654</Data></Parameter><Parameter><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType><Element><DataType>integer</DataType><Data>-2147483648</Data></Element><Element><DataType>integer</DataType><Data>2147483647</Data></Element></Data></Parameter><Parameter><DataType>decimalArray</DataType><Data><ElementDataType>decimal</ElementDataType></Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut aliquet quam sit amet massa tincidunt a interdum purus interdum. Cras.</Data></Element><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc vitae velit tellus, non interdum urna. Quisque vehicula augue eu mauris.</Data></Element><Element></Element></Data></Parameter><Parameter></Parameter></Parameters><ReturnType><DataType>dateTime</DataType></ReturnType></MethodInvocation>";
    }
    
    @Test
    public void InvalidDecompressionBufferSizeArgument() throws Exception {
        try {
            testRemoteReceiverDecompressor = new RemoteReceiverDecompressor(mockRemoteReceiver, 0);
            fail("Exception was not thrown.");
        }
        catch (IllegalArgumentException e) {
            assertTrue(e.getMessage().contains("Argument 'decompressionBufferSize' must be greater than 0."));
        }
    }
    
    @Test
    public void ReceiveSuccessTests() throws Exception {
        // Test with the compressed string produced by the C# GZipStream base 64 converter classes
        String cSharpCompressedString = "H4sIAAAAAAAEAO29B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/Ih7/Hu8WZXqZ101RLT/7aHe881GaL6fVrFhefPbRuj3fPvjo9zh6/EXezqvZ2fKymmYttTSfvMgW+ZH8+t2inR+X5dOszd5cr/Lj5mVW07ctgX5812v92H3u/X702Lx4VCzb/CKvH9+1n/B3R7t79+Szo8d3vfdiIGZZm78pFvlxXWfXPUCPT8t8kS/bXvPHd7vfmKYR2D2wOzs7u9v8vzc7O4/4f2P6zOJsQb0HzIf0bO/ubd/bfbN379H9h/S/MX3Uh3lLwjRtTRPb6+YXlu3hm7xpX/PXv/CC/poXTUr/y9KWPk/lvRTt8O3vcffRR4cff/y97//iX/LJZ7//9p2tb/3CbLE6/P1+99/tx3/P3/Vnfl80uxvCuy2Cs3xaLLLythMnrW83b6ZtB+j2g4d7ewe7n+7d393f+3T/3r0H9x/eu79/7+H9nXv37t9q9gYgvyfgW1KoKS6W+ezJddvnloPbwphU1W1JjKa3oq807IA7z8pG8dxMw+jbbb2OvHzLMTbzqm7PBnTJ9u6n9/ZvrU6m86y+JbnQ9FbkkoYdcF/chlTRNyFyX5tUZbW8GKLUg0/v355Qs2o9KW+tdbnxrYhlmvZmcfzpwwefPry3e2//4NO9e3u790637+04MdgktHGQEYhRgLckyHlZZW0f77PlebEsWiXSLcHckqza5S2oOoDcvfHu/u79h3uf7hIxb0XLOCAD58HeaQzMLYc+ME/vS0GPxW9Jx0AobkHNTUK0TZaA9P/ezr1PD+7vP3hw/+B2TLoJZh/kg69N5KhK+V1v+7avaW9J3FA534K6m7X5vb0Hn96KoBvBAMrXp+HNFuwmMlrDflsiep7AbUg47DiQi3k78g2D2N37+qQbcqD2dx7eP/h05+H+/U/H9x4ePDzYf0D/d67TTQRl7/O2xFTP+DaEjDvRz6s6X6TFqlkv0llVVnXaFG0KrEbptFo2+bTN23WdZrNiVTRT+NN5WbTj9OU8a/KyXJPDvUyX67LM0kV2scxG6bJaptN13dBX0vQni8tsQX9d4k/pZZRmZfGL1jl1Nb7VHMaxf3y6WLXXeFV/ft25HApl9nZ2723v3Ed4tLv3aJ+imU/H5FvcFq5GhbecTRtD3mI6h+LN7b3d/Qf7B/c+3b+VbAyBsVA+WD5uOfbhoOiW3f1/RW6+ai3r/6J1trCvkfQ0DQWuxXJazNZL+jDF5NQz6mC1rlnO5M9xelJnzYeIzddG/sV6OU0vizbLVZhbVgEi8xbddb3MxulPrIuGhkkN58V0TeohW1/Qn/maRrqui40DuC2z+R+73ylF8wr4L92QB6X8rt/y7hfdjNH/A4VP6g5qEgAA";
        String compressedBlankString = "H4sIAAAAAAAAAAMAAAAAAAAAAAA=";
        String cSharpCompressedBlankString = "";
        
        when(mockRemoteReceiver.Receive())
            .thenReturn(compressedString)
            .thenReturn(cSharpCompressedString)
            .thenReturn(compressedBlankString)
            .thenReturn(cSharpCompressedBlankString);
        
        String firstReceivedMessage = testRemoteReceiverDecompressor.Receive();
        String secondReceivedMessage = testRemoteReceiverDecompressor.Receive();
        String thirdReceivedMessage = testRemoteReceiverDecompressor.Receive();
        String fourthReceivedMessage = testRemoteReceiverDecompressor.Receive();
        
        verify(mockRemoteReceiver, times(4)).Receive();
        verifyNoMoreInteractions(mockRemoteReceiver);
        assertEquals(expectedUncompressedString, firstReceivedMessage);
        assertEquals(expectedUncompressedString, secondReceivedMessage);
        assertEquals("", thirdReceivedMessage);
        assertEquals("", fourthReceivedMessage);
    }
    
    @Test
    public void ReceiveOneByteBufferSuccessTests() throws Exception {
        // Tests decompressing a message where the decompression buffer is 1 byte
        testRemoteReceiverDecompressor = new RemoteReceiverDecompressor(mockRemoteReceiver, 1);
        
        when(mockRemoteReceiver.Receive()).thenReturn(compressedString);
        
        String firstReceivedMessage = testRemoteReceiverDecompressor.Receive();
        
        verify(mockRemoteReceiver).Receive();
        verifyNoMoreInteractions(mockRemoteReceiver);
        assertEquals(expectedUncompressedString, firstReceivedMessage);
    }
    
    @Test
    public void ReceiveLargeBufferSuccessTests() throws Exception {
        // Tests decompressing a message where the decompression buffer is larger than the received message
        testRemoteReceiverDecompressor = new RemoteReceiverDecompressor(mockRemoteReceiver, 10240);
        
        when(mockRemoteReceiver.Receive()).thenReturn(compressedString);
        
        String firstReceivedMessage = testRemoteReceiverDecompressor.Receive();
        
        verify(mockRemoteReceiver).Receive();
        verifyNoMoreInteractions(mockRemoteReceiver);
        assertEquals(expectedUncompressedString, firstReceivedMessage);
    }
    
    @Test
    public void CancelSuccessTest() {
        testRemoteReceiverDecompressor.CancelReceive();
        
        verify(mockRemoteReceiver).CancelReceive();
        verifyNoMoreInteractions(mockRemoteReceiver);
    }
    
    @Test
    public void ReceiveInvalidCompressedMessage() throws Exception {
        compressedString = "H4sIAAAAAAAEAO29B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/Ih7/Hu8WZXqZ101RLT/7aHe881GaL6fVrFhefPbRuj3fPvjo9zh6/EXezqvZ2fKymmYttTSfvMgW+ZH8+t2inR+X5dOszd5cr/Lj5mVW07ctgX5812v92H3u/X702Lx4VCzb/CKvH9+1n/B3R7t79+Szo8d3vfdiIGZZm78pFvlxXWfXPUCPT8t8kS/bXvPHd7vfmKYR2D2wOzs7u9v8vzc7O4/4f2P6zOJsQb0HzIf0bO/ubd/bfbN379H9h/S/MX3Uh3lLwjRtTRPb6+YXlu3hm7xpX/PXv/CC/poXTUr/y9KWPk/lvRTt8O3vcffRR4cff/y97//iX/LJZ7//9p2tb/3CbLE6/P1+99/tx3/P3/Vnfl80uxvCuy2Cs3xaLLLythMnrW83b6ZtB+j2g4d7ewe7n+7d393f+3T/3r0H9x/eu79/7+H9nXv37t9q9gYgvyfgW1KoKS6W+ezJddvnloPbwphU1W1JjKa3oq807IA7z8pG8dxMw+jbbb2OvHzLMTbzqm7PBnTJ9u6n9/ZvrU6m86y+JbnQ9FbkkoYdcF/chlTRNyFyX5tUZbW8GKLUg0/v355Qs2o9KW+tdbnxrYhlmvZmcfzpwwefPry3e2//4NO9e3u790637+04MdgktHGQEYhRgLckyHlZZW0f77PlebEsWiXSLcHckqza5S2oOoDcvfHu/u79h3uf7hIxb0XLOCAD58HeaQzMLYc+ME/vS0GPxW9Jx0AobkHNTUK0TZaA9P/ezr1PD+7vP3hw/+B2TLoJZh/kg69N5KhK+V1v+7avaW9J3FA534K6m7X5vb0Hn96KoBvBAMrXp+HNFuwmMlrDflsiep7AbUg47DiQi3k78g2D2N37+qQbcqD2dx7eP/h05+H+/U/H9x4ePDzYf0D/d67TTQRl7/O2xFTP+DaEjDvRz6s6X6TFqlkv0llVVnXaFG0KrEbptFo2+bTN23WdZrNiVTRT+NN5WbTj9OU8a/KyXJPDvUyX67LM0kV2scxG6bJaptN13dBX0vQni8tsQX9d4k/pZZRmZfGL1jl1Nb7VHMaxf3y6WLXXeFV/ft25HApl9nZ2723v3Ed4tLv3aJ+imU/H5FvcFq5GhbecTRtD3mI6h+LN7b3d/Qf7B/c+3b+VbAyBsVA+WD5uOfbhoOiW3f1/RW6+ai3r/6J1trCvkfQ0DQWuxXJazNZL+jDF5NQz6mC1rlnO5M9xelJnzYeIddG/sV6OU0vizbLVZhbVgEi8xbddb3MxulPrIuGhkkN58V0TeohW1/Qn/maRrqui40DuC2z+R+73ylF8wr4L92QB6X8rt/y7hfdjNH/A4VP6g5qEgAA";
        
        when(mockRemoteReceiver.Receive()).thenReturn(compressedString);
        
        try {
            testRemoteReceiverDecompressor.Receive();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            assertTrue(e.getMessage().contains("Error decompressing message."));
            assertTrue(e.getCause().getMessage().contains("invalid distance too far back"));
            assertEquals(ZipException.class, e.getCause().getClass());
        }
    }
}

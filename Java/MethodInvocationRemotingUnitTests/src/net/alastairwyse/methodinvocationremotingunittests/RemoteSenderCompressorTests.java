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

import org.junit.Before;
import org.junit.Test;
import static org.mockito.Mockito.*;
import net.alastairwyse.methodinvocationremoting.*;

public class RemoteSenderCompressorTests {
    private IRemoteSender mockRemoteSender;
    private RemoteSenderCompressor testRemoteSenderCompressor;
    
    @Before
    public void setUp() throws Exception {
        mockRemoteSender = mock(IRemoteSender.class);
        testRemoteSenderCompressor = new RemoteSenderCompressor(mockRemoteSender);
    }
    
    @Test
    public void SendSuccessTests() throws Exception {
        String uncompressedString = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MethodInvocation><MethodName>MethodWithAllDataTypeAsParameters</MethodName><Parameters><Parameter><DataType>integer</DataType><Data>123</Data></Parameter><Parameter><DataType>dateTimeArray</DataType><Data><ElementDataType>dateTime</ElementDataType><Element><DataType>dateTime</DataType><Data>0001-01-01T00:00:00.000</Data></Element><Element><DataType>dateTime</DataType><Data>9999-12-31T23:59:59.999</Data></Element></Data></Parameter><Parameter><DataType>string</DataType><Data>&lt;TestString&gt;This is a test string &lt;&gt;?/:\";''[]{}+=_-)(*&amp;^%$#@!|\\&lt;/TestString&gt;</Data></Parameter><Parameter><DataType>decimalArray</DataType><Data><ElementDataType>decimal</ElementDataType><Element><DataType>decimal</DataType><Data>-79228162514264337593543950335</Data></Element><Element><DataType>decimal</DataType><Data>79228162514264337593543950335</Data></Element></Data></Parameter><Parameter><DataType>signedByte</DataType><Data>8</Data></Parameter><Parameter><DataType>boolArray</DataType><Data><ElementDataType>bool</ElementDataType><Element><DataType>bool</DataType><Data>false</Data></Element><Element><DataType>bool</DataType><Data>true</Data></Element></Data></Parameter><Parameter><DataType>shortInteger</DataType><Data>-16343</Data></Parameter><Parameter><DataType>charArray</DataType><Data><ElementDataType>char</ElementDataType><Element><DataType>char</DataType><Data>M</Data></Element><Element><DataType>char</DataType><Data>&lt;</Data></Element></Data></Parameter><Parameter><DataType>longInteger</DataType><Data>76543</Data></Parameter><Parameter><DataType>doubleArray</DataType><Data><ElementDataType>double</ElementDataType><Element><DataType>double</DataType><Data>-1.6976931348623213E-308</Data></Element><Element><DataType>double</DataType><Data>1.6976931348623213E308</Data></Element></Data></Parameter><Parameter><DataType>float</DataType><Data>-Infinity</Data></Parameter><Parameter><DataType>floatArray</DataType><Data><ElementDataType>float</ElementDataType><Element><DataType>float</DataType><Data>-3.14159261E-38</Data></Element><Element><DataType>float</DataType><Data>3.14159272E38</Data></Element></Data></Parameter><Parameter><DataType>double</DataType><Data>Infinity</Data></Parameter><Parameter><DataType>longIntegerArray</DataType><Data><ElementDataType>longInteger</ElementDataType><Element><DataType>longInteger</DataType><Data>-9223372036854775808</Data></Element><Element><DataType>longInteger</DataType><Data>9223372036854775807</Data></Element></Data></Parameter><Parameter><DataType>char</DataType><Data>!</Data></Parameter><Parameter><DataType>shortIntegerArray</DataType><Data><ElementDataType>shortInteger</ElementDataType><Element><DataType>shortInteger</DataType><Data>-32768</Data></Element><Element><DataType>shortInteger</DataType><Data>32767</Data></Element></Data></Parameter><Parameter><DataType>bool</DataType><Data>true</Data></Parameter><Parameter><DataType>signedByteArray</DataType><Data><ElementDataType>signedByte</ElementDataType><Element><DataType>signedByte</DataType><Data>-128</Data></Element><Element><DataType>signedByte</DataType><Data>127</Data></Element></Data></Parameter><Parameter><DataType>decimal</DataType><Data>40958609456.39898479845</Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus in nulla magna, non cursus elit. Vivamus velit dolor, aliquet a.</Data></Element><Element><DataType>string</DataType><Data><Empty></Empty></Data></Element></Data></Parameter><Parameter><DataType>dateTime</DataType><Data>2013-05-01T12:43:56.654</Data></Parameter><Parameter><DataType>integerArray</DataType><Data><ElementDataType>integer</ElementDataType><Element><DataType>integer</DataType><Data>-2147483648</Data></Element><Element><DataType>integer</DataType><Data>2147483647</Data></Element></Data></Parameter><Parameter><DataType>decimalArray</DataType><Data><ElementDataType>decimal</ElementDataType></Data></Parameter><Parameter><DataType>stringArray</DataType><Data><ElementDataType>string</ElementDataType><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut aliquet quam sit amet massa tincidunt a interdum purus interdum. Cras.</Data></Element><Element><DataType>string</DataType><Data>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc vitae velit tellus, non interdum urna. Quisque vehicula augue eu mauris.</Data></Element><Element></Element></Data></Parameter><Parameter></Parameter></Parameters><ReturnType><DataType>dateTime</DataType></ReturnType></MethodInvocation>";
        String expectedCompressedString = "H4sIAAAAAAAAAN1Y23LbNhD9FURJnTQ1bwCvNi3XTfzgmSSTtkr70NsgJCxhhgQVENBEk+TfA4qSIpukspLfqoFGBHBwAJzFLhdKLz+WBVowWfNKXIw82x0hJrIq52J6MdLq1opHl+P0NVOzKr8RiyqjyiA3LW9oycbt459cza6K4iVVdLKcs6v6LZWmVxnq1NlBp9/ad57H6WbgmAvFpkymzrZl1Tf2MGnbxqmzM66PIqeKTXjJrqSkyw5Rel2wkgnVgafO/Z4NtIe7Q+u6rmetysR1z1bFNm3bNW+pDuBMzMfysEW8CSZnQWKKbZq6nEBhaiWNYTvTnBTqfMJq9fuq+2RqajNeI1MoUqYdteNQg2t6L52z0fnTp3/98+nLTxf/WT8+e35Cy/n5vz88efzzo89/NzDnLh/YcizjJS2ghmvRMLttsPdIrSjBOPZCHHg+Dn1CoiAhgU+SwCUkAFlvgPlAYqgJ+VSw/Jel6p6WGMrxvqqgEjdQkL4t8B7dLS1qBtGwd7SSumcwVKdZJdXNQCyxvJD44HCSzagEytVAQXK1wHt0ryFS9Y5sXO5oqYpKTIeUisIALlRe6fcFOOquwDDfXUM7VrTDJAoT4hE/DjHBHrm2iBuDnLafsoexlxAoyG1RUdVd94245YKr5UE0QFnXUwJUHVgcsT3fCxIcekZMkJb9RBueCF/30Rx0pDrkhyq4c8SBOt5xCoCa+5zIMm8CE/+xS8I48KMoiGGHdB9nlzI6WuTekPLomEgLFPducAaouz+aExyFIEH30jQsx2v4/TcY+MUOFXEnE4BIOJw4mBQTJt8whYePl24ogfLdJIhDN/GD0CZJnMR+ZL4BWNBV9gkVc50ZQ4TsT6JfVZKViM9rXaK8KiqJaq5Qs6pTlFWiZpliSktEcz7nddbk06zgykZvZ7RmRaFNwi2Q0EVBUUmngp4iUQmUaVmbrhb6B1/Q0tQWTbWd5RTRgn/QzExlg2zYv/r0upyrZTN0/XusLYeuMtj1iOUGzfXIw2e+uc2EtsktoLz8oPjCDwgtQ/dNC3t+5Mck9EG+MUSzZXmwfzz4UvQ/85t3anv0P2habocZ76lrc3HlIuO5FqYRNcaRuZlgruXKz9qqjV5IWj/EbY5e/BstMrTgirK1M6tVCGh9frtcLQW10a+a12abBjjjmTbhgeqpqTJtdqol37sB6GFzep/rcfpbs37xbcuDXu7sIp3OP0ZfAYVP6g5qEgAA";
        String expectedCompressedBlankString = "H4sIAAAAAAAAAAMAAAAAAAAAAAA=";
        
        testRemoteSenderCompressor.Send(uncompressedString);
        testRemoteSenderCompressor.Send("");
        
        verify(mockRemoteSender).Send(expectedCompressedString);
        verify(mockRemoteSender).Send(expectedCompressedBlankString);
        verifyNoMoreInteractions(mockRemoteSender);
    }
}

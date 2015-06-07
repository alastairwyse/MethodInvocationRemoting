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

package net.alastairwyse.methodinvocationremotingmetricstests;

import org.junit.Before;
import org.junit.Test;

import static org.junit.Assert.fail;
import static org.mockito.Matchers.isA;
import static org.mockito.Mockito.*;
import net.alastairwyse.applicationmetrics.*;
import net.alastairwyse.methodinvocationremoting.*;
import net.alastairwyse.methodinvocationremotingmetrics.*;

/**
 * Unit tests for the metrics logging functionality in class methodinvocationremoting.RemoteReceiverDecompressor.
 * @author Alastair Wyse
 */
public class RemoteReceiverDecompressorMetricsTests {

    private IRemoteReceiver mockUnderlyingRemoteReceiver;
    private IMetricLogger mockMetricLogger;
    private RemoteReceiverDecompressor testRemoteReceiverDecompressor;
    
    @Before
    public void setUp() throws Exception {
        mockUnderlyingRemoteReceiver = mock(IRemoteReceiver.class);
        mockMetricLogger = mock(IMetricLogger.class);
        testRemoteReceiverDecompressor = new RemoteReceiverDecompressor(mockUnderlyingRemoteReceiver, mockMetricLogger);
    }
    
    @Test
    public void ReceiveMetricsTest() throws Exception {
        when(mockUnderlyingRemoteReceiver.Receive())
            .thenReturn("H4sIAAAAAAAAAN1Y23LbNhD9FURJnTQ1bwCvNi3XTfzgmSSTtkr70NsgJCxhhgQVENBEk+TfA4qSIpukspLfqoFGBHBwAJzFLhdKLz+WBVowWfNKXIw82x0hJrIq52J6MdLq1opHl+P0NVOzKr8RiyqjyiA3LW9oycbt459cza6K4iVVdLKcs6v6LZWmVxnq1NlBp9/ad57H6WbgmAvFpkymzrZl1Tf2MGnbxqmzM66PIqeKTXjJrqSkyw5Rel2wkgnVgafO/Z4NtIe7Q+u6rmetysR1z1bFNm3bNW+pDuBMzMfysEW8CSZnQWKKbZq6nEBhaiWNYTvTnBTqfMJq9fuq+2RqajNeI1MoUqYdteNQg2t6L52z0fnTp3/98+nLTxf/WT8+e35Cy/n5vz88efzzo89/NzDnLh/YcizjJS2ghmvRMLttsPdIrSjBOPZCHHg+Dn1CoiAhgU+SwCUkAFlvgPlAYqgJ+VSw/Jel6p6WGMrxvqqgEjdQkL4t8B7dLS1qBtGwd7SSumcwVKdZJdXNQCyxvJD44HCSzagEytVAQXK1wHt0ryFS9Y5sXO5oqYpKTIeUisIALlRe6fcFOOquwDDfXUM7VrTDJAoT4hE/DjHBHrm2iBuDnLafsoexlxAoyG1RUdVd94245YKr5UE0QFnXUwJUHVgcsT3fCxIcekZMkJb9RBueCF/30Rx0pDrkhyq4c8SBOt5xCoCa+5zIMm8CE/+xS8I48KMoiGGHdB9nlzI6WuTekPLomEgLFPducAaouz+aExyFIEH30jQsx2v4/TcY+MUOFXEnE4BIOJw4mBQTJt8whYePl24ogfLdJIhDN/GD0CZJnMR+ZL4BWNBV9gkVc50ZQ4TsT6JfVZKViM9rXaK8KiqJaq5Qs6pTlFWiZpliSktEcz7nddbk06zgykZvZ7RmRaFNwi2Q0EVBUUmngp4iUQmUaVmbrhb6B1/Q0tQWTbWd5RTRgn/QzExlg2zYv/r0upyrZTN0/XusLYeuMtj1iOUGzfXIw2e+uc2EtsktoLz8oPjCDwgtQ/dNC3t+5Mck9EG+MUSzZXmwfzz4UvQ/85t3anv0P2habocZ76lrc3HlIuO5FqYRNcaRuZlgruXKz9qqjV5IWj/EbY5e/BstMrTgirK1M6tVCGh9frtcLQW10a+a12abBjjjmTbhgeqpqTJtdqol37sB6GFzep/rcfpbs37xbcuDXu7sIp3OP0ZfAYVP6g5qEgAA")
            .thenReturn("H4sIAAAAAAAAAAMAAAAAAAAAAAA=");

        testRemoteReceiverDecompressor.Receive();
        testRemoteReceiverDecompressor.Receive();

        verify(mockMetricLogger, times(2)).Begin(isA(StringDecompressTime.class));
        verify(mockMetricLogger, times(6)).Increment(isA(RemoteReceiverDecompressorReadBufferCreated.class));
        verify(mockMetricLogger, times(2)).End(isA(StringDecompressTime.class));
        verify(mockMetricLogger, times(2)).Increment(isA(StringDecompressed.class));
        verifyNoMoreInteractions(mockMetricLogger);
    }
    
    @Test
    public void ReceiveExceptionMetricsTest() throws Exception {
        when(mockUnderlyingRemoteReceiver.Receive()).thenReturn("InvalidCompressedString");
        
        try {
            testRemoteReceiverDecompressor.Receive();
            fail("Exception was not thrown.");
        }
        catch (Exception e) {
            verify(mockMetricLogger).Begin(isA(StringDecompressTime.class));
            verify(mockMetricLogger).CancelBegin(isA(StringDecompressTime.class));
            verifyNoMoreInteractions(mockMetricLogger);
        }
    }
}

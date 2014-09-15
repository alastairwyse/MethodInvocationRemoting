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
using ApplicationMetrics;

namespace ApplicationMetricsUnitTests
{
    public class BlankNameMetric : CountMetric
    {
        public BlankNameMetric()
        {
            base.name = "";
            base.description = "Description";
        }
    }

    public class LongNameMetric : CountMetric
    {
        public LongNameMetric()
        {
            base.name = "012345678901234567890123456789012345678901234567890123456789012345678901234567890";
            base.description = "Description";
        }
    }

    public class WhitespaceNameMetric : CountMetric
    {
        public WhitespaceNameMetric()
        {
            base.name = " WhitespaceNameMetric ";
            base.description = "Description";
        }
    }

    public class DoubleQuoteNameMetric : CountMetric
    {
        public DoubleQuoteNameMetric()
        {
            base.name = "A\"B";
            base.description = "Description";
        }
    }

    public class ControlCharacterNameMetric : CountMetric
    {
        public ControlCharacterNameMetric()
        {
            char controlCharacter = (char)0x02;
            base.name = "A" + controlCharacter.ToString() + "B";
            base.description = "Description";
        }
    }
}
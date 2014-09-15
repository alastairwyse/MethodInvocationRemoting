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
using NMock2;
using ApplicationMetrics;

namespace MethodInvocationRemotingMetricsTests
{
    //******************************************************************************
    //
    // Class: AmountMetricMatcher
    //
    //******************************************************************************
    /// <summary>
    /// Extension of the NMock2.Matcher class to compare ApplicationMetrics.AmountMetric objects.
    /// </summary>
    /// <remarks>See http://nmock.sourceforge.net/advanced.html for explanation of implementation.</remarks>
    class AmountMetricMatcher : Matcher
    {
        // Note that if implemented as described in the url in the remarks, if the value of the AmountMetric passed by the test differs from the expected value, the resulting error is very cryptic (results in simply and 'unexpected invocation' error, without calling the DescribeTo() method to explain the reason).  
        //   Hence, contrary to the sample code, I throw exceptions in the Matches() method which give a much more detailed description of the reason for not matching.

        private AmountMetric amountMetric;

        public AmountMetricMatcher(AmountMetric amountMetric)
        {
            this.amountMetric = amountMetric;
        }

        public override void DescribeTo(System.IO.TextWriter writer)
        {
        }

        public override bool Matches(object o)
        {
            if (amountMetric.GetType() != o.GetType())
            {
                throw new Exception("Was expecting AmountMetric parameter of type '" + amountMetric.GetType().Name + "' but instead received '" + o.GetType().FullName + "'.");
            }

            AmountMetric comparisonAmountMetric = (AmountMetric)o;
            if (amountMetric.Amount != comparisonAmountMetric.Amount)
            {
                throw new Exception(amountMetric.GetType().Name + " value was expected to be " + amountMetric.Amount + " but was " + comparisonAmountMetric.Amount + ".");
            }

            return true;
        }
    }
}

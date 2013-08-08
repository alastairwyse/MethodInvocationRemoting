/*
 * Copyright 2013 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
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

namespace SampleApplication3
{
    //******************************************************************************
    //
    // Class: InterestRateCurve
    //
    //******************************************************************************
    /// <summary>
    /// Represents an interest rate curve.
    /// </summary>
    public class InterestRateCurve
    {
        private string currency;
        private Dictionary<int, double> curve;

        //------------------------------------------------------------------------------
        //
        // Method: InterestRateCurve (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the SampleApplication3.InterestRateCurve class.
        /// </summary>
        public InterestRateCurve()
        {
            curve = new Dictionary<int, double>();
        }

        /// <summary>
        /// The currency of the interest rate curve.
        /// </summary>
        public string Currency
        {
            get
            {
                return currency;
            }
            set
            {
                currency = value;
            }
        }

        /// <summary>
        /// The interest rate curve.
        /// </summary>
        public Dictionary<int, double> Curve
        {
            get
            {
                return curve;
            }
        }

        /// <summary>
        /// Adds a data point to the curve.
        /// </summary>
        /// <param name="term">The term or time dimension of the data point in months.</param>
        /// <param name="rate">The interest rate of the data point.</param>
        public void AddPoint(int term, double rate)
        {
            curve.Add(term, rate);
        }
    }
}

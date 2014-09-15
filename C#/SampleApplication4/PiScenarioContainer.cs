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

namespace SampleApplication4
{
    //******************************************************************************
    //
    // Class: PiScenarioContainer
    //
    //******************************************************************************
    /// <summary>
    /// Container class which holds data used to calculate an approximation of Pi
    /// </summary>
    public class PiScenarioContainer
    {
        private double[] scenarios;
        private decimal piApproximation;

        /// <summary>
        /// The individual Monte Carlo scenario results used to approximate Pi.
        /// </summary>
        public double[] Scenarios
        {
            get
            {
                return scenarios;
            }

            set
            {
                scenarios = value;
            }
        }

        /// <summary>
        /// The approximation of Pi.
        /// </summary>
        public decimal PiApproximation
        {
            get
            {
                return piApproximation;
            }

            set
            {
                piApproximation = value;
            }
        }

        //******************************************************************************
        //
        // Method: PiScenarioContainer (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the SampleApplication4.PiScenarioContainer class.
        /// </summary>
        /// <param name="scenarios">The individual Monte Carlo scenario results used to approximate Pi.</param>
        /// <param name="piApproximation">The approximation of Pi.</param>
        public PiScenarioContainer(double[] scenarios, decimal piApproximation)
        {
            this.scenarios = scenarios;
            this.piApproximation = piApproximation;
        }
    }
}

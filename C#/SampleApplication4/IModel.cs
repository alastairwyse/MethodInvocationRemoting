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
    // Interface: IModel
    //
    //******************************************************************************
    /// <summary>
    /// Defines methods available on the model layer of the Pi Approximator application.
    /// </summary>
    interface IModel
    {
        //------------------------------------------------------------------------------
        //
        // Method: ApproximatePi
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Calculates an approximate value of Pi using Monte Carlo simulation.
        /// </summary>
        /// <param name="scenarioCount">The number of Monte Carlo scenarios to use to approximate Pi.</param>
        /// <returns>A container class containing the scenario results, and the approximate value of Pi.</returns>
        PiScenarioContainer ApproximatePi(int scenarioCount);

        //------------------------------------------------------------------------------
        //
        // Method: ValidateScenarioCount
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Validates that the inputted string representing the number of scenarios is a positive integer in the allowed range.
        /// </summary>
        /// <param name="scenarioCount">A string containing the value to be validated.</param>
        /// <returns>The results of the validation.</returns>
        ValidationResult ValidateScenarioCount(string scenarioCount);
    }
}

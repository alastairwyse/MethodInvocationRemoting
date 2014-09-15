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
using MethodInvocationRemoting;

namespace SampleApplication4
{
    //******************************************************************************
    //
    // Class: Model
    //
    //******************************************************************************
    /// <summary>
    /// The model layer of the Pi Approximator application.
    /// </summary>
    public class Model : IModel
    {
        private static int maxScenarioLimit = 2000;
        private IMethodInvocationRemoteSender methodInvocationSender;

        //******************************************************************************
        //
        // Method: Model (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the SampleApplication4.Model class.
        /// </summary>
        /// <param name="methodInvocationSender">The method invocation remote sender to use generate the Monte Carlo scenarios.</param>
        public Model(IMethodInvocationRemoteSender methodInvocationSender)
        {
            this.methodInvocationSender = methodInvocationSender;
        }

        public PiScenarioContainer ApproximatePi(int scenarioCount)
        {
            // Generate the scenarios via remote method call
            double[] scenarios = (double[])methodInvocationSender.InvokeMethod(new MethodInvocation("GenerateScenarios", new object[] { scenarioCount }, typeof(double[])));
            
            // Calculate the number of 'hits', i.e. scenarios with value <= 1
            int hits = 0;
            foreach (double currentScenario in scenarios)
            {
                if (currentScenario <= 1.0)
                {
                    hits++;
                }
            }

            // Calculate value of Pi
            decimal piValue = Decimal.Divide((Decimal)hits, (Decimal)scenarios.Length);
            piValue = Decimal.Multiply(piValue, 4m);

            return new PiScenarioContainer(scenarios, piValue);
        }

        public ValidationResult ValidateScenarioCount(string scenarioCount)
        {
            ValidationResult returnValidationResult;
            string validationFailedMessage = "The scenario count value must be a positive integer less than " + maxScenarioLimit + ".";
            int integerScenarioCount;
            bool isInteger = Int32.TryParse(scenarioCount, out integerScenarioCount);
            if (isInteger == false)
            {
                returnValidationResult = new ValidationResult(false, validationFailedMessage);
            }
            else
            {
                if ((integerScenarioCount < 1) || (integerScenarioCount > maxScenarioLimit))
                {
                    returnValidationResult = new ValidationResult(false, validationFailedMessage);
                }
                else
                {
                    returnValidationResult = new ValidationResult(true, "");
                }
            }
            return returnValidationResult;
        }
    }
}

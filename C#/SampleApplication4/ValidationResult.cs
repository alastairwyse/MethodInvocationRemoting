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
    // Class: ValidationResult
    //
    //******************************************************************************
    /// <summary>
    /// Contains the results of validating a data item.
    /// </summary>
    public class ValidationResult
    {
        private bool isValid;
        private string validationError;

        /// <summary>
        /// Whether the validation was successful.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return isValid;
            }
        }

        /// <summary>
        /// The error message generated in the case that the validation failed.
        /// </summary>
        public string ValidationError
        {
            get
            {
                return validationError;
            }
        }

        //------------------------------------------------------------------------------
        //
        // Method: ValidationResult (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the SampleApplication4.ValidationResult class.
        /// </summary>
        public ValidationResult(bool isValid, string validationError)
        {
            this.isValid = isValid;
            this.validationError = validationError;
        }
    }
}

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
    // Interface: IPresenter
    //
    //******************************************************************************
    /// <summary>
    /// Defines methods available on the presenter component of the Pi Approximator application.
    /// </summary>
    interface IPresenter
    {
        /// <summary>
        /// Indicates whether a process to exit the application has been initiated.
        /// </summary>
        bool ExitInitiated
        {
            get;
        }

        //------------------------------------------------------------------------------
        //
        // Method: ApproximatePi
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Calls the model layer to approximate the value of Pi, and updates the view layer with the result.
        /// </summary>
        /// <param name="numberOfScenarios">The number of senarios to use in the approximation.</param>
        void ApproximatePi(string numberOfScenarios);

        //------------------------------------------------------------------------------
        //
        // Method: Exit
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Exits from the application.
        /// </summary>
        void Exit();
    }
}

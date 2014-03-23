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
    // Interface: IMainView
    //
    //******************************************************************************
    /// <summary>
    /// Defines methods available on the main view of the Pi Approximator application.
    /// </summary>
    interface IMainView
    {
        /// <summary>
        /// The presenter component associated with this view.
        /// </summary>
        IPresenter Presenter
        {
            set;
        }

        //------------------------------------------------------------------------------
        //
        // Method: ClearPiValue
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Clears the field containing the approximated value of Pi.
        /// </summary>
        void ClearPiValue();

        //------------------------------------------------------------------------------
        //
        // Method: PopulatePiValue
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Populates the field containing the approximated value of Pi.
        /// </summary>
        /// <param name="piValue">The approximated value of Pi.</param>
        void PopulatePiValue(decimal piValue);

        //------------------------------------------------------------------------------
        //
        // Method: AddScenarioValue
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Adds an entry to the grid holding the scenario values.
        /// </summary>
        /// <param name="scenarioNumber">The number of the scenario.</param>
        /// <param name="scenarioValue">The value of the scenario.</param>
        void AddScenarioValue(int scenarioNumber, double scenarioValue);

        //------------------------------------------------------------------------------
        //
        // Method: ClearScenarios
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Removes all entries from the grid holding the scenario values.
        /// </summary>
        void ClearScenarios();

        //------------------------------------------------------------------------------
        //
        // Method: Disable
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Disables all controls in the view.
        /// </summary>
        void Disable();

        //------------------------------------------------------------------------------
        //
        // Method: Enable
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Enables all controls in the view.
        /// </summary>
        void Enable();

        //------------------------------------------------------------------------------
        //
        // Method: ShowErrorMessage
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Displays an error message in a diaglog box.
        /// </summary>
        void ShowErrorMessage(string message);

        //------------------------------------------------------------------------------
        //
        // Method: Close
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Closes the view.
        /// </summary>
        void Close();
    }
}

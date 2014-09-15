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
using System.ComponentModel;

namespace SampleApplication4
{
    //******************************************************************************
    //
    // Class: Presenter
    //
    //******************************************************************************
    /// <summary>
    /// The presenter layer of the Pi Approximator application.
    /// </summary>
    public class Presenter : IPresenter
    {
        private IMainView mainView;
        private IModel model;
        private bool exitInitiated;

        public bool ExitInitiated
        {
            get
            {
                return exitInitiated;
            }
        }

        //******************************************************************************
        //
        // Method: Presenter (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the SampleApplication4.Presenter class.
        /// </summary>
        /// <param name="mainView">The main view to associate with the presenter.</param>
        /// <param name="model">The model layer to associate with the presenter.</param>
        public Presenter(IMainView mainView, IModel model)
        {
            this.mainView = mainView;
            this.model = model;
            exitInitiated = false;
        }

        public void ApproximatePi(string numberOfScenarios)
        {
            // Validate the number of scenarios
            ValidationResult validationResult = model.ValidateScenarioCount(numberOfScenarios);
            if (validationResult.IsValid == false)
            {
                mainView.ShowErrorMessage(validationResult.ValidationError);
            }
            else
            {
                // Clear existing data in the view
                mainView.ClearPiValue();
                mainView.ClearScenarios();
                mainView.Disable();

                // Run the approximation in a worker thread
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += new DoWorkEventHandler(ApproximatePiWorkerThreadRoutine);
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ApproximatePiWorkCompleteRoutine);
                worker.RunWorkerAsync(Convert.ToInt32(numberOfScenarios));
            }
        }

        public void Exit()
        {
            exitInitiated = true;
            mainView.Close();
        }

        /// <summary>
        /// DoWorkEventHandler for public method ApproximatePi.
        /// </summary>
        private void ApproximatePiWorkerThreadRoutine(object sender, DoWorkEventArgs e)
        {
            // Call the model layer to calculate the scenarios
            int numberOfScenarios = (int)e.Argument;
            PiScenarioContainer scenarioContainer = model.ApproximatePi(numberOfScenarios);
            e.Result = scenarioContainer;
        }

        /// <summary>
        /// RunWorkerCompletedEventHandler for public method ApproximatePi.
        /// </summary>
        private void ApproximatePiWorkCompleteRoutine(object sender, RunWorkerCompletedEventArgs e)
        {
            // If an exeception occurred during the worker thread execution, handle it
            if (e.Error != null)
            {
                mainView.ShowErrorMessage(e.Error.Message);
                mainView.Enable();
            }
            else
            {
                // Populate the view
                PiScenarioContainer scenarioContainer = (PiScenarioContainer)e.Result;
                for (int i = 0; i < scenarioContainer.Scenarios.Length; i = i + 1)
                {
                    mainView.AddScenarioValue(i + 1, scenarioContainer.Scenarios[i]);
                }
                mainView.PopulatePiValue(scenarioContainer.PiApproximation);
                mainView.Enable();
            }
        }
    }
}

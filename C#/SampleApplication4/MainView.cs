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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SampleApplication4
{
    //******************************************************************************
    //
    // Class: MainView
    //
    //******************************************************************************
    /// <summary>
    /// The main view of the Pi Approximator application.
    /// </summary>
    public partial class MainView : Form, IMainView
    {
        private IPresenter presenter;

        public IPresenter Presenter
        {
            set
            {
                this.presenter = value;
            }
        }

        //******************************************************************************
        //
        // Method: MainView (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the SampleApplication4.MainView class.
        /// </summary>
        public MainView()
        {
            InitializeComponent();
        }

        public void ClearPiValue()
        {
            piValueTextBox.Text = "";
        }

        public void PopulatePiValue(decimal piValue)
        {
            piValueTextBox.Text = piValue.ToString();
        }

        public void AddScenarioValue(int scenarioNumber, double scenarioValue)
        {
            DataGridViewRow newRow = new DataGridViewRow();
            newRow.CreateCells(scenarioValuesGrid);
            newRow.Cells[scenarioValuesGrid.Columns["numberColumn"].Index].Value = scenarioNumber;
            newRow.Cells[scenarioValuesGrid.Columns["valueColumn"].Index].Value = scenarioValue;
            scenarioValuesGrid.Rows.Add(newRow);
        }

        public void ClearScenarios()
        {
            scenarioValuesGrid.Rows.Clear();
        }

        public void Disable()
        {
            scenarioCountTextBox.Enabled = false;
            calculateButton.Enabled = false;
            piValueTextBox.Enabled = false;
            Cursor = Cursors.WaitCursor;
        }

        public void Enable()
        {
            scenarioCountTextBox.Enabled = true;
            calculateButton.Enabled = true;
            piValueTextBox.Enabled = true;
            Cursor = Cursors.Default;
        }

        public void ShowErrorMessage(string message)
        {
            MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        new void Close()
        {
            base.Close();
        }

        private void calculateButton_Click(object sender, EventArgs e)
        {
            presenter.ApproximatePi(scenarioCountTextBox.Text);
        }

        private void MainView_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (presenter.ExitInitiated == false)
            {
                presenter.Exit();
            }
        }
    }
}
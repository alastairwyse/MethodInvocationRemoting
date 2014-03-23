namespace SampleApplication4
{
    partial class MainView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.calculateButton = new System.Windows.Forms.Button();
            this.scenarioCountTextBox = new System.Windows.Forms.TextBox();
            this.scenarioCountLabel = new System.Windows.Forms.Label();
            this.piValueTextBox = new System.Windows.Forms.TextBox();
            this.piValueLabel = new System.Windows.Forms.Label();
            this.scenarioValuesGrid = new System.Windows.Forms.DataGridView();
            this.scenarioValuesLabel = new System.Windows.Forms.Label();
            this.inputsGroupBox = new System.Windows.Forms.GroupBox();
            this.resultsGroupBox = new System.Windows.Forms.GroupBox();
            this.numberColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.valueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.scenarioValuesGrid)).BeginInit();
            this.inputsGroupBox.SuspendLayout();
            this.resultsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // calculateButton
            // 
            this.calculateButton.Location = new System.Drawing.Point(293, 35);
            this.calculateButton.Name = "calculateButton";
            this.calculateButton.Size = new System.Drawing.Size(75, 23);
            this.calculateButton.TabIndex = 0;
            this.calculateButton.Text = "Calculate";
            this.calculateButton.UseVisualStyleBackColor = true;
            this.calculateButton.Click += new System.EventHandler(this.calculateButton_Click);
            // 
            // scenarioCountTextBox
            // 
            this.scenarioCountTextBox.Location = new System.Drawing.Point(118, 15);
            this.scenarioCountTextBox.Name = "scenarioCountTextBox";
            this.scenarioCountTextBox.Size = new System.Drawing.Size(152, 20);
            this.scenarioCountTextBox.TabIndex = 1;
            // 
            // scenarioCountLabel
            // 
            this.scenarioCountLabel.AutoSize = true;
            this.scenarioCountLabel.Location = new System.Drawing.Point(8, 19);
            this.scenarioCountLabel.Name = "scenarioCountLabel";
            this.scenarioCountLabel.Size = new System.Drawing.Size(106, 13);
            this.scenarioCountLabel.TabIndex = 2;
            this.scenarioCountLabel.Text = "Number of Scenarios";
            // 
            // piValueTextBox
            // 
            this.piValueTextBox.Location = new System.Drawing.Point(133, 13);
            this.piValueTextBox.Name = "piValueTextBox";
            this.piValueTextBox.ReadOnly = true;
            this.piValueTextBox.Size = new System.Drawing.Size(185, 20);
            this.piValueTextBox.TabIndex = 3;
            // 
            // piValueLabel
            // 
            this.piValueLabel.AutoSize = true;
            this.piValueLabel.Location = new System.Drawing.Point(8, 16);
            this.piValueLabel.Name = "piValueLabel";
            this.piValueLabel.Size = new System.Drawing.Size(119, 13);
            this.piValueLabel.TabIndex = 4;
            this.piValueLabel.Text = "Approximate Value of Pi";
            // 
            // scenarioValuesGrid
            // 
            this.scenarioValuesGrid.AllowUserToAddRows = false;
            this.scenarioValuesGrid.AllowUserToDeleteRows = false;
            this.scenarioValuesGrid.AllowUserToResizeColumns = false;
            this.scenarioValuesGrid.AllowUserToResizeRows = false;
            this.scenarioValuesGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.scenarioValuesGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.numberColumn,
            this.valueColumn});
            this.scenarioValuesGrid.Location = new System.Drawing.Point(12, 54);
            this.scenarioValuesGrid.Name = "scenarioValuesGrid";
            this.scenarioValuesGrid.ReadOnly = true;
            this.scenarioValuesGrid.RowHeadersVisible = false;
            this.scenarioValuesGrid.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.scenarioValuesGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.scenarioValuesGrid.Size = new System.Drawing.Size(358, 132);
            this.scenarioValuesGrid.TabIndex = 5;
            // 
            // scenarioValuesLabel
            // 
            this.scenarioValuesLabel.AutoSize = true;
            this.scenarioValuesLabel.Location = new System.Drawing.Point(8, 38);
            this.scenarioValuesLabel.Name = "scenarioValuesLabel";
            this.scenarioValuesLabel.Size = new System.Drawing.Size(84, 13);
            this.scenarioValuesLabel.TabIndex = 6;
            this.scenarioValuesLabel.Text = "Scenario Values";
            // 
            // inputsGroupBox
            // 
            this.inputsGroupBox.Controls.Add(this.calculateButton);
            this.inputsGroupBox.Controls.Add(this.scenarioCountTextBox);
            this.inputsGroupBox.Controls.Add(this.scenarioCountLabel);
            this.inputsGroupBox.Location = new System.Drawing.Point(10, 10);
            this.inputsGroupBox.Name = "inputsGroupBox";
            this.inputsGroupBox.Size = new System.Drawing.Size(382, 70);
            this.inputsGroupBox.TabIndex = 7;
            this.inputsGroupBox.TabStop = false;
            this.inputsGroupBox.Text = "Inputs";
            // 
            // resultsGroupBox
            // 
            this.resultsGroupBox.Controls.Add(this.piValueLabel);
            this.resultsGroupBox.Controls.Add(this.piValueTextBox);
            this.resultsGroupBox.Controls.Add(this.scenarioValuesGrid);
            this.resultsGroupBox.Controls.Add(this.scenarioValuesLabel);
            this.resultsGroupBox.Location = new System.Drawing.Point(10, 88);
            this.resultsGroupBox.Name = "resultsGroupBox";
            this.resultsGroupBox.Size = new System.Drawing.Size(382, 198);
            this.resultsGroupBox.TabIndex = 8;
            this.resultsGroupBox.TabStop = false;
            this.resultsGroupBox.Text = "Results";
            // 
            // numberColumn
            // 
            this.numberColumn.HeaderText = "Number";
            this.numberColumn.Name = "numberColumn";
            this.numberColumn.ReadOnly = true;
            // 
            // valueColumn
            // 
            this.valueColumn.HeaderText = "Value";
            this.valueColumn.Name = "valueColumn";
            this.valueColumn.ReadOnly = true;
            this.valueColumn.Width = 239;
            // 
            // MainView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(402, 298);
            this.Controls.Add(this.resultsGroupBox);
            this.Controls.Add(this.inputsGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainView";
            this.Text = "Pi Approximator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainView_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.scenarioValuesGrid)).EndInit();
            this.inputsGroupBox.ResumeLayout(false);
            this.inputsGroupBox.PerformLayout();
            this.resultsGroupBox.ResumeLayout(false);
            this.resultsGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button calculateButton;
        private System.Windows.Forms.TextBox scenarioCountTextBox;
        private System.Windows.Forms.Label scenarioCountLabel;
        private System.Windows.Forms.TextBox piValueTextBox;
        private System.Windows.Forms.Label piValueLabel;
        private System.Windows.Forms.DataGridView scenarioValuesGrid;
        private System.Windows.Forms.Label scenarioValuesLabel;
        private System.Windows.Forms.GroupBox inputsGroupBox;
        private System.Windows.Forms.GroupBox resultsGroupBox;
        private System.Windows.Forms.DataGridViewTextBoxColumn numberColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn valueColumn;
    }
}
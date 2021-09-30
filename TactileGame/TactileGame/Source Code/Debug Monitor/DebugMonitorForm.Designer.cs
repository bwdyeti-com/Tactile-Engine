#if DEBUGMONITOR
namespace TactileGame.Debug_Monitor
{
    partial class DebugMonitorForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.TabPanel = new System.Windows.Forms.Panel();
            this.DebugTabSelector = new System.Windows.Forms.TabControl();
            this.OverviewTabPage = new System.Windows.Forms.TabPage();
            this.DebugControlsButton = new System.Windows.Forms.Button();
            this.VariablesTabPage = new System.Windows.Forms.TabPage();
            this.VariableGroupComboBox = new System.Windows.Forms.ComboBox();
            this.RankingTabPage = new System.Windows.Forms.TabPage();
            this.RngTabPage = new System.Windows.Forms.TabPage();
            this.ReseedRngButton = new System.Windows.Forms.Button();
            this.AudioTabPage = new System.Windows.Forms.TabPage();
            this.InputTabPage = new System.Windows.Forms.TabPage();
            this.MonitorPanel = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.BattalionTabPage = new System.Windows.Forms.TabPage();
            this.BattalionSpinner = new System.Windows.Forms.NumericUpDown();
            this.BattalionLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.TabPanel.SuspendLayout();
            this.DebugTabSelector.SuspendLayout();
            this.OverviewTabPage.SuspendLayout();
            this.VariablesTabPage.SuspendLayout();
            this.RngTabPage.SuspendLayout();
            this.MonitorPanel.SuspendLayout();
            this.BattalionTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BattalionSpinner)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.TabPanel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.MonitorPanel, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(304, 401);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // TabPanel
            // 
            this.TabPanel.Controls.Add(this.DebugTabSelector);
            this.TabPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabPanel.Location = new System.Drawing.Point(3, 3);
            this.TabPanel.Name = "TabPanel";
            this.TabPanel.Size = new System.Drawing.Size(298, 50);
            this.TabPanel.TabIndex = 0;
            // 
            // DebugTabSelector
            // 
            this.DebugTabSelector.Controls.Add(this.OverviewTabPage);
            this.DebugTabSelector.Controls.Add(this.BattalionTabPage);
            this.DebugTabSelector.Controls.Add(this.VariablesTabPage);
            this.DebugTabSelector.Controls.Add(this.RankingTabPage);
            this.DebugTabSelector.Controls.Add(this.RngTabPage);
            this.DebugTabSelector.Controls.Add(this.AudioTabPage);
            this.DebugTabSelector.Controls.Add(this.InputTabPage);
            this.DebugTabSelector.Dock = System.Windows.Forms.DockStyle.Top;
            this.DebugTabSelector.Location = new System.Drawing.Point(0, 0);
            this.DebugTabSelector.Name = "DebugTabSelector";
            this.DebugTabSelector.SelectedIndex = 0;
            this.DebugTabSelector.Size = new System.Drawing.Size(298, 75);
            this.DebugTabSelector.TabIndex = 1;
            this.DebugTabSelector.SelectedIndexChanged += new System.EventHandler(this.DebugTabSelector_SelectedIndexChanged);
            // 
            // OverviewTabPage
            // 
            this.OverviewTabPage.Controls.Add(this.DebugControlsButton);
            this.OverviewTabPage.Location = new System.Drawing.Point(4, 22);
            this.OverviewTabPage.Margin = new System.Windows.Forms.Padding(0);
            this.OverviewTabPage.Name = "OverviewTabPage";
            this.OverviewTabPage.Size = new System.Drawing.Size(290, 49);
            this.OverviewTabPage.TabIndex = 0;
            this.OverviewTabPage.Text = "Overview";
            this.OverviewTabPage.UseVisualStyleBackColor = true;
            // 
            // DebugControlsButton
            // 
            this.DebugControlsButton.Location = new System.Drawing.Point(3, 3);
            this.DebugControlsButton.Name = "DebugControlsButton";
            this.DebugControlsButton.Size = new System.Drawing.Size(88, 23);
            this.DebugControlsButton.TabIndex = 2;
            this.DebugControlsButton.Text = "Debug Menu";
            this.DebugControlsButton.UseVisualStyleBackColor = true;
            this.DebugControlsButton.Click += new System.EventHandler(this.DebugControlsButton_Click);
            // 
            // VariablesTabPage
            // 
            this.VariablesTabPage.Controls.Add(this.VariableGroupComboBox);
            this.VariablesTabPage.Location = new System.Drawing.Point(4, 22);
            this.VariablesTabPage.Margin = new System.Windows.Forms.Padding(0);
            this.VariablesTabPage.Name = "VariablesTabPage";
            this.VariablesTabPage.Size = new System.Drawing.Size(290, 49);
            this.VariablesTabPage.TabIndex = 1;
            this.VariablesTabPage.Text = "Variables";
            this.VariablesTabPage.UseVisualStyleBackColor = true;
            // 
            // VariableGroupComboBox
            // 
            this.VariableGroupComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.VariableGroupComboBox.FormattingEnabled = true;
            this.VariableGroupComboBox.Items.AddRange(new object[] {
            "079-099"});
            this.VariableGroupComboBox.Location = new System.Drawing.Point(3, 3);
            this.VariableGroupComboBox.Name = "VariableGroupComboBox";
            this.VariableGroupComboBox.Size = new System.Drawing.Size(80, 21);
            this.VariableGroupComboBox.TabIndex = 0;
            this.VariableGroupComboBox.SelectedIndexChanged += new System.EventHandler(this.VariableGroupComboBox_SelectedIndexChanged);
            // 
            // RankingTabPage
            // 
            this.RankingTabPage.Location = new System.Drawing.Point(4, 22);
            this.RankingTabPage.Name = "RankingTabPage";
            this.RankingTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.RankingTabPage.Size = new System.Drawing.Size(290, 49);
            this.RankingTabPage.TabIndex = 2;
            this.RankingTabPage.Text = "Ranking";
            this.RankingTabPage.UseVisualStyleBackColor = true;
            // 
            // RngTabPage
            // 
            this.RngTabPage.Controls.Add(this.ReseedRngButton);
            this.RngTabPage.Location = new System.Drawing.Point(4, 22);
            this.RngTabPage.Name = "RngTabPage";
            this.RngTabPage.Size = new System.Drawing.Size(290, 49);
            this.RngTabPage.TabIndex = 3;
            this.RngTabPage.Text = "Rng";
            this.RngTabPage.UseVisualStyleBackColor = true;
            // 
            // ReseedRngButton
            // 
            this.ReseedRngButton.Location = new System.Drawing.Point(3, 3);
            this.ReseedRngButton.Name = "ReseedRngButton";
            this.ReseedRngButton.Size = new System.Drawing.Size(88, 23);
            this.ReseedRngButton.TabIndex = 0;
            this.ReseedRngButton.Text = "Re-seed Rng";
            this.ReseedRngButton.UseVisualStyleBackColor = true;
            this.ReseedRngButton.Click += new System.EventHandler(this.ReseedRngButton_Click);
            // 
            // AudioTabPage
            // 
            this.AudioTabPage.Location = new System.Drawing.Point(4, 22);
            this.AudioTabPage.Name = "AudioTabPage";
            this.AudioTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.AudioTabPage.Size = new System.Drawing.Size(290, 49);
            this.AudioTabPage.TabIndex = 4;
            this.AudioTabPage.Text = "Audio";
            this.AudioTabPage.UseVisualStyleBackColor = true;
            // 
            // InputTabPage
            // 
            this.InputTabPage.Location = new System.Drawing.Point(4, 22);
            this.InputTabPage.Name = "InputTabPage";
            this.InputTabPage.Size = new System.Drawing.Size(290, 49);
            this.InputTabPage.TabIndex = 5;
            this.InputTabPage.Text = "Inputs";
            this.InputTabPage.UseVisualStyleBackColor = true;
            // 
            // MonitorPanel
            // 
            this.MonitorPanel.Controls.Add(this.label1);
            this.MonitorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MonitorPanel.Location = new System.Drawing.Point(3, 59);
            this.MonitorPanel.Name = "MonitorPanel";
            this.MonitorPanel.Size = new System.Drawing.Size(298, 339);
            this.MonitorPanel.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(37, 103);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(211, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Debug monitor goes here, added at runtime";
            // 
            // BattalionTabPage
            // 
            this.BattalionTabPage.Controls.Add(this.BattalionLabel);
            this.BattalionTabPage.Controls.Add(this.BattalionSpinner);
            this.BattalionTabPage.Location = new System.Drawing.Point(4, 22);
            this.BattalionTabPage.Name = "BattalionTabPage";
            this.BattalionTabPage.Size = new System.Drawing.Size(290, 49);
            this.BattalionTabPage.TabIndex = 6;
            this.BattalionTabPage.Text = "Battalion";
            this.BattalionTabPage.UseVisualStyleBackColor = true;
            // 
            // BattalionSpinner
            // 
            this.BattalionSpinner.Location = new System.Drawing.Point(69, 3);
            this.BattalionSpinner.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.BattalionSpinner.Name = "BattalionSpinner";
            this.BattalionSpinner.Size = new System.Drawing.Size(80, 20);
            this.BattalionSpinner.TabIndex = 0;
            this.BattalionSpinner.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.BattalionSpinner.ValueChanged += new System.EventHandler(this.BattalionSpinner_ValueChanged);
            // 
            // BattalionLabel
            // 
            this.BattalionLabel.AutoSize = true;
            this.BattalionLabel.Location = new System.Drawing.Point(3, 5);
            this.BattalionLabel.Name = "BattalionLabel";
            this.BattalionLabel.Size = new System.Drawing.Size(60, 13);
            this.BattalionLabel.TabIndex = 1;
            this.BattalionLabel.Text = "Battalion Id";
            // 
            // DebugMonitorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(304, 401);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "DebugMonitorForm";
            this.Text = "DebugMonitorForm";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.TabPanel.ResumeLayout(false);
            this.DebugTabSelector.ResumeLayout(false);
            this.OverviewTabPage.ResumeLayout(false);
            this.VariablesTabPage.ResumeLayout(false);
            this.RngTabPage.ResumeLayout(false);
            this.MonitorPanel.ResumeLayout(false);
            this.MonitorPanel.PerformLayout();
            this.BattalionTabPage.ResumeLayout(false);
            this.BattalionTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BattalionSpinner)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TabControl DebugTabSelector;
        private System.Windows.Forms.TabPage OverviewTabPage;
        private System.Windows.Forms.TabPage VariablesTabPage;
        private System.Windows.Forms.Panel TabPanel;
        private System.Windows.Forms.ComboBox VariableGroupComboBox;
        private System.Windows.Forms.Panel MonitorPanel;
        private System.Windows.Forms.TabPage RankingTabPage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage RngTabPage;
        private System.Windows.Forms.Button ReseedRngButton;
        private System.Windows.Forms.Button DebugControlsButton;
        private System.Windows.Forms.TabPage AudioTabPage;
        private System.Windows.Forms.TabPage InputTabPage;
        private System.Windows.Forms.TabPage BattalionTabPage;
        private System.Windows.Forms.Label BattalionLabel;
        private System.Windows.Forms.NumericUpDown BattalionSpinner;
    }
}
#endif

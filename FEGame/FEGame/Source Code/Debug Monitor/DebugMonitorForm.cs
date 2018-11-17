#if DEBUGMONITOR
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FEGame.Debug_Monitor
{
    public partial class DebugMonitorForm : Form
    {
        private DebugMonitorControl debugMonitorControl1;

        public DebugMonitorForm(Game1 game)
        {
            InitializeComponent();

            this.MonitorPanel.Controls.Remove(label1);
            // This thing won't stay added in design viewer
            this.debugMonitorControl1 = new DebugMonitorControl();
            this.MonitorPanel.Controls.Add(this.debugMonitorControl1);
            this.debugMonitorControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.debugMonitorControl1.Location = new System.Drawing.Point(0, 0);
            this.debugMonitorControl1.Name = "debugMonitorControl1";
            this.debugMonitorControl1.Size = new System.Drawing.Size(298, 343);
            this.debugMonitorControl1.TabIndex = 0;
            this.debugMonitorControl1.Text = "debugMonitorControl1";

            this.Owner = Form.FromHandle(game.Window.Handle) as Form;
            this.Icon = this.Owner.Icon;
        }

        internal void set_event_data_size(int sizePerPage, int total)
        {
            VariableGroupComboBox.Items.Clear();

            int i = 0;
            do
            {
                int j = Math.Min(i + sizePerPage, total) - 1;
                VariableGroupComboBox.Items.Add(
                    string.Format("{0:000}-{1:000}", i, j));

                i += sizePerPage;
            }
            while (i < total);

            VariableGroupComboBox.SelectedIndex = 0;
        }

        internal void invalidate_monitor()
        {
            debugMonitorControl1.Invalidate();
        }

        private void DebugTabSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            debugMonitorControl1.change_tab(DebugTabSelector.SelectedIndex);
        }

        private void VariableGroupComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            debugMonitorControl1.change_variable_group(VariableGroupComboBox.SelectedIndex);
        }

        private void ReseedRngButton_Click(object sender, EventArgs e)
        {
            debugMonitorControl1.reseed_rng();
        }

        private void DebugControlsButton_Click(object sender, EventArgs e)
        {
            debugMonitorControl1.open_debug_menu();
        }
    }
}
#endif

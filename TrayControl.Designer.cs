namespace Player
{
    partial class TrayControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TrayControl));
            this.Context = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.NextStrip = new System.Windows.Forms.ToolStripMenuItem();
            this.PlayPauseStrip = new System.Windows.Forms.ToolStripMenuItem();
            this.GCStrip = new System.Windows.Forms.ToolStripMenuItem();
            this.SettingsStrip = new System.Windows.Forms.ToolStripMenuItem();
            this.ClearViewStrip = new System.Windows.Forms.ToolStripMenuItem();
            this.PreviousStrip = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ExitStrip = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.TrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.SwitchViewStrip = new System.Windows.Forms.ToolStripMenuItem();
            this.Context.SuspendLayout();
            this.SuspendLayout();
            // 
            // Context
            // 
            this.Context.AutoSize = false;
            this.Context.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.Context.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Context.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Context.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.PlayPauseStrip,
            this.NextStrip,
            this.PreviousStrip,
            this.toolStripSeparator1,
            this.SwitchViewStrip,
            this.ClearViewStrip,
            this.SettingsStrip,
            this.GCStrip,
            this.toolStripSeparator2,
            this.ExitStrip,
            this.toolStripMenuItem1});
            this.Context.Name = "contextMenuStrip1";
            this.Context.ShowImageMargin = false;
            this.Context.Size = new System.Drawing.Size(190, 305);
            this.Context.Text = "Player";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(186, 6);
            // 
            // NextStrip
            // 
            this.NextStrip.AutoSize = false;
            this.NextStrip.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.NextStrip.Name = "NextStrip";
            this.NextStrip.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.NextStrip.Size = new System.Drawing.Size(190, 28);
            this.NextStrip.Text = "      Next";
            // 
            // PlayPauseStrip
            // 
            this.PlayPauseStrip.AutoSize = false;
            this.PlayPauseStrip.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.PlayPauseStrip.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.PlayPauseStrip.Name = "PlayPauseStrip";
            this.PlayPauseStrip.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.PlayPauseStrip.Size = new System.Drawing.Size(190, 28);
            this.PlayPauseStrip.Text = "      Pause";
            // 
            // GCStrip
            // 
            this.GCStrip.AutoSize = false;
            this.GCStrip.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.GCStrip.Name = "GCStrip";
            this.GCStrip.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.GCStrip.Size = new System.Drawing.Size(190, 28);
            this.GCStrip.Text = "      Call Garbage Collector";
            // 
            // SettingsStrip
            // 
            this.SettingsStrip.AutoSize = false;
            this.SettingsStrip.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.SettingsStrip.Name = "SettingsStrip";
            this.SettingsStrip.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.SettingsStrip.Size = new System.Drawing.Size(190, 28);
            this.SettingsStrip.Text = "      Settings";
            // 
            // ClearViewStrip
            // 
            this.ClearViewStrip.AutoSize = false;
            this.ClearViewStrip.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.ClearViewStrip.Name = "ClearViewStrip";
            this.ClearViewStrip.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.ClearViewStrip.Size = new System.Drawing.Size(190, 28);
            this.ClearViewStrip.Text = "      Clear Main View";
            // 
            // PreviousStrip
            // 
            this.PreviousStrip.AutoSize = false;
            this.PreviousStrip.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.PreviousStrip.Name = "PreviousStrip";
            this.PreviousStrip.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.PreviousStrip.Size = new System.Drawing.Size(190, 28);
            this.PreviousStrip.Text = "      Previous    ";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(186, 6);
            // 
            // ExitStrip
            // 
            this.ExitStrip.AutoSize = false;
            this.ExitStrip.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.ExitStrip.Name = "ExitStrip";
            this.ExitStrip.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.ExitStrip.Size = new System.Drawing.Size(190, 28);
            this.ExitStrip.Text = "      Exit";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.AutoSize = false;
            this.toolStripMenuItem1.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.toolStripMenuItem1.Size = new System.Drawing.Size(190, 28);
            this.toolStripMenuItem1.Text = "      Restart";
            // 
            // TrayIcon
            // 
            this.TrayIcon.ContextMenuStrip = this.Context;
            this.TrayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("TrayIcon.Icon")));
            this.TrayIcon.Text = "Elephant Player";
            this.TrayIcon.Visible = true;
            // 
            // SwitchViewStrip
            // 
            this.SwitchViewStrip.AutoSize = false;
            this.SwitchViewStrip.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.SwitchViewStrip.Name = "SwitchViewStrip";
            this.SwitchViewStrip.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.SwitchViewStrip.Size = new System.Drawing.Size(190, 28);
            this.SwitchViewStrip.Text = "Switch View";
            // 
            // TrayControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "TrayControl";
            this.Size = new System.Drawing.Size(262, 171);
            this.Load += new System.EventHandler(this.TrayControl_Load);
            this.Context.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip Context;
        public System.Windows.Forms.NotifyIcon TrayIcon;
        private System.Windows.Forms.ToolStripMenuItem ClearViewStrip;
        private System.Windows.Forms.ToolStripMenuItem SettingsStrip;
        private System.Windows.Forms.ToolStripMenuItem GCStrip;
        private System.Windows.Forms.ToolStripMenuItem ExitStrip;
        private System.Windows.Forms.ToolStripMenuItem PlayPauseStrip;
        private System.Windows.Forms.ToolStripMenuItem PreviousStrip;
        private System.Windows.Forms.ToolStripMenuItem NextStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem SwitchViewStrip;
    }
}

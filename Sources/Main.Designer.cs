
namespace iReverse_UniSPD_FRP
{
    partial class Main
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
            this.GroupBox4 = new System.Windows.Forms.GroupBox();
            this.PanelSPDOneClick = new System.Windows.Forms.Panel();
            this.GroupBox3 = new System.Windows.Forms.GroupBox();
            this.ListBoxview = new System.Windows.Forms.ListBox();
            this.ListBoxViewSearch = new System.Windows.Forms.TextBox();
            this.ComboPort = new System.Windows.Forms.ComboBox();
            this.Logs = new System.Windows.Forms.RichTextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.btn_STOP = new System.Windows.Forms.Button();
            this.CkFDLLoaded = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lbl_resp = new System.Windows.Forms.Label();
            this.comboBoxTimeout = new System.Windows.Forms.ComboBox();
            this.GroupBox4.SuspendLayout();
            this.GroupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // GroupBox4
            // 
            this.GroupBox4.Controls.Add(this.comboBoxTimeout);
            this.GroupBox4.Controls.Add(this.PanelSPDOneClick);
            this.GroupBox4.Location = new System.Drawing.Point(215, 12);
            this.GroupBox4.Name = "GroupBox4";
            this.GroupBox4.Size = new System.Drawing.Size(220, 386);
            this.GroupBox4.TabIndex = 8;
            this.GroupBox4.TabStop = false;
            this.GroupBox4.Text = "Operation";
            // 
            // PanelSPDOneClick
            // 
            this.PanelSPDOneClick.Location = new System.Drawing.Point(3, 42);
            this.PanelSPDOneClick.Name = "PanelSPDOneClick";
            this.PanelSPDOneClick.Size = new System.Drawing.Size(214, 341);
            this.PanelSPDOneClick.TabIndex = 0;
            // 
            // GroupBox3
            // 
            this.GroupBox3.Controls.Add(this.ListBoxview);
            this.GroupBox3.Controls.Add(this.ListBoxViewSearch);
            this.GroupBox3.Location = new System.Drawing.Point(12, 12);
            this.GroupBox3.Name = "GroupBox3";
            this.GroupBox3.Size = new System.Drawing.Size(197, 386);
            this.GroupBox3.TabIndex = 7;
            this.GroupBox3.TabStop = false;
            this.GroupBox3.Text = "Devices";
            // 
            // ListBoxview
            // 
            this.ListBoxview.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ListBoxview.FormattingEnabled = true;
            this.ListBoxview.Location = new System.Drawing.Point(3, 48);
            this.ListBoxview.Name = "ListBoxview";
            this.ListBoxview.Size = new System.Drawing.Size(191, 338);
            this.ListBoxview.TabIndex = 7;
            this.ListBoxview.SelectedIndexChanged += new System.EventHandler(this.ListBoxview_SelectedIndexChanged);
            // 
            // ListBoxViewSearch
            // 
            this.ListBoxViewSearch.BackColor = System.Drawing.Color.SeaShell;
            this.ListBoxViewSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ListBoxViewSearch.Location = new System.Drawing.Point(3, 16);
            this.ListBoxViewSearch.Name = "ListBoxViewSearch";
            this.ListBoxViewSearch.Size = new System.Drawing.Size(191, 20);
            this.ListBoxViewSearch.TabIndex = 6;
            this.ListBoxViewSearch.TextChanged += new System.EventHandler(this.ListBoxViewSearch_TextChanged);
            // 
            // ComboPort
            // 
            this.ComboPort.BackColor = System.Drawing.Color.SeaShell;
            this.ComboPort.FormattingEnabled = true;
            this.ComboPort.Location = new System.Drawing.Point(441, 27);
            this.ComboPort.Name = "ComboPort";
            this.ComboPort.Size = new System.Drawing.Size(420, 21);
            this.ComboPort.TabIndex = 9;
            this.ComboPort.SelectedIndexChanged += new System.EventHandler(this.ComboPort_SelectedIndexChanged);
            // 
            // Logs
            // 
            this.Logs.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Logs.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Logs.Location = new System.Drawing.Point(441, 52);
            this.Logs.Name = "Logs";
            this.Logs.Size = new System.Drawing.Size(420, 346);
            this.Logs.TabIndex = 10;
            this.Logs.Text = "";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 416);
            this.progressBar1.MarqueeAnimationSpeed = 0;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(763, 23);
            this.progressBar1.TabIndex = 11;
            this.progressBar1.Value = 100;
            // 
            // btn_STOP
            // 
            this.btn_STOP.Location = new System.Drawing.Point(786, 404);
            this.btn_STOP.Name = "btn_STOP";
            this.btn_STOP.Size = new System.Drawing.Size(75, 45);
            this.btn_STOP.TabIndex = 12;
            this.btn_STOP.Text = "STOP";
            this.btn_STOP.UseVisualStyleBackColor = true;
            this.btn_STOP.Click += new System.EventHandler(this.btn_STOP_Click);
            // 
            // CkFDLLoaded
            // 
            this.CkFDLLoaded.AutoSize = true;
            this.CkFDLLoaded.Location = new System.Drawing.Point(763, 29);
            this.CkFDLLoaded.Name = "CkFDLLoaded";
            this.CkFDLLoaded.Size = new System.Drawing.Size(85, 17);
            this.CkFDLLoaded.TabIndex = 14;
            this.CkFDLLoaded.Text = "FDL Loaded";
            this.CkFDLLoaded.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 442);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Response : ";
            // 
            // lbl_resp
            // 
            this.lbl_resp.AutoSize = true;
            this.lbl_resp.Location = new System.Drawing.Point(82, 442);
            this.lbl_resp.Name = "lbl_resp";
            this.lbl_resp.Size = new System.Drawing.Size(0, 13);
            this.lbl_resp.TabIndex = 15;
            // 
            // comboBoxTimeout
            // 
            this.comboBoxTimeout.FormattingEnabled = true;
            this.comboBoxTimeout.Items.AddRange(new object[] {
            "Timeout - 1000 ms",
            "Timeout - 1500 ms",
            "Timeout - 2000 ms",
            "Timeout - 3000 ms"});
            this.comboBoxTimeout.Location = new System.Drawing.Point(6, 15);
            this.comboBoxTimeout.Name = "comboBoxTimeout";
            this.comboBoxTimeout.Size = new System.Drawing.Size(208, 21);
            this.comboBoxTimeout.TabIndex = 1;
            this.comboBoxTimeout.Text = "Timeout - 1000 ms";
            this.comboBoxTimeout.SelectedIndexChanged += new System.EventHandler(this.comboBoxTimeout_SelectedIndexChanged);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(874, 462);
            this.Controls.Add(this.lbl_resp);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CkFDLLoaded);
            this.Controls.Add(this.btn_STOP);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.Logs);
            this.Controls.Add(this.ComboPort);
            this.Controls.Add(this.GroupBox4);
            this.Controls.Add(this.GroupBox3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "iReverse UniSPD FRP Tools - Non-Console [x86] - C# Version - [HadiK-IT] Hadi Khoi" +
    "rudin, S.Kom.";
            this.GroupBox4.ResumeLayout(false);
            this.GroupBox3.ResumeLayout(false);
            this.GroupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.GroupBox GroupBox4;
        internal System.Windows.Forms.GroupBox GroupBox3;
        public System.Windows.Forms.ListBox ListBoxview;
        internal System.Windows.Forms.TextBox ListBoxViewSearch;
        public System.Windows.Forms.RichTextBox Logs;
        public System.Windows.Forms.ComboBox ComboPort;
        public System.Windows.Forms.ProgressBar progressBar1;
        public System.Windows.Forms.CheckBox CkFDLLoaded;
        public System.Windows.Forms.Button btn_STOP;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label lbl_resp;
        public System.Windows.Forms.Panel PanelSPDOneClick;
        public System.Windows.Forms.ComboBox comboBoxTimeout;
    }
}


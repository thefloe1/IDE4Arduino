namespace IDE4Arduino
{
    partial class TerminalWindow
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
            this.components = new System.ComponentModel.Container();
            this.cbPorts = new System.Windows.Forms.ComboBox();
            this.cbBaud = new System.Windows.Forms.ComboBox();
            this.databox = new System.Windows.Forms.TextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.aSCIIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hEXToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendData = new System.Windows.Forms.TextBox();
            this.cbAddNewline = new System.Windows.Forms.ComboBox();
            this.button2 = new System.Windows.Forms.Button();
            this.openPort = new System.Windows.Forms.CheckBox();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbPorts
            // 
            this.cbPorts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPorts.FormattingEnabled = true;
            this.cbPorts.Location = new System.Drawing.Point(6, 4);
            this.cbPorts.Name = "cbPorts";
            this.cbPorts.Size = new System.Drawing.Size(74, 21);
            this.cbPorts.TabIndex = 0;
            // 
            // cbBaud
            // 
            this.cbBaud.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbBaud.FormattingEnabled = true;
            this.cbBaud.Items.AddRange(new object[] {
            "1200",
            "2400",
            "4800",
            "9600",
            "19200",
            "28400",
            "38400",
            "57600",
            "115200"});
            this.cbBaud.Location = new System.Drawing.Point(6, 31);
            this.cbBaud.Name = "cbBaud";
            this.cbBaud.Size = new System.Drawing.Size(74, 21);
            this.cbBaud.TabIndex = 1;
            // 
            // databox
            // 
            this.databox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.databox.ContextMenuStrip = this.contextMenuStrip1;
            this.databox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.databox.Location = new System.Drawing.Point(86, 4);
            this.databox.Multiline = true;
            this.databox.Name = "databox";
            this.databox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.databox.Size = new System.Drawing.Size(686, 185);
            this.databox.TabIndex = 3;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aSCIIToolStripMenuItem,
            this.hEXToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(103, 48);
            // 
            // aSCIIToolStripMenuItem
            // 
            this.aSCIIToolStripMenuItem.Checked = true;
            this.aSCIIToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.aSCIIToolStripMenuItem.Name = "aSCIIToolStripMenuItem";
            this.aSCIIToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.aSCIIToolStripMenuItem.Text = "ASCII";
            this.aSCIIToolStripMenuItem.Click += new System.EventHandler(this.aSCIIToolStripMenuItem_Click);
            // 
            // hEXToolStripMenuItem
            // 
            this.hEXToolStripMenuItem.Name = "hEXToolStripMenuItem";
            this.hEXToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.hEXToolStripMenuItem.Text = "HEX";
            this.hEXToolStripMenuItem.Click += new System.EventHandler(this.hEXToolStripMenuItem_Click);
            // 
            // sendData
            // 
            this.sendData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sendData.Location = new System.Drawing.Point(86, 195);
            this.sendData.Name = "sendData";
            this.sendData.Size = new System.Drawing.Size(543, 20);
            this.sendData.TabIndex = 4;
            // 
            // cbAddNewline
            // 
            this.cbAddNewline.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAddNewline.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAddNewline.FormattingEnabled = true;
            this.cbAddNewline.Items.AddRange(new object[] {
            "none",
            "CR",
            "CR+LF"});
            this.cbAddNewline.Location = new System.Drawing.Point(635, 195);
            this.cbAddNewline.Name = "cbAddNewline";
            this.cbAddNewline.Size = new System.Drawing.Size(70, 21);
            this.cbAddNewline.TabIndex = 5;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(711, 195);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(61, 23);
            this.button2.TabIndex = 6;
            this.button2.Text = "send";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // openPort
            // 
            this.openPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.openPort.Appearance = System.Windows.Forms.Appearance.Button;
            this.openPort.AutoSize = true;
            this.openPort.Location = new System.Drawing.Point(6, 192);
            this.openPort.Name = "openPort";
            this.openPort.Size = new System.Drawing.Size(74, 23);
            this.openPort.TabIndex = 7;
            this.openPort.Text = "connect(ed)";
            this.openPort.UseVisualStyleBackColor = true;
            this.openPort.Click += new System.EventHandler(this.checkBox1_Click);
            // 
            // TerminalWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(777, 220);
            this.Controls.Add(this.openPort);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.cbAddNewline);
            this.Controls.Add(this.sendData);
            this.Controls.Add(this.databox);
            this.Controls.Add(this.cbBaud);
            this.Controls.Add(this.cbPorts);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "TerminalWindow";
            this.Text = "TerminalWindow";
            this.Activated += new System.EventHandler(this.TerminalWindow_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TerminalWindow_FormClosing);
            this.Load += new System.EventHandler(this.TerminalWindow_Load);
            this.Enter += new System.EventHandler(this.TerminalWindow_Enter);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbPorts;
        private System.Windows.Forms.ComboBox cbBaud;
        private System.Windows.Forms.TextBox databox;
        private System.Windows.Forms.TextBox sendData;
        private System.Windows.Forms.ComboBox cbAddNewline;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox openPort;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem aSCIIToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hEXToolStripMenuItem;
    }
}
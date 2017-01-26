namespace BackgroundTracking
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.iconTracking = new System.Windows.Forms.NotifyIcon(this.components);
            this.btnWriteToFile = new System.Windows.Forms.Button();
            this.contextMenuIcon = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.writeToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuIcon.SuspendLayout();
            this.SuspendLayout();
            // 
            // iconTracking
            // 
            this.iconTracking.ContextMenuStrip = this.contextMenuIcon;
            this.iconTracking.Icon = ((System.Drawing.Icon)(resources.GetObject("iconTracking.Icon")));
            this.iconTracking.Text = "Tracking";
            this.iconTracking.Visible = true;
            this.iconTracking.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.iconTracking_MouseDoubleClick);
            // 
            // btnWriteToFile
            // 
            this.btnWriteToFile.Location = new System.Drawing.Point(12, 12);
            this.btnWriteToFile.Name = "btnWriteToFile";
            this.btnWriteToFile.Size = new System.Drawing.Size(138, 33);
            this.btnWriteToFile.TabIndex = 2;
            this.btnWriteToFile.Text = "Write to file";
            this.btnWriteToFile.UseVisualStyleBackColor = true;
            this.btnWriteToFile.Click += new System.EventHandler(this.btnWriteToFile_Click);
            // 
            // contextMenuIcon
            // 
            this.contextMenuIcon.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuIcon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.writeToFileToolStripMenuItem});
            this.contextMenuIcon.Name = "contextMenuIcon";
            this.contextMenuIcon.Size = new System.Drawing.Size(164, 30);
            // 
            // writeToFileToolStripMenuItem
            // 
            this.writeToFileToolStripMenuItem.Name = "writeToFileToolStripMenuItem";
            this.writeToFileToolStripMenuItem.Size = new System.Drawing.Size(163, 26);
            this.writeToFileToolStripMenuItem.Text = "Write to file";
            this.writeToFileToolStripMenuItem.Click += new System.EventHandler(this.writeToFileToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(405, 409);
            this.Controls.Add(this.btnWriteToFile);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.contextMenuIcon.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.NotifyIcon iconTracking;
        private System.Windows.Forms.Button btnWriteToFile;
        private System.Windows.Forms.ContextMenuStrip contextMenuIcon;
        private System.Windows.Forms.ToolStripMenuItem writeToFileToolStripMenuItem;
    }
}


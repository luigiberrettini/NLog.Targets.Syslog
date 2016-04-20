namespace TestApp
{
    partial class Form1
    {
        /// <summary>Required designer variable</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>Clean up any resources being used</summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>Required method for designer support: do not modify the contents of this method with the code editor</summary>
        private void InitializeComponent()
        {
            buttonTrace = new System.Windows.Forms.Button();
            buttonDebug = new System.Windows.Forms.Button();
            buttonInfo = new System.Windows.Forms.Button();
            buttonWarn = new System.Windows.Forms.Button();
            buttonError = new System.Windows.Forms.Button();
            buttonFatal = new System.Windows.Forms.Button();
            SuspendLayout();

            buttonTrace.Location = new System.Drawing.Point(34, 26);
            buttonTrace.Name = "buttonTrace";
            buttonTrace.Size = new System.Drawing.Size(212, 38);
            buttonTrace.TabIndex = 0;
            buttonTrace.Text = "Trace Event";
            buttonTrace.UseVisualStyleBackColor = true;
            buttonTrace.Click += new System.EventHandler(ButtonLogClick);

            buttonDebug.Location = new System.Drawing.Point(34, 70);
            buttonDebug.Name = "buttonDebug";
            buttonDebug.Size = new System.Drawing.Size(212, 38);
            buttonDebug.TabIndex = 1;
            buttonDebug.Text = "Debug Event";
            buttonDebug.UseVisualStyleBackColor = true;
            buttonDebug.Click += new System.EventHandler(ButtonLogClick);

            buttonInfo.Location = new System.Drawing.Point(34, 114);
            buttonInfo.Name = "buttonInfo";
            buttonInfo.Size = new System.Drawing.Size(212, 38);
            buttonInfo.TabIndex = 2;
            buttonInfo.Text = "Info Event";
            buttonInfo.UseVisualStyleBackColor = true;
            buttonInfo.Click += new System.EventHandler(ButtonLogClick);

            buttonWarn.Location = new System.Drawing.Point(34, 158);
            buttonWarn.Name = "buttonWarn";
            buttonWarn.Size = new System.Drawing.Size(212, 38);
            buttonWarn.TabIndex = 3;
            buttonWarn.Text = "Warn Event";
            buttonWarn.UseVisualStyleBackColor = true;
            buttonWarn.Click += new System.EventHandler(ButtonLogClick);

            buttonError.Location = new System.Drawing.Point(34, 202);
            buttonError.Name = "buttonError";
            buttonError.Size = new System.Drawing.Size(212, 38);
            buttonError.TabIndex = 4;
            buttonError.Text = "Error Event";
            buttonError.UseVisualStyleBackColor = true;
            buttonError.Click += new System.EventHandler(ButtonLogClick);

            buttonFatal.Location = new System.Drawing.Point(34, 246);
            buttonFatal.Name = "buttonFatal";
            buttonFatal.Size = new System.Drawing.Size(212, 38);
            buttonFatal.TabIndex = 5;
            buttonFatal.Text = "Fatal Event";
            buttonFatal.UseVisualStyleBackColor = true;
            buttonFatal.Click += new System.EventHandler(ButtonLogClick);

            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(284, 330);
            Controls.Add(buttonFatal);
            Controls.Add(buttonError);
            Controls.Add(buttonWarn);
            Controls.Add(buttonInfo);
            Controls.Add(buttonDebug);
            Controls.Add(buttonTrace);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form1";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Syslog Target Demo";
            ResumeLayout(false);
        }

        private System.Windows.Forms.Button buttonTrace;
        private System.Windows.Forms.Button buttonDebug;
        private System.Windows.Forms.Button buttonInfo;
        private System.Windows.Forms.Button buttonWarn;
        private System.Windows.Forms.Button buttonError;
        private System.Windows.Forms.Button buttonFatal;
    }
}
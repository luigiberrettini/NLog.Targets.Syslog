namespace TestApp
{
    partial class FormTest
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            buttonTrace = new System.Windows.Forms.Button();
            buttonDebug = new System.Windows.Forms.Button();
            buttonInfo = new System.Windows.Forms.Button();
            buttonWarn = new System.Windows.Forms.Button();
            buttonError = new System.Windows.Forms.Button();
            buttonFatal = new System.Windows.Forms.Button();
            buttonMultiple = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // buttonTrace
            // 
            buttonTrace.Location = new System.Drawing.Point(36, 26);
            buttonTrace.Name = "buttonTrace";
            buttonTrace.Size = new System.Drawing.Size(212, 38);
            buttonTrace.TabIndex = 0;
            buttonTrace.Text = "Trace event";
            buttonTrace.UseVisualStyleBackColor = true;
            buttonTrace.Click += new System.EventHandler(ButtonLogClick);
            // 
            // buttonDebug
            // 
            buttonDebug.Location = new System.Drawing.Point(36, 70);
            buttonDebug.Name = "buttonDebug";
            buttonDebug.Size = new System.Drawing.Size(212, 38);
            buttonDebug.TabIndex = 1;
            buttonDebug.Text = "Debug event";
            buttonDebug.UseVisualStyleBackColor = true;
            buttonDebug.Click += new System.EventHandler(ButtonLogClick);
            // 
            // buttonInfo
            // 
            buttonInfo.Location = new System.Drawing.Point(36, 114);
            buttonInfo.Name = "buttonInfo";
            buttonInfo.Size = new System.Drawing.Size(212, 38);
            buttonInfo.TabIndex = 2;
            buttonInfo.Text = "Info event";
            buttonInfo.UseVisualStyleBackColor = true;
            buttonInfo.Click += new System.EventHandler(ButtonLogClick);
            // 
            // buttonWarn
            // 
            buttonWarn.Location = new System.Drawing.Point(36, 158);
            buttonWarn.Name = "buttonWarn";
            buttonWarn.Size = new System.Drawing.Size(212, 38);
            buttonWarn.TabIndex = 3;
            buttonWarn.Text = "Warn event";
            buttonWarn.UseVisualStyleBackColor = true;
            buttonWarn.Click += new System.EventHandler(ButtonLogClick);
            // 
            // buttonError
            // 
            buttonError.Location = new System.Drawing.Point(36, 202);
            buttonError.Name = "buttonError";
            buttonError.Size = new System.Drawing.Size(212, 38);
            buttonError.TabIndex = 4;
            buttonError.Text = "Error event";
            buttonError.UseVisualStyleBackColor = true;
            buttonError.Click += new System.EventHandler(ButtonLogClick);
            // 
            // buttonFatal
            // 
            buttonFatal.Location = new System.Drawing.Point(36, 246);
            buttonFatal.Name = "buttonFatal";
            buttonFatal.Size = new System.Drawing.Size(212, 38);
            buttonFatal.TabIndex = 5;
            buttonFatal.Text = "Fatal event";
            buttonFatal.UseVisualStyleBackColor = true;
            buttonFatal.Click += new System.EventHandler(ButtonLogClick);
            // 
            // buttonMultiple
            // 
            buttonMultiple.Location = new System.Drawing.Point(36, 290);
            buttonMultiple.Name = "buttonMultiple";
            buttonMultiple.Size = new System.Drawing.Size(212, 38);
            buttonMultiple.TabIndex = 6;
            buttonMultiple.Text = "Multiple events";
            buttonMultiple.UseVisualStyleBackColor = true;
            buttonMultiple.Click += new System.EventHandler(ButtonLogClick);
            // 
            // FormTest
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(284, 352);
            Controls.Add(buttonMultiple);
            Controls.Add(buttonFatal);
            Controls.Add(buttonError);
            Controls.Add(buttonWarn);
            Controls.Add(buttonInfo);
            Controls.Add(buttonDebug);
            Controls.Add(buttonTrace);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormTest";
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
        private System.Windows.Forms.Button buttonMultiple;
    }
}
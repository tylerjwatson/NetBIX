namespace NetBIX.WindowsUnitTest {
    partial class CProgressWnd {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.pgBar = new System.Windows.Forms.ProgressBar();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSummary = new System.Windows.Forms.Label();
            this.lblItemsSec = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // pgBar
            // 
            this.pgBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pgBar.Location = new System.Drawing.Point(12, 30);
            this.pgBar.Name = "pgBar";
            this.pgBar.Size = new System.Drawing.Size(378, 23);
            this.pgBar.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(0, 13);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Click += new System.EventHandler(this.lblTitle_Click);
            // 
            // lblSummary
            // 
            this.lblSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSummary.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSummary.Location = new System.Drawing.Point(209, 60);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Size = new System.Drawing.Size(182, 13);
            this.lblSummary.TabIndex = 2;
            this.lblSummary.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblItemsSec
            // 
            this.lblItemsSec.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblItemsSec.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblItemsSec.Location = new System.Drawing.Point(10, 60);
            this.lblItemsSec.Name = "lblItemsSec";
            this.lblItemsSec.Size = new System.Drawing.Size(182, 13);
            this.lblItemsSec.TabIndex = 3;
            this.lblItemsSec.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CProgressWnd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(403, 82);
            this.ControlBox = false;
            this.Controls.Add(this.lblItemsSec);
            this.Controls.Add(this.lblSummary);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.pgBar);
            this.Name = "CProgressWnd";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "CProgressWnd";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CProgressWnd_FormClosing);
            this.Load += new System.EventHandler(this.CProgressWnd_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar pgBar;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSummary;
        private System.Windows.Forms.Label lblItemsSec;
    }
}
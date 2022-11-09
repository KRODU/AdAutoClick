namespace AdAutoClick
{
    partial class ADClickProc
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnInit = new System.Windows.Forms.Button();
            this.lblCaption = new System.Windows.Forms.Label();
            this.lblTimeout = new System.Windows.Forms.Label();
            this.timerTimeoutCaption = new System.Windows.Forms.Timer(this.components);
            this.logFlushTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(232, 45);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "시작";
            this.btnStart.UseVisualStyleBackColor = true;
            // 
            // btnInit
            // 
            this.btnInit.Location = new System.Drawing.Point(122, 63);
            this.btnInit.Name = "btnInit";
            this.btnInit.Size = new System.Drawing.Size(122, 24);
            this.btnInit.TabIndex = 1;
            this.btnInit.Text = "클릭한 광고 초기화";
            this.btnInit.UseVisualStyleBackColor = true;
            // 
            // lblCaption
            // 
            this.lblCaption.AutoSize = true;
            this.lblCaption.Location = new System.Drawing.Point(12, 90);
            this.lblCaption.Name = "lblCaption";
            this.lblCaption.Size = new System.Drawing.Size(126, 60);
            this.lblCaption.TabIndex = 2;
            this.lblCaption.Text = "로테이션: X/X\r\n광고목록: X/X\r\n클릭중인 광고 X/X\r\n총 X개의 광고 클릭 됨";
            // 
            // lblTimeout
            // 
            this.lblTimeout.AutoSize = true;
            this.lblTimeout.Location = new System.Drawing.Point(12, 164);
            this.lblTimeout.Name = "lblTimeout";
            this.lblTimeout.Size = new System.Drawing.Size(77, 15);
            this.lblTimeout.TabIndex = 3;
            this.lblTimeout.Text = "Timeout: 0/0";
            // 
            // timerTimeoutCaption
            // 
            this.timerTimeoutCaption.Interval = 1000;
            // 
            // logFlushTimer
            // 
            this.logFlushTimer.Interval = 3000;
            // 
            // ADClickProc
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(256, 188);
            this.Controls.Add(this.lblTimeout);
            this.Controls.Add(this.lblCaption);
            this.Controls.Add(this.btnInit);
            this.Controls.Add(this.btnStart);
            this.Name = "ADClickProc";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button btnStart;
        private Button btnInit;
        private Label lblCaption;
        private Label lblTimeout;
        private System.Windows.Forms.Timer timerTimeoutCaption;
        private System.Windows.Forms.Timer logFlushTimer;
    }
}
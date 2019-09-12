namespace MiniBaccarat.SimpleClient
{
    partial class MainForm
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
            this.btnStop = new System.Windows.Forms.Button();
            this.mmLog = new System.Windows.Forms.RichTextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.cbbBetAmount = new System.Windows.Forms.ComboBox();
            this.cbbBetPool = new System.Windows.Forms.ComboBox();
            this.btnPlaceBet = new System.Windows.Forms.Button();
            this.mmLog2 = new System.Windows.Forms.RichTextBox();
            this.edtShoeCode = new System.Windows.Forms.TextBox();
            this.edtRound = new System.Windows.Forms.TextBox();
            this.edtClientId = new System.Windows.Forms.TextBox();
            this.edtFrontEnd = new System.Windows.Forms.TextBox();
            this.edtGameServer = new System.Windows.Forms.TextBox();
            this.edtTableCode = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(878, 25);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 8;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // mmLog
            // 
            this.mmLog.Location = new System.Drawing.Point(20, 54);
            this.mmLog.Name = "mmLog";
            this.mmLog.Size = new System.Drawing.Size(952, 211);
            this.mmLog.TabIndex = 7;
            this.mmLog.Text = "";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(30, 25);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 6;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // cbbBetAmount
            // 
            this.cbbBetAmount.FormattingEnabled = true;
            this.cbbBetAmount.Items.AddRange(new object[] {
            "1",
            "2",
            "5",
            "10"});
            this.cbbBetAmount.Location = new System.Drawing.Point(20, 284);
            this.cbbBetAmount.Name = "cbbBetAmount";
            this.cbbBetAmount.Size = new System.Drawing.Size(65, 21);
            this.cbbBetAmount.TabIndex = 9;
            this.cbbBetAmount.Text = "1";
            // 
            // cbbBetPool
            // 
            this.cbbBetPool.FormattingEnabled = true;
            this.cbbBetPool.Items.AddRange(new object[] {
            "Banker",
            "Player",
            "Tie"});
            this.cbbBetPool.Location = new System.Drawing.Point(103, 284);
            this.cbbBetPool.Name = "cbbBetPool";
            this.cbbBetPool.Size = new System.Drawing.Size(77, 21);
            this.cbbBetPool.TabIndex = 10;
            this.cbbBetPool.Text = "Banker";
            // 
            // btnPlaceBet
            // 
            this.btnPlaceBet.Location = new System.Drawing.Point(897, 282);
            this.btnPlaceBet.Name = "btnPlaceBet";
            this.btnPlaceBet.Size = new System.Drawing.Size(75, 23);
            this.btnPlaceBet.TabIndex = 11;
            this.btnPlaceBet.Text = "Place Bet";
            this.btnPlaceBet.UseVisualStyleBackColor = true;
            this.btnPlaceBet.Click += new System.EventHandler(this.btnPlaceBet_Click);
            // 
            // mmLog2
            // 
            this.mmLog2.Location = new System.Drawing.Point(20, 321);
            this.mmLog2.Name = "mmLog2";
            this.mmLog2.Size = new System.Drawing.Size(952, 128);
            this.mmLog2.TabIndex = 12;
            this.mmLog2.Text = "";
            // 
            // edtShoeCode
            // 
            this.edtShoeCode.Location = new System.Drawing.Point(435, 284);
            this.edtShoeCode.Name = "edtShoeCode";
            this.edtShoeCode.Size = new System.Drawing.Size(170, 20);
            this.edtShoeCode.TabIndex = 13;
            // 
            // edtRound
            // 
            this.edtRound.Location = new System.Drawing.Point(611, 284);
            this.edtRound.Name = "edtRound";
            this.edtRound.Size = new System.Drawing.Size(111, 20);
            this.edtRound.TabIndex = 14;
            // 
            // edtClientId
            // 
            this.edtClientId.Location = new System.Drawing.Point(728, 285);
            this.edtClientId.Name = "edtClientId";
            this.edtClientId.Size = new System.Drawing.Size(151, 20);
            this.edtClientId.TabIndex = 15;
            // 
            // edtFrontEnd
            // 
            this.edtFrontEnd.Location = new System.Drawing.Point(204, 284);
            this.edtFrontEnd.Name = "edtFrontEnd";
            this.edtFrontEnd.Size = new System.Drawing.Size(62, 20);
            this.edtFrontEnd.TabIndex = 16;
            // 
            // edtGameServer
            // 
            this.edtGameServer.Location = new System.Drawing.Point(272, 284);
            this.edtGameServer.Name = "edtGameServer";
            this.edtGameServer.Size = new System.Drawing.Size(62, 20);
            this.edtGameServer.TabIndex = 17;
            // 
            // edtTableCode
            // 
            this.edtTableCode.Location = new System.Drawing.Point(340, 284);
            this.edtTableCode.Name = "edtTableCode";
            this.edtTableCode.Size = new System.Drawing.Size(89, 20);
            this.edtTableCode.TabIndex = 18;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 461);
            this.Controls.Add(this.edtTableCode);
            this.Controls.Add(this.edtGameServer);
            this.Controls.Add(this.edtFrontEnd);
            this.Controls.Add(this.edtClientId);
            this.Controls.Add(this.edtRound);
            this.Controls.Add(this.edtShoeCode);
            this.Controls.Add(this.mmLog2);
            this.Controls.Add(this.btnPlaceBet);
            this.Controls.Add(this.cbbBetPool);
            this.Controls.Add(this.cbbBetAmount);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.mmLog);
            this.Controls.Add(this.btnStart);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Simple Client";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.RichTextBox mmLog;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.ComboBox cbbBetAmount;
        private System.Windows.Forms.ComboBox cbbBetPool;
        private System.Windows.Forms.Button btnPlaceBet;
        private System.Windows.Forms.RichTextBox mmLog2;
        private System.Windows.Forms.TextBox edtShoeCode;
        private System.Windows.Forms.TextBox edtRound;
        private System.Windows.Forms.TextBox edtClientId;
        private System.Windows.Forms.TextBox edtFrontEnd;
        private System.Windows.Forms.TextBox edtGameServer;
        private System.Windows.Forms.TextBox edtTableCode;
    }
}


namespace PuppetMaster
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
            this.LabelOpID_start = new System.Windows.Forms.Label();
            this.ButtonStart = new System.Windows.Forms.Button();
            this.ButtonInterval = new System.Windows.Forms.Button();
            this.LabelOpID_interval = new System.Windows.Forms.Label();
            this.TextBoxMilliseconds_interval = new System.Windows.Forms.TextBox();
            this.LabelMilliseconds_interval = new System.Windows.Forms.Label();
            this.ButtonStatus = new System.Windows.Forms.Button();
            this.LabelTitle1 = new System.Windows.Forms.Label();
            this.LabelTitle2 = new System.Windows.Forms.Label();
            this.ButtonCrash = new System.Windows.Forms.Button();
            this.LabelCrashOpID = new System.Windows.Forms.Label();
            this.ButtonFreeze = new System.Windows.Forms.Button();
            this.LabelFreezeOpID = new System.Windows.Forms.Label();
            this.ButtonUnfreeze = new System.Windows.Forms.Button();
            this.LabelUnfreezeOpID = new System.Windows.Forms.Label();
            this.TextBoxWait = new System.Windows.Forms.TextBox();
            this.ButtonWait = new System.Windows.Forms.Button();
            this.LabelWait = new System.Windows.Forms.Label();
            this.LabelStatus = new System.Windows.Forms.Label();
            this.LabelCrashIndex = new System.Windows.Forms.Label();
            this.LabelFreezeID = new System.Windows.Forms.Label();
            this.LabelUnfreezeID = new System.Windows.Forms.Label();
            this.CrashID = new System.Windows.Forms.NumericUpDown();
            this.FreezeID = new System.Windows.Forms.NumericUpDown();
            this.UnfreezeID = new System.Windows.Forms.NumericUpDown();
            this.CrashOpID = new System.Windows.Forms.ComboBox();
            this.FreezeOpID = new System.Windows.Forms.ComboBox();
            this.UnfreezeOpID = new System.Windows.Forms.ComboBox();
            this.StartOpID = new System.Windows.Forms.ComboBox();
            this.IntervalOpID = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.RunAllCmdsButton = new System.Windows.Forms.Button();
            this.NextCmdButton = new System.Windows.Forms.Button();
            this.RichTBEventLog = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.CrashID)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FreezeID)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.UnfreezeID)).BeginInit();
            this.SuspendLayout();
            // 
            // LabelOpID_start
            // 
            this.LabelOpID_start.AutoSize = true;
            this.LabelOpID_start.Location = new System.Drawing.Point(29, 53);
            this.LabelOpID_start.Name = "LabelOpID_start";
            this.LabelOpID_start.Size = new System.Drawing.Size(62, 13);
            this.LabelOpID_start.TabIndex = 0;
            this.LabelOpID_start.Text = "Operator id:";
            // 
            // ButtonStart
            // 
            this.ButtonStart.Location = new System.Drawing.Point(214, 53);
            this.ButtonStart.Name = "ButtonStart";
            this.ButtonStart.Size = new System.Drawing.Size(75, 23);
            this.ButtonStart.TabIndex = 1;
            this.ButtonStart.Text = "Start";
            this.ButtonStart.UseVisualStyleBackColor = true;
            this.ButtonStart.Click += new System.EventHandler(this.ButtonStart_Click);
            // 
            // ButtonInterval
            // 
            this.ButtonInterval.Location = new System.Drawing.Point(215, 123);
            this.ButtonInterval.Name = "ButtonInterval";
            this.ButtonInterval.Size = new System.Drawing.Size(75, 23);
            this.ButtonInterval.TabIndex = 4;
            this.ButtonInterval.Text = "Interval";
            this.ButtonInterval.UseVisualStyleBackColor = true;
            this.ButtonInterval.Click += new System.EventHandler(this.ButtonInterval_Click);
            // 
            // LabelOpID_interval
            // 
            this.LabelOpID_interval.AutoSize = true;
            this.LabelOpID_interval.Location = new System.Drawing.Point(30, 90);
            this.LabelOpID_interval.Name = "LabelOpID_interval";
            this.LabelOpID_interval.Size = new System.Drawing.Size(62, 13);
            this.LabelOpID_interval.TabIndex = 3;
            this.LabelOpID_interval.Text = "Operator id:";
            // 
            // TextBoxMilliseconds_interval
            // 
            this.TextBoxMilliseconds_interval.Location = new System.Drawing.Point(99, 125);
            this.TextBoxMilliseconds_interval.Name = "TextBoxMilliseconds_interval";
            this.TextBoxMilliseconds_interval.Size = new System.Drawing.Size(100, 20);
            this.TextBoxMilliseconds_interval.TabIndex = 7;
            // 
            // LabelMilliseconds_interval
            // 
            this.LabelMilliseconds_interval.AutoSize = true;
            this.LabelMilliseconds_interval.Location = new System.Drawing.Point(30, 125);
            this.LabelMilliseconds_interval.Name = "LabelMilliseconds_interval";
            this.LabelMilliseconds_interval.Size = new System.Drawing.Size(67, 13);
            this.LabelMilliseconds_interval.TabIndex = 6;
            this.LabelMilliseconds_interval.Text = "Milliseconds:";
            // 
            // ButtonStatus
            // 
            this.ButtonStatus.Location = new System.Drawing.Point(214, 162);
            this.ButtonStatus.Name = "ButtonStatus";
            this.ButtonStatus.Size = new System.Drawing.Size(75, 23);
            this.ButtonStatus.TabIndex = 8;
            this.ButtonStatus.Text = "Status";
            this.ButtonStatus.UseVisualStyleBackColor = true;
            this.ButtonStatus.Click += new System.EventHandler(this.ButtonStatus_Click);
            // 
            // LabelTitle1
            // 
            this.LabelTitle1.AutoSize = true;
            this.LabelTitle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelTitle1.Location = new System.Drawing.Point(29, 12);
            this.LabelTitle1.Name = "LabelTitle1";
            this.LabelTitle1.Size = new System.Drawing.Size(141, 20);
            this.LabelTitle1.TabIndex = 10;
            this.LabelTitle1.Text = "Main Commands";
            // 
            // LabelTitle2
            // 
            this.LabelTitle2.AutoSize = true;
            this.LabelTitle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelTitle2.Location = new System.Drawing.Point(29, 204);
            this.LabelTitle2.Name = "LabelTitle2";
            this.LabelTitle2.Size = new System.Drawing.Size(190, 20);
            this.LabelTitle2.TabIndex = 11;
            this.LabelTitle2.Text = "Debugging Commands";
            // 
            // ButtonCrash
            // 
            this.ButtonCrash.Location = new System.Drawing.Point(214, 238);
            this.ButtonCrash.Name = "ButtonCrash";
            this.ButtonCrash.Size = new System.Drawing.Size(75, 23);
            this.ButtonCrash.TabIndex = 13;
            this.ButtonCrash.Text = "Crash";
            this.ButtonCrash.UseVisualStyleBackColor = true;
            this.ButtonCrash.Click += new System.EventHandler(this.ButtonCrash_Click);
            // 
            // LabelCrashOpID
            // 
            this.LabelCrashOpID.AutoSize = true;
            this.LabelCrashOpID.Location = new System.Drawing.Point(28, 244);
            this.LabelCrashOpID.Name = "LabelCrashOpID";
            this.LabelCrashOpID.Size = new System.Drawing.Size(62, 13);
            this.LabelCrashOpID.TabIndex = 12;
            this.LabelCrashOpID.Text = "Operator id:";
            // 
            // ButtonFreeze
            // 
            this.ButtonFreeze.Location = new System.Drawing.Point(215, 274);
            this.ButtonFreeze.Name = "ButtonFreeze";
            this.ButtonFreeze.Size = new System.Drawing.Size(75, 23);
            this.ButtonFreeze.TabIndex = 16;
            this.ButtonFreeze.Text = "Freeze";
            this.ButtonFreeze.UseVisualStyleBackColor = true;
            this.ButtonFreeze.Click += new System.EventHandler(this.ButtonFreeze_Click);
            // 
            // LabelFreezeOpID
            // 
            this.LabelFreezeOpID.AutoSize = true;
            this.LabelFreezeOpID.Location = new System.Drawing.Point(30, 274);
            this.LabelFreezeOpID.Name = "LabelFreezeOpID";
            this.LabelFreezeOpID.Size = new System.Drawing.Size(62, 13);
            this.LabelFreezeOpID.TabIndex = 15;
            this.LabelFreezeOpID.Text = "Operator id:";
            // 
            // ButtonUnfreeze
            // 
            this.ButtonUnfreeze.Location = new System.Drawing.Point(214, 310);
            this.ButtonUnfreeze.Name = "ButtonUnfreeze";
            this.ButtonUnfreeze.Size = new System.Drawing.Size(75, 23);
            this.ButtonUnfreeze.TabIndex = 19;
            this.ButtonUnfreeze.Text = "Unfreeze";
            this.ButtonUnfreeze.UseVisualStyleBackColor = true;
            this.ButtonUnfreeze.Click += new System.EventHandler(this.ButtonUnfreeze_Click);
            // 
            // LabelUnfreezeOpID
            // 
            this.LabelUnfreezeOpID.AutoSize = true;
            this.LabelUnfreezeOpID.Location = new System.Drawing.Point(29, 310);
            this.LabelUnfreezeOpID.Name = "LabelUnfreezeOpID";
            this.LabelUnfreezeOpID.Size = new System.Drawing.Size(62, 13);
            this.LabelUnfreezeOpID.TabIndex = 18;
            this.LabelUnfreezeOpID.Text = "Operator id:";
            // 
            // TextBoxWait
            // 
            this.TextBoxWait.Location = new System.Drawing.Point(97, 345);
            this.TextBoxWait.Name = "TextBoxWait";
            this.TextBoxWait.Size = new System.Drawing.Size(100, 20);
            this.TextBoxWait.TabIndex = 23;
            // 
            // ButtonWait
            // 
            this.ButtonWait.Location = new System.Drawing.Point(214, 345);
            this.ButtonWait.Name = "ButtonWait";
            this.ButtonWait.Size = new System.Drawing.Size(75, 23);
            this.ButtonWait.TabIndex = 22;
            this.ButtonWait.Text = "Wait";
            this.ButtonWait.UseVisualStyleBackColor = true;
            this.ButtonWait.Click += new System.EventHandler(this.ButtonWait_Click);
            // 
            // LabelWait
            // 
            this.LabelWait.AutoSize = true;
            this.LabelWait.Location = new System.Drawing.Point(29, 345);
            this.LabelWait.Name = "LabelWait";
            this.LabelWait.Size = new System.Drawing.Size(67, 13);
            this.LabelWait.TabIndex = 21;
            this.LabelWait.Text = "Milliseconds:";
            // 
            // LabelStatus
            // 
            this.LabelStatus.AutoSize = true;
            this.LabelStatus.Location = new System.Drawing.Point(29, 167);
            this.LabelStatus.Name = "LabelStatus";
            this.LabelStatus.Size = new System.Drawing.Size(166, 13);
            this.LabelStatus.TabIndex = 24;
            this.LabelStatus.Text = "All nodes print their current status:";
            // 
            // LabelCrashIndex
            // 
            this.LabelCrashIndex.AutoSize = true;
            this.LabelCrashIndex.Location = new System.Drawing.Point(146, 241);
            this.LabelCrashIndex.Name = "LabelCrashIndex";
            this.LabelCrashIndex.Size = new System.Drawing.Size(19, 13);
            this.LabelCrashIndex.TabIndex = 25;
            this.LabelCrashIndex.Text = "Id:";
            // 
            // LabelFreezeID
            // 
            this.LabelFreezeID.AutoSize = true;
            this.LabelFreezeID.Location = new System.Drawing.Point(146, 279);
            this.LabelFreezeID.Name = "LabelFreezeID";
            this.LabelFreezeID.Size = new System.Drawing.Size(19, 13);
            this.LabelFreezeID.TabIndex = 27;
            this.LabelFreezeID.Text = "Id:";
            // 
            // LabelUnfreezeID
            // 
            this.LabelUnfreezeID.AutoSize = true;
            this.LabelUnfreezeID.Location = new System.Drawing.Point(146, 316);
            this.LabelUnfreezeID.Name = "LabelUnfreezeID";
            this.LabelUnfreezeID.Size = new System.Drawing.Size(19, 13);
            this.LabelUnfreezeID.TabIndex = 29;
            this.LabelUnfreezeID.Text = "Id:";
            // 
            // CrashID
            // 
            this.CrashID.Location = new System.Drawing.Point(171, 241);
            this.CrashID.Name = "CrashID";
            this.CrashID.Size = new System.Drawing.Size(37, 20);
            this.CrashID.TabIndex = 31;
            // 
            // FreezeID
            // 
            this.FreezeID.Location = new System.Drawing.Point(172, 277);
            this.FreezeID.Name = "FreezeID";
            this.FreezeID.Size = new System.Drawing.Size(37, 20);
            this.FreezeID.TabIndex = 32;
            // 
            // UnfreezeID
            // 
            this.UnfreezeID.Location = new System.Drawing.Point(172, 311);
            this.UnfreezeID.Name = "UnfreezeID";
            this.UnfreezeID.Size = new System.Drawing.Size(37, 20);
            this.UnfreezeID.TabIndex = 33;
            // 
            // CrashOpID
            // 
            this.CrashOpID.FormattingEnabled = true;
            this.CrashOpID.Location = new System.Drawing.Point(97, 241);
            this.CrashOpID.Name = "CrashOpID";
            this.CrashOpID.Size = new System.Drawing.Size(43, 21);
            this.CrashOpID.TabIndex = 34;
            // 
            // FreezeOpID
            // 
            this.FreezeOpID.FormattingEnabled = true;
            this.FreezeOpID.Location = new System.Drawing.Point(97, 274);
            this.FreezeOpID.Name = "FreezeOpID";
            this.FreezeOpID.Size = new System.Drawing.Size(43, 21);
            this.FreezeOpID.TabIndex = 35;
            // 
            // UnfreezeOpID
            // 
            this.UnfreezeOpID.FormattingEnabled = true;
            this.UnfreezeOpID.Location = new System.Drawing.Point(97, 310);
            this.UnfreezeOpID.Name = "UnfreezeOpID";
            this.UnfreezeOpID.Size = new System.Drawing.Size(43, 21);
            this.UnfreezeOpID.TabIndex = 36;
            // 
            // StartOpID
            // 
            this.StartOpID.FormattingEnabled = true;
            this.StartOpID.Location = new System.Drawing.Point(97, 53);
            this.StartOpID.Name = "StartOpID";
            this.StartOpID.Size = new System.Drawing.Size(102, 21);
            this.StartOpID.TabIndex = 37;
            // 
            // IntervalOpID
            // 
            this.IntervalOpID.FormattingEnabled = true;
            this.IntervalOpID.Location = new System.Drawing.Point(98, 90);
            this.IntervalOpID.Name = "IntervalOpID";
            this.IntervalOpID.Size = new System.Drawing.Size(101, 21);
            this.IntervalOpID.TabIndex = 38;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(320, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 20);
            this.label1.TabIndex = 40;
            this.label1.Text = "Event Log:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(29, 382);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(243, 20);
            this.label2.TabIndex = 41;
            this.label2.Text = "Execute Commands from file:";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // RunAllCmdsButton
            // 
            this.RunAllCmdsButton.Location = new System.Drawing.Point(33, 414);
            this.RunAllCmdsButton.Name = "RunAllCmdsButton";
            this.RunAllCmdsButton.Size = new System.Drawing.Size(144, 23);
            this.RunAllCmdsButton.TabIndex = 43;
            this.RunAllCmdsButton.Text = "Run all commands";
            this.RunAllCmdsButton.UseVisualStyleBackColor = true;
            this.RunAllCmdsButton.Click += new System.EventHandler(this.RunAllCmdsButton_Click);
            // 
            // NextCmdButton
            // 
            this.NextCmdButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.NextCmdButton.Location = new System.Drawing.Point(33, 443);
            this.NextCmdButton.Name = "NextCmdButton";
            this.NextCmdButton.Size = new System.Drawing.Size(144, 23);
            this.NextCmdButton.TabIndex = 44;
            this.NextCmdButton.Text = "Next Command";
            this.NextCmdButton.UseVisualStyleBackColor = true;
            this.NextCmdButton.Click += new System.EventHandler(this.NextCmdButton_Click);
            // 
            // RichTBEventLog
            // 
            this.RichTBEventLog.BackColor = System.Drawing.SystemColors.MenuText;
            this.RichTBEventLog.ForeColor = System.Drawing.SystemColors.Window;
            this.RichTBEventLog.Location = new System.Drawing.Point(315, 49);
            this.RichTBEventLog.Name = "RichTBEventLog";
            this.RichTBEventLog.Size = new System.Drawing.Size(311, 399);
            this.RichTBEventLog.TabIndex = 45;
            this.RichTBEventLog.Text = "";
            this.RichTBEventLog.TextChanged += new System.EventHandler(this.RichTBEventLog_TextChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(670, 486);
            this.Controls.Add(this.RichTBEventLog);
            this.Controls.Add(this.NextCmdButton);
            this.Controls.Add(this.RunAllCmdsButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.IntervalOpID);
            this.Controls.Add(this.StartOpID);
            this.Controls.Add(this.UnfreezeOpID);
            this.Controls.Add(this.FreezeOpID);
            this.Controls.Add(this.CrashOpID);
            this.Controls.Add(this.UnfreezeID);
            this.Controls.Add(this.FreezeID);
            this.Controls.Add(this.CrashID);
            this.Controls.Add(this.LabelUnfreezeID);
            this.Controls.Add(this.LabelFreezeID);
            this.Controls.Add(this.LabelCrashIndex);
            this.Controls.Add(this.LabelStatus);
            this.Controls.Add(this.TextBoxWait);
            this.Controls.Add(this.ButtonWait);
            this.Controls.Add(this.LabelWait);
            this.Controls.Add(this.ButtonUnfreeze);
            this.Controls.Add(this.LabelUnfreezeOpID);
            this.Controls.Add(this.ButtonFreeze);
            this.Controls.Add(this.LabelFreezeOpID);
            this.Controls.Add(this.ButtonCrash);
            this.Controls.Add(this.LabelCrashOpID);
            this.Controls.Add(this.LabelTitle2);
            this.Controls.Add(this.LabelTitle1);
            this.Controls.Add(this.ButtonStatus);
            this.Controls.Add(this.TextBoxMilliseconds_interval);
            this.Controls.Add(this.LabelMilliseconds_interval);
            this.Controls.Add(this.ButtonInterval);
            this.Controls.Add(this.LabelOpID_interval);
            this.Controls.Add(this.ButtonStart);
            this.Controls.Add(this.LabelOpID_start);
            this.Name = "MainForm";
            this.Text = "Puppet Master";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.CrashID)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FreezeID)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.UnfreezeID)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LabelOpID_start;
        private System.Windows.Forms.Button ButtonStart;
        private System.Windows.Forms.Button ButtonInterval;
        private System.Windows.Forms.Label LabelOpID_interval;
        private System.Windows.Forms.TextBox TextBoxMilliseconds_interval;
        private System.Windows.Forms.Label LabelMilliseconds_interval;
        private System.Windows.Forms.Button ButtonStatus;
        private System.Windows.Forms.Label LabelTitle1;
        private System.Windows.Forms.Label LabelTitle2;
        private System.Windows.Forms.Button ButtonCrash;
        private System.Windows.Forms.Label LabelCrashOpID;
        private System.Windows.Forms.Button ButtonFreeze;
        private System.Windows.Forms.Label LabelFreezeOpID;
        private System.Windows.Forms.Button ButtonUnfreeze;
        private System.Windows.Forms.Label LabelUnfreezeOpID;
        private System.Windows.Forms.TextBox TextBoxWait;
        private System.Windows.Forms.Button ButtonWait;
        private System.Windows.Forms.Label LabelWait;
        private System.Windows.Forms.Label LabelStatus;
        private System.Windows.Forms.Label LabelCrashIndex;
        private System.Windows.Forms.Label LabelFreezeID;
        private System.Windows.Forms.Label LabelUnfreezeID;
        private System.Windows.Forms.NumericUpDown CrashID;
        private System.Windows.Forms.NumericUpDown FreezeID;
        private System.Windows.Forms.NumericUpDown UnfreezeID;
        private System.Windows.Forms.ComboBox CrashOpID;
        private System.Windows.Forms.ComboBox FreezeOpID;
        private System.Windows.Forms.ComboBox UnfreezeOpID;
        private System.Windows.Forms.ComboBox StartOpID;
        private System.Windows.Forms.ComboBox IntervalOpID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button RunAllCmdsButton;
        private System.Windows.Forms.Button NextCmdButton;
        private System.Windows.Forms.RichTextBox RichTBEventLog;
    }
}


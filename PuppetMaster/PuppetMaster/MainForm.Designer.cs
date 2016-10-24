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
            this.TextBoxOpID_start = new System.Windows.Forms.TextBox();
            this.TextBoxOpID_interval = new System.Windows.Forms.TextBox();
            this.ButtonInterval = new System.Windows.Forms.Button();
            this.LabelOpID_interval = new System.Windows.Forms.Label();
            this.TextBoxMilliseconds_interval = new System.Windows.Forms.TextBox();
            this.LabelMilliseconds_interval = new System.Windows.Forms.Label();
            this.ButtonStatus = new System.Windows.Forms.Button();
            this.TextBoxStatus = new System.Windows.Forms.TextBox();
            this.LabelTitle1 = new System.Windows.Forms.Label();
            this.LabelTitle2 = new System.Windows.Forms.Label();
            this.TexBoxCrash = new System.Windows.Forms.TextBox();
            this.ButtonCrash = new System.Windows.Forms.Button();
            this.LabelCrash = new System.Windows.Forms.Label();
            this.TextBoxFreeze = new System.Windows.Forms.TextBox();
            this.ButtonFreeze = new System.Windows.Forms.Button();
            this.LabelFreeze = new System.Windows.Forms.Label();
            this.TextBoxUnfreeze = new System.Windows.Forms.TextBox();
            this.ButtonUnfreeze = new System.Windows.Forms.Button();
            this.LabelUnfreeze = new System.Windows.Forms.Label();
            this.TextBoxWait = new System.Windows.Forms.TextBox();
            this.ButtonWait = new System.Windows.Forms.Button();
            this.LabelWait = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LabelOpID_start
            // 
            this.LabelOpID_start.AutoSize = true;
            this.LabelOpID_start.Location = new System.Drawing.Point(29, 62);
            this.LabelOpID_start.Name = "LabelOpID_start";
            this.LabelOpID_start.Size = new System.Drawing.Size(62, 13);
            this.LabelOpID_start.TabIndex = 0;
            this.LabelOpID_start.Text = "Operator id:";
            // 
            // ButtonStart
            // 
            this.ButtonStart.Location = new System.Drawing.Point(214, 62);
            this.ButtonStart.Name = "ButtonStart";
            this.ButtonStart.Size = new System.Drawing.Size(75, 23);
            this.ButtonStart.TabIndex = 1;
            this.ButtonStart.Text = "Start";
            this.ButtonStart.UseVisualStyleBackColor = true;
            this.ButtonStart.Click += new System.EventHandler(this.ButtonStart_Click);
            // 
            // TextBoxOpID_start
            // 
            this.TextBoxOpID_start.Location = new System.Drawing.Point(98, 62);
            this.TextBoxOpID_start.Name = "TextBoxOpID_start";
            this.TextBoxOpID_start.Size = new System.Drawing.Size(100, 20);
            this.TextBoxOpID_start.TabIndex = 2;
            // 
            // TextBoxOpID_interval
            // 
            this.TextBoxOpID_interval.Location = new System.Drawing.Point(98, 127);
            this.TextBoxOpID_interval.Name = "TextBoxOpID_interval";
            this.TextBoxOpID_interval.Size = new System.Drawing.Size(100, 20);
            this.TextBoxOpID_interval.TabIndex = 5;
            // 
            // ButtonInterval
            // 
            this.ButtonInterval.Location = new System.Drawing.Point(214, 127);
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
            this.LabelOpID_interval.Location = new System.Drawing.Point(29, 127);
            this.LabelOpID_interval.Name = "LabelOpID_interval";
            this.LabelOpID_interval.Size = new System.Drawing.Size(62, 13);
            this.LabelOpID_interval.TabIndex = 3;
            this.LabelOpID_interval.Text = "Operator id:";
            // 
            // TextBoxMilliseconds_interval
            // 
            this.TextBoxMilliseconds_interval.Location = new System.Drawing.Point(98, 157);
            this.TextBoxMilliseconds_interval.Name = "TextBoxMilliseconds_interval";
            this.TextBoxMilliseconds_interval.Size = new System.Drawing.Size(100, 20);
            this.TextBoxMilliseconds_interval.TabIndex = 7;
            // 
            // LabelMilliseconds_interval
            // 
            this.LabelMilliseconds_interval.AutoSize = true;
            this.LabelMilliseconds_interval.Location = new System.Drawing.Point(29, 157);
            this.LabelMilliseconds_interval.Name = "LabelMilliseconds_interval";
            this.LabelMilliseconds_interval.Size = new System.Drawing.Size(67, 13);
            this.LabelMilliseconds_interval.TabIndex = 6;
            this.LabelMilliseconds_interval.Text = "Milliseconds:";
            // 
            // ButtonStatus
            // 
            this.ButtonStatus.Location = new System.Drawing.Point(498, 64);
            this.ButtonStatus.Name = "ButtonStatus";
            this.ButtonStatus.Size = new System.Drawing.Size(75, 23);
            this.ButtonStatus.TabIndex = 8;
            this.ButtonStatus.Text = "Status";
            this.ButtonStatus.UseVisualStyleBackColor = true;
            this.ButtonStatus.Click += new System.EventHandler(this.ButtonStatus_Click);
            // 
            // TextBoxStatus
            // 
            this.TextBoxStatus.Location = new System.Drawing.Point(330, 64);
            this.TextBoxStatus.Multiline = true;
            this.TextBoxStatus.Name = "TextBoxStatus";
            this.TextBoxStatus.Size = new System.Drawing.Size(152, 113);
            this.TextBoxStatus.TabIndex = 9;
            // 
            // LabelTitle1
            // 
            this.LabelTitle1.AutoSize = true;
            this.LabelTitle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelTitle1.Location = new System.Drawing.Point(29, 21);
            this.LabelTitle1.Name = "LabelTitle1";
            this.LabelTitle1.Size = new System.Drawing.Size(166, 24);
            this.LabelTitle1.TabIndex = 10;
            this.LabelTitle1.Text = "Main Commands";
            // 
            // LabelTitle2
            // 
            this.LabelTitle2.AutoSize = true;
            this.LabelTitle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelTitle2.Location = new System.Drawing.Point(28, 212);
            this.LabelTitle2.Name = "LabelTitle2";
            this.LabelTitle2.Size = new System.Drawing.Size(224, 24);
            this.LabelTitle2.TabIndex = 11;
            this.LabelTitle2.Text = "Debugging Commands";
            // 
            // TexBoxCrash
            // 
            this.TexBoxCrash.Location = new System.Drawing.Point(112, 254);
            this.TexBoxCrash.Name = "TexBoxCrash";
            this.TexBoxCrash.Size = new System.Drawing.Size(86, 20);
            this.TexBoxCrash.TabIndex = 14;
            // 
            // ButtonCrash
            // 
            this.ButtonCrash.Location = new System.Drawing.Point(214, 254);
            this.ButtonCrash.Name = "ButtonCrash";
            this.ButtonCrash.Size = new System.Drawing.Size(75, 23);
            this.ButtonCrash.TabIndex = 13;
            this.ButtonCrash.Text = "Crash";
            this.ButtonCrash.UseVisualStyleBackColor = true;
            // 
            // LabelCrash
            // 
            this.LabelCrash.AutoSize = true;
            this.LabelCrash.Location = new System.Drawing.Point(29, 254);
            this.LabelCrash.Name = "LabelCrash";
            this.LabelCrash.Size = new System.Drawing.Size(77, 13);
            this.LabelCrash.TabIndex = 12;
            this.LabelCrash.Text = "Process name:";
            // 
            // TextBoxFreeze
            // 
            this.TextBoxFreeze.Location = new System.Drawing.Point(113, 290);
            this.TextBoxFreeze.Name = "TextBoxFreeze";
            this.TextBoxFreeze.Size = new System.Drawing.Size(86, 20);
            this.TextBoxFreeze.TabIndex = 17;
            // 
            // ButtonFreeze
            // 
            this.ButtonFreeze.Location = new System.Drawing.Point(215, 290);
            this.ButtonFreeze.Name = "ButtonFreeze";
            this.ButtonFreeze.Size = new System.Drawing.Size(75, 23);
            this.ButtonFreeze.TabIndex = 16;
            this.ButtonFreeze.Text = "Freeze";
            this.ButtonFreeze.UseVisualStyleBackColor = true;
            // 
            // LabelFreeze
            // 
            this.LabelFreeze.AutoSize = true;
            this.LabelFreeze.Location = new System.Drawing.Point(30, 290);
            this.LabelFreeze.Name = "LabelFreeze";
            this.LabelFreeze.Size = new System.Drawing.Size(77, 13);
            this.LabelFreeze.TabIndex = 15;
            this.LabelFreeze.Text = "Process name:";
            // 
            // TextBoxUnfreeze
            // 
            this.TextBoxUnfreeze.Location = new System.Drawing.Point(112, 326);
            this.TextBoxUnfreeze.Name = "TextBoxUnfreeze";
            this.TextBoxUnfreeze.Size = new System.Drawing.Size(86, 20);
            this.TextBoxUnfreeze.TabIndex = 20;
            // 
            // ButtonUnfreeze
            // 
            this.ButtonUnfreeze.Location = new System.Drawing.Point(214, 326);
            this.ButtonUnfreeze.Name = "ButtonUnfreeze";
            this.ButtonUnfreeze.Size = new System.Drawing.Size(75, 23);
            this.ButtonUnfreeze.TabIndex = 19;
            this.ButtonUnfreeze.Text = "Unfreeze";
            this.ButtonUnfreeze.UseVisualStyleBackColor = true;
            // 
            // LabelUnfreeze
            // 
            this.LabelUnfreeze.AutoSize = true;
            this.LabelUnfreeze.Location = new System.Drawing.Point(29, 326);
            this.LabelUnfreeze.Name = "LabelUnfreeze";
            this.LabelUnfreeze.Size = new System.Drawing.Size(77, 13);
            this.LabelUnfreeze.TabIndex = 18;
            this.LabelUnfreeze.Text = "Process name:";
            // 
            // TextBoxWait
            // 
            this.TextBoxWait.Location = new System.Drawing.Point(112, 361);
            this.TextBoxWait.Name = "TextBoxWait";
            this.TextBoxWait.Size = new System.Drawing.Size(86, 20);
            this.TextBoxWait.TabIndex = 23;
            // 
            // ButtonWait
            // 
            this.ButtonWait.Location = new System.Drawing.Point(214, 361);
            this.ButtonWait.Name = "ButtonWait";
            this.ButtonWait.Size = new System.Drawing.Size(75, 23);
            this.ButtonWait.TabIndex = 22;
            this.ButtonWait.Text = "Wait";
            this.ButtonWait.UseVisualStyleBackColor = true;
            // 
            // LabelWait
            // 
            this.LabelWait.AutoSize = true;
            this.LabelWait.Location = new System.Drawing.Point(29, 361);
            this.LabelWait.Name = "LabelWait";
            this.LabelWait.Size = new System.Drawing.Size(67, 13);
            this.LabelWait.TabIndex = 21;
            this.LabelWait.Text = "Milliseconds:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(603, 404);
            this.Controls.Add(this.TextBoxWait);
            this.Controls.Add(this.ButtonWait);
            this.Controls.Add(this.LabelWait);
            this.Controls.Add(this.TextBoxUnfreeze);
            this.Controls.Add(this.ButtonUnfreeze);
            this.Controls.Add(this.LabelUnfreeze);
            this.Controls.Add(this.TextBoxFreeze);
            this.Controls.Add(this.ButtonFreeze);
            this.Controls.Add(this.LabelFreeze);
            this.Controls.Add(this.TexBoxCrash);
            this.Controls.Add(this.ButtonCrash);
            this.Controls.Add(this.LabelCrash);
            this.Controls.Add(this.LabelTitle2);
            this.Controls.Add(this.LabelTitle1);
            this.Controls.Add(this.TextBoxStatus);
            this.Controls.Add(this.ButtonStatus);
            this.Controls.Add(this.TextBoxMilliseconds_interval);
            this.Controls.Add(this.LabelMilliseconds_interval);
            this.Controls.Add(this.TextBoxOpID_interval);
            this.Controls.Add(this.ButtonInterval);
            this.Controls.Add(this.LabelOpID_interval);
            this.Controls.Add(this.TextBoxOpID_start);
            this.Controls.Add(this.ButtonStart);
            this.Controls.Add(this.LabelOpID_start);
            this.Name = "MainForm";
            this.Text = "Puppet Master";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LabelOpID_start;
        private System.Windows.Forms.Button ButtonStart;
        private System.Windows.Forms.TextBox TextBoxOpID_start;
        private System.Windows.Forms.TextBox TextBoxOpID_interval;
        private System.Windows.Forms.Button ButtonInterval;
        private System.Windows.Forms.Label LabelOpID_interval;
        private System.Windows.Forms.TextBox TextBoxMilliseconds_interval;
        private System.Windows.Forms.Label LabelMilliseconds_interval;
        private System.Windows.Forms.Button ButtonStatus;
        private System.Windows.Forms.TextBox TextBoxStatus;
        private System.Windows.Forms.Label LabelTitle1;
        private System.Windows.Forms.Label LabelTitle2;
        private System.Windows.Forms.TextBox TexBoxCrash;
        private System.Windows.Forms.Button ButtonCrash;
        private System.Windows.Forms.Label LabelCrash;
        private System.Windows.Forms.TextBox TextBoxFreeze;
        private System.Windows.Forms.Button ButtonFreeze;
        private System.Windows.Forms.Label LabelFreeze;
        private System.Windows.Forms.TextBox TextBoxUnfreeze;
        private System.Windows.Forms.Button ButtonUnfreeze;
        private System.Windows.Forms.Label LabelUnfreeze;
        private System.Windows.Forms.TextBox TextBoxWait;
        private System.Windows.Forms.Button ButtonWait;
        private System.Windows.Forms.Label LabelWait;
    }
}


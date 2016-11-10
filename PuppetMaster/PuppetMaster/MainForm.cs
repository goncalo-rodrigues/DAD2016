﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster
{
    public partial class MainForm : Form
    {
       

        PuppetMaster master = null;
        TextReader commandReader = null;
        public delegate void UpdateFormDelegate(string message);

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            string opID = StartOpID.Text;
            String[] args = { opID };
            master.ExecuteCommand("start", args);
        }

        private void ButtonInterval_Click(object sender, EventArgs e)
        {
            String[] args = { IntervalOpID.Text, TextBoxMilliseconds_interval.Text };
            master.ExecuteCommand("interval", args);
        }

        private void ButtonStatus_Click(object sender, EventArgs e)
        {
            master.ExecuteCommand("status");
        }

        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(PuppetMaster master)
        {
            this.master = master;

            PMLoggerService.form = this;
            InitializeComponent();

            foreach (var node in this.master.nodes)
            {
                StartOpID.Items.Add(node.Key);
                IntervalOpID.Items.Add(node.Key);
                CrashOpID.Items.Add(node.Key);
                FreezeOpID.Items.Add(node.Key);
                UnfreezeOpID.Items.Add(node.Key);
            }

            //Configure windows position
            Console.Title = "PuppetMasterInterface";
            var screen = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            var width = screen.Width;
            var height = screen.Height;

            commandReader = new StringReader(master.commandsToBeExecuted);
        }


        private void Form1_Load(object sender, EventArgs e)
        {
        }
        private void ButtonCrash_Click(object sender, EventArgs e)
        {
            string opID = CrashOpID.Text;
            string index = CrashID.Value.ToString();

            String[] args = { opID, index };
            master.ExecuteCommand("crash", args);
        }

        private void ButtonFreeze_Click(object sender, EventArgs e)
        {
            string opID = FreezeOpID.Text;
            string index = FreezeID.Value.ToString();
            String[] args = { opID, index };
           
            master.ExecuteCommand("freeze", args);
        }

        private void ButtonUnfreeze_Click(object sender, EventArgs e)
        {
            string opID = FreezeOpID.Text;
            string index = UnfreezeID.Value.ToString();
            String[] args = { opID, index };

            master.ExecuteCommand("unfreeze", args);
        }

        private void ButtonWait_Click(object sender, EventArgs e)
        {
            string ms = TextBoxWait.Text;
            String[] args = { ms };
            master.ExecuteCommand("unfreeze", args);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            List<Task> tasks = new List<Task>();
            foreach (var node in this.master.nodes)
            {
                for (int i = 0; i < node.Value.Replicas.Count; i++)
                { 
                    String[] args = { node.Key, i.ToString() };
                    tasks.Add(Task.Run(() => master.ExecuteCommand("crash", args)));
                }
            }
            Task.WaitAll(tasks.ToArray());

        }

        public void LogEvent(String s) {
            RichTBEventLog.Text = RichTBEventLog.Text + "\r\n" + s; 
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void RunAllCmdsButton_Click(object sender, EventArgs e)
        {
            master.ExecuteCommands();
        }

        private async void NextCmdButton_Click(object sender, EventArgs e)
        {
            var success = await master.ExecuteNextCommand(commandReader);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var screen = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            var width = screen.Width;
            var height = screen.Height;
            this.Size = new Size(width/2, height-(height / 3));
            this.Location = new Point(0, height / 3);
            this.StartPosition = FormStartPosition.Manual;
        }

        private void RichTBEventLog_TextChanged(object sender, EventArgs e)
        {
            RichTBEventLog.SelectionStart = RichTBEventLog.Text.Length;
            RichTBEventLog.ScrollToCaret();
        }
    }
}

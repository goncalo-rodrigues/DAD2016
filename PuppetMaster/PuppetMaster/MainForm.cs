using System;
using System.IO;
using System.Windows.Forms;

namespace PuppetMaster
{
    public partial class MainForm : Form
    {
       

        PuppetMaster master = null;
        TextReader commandReader = null;

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

            String[] args = { opID };
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
            foreach (var node in this.master.nodes)
            {
                for (int i = 0; i < node.Value.Replicas.Count; i++)
                { 
                    String[] args = { node.Key, i.ToString() };
                    master.ExecuteCommand("crash", args);
                }
            }

        }

        public void LogEvent(String s) {
            TextBoxEventLog.Text = TextBoxEventLog.Text + "\n\r" + s; 
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
    }
}

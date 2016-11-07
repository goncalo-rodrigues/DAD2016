using System;

using System.Windows.Forms;

namespace PuppetMaster
{
    public partial class MainForm : Form
    {
       

        PuppetMaster master = null;

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

            foreach (var node in this.master.nodes){
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

        }


        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void Form1_Closing(object sender, EventArgs e)
        {

            foreach (var node in this.master.nodes)
            {
                for (int i = 0; i < node.Value.Replicas.Count; i++)
                {
                    master.Crash(node.Key, i);
                }
            }

        }


        private void ButtonCrash_Click(object sender, EventArgs e)
        {
            string opID = CrashOpID.Text;
            int index = Convert.ToInt32(CrashID.Value);
            master.Crash(opID,index);

        }

        private void ButtonFreeze_Click(object sender, EventArgs e)
        {
            string opID = FreezeOpID.Text;
            int index = Convert.ToInt32(FreezeID.Value);
            master.Freeze(opID, index); 
        }

        private void ButtonUnfreeze_Click(object sender, EventArgs e)
        {
            string opID = UnfreezeOpID.Text;
            int index = Convert.ToInt32(UnfreezeID.Value);
            master.Unfreeze(opID, index);

        }

        private void ButtonWait_Click(object sender, EventArgs e)
        {
            int ms = Convert.ToInt32(TextBoxWait.Text);
            master.Wait(ms);
        }

    
    }
}

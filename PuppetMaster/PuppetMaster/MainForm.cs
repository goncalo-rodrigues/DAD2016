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
            master.Start(opID); // este start apenas tem como proposito começar a processar.. a inicializaçao da estrutura deve ocorrer assim que se inicia o executavel do processo

        }

        private void ButtonInterval_Click(object sender, EventArgs e)
        {
            string opID = IntervalOpID.Text;
            int millisecons = Convert.ToInt32(TextBoxMilliseconds_interval.Text);
            master.Interval(opID, millisecons);
        }

        private void ButtonStatus_Click(object sender, EventArgs e)
        {
            master.Status();
        }

        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(PuppetMaster master)
        {
            
            this.master = master;
         
            InitializeComponent();

            foreach (var i in this.master.nodes){
                StartOpID.Items.Add(i.Key);
                IntervalOpID.Items.Add(i.Key);
                CrashOpID.Items.Add(i.Key);
                FreezeOpID.Items.Add(i.Key);
                UnfreezeOpID.Items.Add(i.Key);
            }

            //Configure windows position
            Console.Title = "PuppetMasterInterface";
            var screen = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            var width = screen.Width;
            var height = screen.Height;

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            // init puppetMaster
            //statusCmd = new StatusCommand(master);
            //startCmd = new StartCommand(master);
            //intervalCmd = new IntervalCommand(master);
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

       
    }
}

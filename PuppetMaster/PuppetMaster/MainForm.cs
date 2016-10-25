using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster
{
    public partial class MainForm : Form
    {
        PuppetMaster master = new PuppetMaster();
        ACommand statusCmd = null, startCmd = null, intervalCmd = null;

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            string opID = TextBoxOpID_start.Text;
            master.Start(opID);
        }

        private void ButtonInterval_Click(object sender, EventArgs e)
        {
            string opID = TextBoxOpID_interval.Text;
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


        private void Form1_Load(object sender, EventArgs e)
        {
            // init puppetMaster
            statusCmd = new StatusCommand(master);
            startCmd = new StartCommand(master);
            intervalCmd = new IntervalCommand(master);
        }
    }
}

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
        PuppetMaster master = null;
        ACommand statusCmd = null, startCmd = null, intervalCmd = null;

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

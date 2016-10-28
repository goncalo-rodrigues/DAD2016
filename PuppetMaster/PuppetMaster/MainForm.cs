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

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            string opID = TextBoxOpID_start.Text;
            master.Start(opID.ToUpper()); // este start apenas tem como proposito começar a processar.. a inicializaçao da estrutura deve ocorrer assim que se inicia o executavel do processo
        }

        private void ButtonInterval_Click(object sender, EventArgs e)
        {
            string opID = TextBoxOpID_interval.Text;
            int millisecons = Convert.ToInt32(TextBoxMilliseconds_interval.Text);
            master.Interval(opID.ToUpper(), millisecons);
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
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            // init puppetMaster
            //statusCmd = new StatusCommand(master);
            //startCmd = new StartCommand(master);
            //intervalCmd = new IntervalCommand(master);
        }
    }
}

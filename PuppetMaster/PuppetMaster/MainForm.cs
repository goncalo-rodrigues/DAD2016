﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Graph = System.Windows.Forms.DataVisualization.Charting;

namespace PuppetMaster
{
    public partial class MainForm : Form
    {


        //Graph.Chart chart;
        const int chartX = 30;
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
            string opID = UnfreezeOpID.Text;
            string index = UnfreezeID.Value.ToString();
            String[] args = { opID, index };

            master.ExecuteCommand("unfreeze", args);
        }

        private void ButtonWait_Click(object sender, EventArgs e)
        {
            string ms = TextBoxWait.Text;
            String[] args = { ms };
            master.ExecuteCommand("wait", args);
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
            Environment.Exit(0);
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
            InitChart();
        }
        //public int[] xVals = new int[chartX];
        private int currentX;
        private void InitChart()
        {
            currentX = chartX;
            // Create new Graph
            //chart = new Graph.Chart();
            //chart.Location = new System.Drawing.Point(10, 10);
            //chart.Size = new System.Drawing.Size(700, 700);
            // Add a chartarea called "draw", add axes to it and color the area black
            chart.ChartAreas.Add("draw");
            chart.ChartAreas["draw"].AxisX.Minimum = 0;
            chart.ChartAreas["draw"].AxisX.Maximum = chartX;
            chart.ChartAreas["draw"].AxisX.Interval = 5;
            chart.ChartAreas["draw"].AxisX.MajorGrid.LineColor = Color.White;
            chart.ChartAreas["draw"].AxisX.MajorGrid.LineDashStyle = Graph.ChartDashStyle.Dash;
            chart.ChartAreas["draw"].AxisY.Minimum = 0;
            chart.ChartAreas["draw"].AxisY.Maximum = 300;
            chart.ChartAreas["draw"].AxisY.Interval =50;
            chart.ChartAreas["draw"].AxisY.MajorGrid.LineColor = Color.White;
            chart.ChartAreas["draw"].AxisY.MajorGrid.LineDashStyle = Graph.ChartDashStyle.Dash;

            chart.ChartAreas["draw"].BackColor = Color.Black;

            // Create a new function series
            chart.Series.Add("MyFunc");
            // Set the type to line      
            chart.Series["MyFunc"].ChartType = Graph.SeriesChartType.Line;
            // Color the line of the graph light green and give it a thickness of 3
            chart.Series["MyFunc"].Color = Color.LightGreen;
            chart.Series["MyFunc"].BorderWidth = 3;
            //This function cannot include zero, and we walk through it in steps of 0.1 to add coordinates to our series
            for (double x = 0; x < chartX; x += 1)
            {
                chart.Series["MyFunc"].Points.AddXY(x, 0);
            }
            chart.Series["MyFunc"].LegendText = "Throughput";
            // Create a new legend called "MyLegend".
            chart.Legends.Add("MyLegend");
            chart.Legends["MyLegend"].BorderColor = Color.Tomato; // I like tomato juice!
            //Controls.Add(this.chart);
        }
        
        public void AddNewPointToChart(string value)
        {
            chart.ChartAreas["draw"].AxisX.Minimum += 1;
            chart.ChartAreas["draw"].AxisX.Maximum += 1;
            chart.Series["MyFunc"].Points.RemoveAt(0);
            chart.Series["MyFunc"].Points.AddXY(currentX+=1, Int32.Parse(value));
        }

        private void RichTBEventLog_TextChanged(object sender, EventArgs e)
        {
            RichTBEventLog.SelectionStart = RichTBEventLog.Text.Length;
            RichTBEventLog.ScrollToCaret();
        }

        private void UnfreezeOpID_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}

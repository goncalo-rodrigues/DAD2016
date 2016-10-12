using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var config = @"Semantics exactly-once
LoggingLevel full
OP1 input_ops tweeters.data
rep_fact 2 routing hashing(1)
address tcp://1.2.3.4:11000/op, tcp://1.2.3.5:11000/op
operator_spec FILTER 3,""="",""www.tecnico.ulisboa.pt""
OP2 input_ops OP1
rep_fact 2 routing random
address tcp://1.2.3.4:11000/op, tcp://1.2.3.5:11000/op
operator_spec FILTER 3,""="",""www.tecnico.ulisboa.pt""
";
            PuppetMaster.ReadAndInitializeSystem(config);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            
        }
    }
}

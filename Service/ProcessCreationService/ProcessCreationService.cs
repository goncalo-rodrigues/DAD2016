using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ProcessCreationService
{
    public partial class ProcessCreationService : ServiceBase
    {
        PCServer server;
        public ProcessCreationService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            server = new PCServer();
            server.StartServer();
        }

        protected override void OnStop()
        {
            server.StopServer();
        }
    }
}

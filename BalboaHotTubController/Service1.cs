using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.ServiceModel;
using System.Configuration;
using System.Threading;

namespace BalboaHotTubController
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        ServiceHost serviceHost = null;
        protected override void OnStart(string[] args)
        {
            try
            {
                string serviceAddress = $"http://{Properties.Settings.Default.hostName}:2001";

                Uri baseAddress = new Uri(serviceAddress);
                serviceHost?.Close();

                // Create a ServiceHost for the CalculatorService type and 
                // provide the base address.
                serviceHost = new ServiceHost(typeof(BalboaHotTub), baseAddress);

                Trace.WriteLine(serviceHost.BaseAddresses[0].AbsoluteUri);

                // Open the ServiceHostBase to create listeners and start 
                // listening for messages.
                serviceHost.Open();
            }
            catch (Exception ex)
            {
                Trace.AutoFlush = true;
                Trace.WriteLine(ex);
            }
        }

        protected override void OnStop()
        {
            serviceHost?.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Common.Helpers.Azure;
using System.Threading;

namespace CSWebMonitor.Hubs
{
    public class CSMonitorHub : Hub
    {
        //Timer t = new Timer();

        public CSMonitorHub()
        {
            var t = new Thread(new ThreadStart(RunPulse));
            t.Start();
            //Run();
        }

        //public void Send(string name, string message)
        //{
        //    // Call the broadcastMessage method to update clients.
        //    Clients.All.broadcastMessage(name, message);
        //}


        public void RunPulse()
        {
            //Call the broadcastMessage method to update clients.
            ServiceBusHelper.ReceiveMessageAsync("democswebapp_pulse", "AllMessage", m =>
            {
                var jobName = (string)m.Properties["JobName"];

                var instanceid = (string)m.Properties["InstanceId"];

                var state = ((bool)m.Properties["State"]) ? "Down" : "Up";

                Clients.All.pulse(jobName, instanceid, state);
            });
        }

        //public void Run()
        //{
        //    while (true)
        //    {
        //        Clients.All.tick(Environment.TickCount);
        //        Thread.Sleep(1000);
        //    }
        //}

    }
}
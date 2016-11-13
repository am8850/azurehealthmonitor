using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Common.Helpers.RoleHelper;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace WebRole1
{
    public class WebRole : RoleEntryPoint
    {

        private readonly IRoleStateMonitor monitor;
        private readonly TelemetryClient telemetry = new TelemetryClient();

        private volatile bool isBusy = false;
        private bool HasBadHealth = false;
        private string HealthStatus;
        private bool StopCommand = false;
        private string InstanceId;


        public WebRole()
        {
            // Todo: Inject the monitor
            InstanceId = RoleEnvironment.CurrentRoleInstance.Id.Split('.')[2];

            monitor = new ServiceBusRoleStateMonitor("DemoCSWebApp", InstanceId);
        }

        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            RoleEnvironment.StatusCheck += RoleEnvironment_StatusCheck;

            monitor.ProcessCommands();

            return base.OnStart();
        }

        private void RoleEnvironment_StatusCheck(object sender, RoleInstanceStatusCheckEventArgs e)
        {

            Healthcheck();

            CheckCommand();

            isBusy = HasBadHealth || StopCommand;

            monitor.NotifyState(isBusy, RoleEnvironment.CurrentRoleInstance.Id);

            if (isBusy)
            {
                e.SetBusy();
#if (DEBUG)
                Debug.WriteLine(InstanceId + " set to busy");
#else
                Trace.WriteLine(InstanceId + " set to busy");
                telemetry.TrackEvent(InstanceId + "Disabled");
                telemetry.TrackTrace(InstanceId + " for job DemoCSWebApp has been disabled.", SeverityLevel.Warning);
#endif
            }

#if (DEBUG)
            Debug.WriteLine(DateTime.Now.ToLongTimeString());
#endif
        }

        private void CheckCommand()
        {
            if (monitor.Command == "Stop")
                StopCommand = true;

            if (monitor.Command == "Start")
                StopCommand = false;
        }

        private void Healthcheck()
        {
            HasBadHealth = false;

            HealthStatus = "OK";
        }
    }
}

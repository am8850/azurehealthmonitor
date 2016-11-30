using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Common.Helpers.RoleHelper;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private readonly TelemetryClient telemetry = new TelemetryClient();
        private readonly IRoleStateMonitor monitor;

        private readonly string CSJobName = "WorkerRole1";

        private volatile bool isBusy = false;
        private bool HasBadHealth = false;
        private string HealthStatus;
        private bool StopCommand = false;
        private string InstanceId;

        public WorkerRole()
        {
            // Todo: Inject the monitor
            InstanceId = RoleEnvironment.CurrentRoleInstance.Id.Split('.')[2];

            monitor = new ServiceBusRoleStateMonitor(CSJobName, InstanceId);
        }


        public override void Run()
        {
            Trace.TraceInformation("WorkerRole1 is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            RoleEnvironment.StatusCheck += RoleEnvironment_StatusCheck;

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole1 has been started");

            return result;
        }

        private void RoleEnvironment_StatusCheck(object sender, RoleInstanceStatusCheckEventArgs e)
        {
            Healthcheck();

            CheckCommand();

            isBusy = HasBadHealth || StopCommand;

            monitor.NotifyState(isBusy, CSJobName, RoleEnvironment.CurrentRoleInstance.Id);

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
            if (string.Compare(monitor.Command, "stop", true) == 0)
                StopCommand = true;
            if (string.Compare(monitor.Command, "start", true) == 0)
                StopCommand = false;
        }

        private void Healthcheck()
        {
            HasBadHealth = false;
            HealthStatus = "OK";
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Trace.TraceInformation("Working");

                    await Task.Delay(10000);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}

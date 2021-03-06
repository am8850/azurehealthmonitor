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
using System.Threading.Tasks;
using System.Threading;

namespace WebRole1
{
    public class WebRole : RoleEntryPoint
    {
        // Async fields
        /// <summary>
        /// 
        /// </summary>
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        /// <summary>
        /// 
        /// </summary>
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        // Monitor and telemetry fields
        /// <summary>
        /// 
        /// </summary>
        private readonly IRoleStateMonitor monitor;
        /// <summary>
        /// 
        /// </summary>
        private readonly TelemetryClient telemetry = new TelemetryClient();

        private readonly string CSJobName = "DemoCSWebApp";

        // Support fields
        private volatile bool isBusy = false;
        private bool HasBadHealth = false;
        private string HealthStatus;
        private bool StopCommand = false;
        private string InstanceId;
        private bool RecycleCommand = false;

        // Constructor
        /// <summary>
        /// 
        /// </summary>
        public WebRole()
        {
            // Todo: Inject the monitor
            InstanceId = RoleEnvironment.CurrentRoleInstance.Id.Split('.')[2];

            monitor = new ServiceBusRoleStateMonitor(CSJobName, InstanceId);

#if (DEBUG)
            monitor.PulseTimeToLive = new TimeSpan(0, 0, 15);
#endif
        }

        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            Trace.TraceInformation("Webrole is starting");

            RoleEnvironment.StatusCheck += RoleEnvironment_StatusCheck;

            monitor.ProcessCommands();

            return base.OnStart();
        }

        public override void Run()
        {
            Trace.TraceInformation("Webrole is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        private void RoleEnvironment_StatusCheck(object sender, RoleInstanceStatusCheckEventArgs e)
        {

            isBusy = HasBadHealth || StopCommand;

            if (RecycleCommand)
                RoleEnvironment.RequestRecycle();

            //monitor.NotifyState(isBusy, RoleEnvironment.CurrentRoleInstance.Id);

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

            if (monitor.Command == "Recycle")
                RecycleCommand = true;

        }

        private async Task Healthcheck()
        {
            HasBadHealth = false;

            HealthStatus = "OK";
        }

        public override void OnStop()
        {
            Trace.TraceInformation("Webrole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("Webrole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                // Send the pulse every second
                await monitor.NotifyStateAsync(this.isBusy, CSJobName, InstanceId);

                await Healthcheck();

                CheckCommand();

                // Run approximately every 5 seconds
                await Task.Delay(10000);
            }
        }
    }
}

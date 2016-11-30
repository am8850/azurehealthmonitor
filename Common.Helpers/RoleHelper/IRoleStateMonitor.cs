using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers.RoleHelper
{
    public interface IRoleStateMonitor
    {
        string Command { get; set; }

        TimeSpan PulseTimeToLive { get; set; }

        void ProcessCommands();

        void NotifyState(bool state, string jobName, string instanceId, string message = null);

        Task NotifyStateAsync(bool state, string jobName, string InstanceId, string message = null);
    }
}

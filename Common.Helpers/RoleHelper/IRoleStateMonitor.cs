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

        void ProcessCommands();

        void NotifyState(bool state, string instanceId, string message = null);

    }
}

using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers.RoleHelper
{
    public class ServiceBusRoleStateNotifier : IRoleStateNotifier
    {

        public string CloudServiceName { get; set; }

        public ServiceBusRoleStateNotifier(string cloudServiceName)
        {
            CloudServiceName = cloudServiceName;
            Azure.ServiceBusHelper.CreateTopic(cloudServiceName);
            Azure.ServiceBusHelper.CreateSubscription(cloudServiceName, cloudServiceName);
        }

        public void NotifyState(bool state, string instanceId)
        {
            // Try to notify the status
            if (!string.IsNullOrEmpty(CloudServiceName))
            {
                var statusMessage = new BrokeredMessage();
                statusMessage.Properties["InstanceId"] = instanceId;
                statusMessage.Properties["State"] = state;
                statusMessage.TimeToLive = new TimeSpan(0, 1, 0);

                try
                {
                    Azure.ServiceBusHelper.SendMessage(CloudServiceName, statusMessage);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}

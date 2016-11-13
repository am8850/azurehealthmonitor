using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers.RoleHelper
{
    public class ServiceBusRoleStateMonitor : IRoleStateMonitor
    {
        private string NotificationTopic;
        public string CloudServiceName { get; set; }

        public string InstanceId { get; set; }

        public string Command { get; set; }

        public string Subscription { get; set; }
        public ServiceBusRoleStateMonitor(string cloudServiceName, string instanceId)
        {
            //
            CloudServiceName = cloudServiceName;
            InstanceId = instanceId;

            // Create Topic and subscription for health pulse
            NotificationTopic = CloudServiceName + "_Pulse";

            Azure.ServiceBusHelper.CreateTopic(NotificationTopic);

            Azure.ServiceBusHelper.CreateSubscription(NotificationTopic, "AllMessage");

            // Create Topic and subscription for Cloud Service Commands
            Azure.ServiceBusHelper.CreateTopic(CloudServiceName);

            Azure.ServiceBusHelper.CreateSubscription(CloudServiceName, InstanceId, new SqlFilter("InstanceId = '" + InstanceId + "'"));
        }

        public void ProcessCommands()
        {
            Azure.ServiceBusHelper.ReceiveMessage(CloudServiceName, InstanceId, m =>
            {
                Command = m.Properties["Command"].ToString();

                Debug.WriteLine(m);
            });
        }

        public void NotifyState(bool state, string instanceId, string message = null)
        {
            // Try to notify the status
            if (!string.IsNullOrEmpty(NotificationTopic))
            {
                var statusMessage = new BrokeredMessage();

                statusMessage.Properties["InstanceId"] = instanceId;

                statusMessage.Properties["State"] = state;

                statusMessage.Properties["Message"] = message ?? string.Empty;

                // Message will automatically expire in 1 minute
                statusMessage.TimeToLive = new TimeSpan(0, 1, 0);

                try
                {
                    Azure.ServiceBusHelper.SendMessage(NotificationTopic, statusMessage);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}

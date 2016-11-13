using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers.RoleHelper
{
    public class ServiceBusCommandProcessor : ICommandProcessor
    {

        public string CloudServiceName { get; set; }

        public string InstanceId { get; set; }

        public string Command { get; set; }

        public string Subscription { get; set; }
        public ServiceBusCommandProcessor(string cloudServiceName, string instanceId)
        {
            //
            CloudServiceName = cloudServiceName;
            InstanceId = instanceId;

            // Create a subscription for the instance id
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
    }
}

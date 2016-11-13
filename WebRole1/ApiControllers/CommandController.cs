using Common.Helpers.Azure;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebRole1.ApiControllers
{
    public class CommandController : ApiController
    {
        public async Task Post([FromBody]string value)
        {
            var message = new BrokeredMessage();
            message.Properties["InstanceId"] = RoleEnvironment.CurrentRoleInstance.Id.Split('.')[2];
            message.Properties["Command"] = value ?? "None";
            message.TimeToLive = new TimeSpan(0, 0, 1);
            await ServiceBusHelper.SendMessageAsync("DemoCSWebApp", message);
        }
    }
}

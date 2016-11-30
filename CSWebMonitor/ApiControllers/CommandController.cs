using Common.Helpers.Azure;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CSWebMonitor.ApiControllers
{
    public class CommandController : ApiController
    {
        public async Task<IHttpActionResult> Post([FromBody]ReqModel model)
        {
            var message = new BrokeredMessage();

            message.Properties["JobName"] = model.jobname;

            message.Properties["InstanceId"] = model.instanceid;

            message.Properties["Command"] = (model.state == 0) ? "Stop" : "Start";

            message.TimeToLive = new TimeSpan(0, 0, 2);

            await ServiceBusHelper.SendMessageAsync("DemoCSWebApp", message);

            return Ok();

        }

        public class ReqModel
        {
            public string jobname { get; set; }
            public string instanceid { get; set; }
            public int state { get; set; }
        }
    }
}

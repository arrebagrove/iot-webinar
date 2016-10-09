using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json.Linq;

namespace Web
{
    public class ListenerController : ApiController
    {
        [Route("api/listener")]
        public Task Post([FromBody]dynamic info)
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<MapHub>();

            return hub.Clients.Group("mapViews").updateBus(info);
        }
    }
}

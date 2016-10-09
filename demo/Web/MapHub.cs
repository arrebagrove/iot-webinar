using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNet.SignalR;

namespace Web
{
    public class MapHub : Hub
    {
        public Task RegisterMapView()
        {
            return Groups.Add(Context.ConnectionId, "mapViews");
        }
    }
}

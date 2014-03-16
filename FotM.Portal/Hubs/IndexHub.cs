using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace FotM.Portal.Hubs
{
    [HubName("indexHub")]
    public class IndexHub: Hub
    {
    }

    public class Reactive
    {
        private static readonly Reactive _instance = new Reactive();
        private IHubConnectionContext Clients { get; set; }

        private Reactive()
        {
            Clients = GlobalHost.ConnectionManager.GetHubContext<IndexHub>().Clients;
        }

        public Reactive Instance
        {
            get { return _instance; }
        }

        public void Start()
        {
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FotM.Messaging;
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

        public static Reactive Instance
        {
            get { return _instance; }
        }

        private readonly IHubConnectionContext _clients;
        private readonly StatsUpdateListener _statsUpdateListener; 

        private Reactive()
        {
            _clients = GlobalHost.ConnectionManager.GetHubContext<IndexHub>().Clients;
            _statsUpdateListener = new StatsUpdateListener(OnStatsUpdateReceived);
        }

        private void OnStatsUpdateReceived(StatsUpdateMessage msg)
        {
            _clients.All.update(msg.Stats);
        }

        public void Start()
        {
            _statsUpdateListener.Listen();
        }
    }
}
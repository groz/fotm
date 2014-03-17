﻿using FotM.Messaging;
using FotM.Portal.ViewModels;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace FotM.Portal.Infrastructure
{
    public class ReactiveUpdateManager
    {
        private static readonly ReactiveUpdateManager _instance = new ReactiveUpdateManager();

        public static ReactiveUpdateManager Instance
        {
            get { return _instance; }
        }

        private readonly IHubConnectionContext _clients;
        private readonly StatsUpdateListener _statsUpdateListener;
        private StatsUpdateMessage _latestMessage = null;

        private ReactiveUpdateManager()
        {
            _clients = GlobalHost.ConnectionManager.GetHubContext<IndexHub>().Clients;
            _statsUpdateListener = new StatsUpdateListener(OnStatsUpdateReceived);
        }

        private void OnStatsUpdateReceived(StatsUpdateMessage msg)
        {
            _latestMessage = msg;
            var armoryViewModel = CreateViewModel(msg);
            _clients.All.update(armoryViewModel);
        }

        public void Start()
        {
            _statsUpdateListener.Listen();
        }

        public void SendLatestUpdate(dynamic caller)
        {
            if (_latestMessage != null)
            {
                var armoryViewModel = CreateViewModel(_latestMessage);
                caller.update(armoryViewModel);
            }
        }

        private ArmoryViewModel CreateViewModel(StatsUpdateMessage msg)
        {
            return new ArmoryViewModel(msg.Stats, 20, 10);
        }
    }
}
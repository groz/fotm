using System;
using System.Linq;
using System.Net;
using FotM.Messaging;
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
        private readonly QueryLatestStatsClient _queryLastStatsClient;

        private ReactiveUpdateManager()
        {
            _clients = GlobalHost.ConnectionManager.GetHubContext<IndexHub>().Clients;
            _statsUpdateListener = new StatsUpdateListener(OnStatsUpdateReceived);
            _queryLastStatsClient = new QueryLatestStatsClient();
        }

        private void OnStatsUpdateReceived(StatsUpdateMessage msg)
        {
            _latestMessage = msg;
            var armoryViewModel = CreateViewModel(msg);

            if (armoryViewModel.AllTimeLeaders.Any())
            {
                _clients.All.update(armoryViewModel);
            }
        }

        public void Start()
        {
            _statsUpdateListener.Listen();

            _queryLastStatsClient.Send(new QueryLatestStatsMessage()
            {
                QueryingHost = Dns.GetHostName()
                // TODO: create private queue, send it in message and listen to it for response
            });
        }

        public void SendLatestUpdate(dynamic caller)
        {
            if (_latestMessage != null)
            {
                var armoryViewModel = CreateViewModel(_latestMessage);

                if (armoryViewModel.AllTimeLeaders.Any())
                {
                    caller.update(armoryViewModel);
                }
            }
        }

        private ArmoryViewModel CreateViewModel(StatsUpdateMessage msg)
        {
            return new ArmoryViewModel(msg.Stats, 20, 10, 10, TimeSpan.FromHours(2));
        }

        public ArmoryViewModel GetLatestViewModel()
        {
            return _latestMessage != null
                ? CreateViewModel(_latestMessage)
                : null;
        }
    }
}
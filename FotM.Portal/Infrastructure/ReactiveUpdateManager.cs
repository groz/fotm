using System;
using System.Linq;
using System.Net;
using FotM.Messaging;
using FotM.Portal.ViewModels;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Threading;

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
        private int nCurrentViewers = 0;

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

                caller.updateViewerCount(nCurrentViewers);
            }
        }

        private ArmoryViewModel CreateViewModel(StatsUpdateMessage msg)
        {
            return new ArmoryViewModel(msg.Stats, TimeSpan.FromHours(2),
                nTeamsToShow: 20, 
                nSetupsToShow: 10, 
                nPlayingNowMax: 10,
                nTeamsPerSpec: 10);
        }

        public ArmoryViewModel GetLatestViewModel()
        {
            return _latestMessage != null
                ? CreateViewModel(_latestMessage)
                : null;
        }

        public void ClientJoined()
        {
            int n = Interlocked.Increment(ref nCurrentViewers);
            _clients.All.updateViewerCount(n);
        }

        public void ClientLeft()
        {
            int n = Interlocked.Decrement(ref nCurrentViewers);
            _clients.All.updateViewerCount(n);
        }
    }
}
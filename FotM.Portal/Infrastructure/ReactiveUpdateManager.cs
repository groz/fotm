using System;
using System.Linq;
using System.Net;
using FotM.Messaging;
using FotM.Messaging.Messages;
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
        private readonly ISubscriber<StatsUpdateMessage> _statsUpdateListener;

        private StatsUpdateMessage _latestMessage = null;

        private ReactiveUpdateManager()
        {
            _clients = GlobalHost.ConnectionManager.GetHubContext<IndexHub>().Clients;
            _statsUpdateListener = new AzureTopicSubscriber<StatsUpdateMessage>(Constants.StatsUpdateTopic, true);
        }

        public void Start()
        {
            // listen to regular topic updates
            _statsUpdateListener.Subscribe(OnStatsUpdateReceived);

            // create private queue, send it in message and listen to it for response
            var hostName = Dns.GetHostName();

            var receiver = new AzureQueueClient<StatsUpdateMessage>(hostName, true);
            receiver.Subscribe(OnStatsUpdateReceived);

            var requester = new AzureQueueClient<QueryLatestStatsMessage>(Constants.QueryLatestStatsQueue, true);

            requester.Publish(new QueryLatestStatsMessage()
            {
                QueryingHost = hostName
            });
        }

        private bool OnStatsUpdateReceived(StatsUpdateMessage msg)
        {
            var armoryViewModel = CreateViewModel(msg);

            if (armoryViewModel.AllTimeLeaders.Any())
            {
                _clients.All.update(armoryViewModel);
            }

            _latestMessage = msg;

            return true;
        }

        private static ArmoryViewModel CreateViewModel(StatsUpdateMessage msg)
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
    }
}
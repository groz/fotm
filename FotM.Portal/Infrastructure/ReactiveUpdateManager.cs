using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using FotM.Config;
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
        public static readonly ReactiveUpdateManager Instance = new ReactiveUpdateManager();

        private readonly IHubConnectionContext _clients;
        private readonly ISubscriber<StatsUpdateMessage> _statsUpdateListener;

        private TeamStatsRepository _repository = null;

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
            string hostName = Dns.GetHostName();

            if (RegionalConfig.Instance.Region != "TEST")
            {
                hostName += Guid.NewGuid().ToString().Substring(0, 4);
            }

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
            _repository = new TeamStatsRepository(msg.Stats);

            var armoryViewModel = CreateViewModel(_repository);

            if (armoryViewModel.AllTimeLeaders.Any())
            {
                _clients.All.updateNow(armoryViewModel.PlayingNow);
            }

            return true;
        }

        private static ArmoryViewModel CreateViewModel(TeamStatsRepository repository)
        {
            return new ArmoryViewModel(repository, 
                TimeSpan.FromHours(1.5),
                nTeamsToShow: 15, 
                nSetupsToShow: 10, 
                nPlayingNowMax: 15);
        }

        public ArmoryViewModel GetLatestViewModel()
        {
            return _repository != null
                ? CreateViewModel(_repository)
                : null;
        }

        public void SendLatestUpdate(dynamic caller)
        {
            var armoryViewModel = GetLatestViewModel();

            if (armoryViewModel != null)
                caller.updateAll(armoryViewModel);
        }

        public void SendTeamsForSetup(dynamic caller, Guid requestGuid, TeamSetupViewModel teamSetupViewModel)
        {
            if (_repository == null)
                return;

            var teams = _repository.QueryTeamsForSetup(teamSetupViewModel, nMaxTeams: 15);

            caller.showSetupTeams(requestGuid, teams);
        }

        public void SendSetupsForFilters(dynamic caller, Guid requestGuid, TeamFilter[] teamFilters)
        {
            if (_repository == null)
                return;

            var teamSetups = _repository.QueryFilteredSetups(teamFilters, 10);
            var teams = _repository.QueryFilteredTeams(teamFilters, 15);

            caller.showFilteredSetups(requestGuid, teamSetups, teams);
        }
    }
}
using FotM.Portal.ViewModels;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;

namespace FotM.Portal.Infrastructure
{
    [HubName("indexHub")]
    public class IndexHub: Hub
    {
        public void QueryLatestUpdate()
        {
            ReactiveUpdateManager.Instance.SendLatestUpdate(Clients.Caller);
        }

        public void QueryTeamsForSetup(TeamSetupViewModel teamSetupViewModel)
        {
            ReactiveUpdateManager.Instance.SendTeamsForSetup(Clients.Caller, teamSetupViewModel);
        }

        public void QueryFilteredSetups(TeamFilter[] teamFilters)
        {
            ReactiveUpdateManager.Instance.SendSetupsForFilters(Clients.Caller, teamFilters);
        }

    }
}
using System;
using FotM.Portal.ViewModels;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace FotM.Portal.Infrastructure
{
    [HubName("indexHub")]
    public class IndexHub: Hub
    {
        public void QueryLatestUpdate()
        {
            ReactiveUpdateManager.Instance.SendLatestUpdate(Clients.Caller);
        }

        public void QueryTeamsForSetup(Guid requestGuid, TeamSetupViewModel teamSetupViewModel)
        {
            ReactiveUpdateManager.Instance.SendTeamsForSetup(Clients.Caller, requestGuid, teamSetupViewModel);
        }

        public void QueryFilteredSetups(Guid requestGuid, TeamFilter[] teamFilters)
        {
            ReactiveUpdateManager.Instance.SendSetupsForFilters(Clients.Caller, requestGuid, teamFilters);
        }

    }
}
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

        public override System.Threading.Tasks.Task OnConnected()
        {
            ReactiveUpdateManager.Instance.ClientJoined();
            return base.OnConnected();
        }

        public override System.Threading.Tasks.Task OnDisconnected()
        {
            ReactiveUpdateManager.Instance.ClientLeft();
            return base.OnDisconnected();
        }
    }
}
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace FotM.Portal.Infrastructure
{
    [HubName("indexHub")]
    public class IndexHub: Hub
    {
    }
}
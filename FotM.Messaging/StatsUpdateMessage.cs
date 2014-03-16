using FotM.Domain;

namespace FotM.Messaging
{
    public class StatsUpdateMessage
    {
        public TeamStats[] Stats { get; set; }
    }
}
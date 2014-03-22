using System;
using FotM.Domain;

namespace FotM.Messaging.Messages
{
    [Serializable]
    public class StatsUpdateMessage
    {
        public TeamStats[] Stats { get; set; }
    }
}
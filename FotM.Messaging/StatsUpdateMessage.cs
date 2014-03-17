using System;
using FotM.Domain;

namespace FotM.Messaging
{
    [Serializable]
    public class StatsUpdateMessage
    {
        public TeamStats[] Stats { get; set; }
    }
}
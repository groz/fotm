using System;

namespace FotM.Messaging
{
    [Serializable]
    public class QueryLatestStatsMessage
    {
        public string QueryingHost { get; set; }
    }
}
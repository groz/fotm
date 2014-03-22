using System;

namespace FotM.Messaging.Messages
{
    [Serializable]
    public class QueryLatestStatsMessage
    {
        public string QueryingHost { get; set; }
    }
}
using System;
using FotM.Messaging.Messages;
using FotM.Utilities;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace FotM.Messaging
{
    public class LatestStatsPublisher
    {
        private readonly QueueClient _queueClient;

        public LatestStatsPublisher(string replyQueue)
        {
            _queueClient = QueueClient.CreateFromConnectionString(Constants.ConnectionString, replyQueue);
        }

        public void Send(QueryLatestStatsMessage message)
        {
            var brokeredMessage = new BrokeredMessage(message);
            _queueClient.Send(brokeredMessage);
        }

        public void OnReceive(Action<StatsUpdateMessage> onReceiveAction)
        {
            _queueClient.OnMessage(msg => HandleMessage(msg, onReceiveAction));
        }

        private void HandleMessage(BrokeredMessage msg, Action<StatsUpdateMessage> onReceiveAction)
        {
            string compressed = msg.GetBody<string>();
            string json = CompressionUtils.UnzipFromBase64(compressed);

            var message = JsonConvert.DeserializeObject<StatsUpdateMessage>(json);
            onReceiveAction(message);

            msg.Complete();
        }

    }
}
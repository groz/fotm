using FotM.Utilities;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace FotM.Messaging
{
    public class StatsUpdatePublisher
    {
        private readonly TopicClient _topicClient;

        public StatsUpdatePublisher()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(Constants.ConnectionString);

            if (!namespaceManager.TopicExists(Constants.StatsUpdateTopic))
            {
                namespaceManager.CreateTopic(Constants.StatsUpdateTopic);
            }

            _topicClient = TopicClient.CreateFromConnectionString(Constants.ConnectionString, Constants.StatsUpdateTopic);
        }

        public void Publish(StatsUpdateMessage message)
        {
            var json = JsonConvert.SerializeObject(message);
            var compressed = CompressionUtils.ZipToBase64(json);
            var brokeredMessage = new BrokeredMessage(compressed);
            _topicClient.Send(brokeredMessage);
        }
    }
}
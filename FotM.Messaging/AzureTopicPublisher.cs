using Microsoft.ServiceBus.Messaging;

namespace FotM.Messaging
{
    public class AzureTopicPublisher<TMessageType> : AzureTopicClient, IPublisher<TMessageType>
    {
        private readonly TopicClient _topicClient;

        public AzureTopicPublisher(string topicName, bool createIfNotExists)
            : base(topicName, createIfNotExists)
        {
            _topicClient = TopicClient.CreateFromConnectionString(Constants.ConnectionString, topicName);
        }

        public void Publish(TMessageType message)
        {
            _topicClient.Send( message.ToBrokeredMessage() );
        }
    }
}
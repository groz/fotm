using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace FotM.Messaging
{
    public abstract class AzureTopicClient
    {
        protected AzureTopicClient(string topicName, bool createIfNotExists)
        {
            if (createIfNotExists)
            {
                var namespaceManager = NamespaceManager.CreateFromConnectionString(Constants.ConnectionString);

                if (!namespaceManager.TopicExists(Constants.StatsUpdateTopic))
                {
                    var td = new TopicDescription(topicName)
                    {
                        DefaultMessageTimeToLive = Constants.DefaultMessageLifespan
                    };

                    namespaceManager.CreateTopic(td);
                }
            }
        }
    }
}
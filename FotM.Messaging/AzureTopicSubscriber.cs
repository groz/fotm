using System;
using System.Net;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace FotM.Messaging
{
    public class AzureTopicSubscriber<TMessageType> : AzureTopicClient, ISubscriber<TMessageType>
    {
        private readonly SubscriptionClient _subscriptionClient;

        public AzureTopicSubscriber(string topicName, bool createIfNotExists) 
            : base(topicName, createIfNotExists)
        {
            string subscriptionName = Dns.GetHostName() + Guid.NewGuid().ToString().Substring(0, 4);

            var namespaceManager = NamespaceManager.CreateFromConnectionString(Constants.ConnectionString);

            if (!namespaceManager.SubscriptionExists(topicName, subscriptionName))
            {
                namespaceManager.CreateSubscription(topicName, subscriptionName);
            }

            _subscriptionClient = SubscriptionClient.CreateFromConnectionString(Constants.ConnectionString,
                topicName,
                subscriptionName);

        }

        public void Receive(Func<TMessageType, bool> handle, TimeSpan? timeout = null)
        {
            var brokeredMessage = _subscriptionClient.Receive(timeout ?? TimeSpan.Zero);

            if (brokeredMessage != null)
            {
                brokeredMessage.Process(handle);
            }
        }

        public void Subscribe(Func<TMessageType, bool> handle)
        {
            _subscriptionClient.OnMessage(msg => msg.Process(handle));
        }
    }
}
using System;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace FotM.Messaging
{
    public class AzureQueueClient<TMessageType> : IPublisher<TMessageType>, ISubscriber<TMessageType>
    {
        private readonly QueueClient _queueClient;

        public AzureQueueClient(string queueName, bool createIfNotExists)
        {
            if (createIfNotExists)
            {
                var namespaceManager = NamespaceManager.CreateFromConnectionString(Constants.ConnectionString);

                if (!namespaceManager.QueueExists(queueName))
                {
                    var qd = new QueueDescription(queueName)
                    {
                        DefaultMessageTimeToLive = Constants.DefaultMessageLifespan
                    };

                    namespaceManager.CreateQueue(qd);
                }
            }

            _queueClient = QueueClient.CreateFromConnectionString(Constants.ConnectionString, queueName);
        }

        public void Publish(TMessageType message)
        {
            _queueClient.Send(message.ToBrokeredMessage());
        }

        public void Receive(Func<TMessageType, bool> handle, TimeSpan? timeout = null)
        {
            var brokeredMessage = _queueClient.Receive(timeout ?? TimeSpan.Zero);

            if (brokeredMessage != null)
            {
                brokeredMessage.Process(handle);
            }
        }

        public void Subscribe(Func<TMessageType, bool> handle)
        {
            _queueClient.OnMessage(msg => msg.Process(handle));
        }

    }
}
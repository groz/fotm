using System;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace FotM.Messaging
{
    public class QueryLatestStatsClient
    {
        private readonly QueueClient _queueClient;

        public QueryLatestStatsClient()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(Constants.ConnectionString);

            if (!namespaceManager.QueueExists(Constants.QueryLatestStatsQueue))
            {
                namespaceManager.CreateQueue(Constants.QueryLatestStatsQueue);
            }

            _queueClient = QueueClient.CreateFromConnectionString(Constants.ConnectionString,
                Constants.QueryLatestStatsQueue);
        }

        public void Send(QueryLatestStatsMessage message)
        {
            var brokeredMessage = new BrokeredMessage(message);
            _queueClient.Send(brokeredMessage);
        }

        public void Receive(Predicate<QueryLatestStatsMessage> onReceived, TimeSpan timeout)
        {
            var brokeredMessage = _queueClient.Receive(timeout);

            if (brokeredMessage != null)
            {
                var msg = brokeredMessage.GetBody<QueryLatestStatsMessage>();
                bool handled = onReceived(msg);

                if (handled)
                {
                    brokeredMessage.Complete();
                }
            }
        }
    }
}
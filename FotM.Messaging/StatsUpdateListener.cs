using System;
using System.Net;
using FotM.Utilities;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace FotM.Messaging
{
    public class StatsUpdateListener
    {
        private readonly SubscriptionClient _subscriptionClient;
        private readonly Action<StatsUpdateMessage> _onReceiveAction;

        public StatsUpdateListener(Action<StatsUpdateMessage> onReceiveAction)
        {
            _onReceiveAction = onReceiveAction;

            string subscriptionName = Dns.GetHostName();
            var namespaceManager = NamespaceManager.CreateFromConnectionString(Constants.ConnectionString);

            if (!namespaceManager.SubscriptionExists(Constants.StatsUpdateTopic, subscriptionName))
            {
                namespaceManager.CreateSubscription(Constants.StatsUpdateTopic, subscriptionName);
            }

            _subscriptionClient = SubscriptionClient.CreateFromConnectionString(Constants.ConnectionString,
                Constants.StatsUpdateTopic,
                subscriptionName);
        }

        public void Listen()
        {
            _subscriptionClient.OnMessage(HandleMessage);
        }

        private void HandleMessage(BrokeredMessage msg)
        {
            string compressed = msg.GetBody<string>();
            string json = CompressionUtils.UnzipFromBase64(compressed);

            var message = JsonConvert.DeserializeObject<StatsUpdateMessage>(json);
            _onReceiveAction(message);

            msg.Complete();
        }
    }
}
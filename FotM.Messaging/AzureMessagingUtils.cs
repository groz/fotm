using System;
using FotM.Utilities;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace FotM.Messaging
{
    public static class AzureMessagingUtils
    {
        public static BrokeredMessage ToBrokeredMessage<TMessage>(this TMessage msg)
        {
            var json = JsonConvert.SerializeObject(msg);
            var compressed = CompressionUtils.ZipToBase64(json);
            return new BrokeredMessage(compressed);
        }

        public static TMessage To<TMessage>(this BrokeredMessage msg)
        {
            string compressed = msg.GetBody<string>();
            string json = CompressionUtils.UnzipFromBase64(compressed);
            return JsonConvert.DeserializeObject<TMessage>(json);
        }

        public static void Process<TMessageType>(
            this BrokeredMessage brokeredMessage,
            Func<TMessageType, bool> handle)
        {
            try
            {
                var msg = brokeredMessage.To<TMessageType>();

                bool isHandled = handle(msg);

                if (isHandled)
                {
                    brokeredMessage.Complete();
                }
                else
                {
                    brokeredMessage.Abandon();
                }
            }
            catch (Exception ex)
            {
                brokeredMessage.Abandon();
            }
        }
    }
}
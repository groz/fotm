using System;

namespace FotM.Messaging
{
    public interface ISubscriber<TMessageType>
    {
        void Receive(Func<TMessageType, bool> handle, TimeSpan? timeout = null);
        void Subscribe(Func<TMessageType, bool> handler);
    }
}
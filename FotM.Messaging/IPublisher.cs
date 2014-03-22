namespace FotM.Messaging
{
    public interface IPublisher<TMessageType>
    {
        void Publish(TMessageType message);
    }
}
namespace Ranger.RabbitMQ.BusSubscriber
{
    public enum MessageState
    {
        Rejected = 0,
        Failed = 1,
        Succeeded = 2,
        Cancelled = 3
    }
}
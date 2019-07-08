namespace Ranger.RabbitMQ {
    public interface IRejectedEvent : IEvent {
        string Reason { get; }
        string Code { get; }
    }
}
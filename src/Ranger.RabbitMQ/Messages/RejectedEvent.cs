namespace Ranger.RabbitMQ {
    public interface IRejectedEvent : IEvent {
        string Reason { get; set; }
        string Code { get; set; }
    }
}
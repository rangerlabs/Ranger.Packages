namespace Ranger.RabbitMQ {
    public interface IMessage {
        CorrelationContext CorrelationContext { get; set; }
    }
}
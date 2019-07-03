using System.Threading.Tasks;

namespace Ranger.RabbitMQ {
    public interface IEventHandler<TEvent> : IMessageHandler<TEvent> where TEvent : IEvent {
        Task HandleAsync (TEvent command);
    }
}
using System.Threading.Tasks;

namespace Ranger.RabbitMQ
{
    public interface IMessageHandler<TMessage> where TMessage : IMessage
    {
        Task HandleAsync(TMessage message, ICorrelationContext context);
    }
}
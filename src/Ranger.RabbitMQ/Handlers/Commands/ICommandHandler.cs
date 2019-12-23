using System.Threading.Tasks;

namespace Ranger.RabbitMQ
{
    public interface ICommandHandler<TCommand> : IMessageHandler<TCommand> where TCommand : ICommand { }
}
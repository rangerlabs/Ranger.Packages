using System;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace Ranger.RabbitMQ
{
    public interface IBusPublisher : IDisposable
    {
        void Send<TCommand>(TCommand command, ICorrelationContext context)
        where TCommand : ICommand;

        void Publish<TEvent>(TEvent @event, ICorrelationContext context)
        where TEvent : IEvent;

        void Error<TMessage>(TMessage message, BasicDeliverEventArgs ea)
        where TMessage : IMessage;
    }
}
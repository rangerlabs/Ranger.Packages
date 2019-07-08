using System;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace Ranger.RabbitMQ {
    public interface IBusPublisher : IDisposable {
        void Send<TCommand> (TCommand command)
        where TCommand : ICommand;

        void Publish<TEvent> (TEvent @event)
        where TEvent : IEvent;

        void Error<TMessage> (TMessage message, BasicDeliverEventArgs ea)
        where TMessage : IMessage;
    }
}
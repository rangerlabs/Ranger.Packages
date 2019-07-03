using System;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace Ranger.RabbitMQ {
    public interface IBusPublisher : IDisposable {
        void SendAsync<TCommand> (TCommand command)
        where TCommand : ICommand;

        void PublishAsync<TEvent> (TEvent @event)
        where TEvent : IEvent;

        void ErrorAsync<TMessage> (TMessage message, BasicDeliverEventArgs ea)
        where TMessage : IMessage;
    }
}
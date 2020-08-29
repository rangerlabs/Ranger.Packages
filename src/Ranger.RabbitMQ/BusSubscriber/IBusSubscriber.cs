using System;
using System.Threading.Tasks;
using Ranger.Common;

namespace Ranger.RabbitMQ
{
    public interface IBusSubscriber : IAsyncDisposable
    {
        IBusSubscriber SubscribeCommand<TCommand>(Func<TCommand, ICorrelationContext, Task> onReceived, Func<TCommand, RangerException, IRejectedEvent> onError = null)
            where TCommand : ICommand;
        IBusSubscriber SubscribeCommand<TCommand>(Func<TCommand, RangerException, IRejectedEvent> onError = null)
            where TCommand : ICommand;
        IBusSubscriber SubscribeEvent<TEvent>(Func<TEvent, ICorrelationContext, Task> onReceived, Func<TEvent, RangerException, IRejectedEvent> onError = null)
            where TEvent : IEvent;
        IBusSubscriber SubscribeEvent<TEvent>(Func<TEvent, RangerException, IRejectedEvent> onError = null)
            where TEvent : IEvent;
        void UnSubscribeCommand<TCommand>()
            where TCommand : ICommand;
        void UnSubscribeEvent<TEvent>()
            where TEvent : IEvent;
    }
}
using System;
using System.Threading.Tasks;
using Ranger.Common;

namespace Ranger.RabbitMQ.BusSubscriber
{
    public interface IBusSubscriber : IAsyncDisposable
    {
        IBusSubscriber SubscribeCommandWithCallback<TCommand>(Func<TCommand, ICorrelationContext, Task> onReceived, Func<TCommand, RangerException, IRejectedEvent> onError = null)
            where TCommand : ICommand;
        IBusSubscriber SubscribeCommandWithHandler<TCommand>(Func<TCommand, RangerException, IRejectedEvent> onError = null)
            where TCommand : ICommand;
        IBusSubscriber SubscribeEventWithCallback<TEvent>(Func<TEvent, ICorrelationContext, Task> onReceived, Func<TEvent, RangerException, IRejectedEvent> onError = null)
            where TEvent : IEvent;
        IBusSubscriber SubscribeEventWithHandler<TEvent>(Func<TEvent, RangerException, IRejectedEvent> onError = null)
            where TEvent : IEvent;
        void UnsubscribeCommand<TCommand>()
            where TCommand : ICommand;
        void UnsubscribeEvent<TEvent>()
            where TEvent : IEvent;
    }
}
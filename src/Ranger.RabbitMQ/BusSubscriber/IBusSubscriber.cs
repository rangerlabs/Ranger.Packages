using System;
using Ranger.Common;

namespace Ranger.RabbitMQ
{
    public interface IBusSubscriber
    {
        IBusSubscriber SubscribeCommand<TCommand>(Func<TCommand, RangerException, IRejectedEvent> onError = null)
        where TCommand : ICommand;

        IBusSubscriber SubscribeEvent<TEvent>(Func<TEvent, RangerException, IRejectedEvent> onError = null)
        where TEvent : IEvent;
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ranger.RabbitMQ.BusSubscriber
{
    public class ChannelManager
    {
        private readonly IDictionary<Type, ManagedChannel> channelTagDictionary;

        internal string ConsumerTag { get; set; }

        internal ChannelManager()
        {
            channelTagDictionary = new ConcurrentDictionary<Type, ManagedChannel>();
        }
        internal void Update(Type messageType, ManagedChannel managedChannel)
        {
            if (Remove(messageType))
            {
                Add(messageType, managedChannel);
            }
            else
            {
                throw new ArgumentException();
            }
        }
        internal void Add(Type messageType, ManagedChannel managedChannel) => channelTagDictionary.Add(messageType, managedChannel);
        internal bool TryGetValue(Type messageType, out ManagedChannel managedChannel) => channelTagDictionary.TryGetValue(messageType, out managedChannel);
        internal bool Remove(Type messageType) => channelTagDictionary.Remove(messageType);
        internal ICollection<Type> Keys => channelTagDictionary.Keys;
    }
}
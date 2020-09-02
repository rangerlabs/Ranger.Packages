using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ranger.RabbitMQ.BusSubscriber
{
    public class ConsumerTagManager
    {
        private readonly IDictionary<Type, string> tagDictionary;
        private object dictionaryLock = new object();

        internal ConsumerTagManager()
        {
            tagDictionary = new ConcurrentDictionary<Type, string>();
        }

        internal void Add(Type messageType, string consumerTag) => tagDictionary.Add(messageType, consumerTag);

        internal void Update(Type messageType, string consumerTag)
        {
            lock (dictionaryLock)
            {
                if (tagDictionary.ContainsKey(messageType))
                {
                    tagDictionary.Remove(messageType);
                    tagDictionary.Add(messageType, consumerTag);
                }
                else
                {
                    throw new ArgumentException("No type could be found for the type provided");
                }
            }
        }

        internal void TryGetValue(Type messageType, out string value) => tagDictionary.TryGetValue(messageType, out value);

        internal void Remove(Type messageType) => tagDictionary.Remove(messageType);

        internal ICollection<string> ConsumerTags => tagDictionary.Values;


    }
}
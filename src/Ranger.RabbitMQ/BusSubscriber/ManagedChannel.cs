using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ranger.RabbitMQ.BusSubscriber
{
    public class ManagedChannel
    {
        private const int CHANNEL_CANCEL_DELAY_MS = 100;
        private readonly IModel _channel;
        private readonly AsyncEventingBasicConsumer _consumer;
        private ConcurrentBag<int> _executingConsumerCount = new ConcurrentBag<int>();
        private readonly ILogger<ManagedChannel> _logger;

        public ManagedChannel(IModel channel, AsyncEventingBasicConsumer consumer, ILogger<ManagedChannel> logger)
        {
            _channel = channel;
            _consumer = consumer;
            _logger = logger;
        }

        public void Ack(ulong deliverTag)
        {
            _channel.BasicAck(deliverTag, false);
        }

        public void Nack(ulong deliverTag, bool requeue)
        {
            _channel.BasicNack(deliverTag, false, requeue);
        }

        public void Lock()
        {
            _executingConsumerCount.Add(1);
        }

        public void Release()
        {
            _executingConsumerCount.TryTake(out _);
        }

        public async Task DisposeAsync()
        {
            _channel.BasicCancel(_consumer.ConsumerTags[0]);
            while (!_executingConsumerCount.IsEmpty)
            {
                _logger.LogDebug("Consumer executing. Delaying {CancelDelay}ms", CHANNEL_CANCEL_DELAY_MS);
                await Task.Delay(CHANNEL_CANCEL_DELAY_MS);
            }
            _channel.Close();
            _channel.Dispose();
        }
    }
}
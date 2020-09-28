using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ranger.RabbitMQ.BusPublisher
{
    public abstract class BusPublisherBase<TStartup>
        where TStartup : class
    {
        protected internal const int PUBLISHER_CANCEL_DELAY_MS = 100;
        protected internal const int SECONDS_TO_TIMEOUT = 10;
        protected internal readonly IModel Channel;
        protected internal readonly RabbitMQOptions Options;
        protected internal readonly Dictionary<Type, TopologyNames> TopologyDictionary = new Dictionary<Type, TopologyNames>();
        protected internal readonly ConcurrentDictionary<ulong, RangerRabbitMessage> OutstandingConfirms = new ConcurrentDictionary<ulong, RangerRabbitMessage>();
        private readonly ILogger<BusPublisherBase<TStartup>> _logger;

        public BusPublisherBase(IConnection connection,
                                RabbitMQOptions options,
                                ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BusPublisherBase<TStartup>>();
            Options = options;
            Channel = connection.CreateModel();
        }

        protected abstract void ConfigureChannel();

        protected void logNacks(ulong sequenceNumber, bool multiple)
        {
            if (multiple)
            {
                _logger.LogWarning("Cleaning multiple nacks for outstanding confirmations SequenceNumber: {SequenceNumber}", sequenceNumber);
                var confirmed = OutstandingConfirms.Where(k => k.Key <= sequenceNumber);
                foreach (var entry in confirmed)
                    OutstandingConfirms.TryRemove(entry.Key, out _);
            }
            else
                _logger.LogDebug("Cleaning single nack confirmation SequenceNumber: {SequenceNumber}", sequenceNumber);
            OutstandingConfirms.TryRemove(sequenceNumber, out _);
        }

        protected void CleanOutstandingConfirms(ulong sequenceNumber, bool multiple)
        {
            if (multiple)
            {
                _logger.LogDebug("Cleaning multiple outstanding confirmations SequenceNumber: {SequenceNumber}", sequenceNumber);
                var confirmed = OutstandingConfirms.Where(k => k.Key <= sequenceNumber);
                foreach (var entry in confirmed)
                    OutstandingConfirms.TryRemove(entry.Key, out _);
            }
            else
                _logger.LogDebug("Cleaning single confirmation SequenceNumber: {SequenceNumber}", sequenceNumber);
            OutstandingConfirms.TryRemove(sequenceNumber, out _);
        }

        protected void InitializeTopology()
        {
            var MessagesAssembly = typeof(TStartup).Assembly;
            var messageTypes = MessagesAssembly.GetTypes().Where(t => t.IsClass && typeof(IMessage).IsAssignableFrom(t)).ToList();

            foreach (var messageType in messageTypes)
            {
                TopologyNames topologyNames = topologyForMessageType(messageType);
                TopologyDictionary.Add(messageType, topologyNames);
                this.Channel.ExchangeDeclare(
                    topologyNames.Exchange,
                    ExchangeType.Topic,
                    true);
            }
            _logger.LogInformation("Topology initialized");
        }

        protected IBasicProperties CreateMessageHeaders(IModel channel, ICorrelationContext context)
        {
            if (channel is null)
            {
                throw new ArgumentException(nameof(channel) + "was null");
            }
            if (context is null)
            {
                throw new ArgumentException(nameof(context) + "was null");
            }

            var messageProperties = channel.CreateBasicProperties();
            messageProperties.Persistent = false; //outbox handles persistence
            messageProperties.Headers = context.ToStringifiedDictionary();
            return messageProperties;
        }

        private TopologyNames topologyForMessageType(Type type)
        {
            var topologyNames = new TopologyNames
            {
                Exchange = NamingConventions.ExchangeNamingConvention(type, Options.Namespace),
                Queue = NamingConventions.QueueNamingConvention(type, Options.Namespace),
                RoutingKey = NamingConventions.RoutingKeyConvention(type, Options.Namespace),
                ErrorExchange = NamingConventions.ErrorExchangeNamingConvention(type, Options.Namespace),
                ErrorQueue = NamingConventions.ErrorQueueNamingConvention(type, Options.Namespace),
                ErrorRoutingKey = NamingConventions.ErrorRoutingKeyConvention(type, Options.Namespace),
            };
            return topologyNames;
        }

        protected virtual void ErrorPublish<TMessage>(TMessage message, BasicDeliverEventArgs ea) where TMessage : IMessage
        {
            TopologyNames topologyNames;
            TopologyDictionary.TryGetValue(message.GetType(), out topologyNames);
            Channel.Bind<TMessage>(topologyNames.ErrorExchange, topologyNames.ErrorQueue, topologyNames.ErrorRoutingKey, Options);

            var rangerRabbitMessage = new RangerRabbitMessage
            {
                Type = message.GetType().ToString(),
                MessageVersion = 0,
                Headers = JsonConvert.SerializeObject(ea.BasicProperties),
                Body = JsonConvert.SerializeObject(ea.Body)
            };

            try
            {
                OutstandingConfirms.TryAdd(Channel.NextPublishSeqNo, rangerRabbitMessage);
                this.Channel.BasicPublish(topologyNames.ErrorExchange, topologyNames.ErrorRoutingKey.Replace("#.", ""), true, ea.BasicProperties, ea.Body);
                _logger.LogDebug($"Published message to exchange '{topologyNames.Exchange}' with routingkey '{topologyNames.RoutingKey.Replace("#.", "")}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to publish message: '{message.GetType()}'with correlation id: '{CorrelationContext.IdFromBasicDeliverEventArgsHeader(ea).ToString()}'");
                var exception = new RangerPublishException("", ex);
                exception.Data["RangerRabbitMessage"] = rangerRabbitMessage;
                throw exception;
            }
        }

        protected virtual void ChannelPublish<TMessage>(TMessage message, ICorrelationContext context = null) where TMessage : IMessage
        {
            TopologyNames topologyNames;
            TopologyDictionary.TryGetValue(message.GetType(), out topologyNames);
            IBasicProperties messageProperties = CreateMessageHeaders(this.Channel, context);

            var messageContent = String.Empty;
            try
            {
                messageContent = JsonConvert.SerializeObject(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to serialize object of type: '{message.GetType()}'");
                throw;
            }

            var rangerRabbitMessage = new RangerRabbitMessage
            {
                Type = message.GetType().ToString(),
                MessageVersion = 0,
                Headers = JsonConvert.SerializeObject(messageProperties),
                Body = messageContent
            };

            var needsAcked = NeedsAcked(message);
            try
            {
                if (needsAcked)
                {
                    OutstandingConfirms.TryAdd(Channel.NextPublishSeqNo, rangerRabbitMessage);
                }
                this.Channel.BasicPublish(topologyNames.Exchange, topologyNames.RoutingKey.Replace("#.", ""), basicProperties: messageProperties, body: new ReadOnlyMemory<byte>(System.Text.Encoding.Default.GetBytes(messageContent)));
                _logger.LogDebug($"Published message to exchange '{topologyNames.Exchange}' with routingkey '{topologyNames.RoutingKey.Replace("#.", "")}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to publish message: '{message.GetType()}'with correlation id: '{context.CorrelationContextId}'");
                var exception = new RangerPublishException("", ex);
                exception.Data["NeedsAcked"] = needsAcked;
                exception.Data["RangerRabbitMessage"] = rangerRabbitMessage;
                throw exception;
            }
        }

        protected bool NeedsAcked<TMessage>(TMessage message) where TMessage : IMessage
        {
            return !message.GetType().CustomAttributes.Select(a => a.AttributeType).Contains(typeof(NonAckedAttribute));
        }

        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
                if (disposing)
                {
                    int waited = 0;
                    while (!OutstandingConfirms.IsEmpty && waited < SECONDS_TO_TIMEOUT * 1000)
                    {
                        _logger.LogDebug("Publisher waiting for ack's from RabbitMq server. Delaying {CancelDelay}ms", PUBLISHER_CANCEL_DELAY_MS);
                        Thread.Sleep(PUBLISHER_CANCEL_DELAY_MS);
                        waited += PUBLISHER_CANCEL_DELAY_MS;
                    }
                    if (!OutstandingConfirms.IsEmpty)
                    {
                        _logger.LogDebug("Publisher ack timeout exceeded while waiting for acks");
                    }
                    Channel.Close();
                    Channel.Dispose();
                }
            }
        }

        public void Dispose()
        {
            _logger.LogDebug("BusPublisherBase.Dispose() called");
            Dispose(true);
        }
    }
}
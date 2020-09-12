using System;
using System.Linq;
using System.Threading;
using Autofac;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ranger.RabbitMQ.BusPublisher
{
    public class BusPublisherWithOutboxBase<TStartup, TDbContext> : BusPublisherBase<TStartup>
        where TStartup : class
        where TDbContext : DbContext, IOutboxStore
    {
        private readonly ILogger<BusPublisherWithOutboxBase<TStartup, TDbContext>> _logger;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IDataProtector _protector;

        public BusPublisherWithOutboxBase(IConnection connection,
                                RabbitMQOptions options,
                                IDataProtectionProvider dataProtectionProvider,
                                IConfiguration configuration,
                                ILoggerFactory loggerFactory)
            : base(connection, options, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BusPublisherWithOutboxBase<TStartup, TDbContext>>();
            _loggerFactory = loggerFactory;
            _dataProtectionProvider = dataProtectionProvider;
            _configuration = configuration;
            _protector = dataProtectionProvider.CreateProtector(typeof(BusPublisherWithOutbox<TStartup, TDbContext>).Name);
            ConfigureChannel();
            InitializeTopology();
        }

        protected override void ConfigureChannel()
        {
            _logger.LogInformation("Nack'd messages will be written to outbox");
            Channel.ConfirmSelect();
            Channel.BasicAcks += (sender, ea) => CleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);
            Channel.BasicNacks += (sender, ea) => persistToOutbox(ea.DeliveryTag, ea.Multiple, true);
            _logger.LogInformation("Publisher connected");
        }
        protected override void ErrorPublish<TMessage>(TMessage message, BasicDeliverEventArgs ea)
        {
            try
            {
                base.ErrorPublish<TMessage>(message, ea);
            }
            catch (RangerPublishException ex)
            {
                persistFailedPublishToOutbox(ex.Data["RangerRabbitMessage"] as RangerRabbitMessage);
            }
        }

        protected override void ChannelPublish<TMessage>(TMessage message, ICorrelationContext context = null)
        {
            try
            {
                base.ChannelPublish<TMessage>(message, context);
            }
            catch (RangerPublishException ex)
            {
                if ((bool)ex.Data["NeedsAcked"])
                {
                    _logger.LogWarning("Failed message required acking. Persisting to outbox");
                    persistFailedPublishToOutbox(ex.Data["RangerRabbitMessage"] as RangerRabbitMessage);
                }
                else
                {
                    _logger.LogWarning("Failed message does not require acking. Dropping the message");
                }
            }
        }

        private void persistFailedPublishToOutbox(RangerRabbitMessage plainMsg)
        {
            RangerRabbitMessage encryptedMsg = GetEncryptedRangerRabbitMessage(plainMsg);
            using (var dbContext = GetDbContext())
            {
                var outboxMsg = new OutboxMessage
                {
                    Message = encryptedMsg,
                    InsertedAt = DateTime.UtcNow,
                    Nacked = false
                };
                encryptedMsg.OutboxMessage = outboxMsg;
                dbContext.OutboxMessages.Add(outboxMsg);
                dbContext.RangerRabbitMessages.Add(encryptedMsg);
                dbContext.SaveChanges();
            }
            _logger.LogWarning("Failed message successfully saved to outbox");
        }

        private void persistToOutbox(ulong deliveryTag, bool multiple, bool nacked)
        {
            using (var dbContext = GetDbContext())
            {
                if (multiple)
                {
                    var nackedConfirms = OutstandingConfirms.Where(k => k.Key <= deliveryTag);
                    if (nackedConfirms.Any())
                    {
                        _logger.LogWarning("Multiple messages were not ack'd from the RabbitMq server, DeliveryTag: {DeliverTag}, Nacked: {Nacked}", deliveryTag, nacked);
                        foreach (var entry in nackedConfirms)
                        {
                            OutstandingConfirms.TryGetValue(entry.Key, out RangerRabbitMessage plainMsg);
                            RangerRabbitMessage encryptedMsg = GetEncryptedRangerRabbitMessage(plainMsg);
                            var outboxMsg = new OutboxMessage
                            {
                                Message = encryptedMsg,
                                InsertedAt = DateTime.UtcNow,
                                Nacked = nacked
                            };
                            encryptedMsg.OutboxMessage = outboxMsg;
                            dbContext.OutboxMessages.Add(outboxMsg);
                            dbContext.RangerRabbitMessages.Add(encryptedMsg);
                            _logger.LogDebug("Added message with sequence number {SequenceNumber} to pending outbox", entry.Key);
                        }
                        dbContext.SaveChanges();
                        CleanOutstandingConfirms(deliveryTag, multiple);
                        _logger.LogWarning("Not acked message(s) successfully saved to outbox");
                    }
                }
                else
                {
                    _logger.LogWarning("Single message was not ack'd from the RabbitMq server, DeliveryTag: {DeliverTag}, Nacked: {Nacked}", deliveryTag, nacked);
                    OutstandingConfirms.TryGetValue(deliveryTag, out RangerRabbitMessage plainMsg);
                    RangerRabbitMessage encryptedMsg = GetEncryptedRangerRabbitMessage(plainMsg);
                    var outboxMsg = new OutboxMessage
                    {
                        Message = encryptedMsg,
                        InsertedAt = DateTime.UtcNow,
                        Nacked = nacked
                    };
                    encryptedMsg.OutboxMessage = outboxMsg;
                    dbContext.OutboxMessages.Add(outboxMsg);
                    dbContext.RangerRabbitMessages.Add(encryptedMsg);
                    dbContext.SaveChanges();
                    CleanOutstandingConfirms(deliveryTag, multiple);
                    _logger.LogWarning("Not acked message(s) successfully saved to outbox");
                }
            }
        }

        private RangerRabbitMessage GetEncryptedRangerRabbitMessage(RangerRabbitMessage message)
        {
            var encryptedBody = _protector.Protect(message.Body);
            var encryptedHeaders = _protector.Protect(message.Headers);
            message.Body = encryptedBody;
            message.Headers = encryptedHeaders;
            return message;
        }

        private TDbContext GetDbContext()
        {
            return (TDbContext)Activator.CreateInstance(typeof(TDbContext), DbContextOptionsFactory.GetDbContextOptions<TDbContext>(_configuration, _loggerFactory), _dataProtectionProvider);
        }

        private bool disposedValue = false;
        protected new virtual void Dispose(bool disposing)
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
                        persistToOutbox(ulong.MaxValue, true, false);
                        _logger.LogDebug("All un-ack'd messages successfully flushed to disk");
                    }
                    base.Dispose();
                }
            }
        }

        public new void Dispose()
        {
            _logger.LogDebug("BusPublisher.Dispose() called");
            Dispose(true);
        }
    }
}
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
            _logger.LogInformation("Nack'd messages will be written to outbox");
        }

        protected override void ConfigureChannel()
        {
            Channel.ConfirmSelect();
            Channel.BasicAcks += (sender, ea) => cleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);
            Channel.BasicNacks += (sender, ea) => persistToOutbox(ea.DeliveryTag, ea.Multiple, true);
        }

        private void persistToOutbox(ulong deliveryTag, bool multiple, bool nacked)
        {
            using (var dbContext = (TDbContext)Activator.CreateInstance(typeof(TDbContext), DbContextOptionsFactory.GetDbContextOptions<TDbContext>(_configuration, _loggerFactory), _dataProtectionProvider))
            {
                if (multiple)
                {
                    _logger.LogWarning("Multiple messages nacked from the RabbitMq server, DeliveryTag: {DeliverTag}", deliveryTag);
                    var nackedConfirms = OutstandingConfirms.Where(k => k.Key <= deliveryTag);
                    foreach (var entry in nackedConfirms)
                    {
                        RangerRabbitMessage message = GetEncryptedRangerRabbitMessage(deliveryTag);
                        dbContext.Outbox.Add(new OutboxMessage
                        {
                            Message = message,
                            InsertedAt = DateTime.UtcNow,
                            Nacked = nacked
                        });
                    }
                }
                else
                {
                    _logger.LogWarning("Single message nacked from the RabbitMq server, DeliveryTag: {DeliverTag}", deliveryTag);
                    RangerRabbitMessage message = GetEncryptedRangerRabbitMessage(deliveryTag);
                    dbContext.Outbox.Add(new OutboxMessage
                    {
                        Message = message,
                        InsertedAt = DateTime.UtcNow,
                        Nacked = true
                    });
                }
                dbContext.SaveChanges();
                _logger.LogWarning("Nacked messages nacked successfully saved to outbox");
                cleanOutstandingConfirms(deliveryTag, multiple);

                RangerRabbitMessage GetEncryptedRangerRabbitMessage(ulong deliveryTag)
                {
                    OutstandingConfirms.TryGetValue(deliveryTag, out RangerRabbitMessage message);
                    var encryptedBody = _protector.Protect(message.Body);
                    var encryptedHeaders = _protector.Protect(message.Headers);
                    message.Body = encryptedBody;
                    message.Headers = encryptedHeaders;
                    return message;
                }
            }
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
using Microsoft.EntityFrameworkCore;

namespace Ranger.RabbitMQ
{
    public interface IOutboxStore
    {
        DbSet<OutboxMessage> Outbox { get; set; }
    }
}
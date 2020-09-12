using Microsoft.EntityFrameworkCore;

namespace Ranger.RabbitMQ
{
    public interface IOutboxStore
    {
        DbSet<OutboxMessage> OutboxMessages { get; set; }
        DbSet<RangerRabbitMessage> RangerRabbitMessages { get; set; }
    }
}
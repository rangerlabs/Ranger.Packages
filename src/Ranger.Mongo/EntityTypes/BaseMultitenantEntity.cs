using System;

namespace Ranger.Mongo
{
    public abstract class BaseMultitenantEntity : IIdentifiable
    {
        public Guid Id { get; protected set; }
        public string TenantId { get; protected set; }
        public DateTime CreatedDate { get; protected set; }
        public DateTime UpdatedDate { get; protected set; }

        public BaseMultitenantEntity(Guid id, string tenantId)
        {
            Id = id;
            TenantId = tenantId;
            CreatedDate = DateTime.UtcNow;
            SetUpdatedDate();
        }

        protected virtual void SetUpdatedDate()
            => UpdatedDate = DateTime.UtcNow;
    }
}
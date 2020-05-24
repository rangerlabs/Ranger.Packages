using System;

namespace Ranger.Mongo
{
    public abstract class BaseMultitenantEntity : IIdentifiable
    {
        public Guid Id { get; protected set; }
        public string PgsqlDatabaseUsername { get; protected set; }
        public DateTime CreatedDate { get; protected set; }
        public DateTime UpdatedDate { get; protected set; }

        public BaseMultitenantEntity(Guid id, string pgsqlDatabaseUsername)
        {
            if (string.IsNullOrWhiteSpace(pgsqlDatabaseUsername))
            {
                throw new ArgumentException($"{nameof(pgsqlDatabaseUsername)} was null or whitespace.");
            }

            Id = id;
            PgsqlDatabaseUsername = pgsqlDatabaseUsername;
            CreatedDate = DateTime.UtcNow;
            SetUpdatedDate();
        }

        protected virtual void SetUpdatedDate()
            => UpdatedDate = DateTime.UtcNow;
    }
}
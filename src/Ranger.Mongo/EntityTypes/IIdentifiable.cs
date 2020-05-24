using System;

namespace Ranger.Mongo
{
    public interface IIdentifiable
    {
        Guid Id { get; }
    }
}
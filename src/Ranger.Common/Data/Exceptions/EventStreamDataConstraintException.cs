using System;
using System.Runtime.Serialization;

namespace Ranger.Common.Data.Exceptions
{
    public class EventStreamDataConstraintException : Exception
    {
        public EventStreamDataConstraintException()
        {
        }

        public EventStreamDataConstraintException(string message) : base(message)
        {
        }

        public EventStreamDataConstraintException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EventStreamDataConstraintException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
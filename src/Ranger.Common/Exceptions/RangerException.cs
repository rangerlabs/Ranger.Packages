using System;
using System.Runtime.Serialization;

namespace Ranger.Common {
    public class RangerException : Exception {
        public RangerException (string message) : base (message) { }

        public RangerException (string message, Exception innerException) : base (message, innerException) { }

        protected RangerException (SerializationInfo info, StreamingContext context) : base (info, context) { }
    }
}
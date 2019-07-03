using System;

namespace Ranger.Common {
    public class DomainNotFoundException : Exception {
        public DomainNotFoundException () { }

        public DomainNotFoundException (string message) : base (message) { }

        public DomainNotFoundException (string message, Exception innerException) : base (message, innerException) { }
    }
}
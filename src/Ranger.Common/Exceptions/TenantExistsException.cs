using System;

namespace Ranger.Common {
    public class TenantExistsException : Exception {
        public TenantExistsException () { }

        public TenantExistsException (string message) : base (message) { }

        public TenantExistsException (string message, Exception innerException) : base (message, innerException) { }
    }
}
using System;

namespace Ranger.Common.Data.Exceptions
{
    public class NoOpException : Exception
    {
        public NoOpException() { }
        public NoOpException(string message) : base(message) { }
        public NoOpException(string message, Exception inner) : base(message, inner) { }
    }
}
using System;

namespace Ranger.Common {
    [AttributeUsage (AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class EncryptedAttribute : Attribute { }
}
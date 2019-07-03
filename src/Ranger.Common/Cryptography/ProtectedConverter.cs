using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Ranger.Common {
    public class ProtectedConverter : ValueConverter<string, string> {
        class Wrapper {
            readonly IDataProtector dataProtector;

            public Wrapper (IDataProtectionProvider dataProtectionProvider) {
                dataProtector = dataProtectionProvider.CreateProtector (nameof (ProtectedConverter));
            }

            public Expression<Func<string, string>> To => x => x != null ? dataProtector.Protect (x) : null;
            public Expression<Func<string, string>> From => x => x != null ? dataProtector.Unprotect (x) : null;
        }

        public ProtectedConverter (IDataProtectionProvider provider) : this (new Wrapper (provider)) { }

        ProtectedConverter (Wrapper wrapper) : base (wrapper.To, wrapper.From) { }
    }
}
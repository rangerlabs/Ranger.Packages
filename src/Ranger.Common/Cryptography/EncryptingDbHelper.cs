using System.Linq;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Ranger.Common
{
    public class EncryptingDbHelper
    {
        private readonly IDataProtectionProvider dataProtectionProvider;
        public EncryptingDbHelper(IDataProtectionProvider dataProtectionProvider)
        {
            this.dataProtectionProvider = dataProtectionProvider;
        }

        public void SetEncrytedPropertyAccessMode(IMutableEntityType entityType)
        {
            foreach (var property in entityType.GetProperties())
            {
                var attributes = property.PropertyInfo.GetCustomAttributes(typeof(EncryptedAttribute), false);
                if (attributes.Any())
                {
                    property.SetValueConverter(new ProtectedConverter(dataProtectionProvider));
                }
            }
        }
    }
}
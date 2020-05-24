using Newtonsoft.Json;

namespace Ranger.Common
{
    public class ContextTenant
    {
        [JsonConstructor]
        public ContextTenant(string tenantId, string databasePassword, bool enabled)
        {
            this.TenantId = tenantId;
            this.DatabasePassword = databasePassword;
            this.Enabled = enabled;
        }
        public string TenantId { get; }
        public string DatabasePassword { get; }
        public bool Enabled { get; set; }
    }
}
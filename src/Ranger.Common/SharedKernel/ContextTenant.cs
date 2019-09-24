using Newtonsoft.Json;

namespace Ranger.Common
{
    public class ContextTenant
    {
        [JsonConstructor]
        public ContextTenant(string databaseUsername, string databasePassword, bool enabled)
        {
            this.DatabaseUsername = databaseUsername;
            this.DatabasePassword = databasePassword;
            this.Enabled = enabled;
        }
        public string DatabaseUsername { get; }
        public string DatabasePassword { get; }
        public bool Enabled { get; set; }
    }
}
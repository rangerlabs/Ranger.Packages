using Newtonsoft.Json;

namespace Ranger.Common {
    public class ContextTenant {
        [JsonConstructor]
        public ContextTenant (string databaseUsername, string databasePassword) {
            this.DatabaseUsername = databaseUsername;
            this.DatabasePassword = databasePassword;

        }
        public string DatabaseUsername { get; }
        public string DatabasePassword { get; }
    }
}
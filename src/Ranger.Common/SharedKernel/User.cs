using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ranger.Common
{
    public class User
    {
        [JsonConstructor]
        public User(string domain, string email, string firstName, string lastName, string phoneNumber, string role)
        {
            this.Domain = domain;
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.PhoneNumber = phoneNumber;
            this.Role = role;
        }

        public string Domain { get; }
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string PhoneNumber { get; }
        public string Role { get; }
    }
}
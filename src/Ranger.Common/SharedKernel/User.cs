using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ranger.Common
{
    public class User
    {
        [JsonConstructor]
        internal User(string domain, string email, string firstName, string lastName, string phoneNumber, string role, IEnumerable<string> authorizedProjects)
        {
            this.Domain = domain;
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.PhoneNumber = phoneNumber;
            this.Role = role;
            this.AuthorizedProjects = authorizedProjects;
        }

        public string Domain { get; }
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string PhoneNumber { get; }
        public string Role { get; }
        public IEnumerable<string> AuthorizedProjects { get; }
    }
}
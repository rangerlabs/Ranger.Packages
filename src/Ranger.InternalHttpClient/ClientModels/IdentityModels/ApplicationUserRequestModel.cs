namespace Ranger.InternalHttpClient {
    public class ApplicationUserRequestModel {

        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Role Role { get; set; }
        public string PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public string TenantDomain { get; set; }
    }
}
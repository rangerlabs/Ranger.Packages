namespace Ranger.InternalHttpClient
{

    public class HttpClientOptions<T> : IHttpClientOptions where T : ApiClientBase
    {
        public HttpClientOptions(string baseUrl, string identityAuthority, string scope, string clientId, string clientSecret)
        {
            this.BaseUrl = baseUrl;
            this.IdentityAuthority = identityAuthority;
            this.Scope = scope;
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;

        }

        public string BaseUrl { get; }
        public string IdentityAuthority { get; }
        public string Scope { get; }
        public string ClientId { get; }
        public string ClientSecret { get; }
        public string Token { get; set; }
    }
}
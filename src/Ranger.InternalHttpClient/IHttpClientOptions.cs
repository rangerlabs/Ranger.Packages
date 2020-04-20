namespace Ranger.InternalHttpClient
{
    public interface IHttpClientOptions
    {
        string BaseUrl { get; }
        string Scope { get; }
        string ClientId { get; }
        string ClientSecret { get; }
        string Token { get; set; }
    }
}
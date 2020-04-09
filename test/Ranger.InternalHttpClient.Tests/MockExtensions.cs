using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;

namespace Ranger.InternalHttpClient.Tests
{
    public static class MockExtensions
    {
        public static void SetupMockHandler(this Mock<HttpMessageHandler> mockHttpMessageHandler, HttpStatusCode statusCode, StringContent content)
        {
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = content
                });
        }

        public static HttpClient GetClientForHandler(this Mock<HttpMessageHandler> mockHttpMessageHandler)
        {
            return new HttpClient(mockHttpMessageHandler.Object) { BaseAddress = new Uri("http://rangerlabs.io") };
        }

        public static void SetupFactoryForClient(this Mock<IHttpClientFactory> mockFactory, HttpClient client)
        {
            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);
        }

    }
}
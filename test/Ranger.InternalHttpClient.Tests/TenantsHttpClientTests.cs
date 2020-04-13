using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Ranger.InternalHttpClient.Tests
{
    public class TenantsHttpClientTestsFixture
    {
        public readonly ILogger<TenantsHttpClient> logger;

        public TenantsHttpClientTestsFixture()
        {
            logger = Mock.Of<ILogger<TenantsHttpClient>>();
        }
    }

    public class TenantsHttpClientTests : IClassFixture<TenantsHttpClientTestsFixture>
    {
        private readonly Mock<HttpMessageHandler> mockHttpMessageHandler;
        private readonly TenantsHttpClientTestsFixture fixture;

        public TenantsHttpClientTests(TenantsHttpClientTestsFixture fixture)
        {
            this.mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            this.fixture = fixture;
        }

        internal class ResultClass
        {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
        }

        [Fact]
        public async Task GetTenantByDomainAsync_Does_NOT_Throw_When_Missing_Reference_Type_Properties_Result_In_Response()
        {
            var expected = new RangerApiResponse<ResultClass>
            {
                Version = "1.0",
                StatusCode = 200,
                Message = "Some message.",
                IsError = false,
                ResponseException = null,
                Result = new ResultClass
                {
                    Property1 = "value1",
                }
            };

            mockHttpMessageHandler.SetupMockHandler(
                HttpStatusCode.OK,
                new StringContent(@"{""version"":""1.0"",""statusCode"":200,""message"":""Some message."",""isError"":false,""result"":{""property1"": ""value1"", ""property2"": ""value2""}}")
            );
            var client = mockHttpMessageHandler.GetClientForHandler();
            var tenantsClient = new TenantsHttpClient(client, fixture.logger);

            var result = await tenantsClient.GetTenantByDomainAsync<ResultClass>("domain");
            result.Result.Equals(expected.Result);
        }

        [Fact]
        public async Task GetTenantByDomainAsync_Deserializes_On_ApiException_Response()
        {
            var expected = new RangerApiResponse<ResultClass>
            {
                Version = "1.0",
                StatusCode = 500,
                Message = "Some message.",
                IsError = false,
                ResponseException = new ResponseException
                {
                    ExceptionMessage = new ExceptionMessage
                    {
                        Error = new Error
                        {
                            Code = "Some code",
                            Message = "Some message",
                            InnerError = null
                        }
                    }
                },
                Result = null
            };

            mockHttpMessageHandler.SetupMockHandler(
                HttpStatusCode.OK,
                new StringContent(@"{""version"":""1.0"",""statusCode"":400,""isError"":true,""responseException"":{""exceptionMessage"":{""error"":{""code"":""Some code"",""message"":""Some message"",""innerError"":null}}}}")
            );
            var client = mockHttpMessageHandler.GetClientForHandler();
            var tenantsClient = new TenantsHttpClient(client, fixture.logger);

            var result = await tenantsClient.GetTenantByDomainAsync<ResultClass>("domain");
            result.ResponseException.Equals(expected.ResponseException);
        }

        [Fact]
        public async Task GetTenantByDomainAsync_Returns_Value_For_Reference_Type_Result_In_Response()
        {
            var expected = new RangerApiResponse<ResultClass>
            {
                Version = "1.0",
                StatusCode = 200,
                Message = "Some message.",
                IsError = false,
                ResponseException = null,
                Result = new ResultClass
                {
                    Property1 = "value1",
                    Property2 = "value2"
                }
            };

            mockHttpMessageHandler.SetupMockHandler(
                HttpStatusCode.OK,
                new StringContent(@"{""version"":""1.0"",""statusCode"":200,""message"":""Some message."",""isError"":false,""result"":{""property1"": ""value1"", ""property2"": ""value2""}}")
            );
            var client = mockHttpMessageHandler.GetClientForHandler();
            var tenantsClient = new TenantsHttpClient(client, fixture.logger);

            var result = await tenantsClient.GetTenantByDomainAsync<ResultClass>("domain");
            result.Result.Equals(expected.Result);
        }

        [Fact]
        public async Task IsConfirmedAsync_Returns_Value_For_Value_Type_Result_In_Response()
        {
            var expected = new RangerApiResponse<bool>
            {
                Version = "1.0",
                StatusCode = 200,
                Message = "Some message.",
                IsError = false,
                ResponseException = null,
                Result = true
            };

            mockHttpMessageHandler.SetupMockHandler(
                HttpStatusCode.OK,
                new StringContent(@"{""version"":""1.0"",""statusCode"":200,""message"":""Some message."",""isError"":false,""result"":true}")
            );
            var client = mockHttpMessageHandler.GetClientForHandler();
            var tenantsClient = new TenantsHttpClient(client, fixture.logger);

            var result = await tenantsClient.IsConfirmedAsync("domain");
            result.Result.ShouldBeTrue();
        }

        [Fact]
        public async Task IsConfirmedAsync_Returns_Default_Result_For_400_Response()
        {
            var expected = new RangerApiResponse<bool>
            {
                Version = "1.0",
                StatusCode = 400,
                Message = "Some message.",
                IsError = false,
                ResponseException = null,
                Result = true
            };

            mockHttpMessageHandler.SetupMockHandler(
                HttpStatusCode.OK,
                new StringContent(@"{""version"":""1.0"",""statusCode"":400,""message"":""Some message."",""isError"":true}")
            );
            var client = mockHttpMessageHandler.GetClientForHandler();
            var tenantsClient = new TenantsHttpClient(client, fixture.logger);

            var result = await tenantsClient.IsConfirmedAsync("domain");
            result.Result.ShouldBe(default(bool));
        }

        [Fact]
        public void IsConfirmedAsync_Throws_When_Json_Response_Is_Not_Valid_Json()
        {
            var expected = new RangerApiResponse<bool>
            {
                Version = "1.0",
                StatusCode = 400,
                Message = "Some message.",
                IsError = false,
                ResponseException = null,
                Result = true
            };

            mockHttpMessageHandler.SetupMockHandler(
                HttpStatusCode.OK,
                new StringContent(@"{")
            );
            var client = mockHttpMessageHandler.GetClientForHandler();
            var tenantsClient = new TenantsHttpClient(client, fixture.logger);

            Should.ThrowAsync<JsonSerializationException>(tenantsClient.IsConfirmedAsync("domain"));
        }


        [Fact]
        public void IsConfirmedAsync_Throws_Contnet_IsNullOrWhitespace()
        {
            mockHttpMessageHandler.SetupMockHandler(
                HttpStatusCode.OK,
                new StringContent("")
            );
            var client = mockHttpMessageHandler.GetClientForHandler();
            var tenantsClient = new TenantsHttpClient(client, fixture.logger);

            Should.ThrowAsync<HttpClientException>(tenantsClient.DoesExistAsync("domain"));
        }
    }
}
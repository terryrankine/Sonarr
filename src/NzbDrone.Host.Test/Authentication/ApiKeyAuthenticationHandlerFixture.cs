using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Test.Common;
using Sonarr.Http.Authentication;

namespace NzbDrone.App.Test.Authentication
{
    [TestFixture]
    public class ApiKeyAuthenticationHandlerFixture : TestBase
    {
        private const string API_KEY = "valid-api-key-1234567890";
        private const string HEADER_NAME = "X-Api-Key";
        private const string QUERY_NAME = "apikey";

        private ApiKeyAuthenticationHandler _subject;
        private DefaultHttpContext _httpContext;

        [SetUp]
        public void Setup()
        {
            _httpContext = new DefaultHttpContext();

            var options = new ApiKeyAuthenticationOptions
            {
                HeaderName = HEADER_NAME,
                QueryName = QUERY_NAME
            };

            var optionsMonitor = new Mock<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
            optionsMonitor.Setup(o => o.Get(It.IsAny<string>())).Returns(options);
            optionsMonitor.Setup(o => o.CurrentValue).Returns(options);

            var loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);

            var configFileProvider = new Mock<IConfigFileProvider>();
            configFileProvider.Setup(c => c.ApiKey).Returns(API_KEY);

            _subject = new ApiKeyAuthenticationHandler(
                optionsMonitor.Object,
                loggerFactory.Object,
                System.Text.Encodings.Web.UrlEncoder.Default,
                configFileProvider.Object);

            var scheme = new AuthenticationScheme(
                ApiKeyAuthenticationOptions.DefaultScheme,
                ApiKeyAuthenticationOptions.DefaultScheme,
                typeof(ApiKeyAuthenticationHandler));

            _subject.InitializeAsync(scheme, _httpContext).GetAwaiter().GetResult();
        }

        [Test]
        public async Task should_succeed_with_valid_api_key_in_header()
        {
            _httpContext.Request.Headers[HEADER_NAME] = API_KEY;

            var result = await _subject.AuthenticateAsync();

            result.Succeeded.Should().BeTrue();
            result.Ticket.Should().NotBeNull();
        }

        [Test]
        public async Task should_succeed_with_valid_api_key_in_query()
        {
            _httpContext.Request.QueryString = new QueryString($"?{QUERY_NAME}={API_KEY}");

            var result = await _subject.AuthenticateAsync();

            result.Succeeded.Should().BeTrue();
            result.Ticket.Should().NotBeNull();
        }

        [Test]
        public async Task should_succeed_with_valid_api_key_in_bearer_header()
        {
            _httpContext.Request.Headers["Authorization"] = $"Bearer {API_KEY}";

            var result = await _subject.AuthenticateAsync();

            result.Succeeded.Should().BeTrue();
            result.Ticket.Should().NotBeNull();
        }

        [Test]
        public async Task should_not_succeed_with_invalid_api_key()
        {
            _httpContext.Request.Headers[HEADER_NAME] = "invalid-api-key";

            var result = await _subject.AuthenticateAsync();

            result.Succeeded.Should().BeFalse();
        }

        [Test]
        public async Task should_not_succeed_with_empty_api_key()
        {
            _httpContext.Request.Headers[HEADER_NAME] = "";

            var result = await _subject.AuthenticateAsync();

            result.Succeeded.Should().BeFalse();
        }

        [Test]
        public async Task should_not_succeed_with_no_api_key()
        {
            var result = await _subject.AuthenticateAsync();

            result.Succeeded.Should().BeFalse();
        }

        [Test]
        public async Task should_not_succeed_with_different_length_api_key()
        {
            _httpContext.Request.Headers[HEADER_NAME] = "short";

            var result = await _subject.AuthenticateAsync();

            result.Succeeded.Should().BeFalse();
        }
    }
}

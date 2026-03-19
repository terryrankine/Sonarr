using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using NzbDrone.Test.Common;
using Sonarr.Http.Middleware;

namespace NzbDrone.App.Test.Middleware
{
    [TestFixture]
    public class SecurityHeadersMiddlewareFixture : TestBase
    {
        private SecurityHeadersMiddleware _subject;
        private bool _nextCalled;

        [SetUp]
        public void Setup()
        {
            _nextCalled = false;
            _subject = new SecurityHeadersMiddleware(context =>
            {
                _nextCalled = true;
                return Task.CompletedTask;
            });
        }

        [Test]
        public async Task should_add_x_content_type_options_header()
        {
            var context = new DefaultHttpContext();

            await _subject.InvokeAsync(context);

            context.Response.Headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");
        }

        [Test]
        public async Task should_add_x_frame_options_header()
        {
            var context = new DefaultHttpContext();

            await _subject.InvokeAsync(context);

            context.Response.Headers["X-Frame-Options"].ToString().Should().Be("DENY");
        }

        [Test]
        public async Task should_call_next_middleware()
        {
            var context = new DefaultHttpContext();

            await _subject.InvokeAsync(context);

            _nextCalled.Should().BeTrue();
        }
    }
}

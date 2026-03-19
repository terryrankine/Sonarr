using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Sonarr.Http;

namespace NzbDrone.App.Test
{
    [TestFixture]
    public class CorsPolicyFixture
    {
        private ICorsPolicyProvider _corsPolicyProvider;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();

            services.AddCors(options =>
            {
                options.AddPolicy(VersionedApiControllerAttribute.API_CORS_POLICY,
                    builder =>
                    builder.AllowAnyOrigin()
                    .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                    .AllowAnyHeader());

                options.AddPolicy("AllowGet",
                    builder =>
                    builder.AllowAnyOrigin()
                    .WithMethods("GET", "OPTIONS")
                    .AllowAnyHeader());
            });

            var provider = services.BuildServiceProvider();
            _corsPolicyProvider = provider.GetRequiredService<ICorsPolicyProvider>();
        }

        [TestCase("GET")]
        [TestCase("POST")]
        [TestCase("PUT")]
        [TestCase("DELETE")]
        [TestCase("OPTIONS")]
        public async Task api_cors_policy_should_allow_expected_methods(string method)
        {
            var policy = await _corsPolicyProvider.GetPolicyAsync(new DefaultHttpContext(), VersionedApiControllerAttribute.API_CORS_POLICY);

            policy.Should().NotBeNull();
            policy.Methods.Should().Contain(method);
        }

        [TestCase("PATCH")]
        [TestCase("TRACE")]
        [TestCase("HEAD")]
        public async Task api_cors_policy_should_not_allow_unexpected_methods(string method)
        {
            var policy = await _corsPolicyProvider.GetPolicyAsync(new DefaultHttpContext(), VersionedApiControllerAttribute.API_CORS_POLICY);

            policy.Should().NotBeNull();
            policy.Methods.Should().NotContain(method);
        }

        [Test]
        public async Task api_cors_policy_should_allow_any_origin()
        {
            var policy = await _corsPolicyProvider.GetPolicyAsync(new DefaultHttpContext(), VersionedApiControllerAttribute.API_CORS_POLICY);

            policy.Should().NotBeNull();
            policy.AllowAnyOrigin.Should().BeTrue();
        }

        [Test]
        public async Task api_cors_policy_should_allow_any_header()
        {
            var policy = await _corsPolicyProvider.GetPolicyAsync(new DefaultHttpContext(), VersionedApiControllerAttribute.API_CORS_POLICY);

            policy.Should().NotBeNull();
            policy.AllowAnyHeader.Should().BeTrue();
        }

        [Test]
        public async Task allow_get_policy_should_only_allow_get_and_options()
        {
            var policy = await _corsPolicyProvider.GetPolicyAsync(new DefaultHttpContext(), "AllowGet");

            policy.Should().NotBeNull();
            policy.Methods.Should().BeEquivalentTo("GET", "OPTIONS");
        }
    }
}

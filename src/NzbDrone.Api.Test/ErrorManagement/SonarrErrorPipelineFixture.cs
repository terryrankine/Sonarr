using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Serializer;
using NzbDrone.Test.Common;
using Sonarr.Http.ErrorManagement;

namespace NzbDrone.Api.Test.ErrorManagement
{
    [TestFixture]
    public class SonarrErrorPipelineFixture : TestBase<SonarrErrorPipeline>
    {
        private HttpContext _context;
        private Mock<IExceptionHandlerPathFeature> _exceptionFeature;

        [SetUp]
        public void Setup()
        {
            _context = new DefaultHttpContext();
            _context.Response.Body = new MemoryStream();

            _exceptionFeature = new Mock<IExceptionHandlerPathFeature>();
            _context.Features.Set(_exceptionFeature.Object);
        }

        [Test]
        public async Task should_include_exception_description_in_non_production()
        {
            RuntimeInfo.IsProduction.Should().BeFalse("Test environment should not be production");

            var exception = new Exception("Test error message");
            _exceptionFeature.Setup(x => x.Error).Returns(exception);

            await Subject.HandleException(_context);

            _context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(_context.Response.Body).ReadToEndAsync();
            var errorModel = STJson.Deserialize<ErrorModel>(responseBody);

            errorModel.Message.Should().Be("Test error message");
            errorModel.Description.Should().NotBeNullOrWhiteSpace();
            errorModel.Description.Should().Contain("Test error message");
        }

        [Test]
        public async Task should_set_internal_server_error_status_for_generic_exception()
        {
            var exception = new Exception("Something went wrong");
            _exceptionFeature.Setup(x => x.Error).Returns(exception);

            await Subject.HandleException(_context);

            _context.Response.StatusCode.Should().Be(500);

            ExceptionVerification.ExpectedFatals(1);
        }

        [Test]
        public async Task should_return_message_from_exception()
        {
            var exception = new Exception("Specific error");
            _exceptionFeature.Setup(x => x.Error).Returns(exception);

            await Subject.HandleException(_context);

            _context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(_context.Response.Body).ReadToEndAsync();
            var errorModel = STJson.Deserialize<ErrorModel>(responseBody);

            errorModel.Message.Should().Be("Specific error");

            ExceptionVerification.ExpectedFatals(1);
        }
    }
}

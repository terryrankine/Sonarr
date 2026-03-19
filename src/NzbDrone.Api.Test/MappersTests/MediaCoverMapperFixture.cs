using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Test.Common;
using Sonarr.Http.Frontend.Mappers;

namespace NzbDrone.Api.Test.MappersTests
{
    [TestFixture]
    public class MediaCoverMapperFixture : TestBase<MediaCoverMapper>
    {
        private const string AppDataPath = @"C:\ProgramData\Sonarr";

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IAppFolderInfo>()
                .SetupGet(c => c.AppDataFolder)
                .Returns(AppDataPath);

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FileExists(It.IsAny<string>()))
                .Returns(false);
        }

        [Test]
        public void should_map_media_cover_path()
        {
            var result = Subject.Map("/MediaCover/1/banner.jpg");

            result.Should().Be(Path.Combine(AppDataPath, "MediaCover", "1", "banner.jpg"));
        }

        [Test]
        public void should_return_null_for_path_traversal_attempt()
        {
            var result = Subject.Map("/MediaCover/../../config.xml");

            result.Should().BeNull();
        }

        [Test]
        public void should_return_null_for_path_traversal_with_backslashes()
        {
            var result = Subject.Map("/MediaCover/..\\..\\config.xml");

            result.Should().BeNull();
        }

        [Test]
        public void should_handle_can_handle_true()
        {
            Subject.CanHandle("/MediaCover/1/banner.jpg").Should().BeTrue();
        }

        [Test]
        public void should_handle_can_handle_false()
        {
            Subject.CanHandle("/logfile/sonarr.txt").Should().BeFalse();
        }
    }
}

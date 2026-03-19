using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Authentication
{
    [TestFixture]
    public class UserServiceFixture : CoreTest<UserService>
    {
        private const string Username = "admin";
        private const string Password = "password123";

        private User _user;

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IUserRepository>()
                  .Setup(s => s.Insert(It.IsAny<User>()))
                  .Returns((User u) => u);

            _user = Subject.Add(Username, Password);

            Mocker.GetMock<IUserRepository>()
                  .Setup(s => s.FindUser(Username))
                  .Returns(_user);
        }

        [Test]
        public void should_return_user_when_password_is_correct()
        {
            Subject.FindUser(Username, Password).Should().NotBeNull();
        }

        [Test]
        public void should_return_null_when_password_is_incorrect()
        {
            Subject.FindUser(Username, "wrongpassword").Should().BeNull();
        }

        [Test]
        public void should_return_null_when_username_is_empty()
        {
            Subject.FindUser("", Password).Should().BeNull();
        }

        [Test]
        public void should_return_null_when_password_is_empty()
        {
            Subject.FindUser(Username, "").Should().BeNull();
        }

        [Test]
        public void should_return_null_when_user_not_found()
        {
            Mocker.GetMock<IUserRepository>()
                  .Setup(s => s.FindUser("unknown"))
                  .Returns((User)null);

            Subject.FindUser("unknown", Password).Should().BeNull();
        }
    }
}

using System.Text;
using FluentAssertions;
using NLog;
using NUnit.Framework;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.InstrumentationTests
{
    [TestFixture]
    public class CleansingLogTargetFixture : TestBase
    {
        private const string SensitiveApiKeyUrl = "http://myindexer.com/api?apikey=mySecret123&q=test";
        private const string SensitivePasswordString = "Host=postgres14;Username=admin;Password=mySecret123;Port=5432";
        private const string SafeMessage = "Downloaded episode from indexer";

        [Test]
        public void cleansing_console_layout_should_cleanse_api_key()
        {
            var sb = new StringBuilder();
            var layout = new CleansingConsoleLogLayout("${message}");
            var logEvent = LogEventInfo.Create(LogLevel.Info, "Test", SensitiveApiKeyUrl);

            layout.Render(logEvent, sb);

            sb.ToString().Should().NotContain("mySecret123");
            sb.ToString().Should().Contain("(removed)");
        }

        [Test]
        public void cleansing_console_layout_should_cleanse_password()
        {
            var sb = new StringBuilder();
            var layout = new CleansingConsoleLogLayout("${message}");
            var logEvent = LogEventInfo.Create(LogLevel.Info, "Test", SensitivePasswordString);

            layout.Render(logEvent, sb);

            sb.ToString().Should().NotContain("mySecret123");
            sb.ToString().Should().Contain("(removed)");
        }

        [Test]
        public void cleansing_console_layout_should_not_modify_safe_message()
        {
            var sb = new StringBuilder();
            var layout = new CleansingConsoleLogLayout("${message}");
            var logEvent = LogEventInfo.Create(LogLevel.Info, "Test", SafeMessage);

            layout.Render(logEvent, sb);

            sb.ToString().Should().Be(SafeMessage);
        }

        [Test]
        public void cleansing_console_layout_should_cleanse_discord_webhook_url()
        {
            var sb = new StringBuilder();
            var layout = new CleansingConsoleLogLayout("${message}");
            var message = "Sending notification to https://discord.com/api/webhooks/mySecret123/myTokenSecret456";
            var logEvent = LogEventInfo.Create(LogLevel.Info, "Test", message);

            layout.Render(logEvent, sb);

            sb.ToString().Should().NotContain("mySecret123");
            sb.ToString().Should().NotContain("myTokenSecret456");
        }

        [Test]
        public void cleansing_console_layout_should_cleanse_telegram_bot_token()
        {
            var sb = new StringBuilder();
            var layout = new CleansingConsoleLogLayout("${message}");
            var message = "Sending notification to https://api.telegram.org/bot1234567890:mySecret123/sendmessage";
            var logEvent = LogEventInfo.Create(LogLevel.Info, "Test", message);

            layout.Render(logEvent, sb);

            sb.ToString().Should().NotContain("mySecret123");
        }
    }
}

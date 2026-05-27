using Delly.Terminal.Event;
using Xunit;

namespace UnitTests.Terminal.Event
{
    public class CommandOutputLineEventArgsTests
    {
        [Fact]
        public void Constructor_WithNoParameters_InitializesEmptyContent()
        {
            // Arrange & Act
            var eventArgs = new CommandOutputLineEventArgs();

            // Assert
            Assert.NotNull(eventArgs);
            Assert.Equal("", eventArgs.Content);
        }

        [Fact]
        public void Content_CanSetAndGet()
        {
            // Arrange
            var eventArgs = new CommandOutputLineEventArgs();
            string testContent = "test output line";

            // Act
            eventArgs.Content = testContent;

            // Assert
            Assert.Equal(testContent, eventArgs.Content);
        }

        [Fact]
        public void Content_SetEmptyString_HandlesCorrectly()
        {
            // Arrange
            var eventArgs = new CommandOutputLineEventArgs();

            // Act
            eventArgs.Content = "";

            // Assert
            Assert.Equal("", eventArgs.Content);
        }

        [Fact]
        public void Content_SetNull_HandlesCorrectly()
        {
            // Arrange
            var eventArgs = new CommandOutputLineEventArgs();

            // Act
            eventArgs.Content = null!;

            // Assert
            Assert.Null(eventArgs.Content);
        }

        [Fact]
        public void Content_SetMultipleLines_HandlesCorrectly()
        {
            // Arrange
            var eventArgs = new CommandOutputLineEventArgs();
            string multiLineContent = "line1\r\nline2\r\nline3";

            // Act
            eventArgs.Content = multiLineContent;

            // Assert
            Assert.Equal(multiLineContent, eventArgs.Content);
        }

        [Fact]
        public void Content_SetWithSpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            var eventArgs = new CommandOutputLineEventArgs();
            string specialContent = "test\ttab\rcarriage\nnewline";

            // Act
            eventArgs.Content = specialContent;

            // Assert
            Assert.Equal(specialContent, eventArgs.Content);
        }

        [Fact]
        public void Content_SetWithUnicode_HandlesCorrectly()
        {
            // Arrange
            var eventArgs = new CommandOutputLineEventArgs();
            string unicodeContent = "你好世界 Hello World Ñoño café";

            // Act
            eventArgs.Content = unicodeContent;

            // Assert
            Assert.Equal(unicodeContent, eventArgs.Content);
        }

        [Fact]
        public void Content_CanBeOverwritten()
        {
            // Arrange
            var eventArgs = new CommandOutputLineEventArgs();
            eventArgs.Content = "original content";

            // Act
            eventArgs.Content = "new content";

            // Assert
            Assert.Equal("new content", eventArgs.Content);
        }
    }
}
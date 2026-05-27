using Delly.Terminal.Exception;
using System;
using SystemException = System.Exception;
using Xunit;

namespace UnitTests.Terminal.Exception
{
    public class CommandExceptionTests
    {
        [Fact]
        public void Constructor_WithMessage_SetsMessageCorrectly()
        {
            // Arrange
            string testMessage = "Test error message";

            // Act
            var exception = new CommandException(testMessage);

            // Assert
            Assert.NotNull(exception);
            Assert.Equal(testMessage, exception.Message);
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void Constructor_WithMessageAndInnerException_SetsPropertiesCorrectly()
        {
            // Arrange
            string testMessage = "Test error message";
            var innerException = new InvalidOperationException("Inner error");

            // Act
            var exception = new CommandException(testMessage, innerException);

            // Assert
            Assert.NotNull(exception);
            Assert.Equal(testMessage, exception.Message);
            Assert.Same(innerException, exception.InnerException);
        }

        [Fact]
        public void Constructor_WithEmptyMessage_HandlesCorrectly()
        {
            // Arrange & Act
            var exception = new CommandException("");

            // Assert
            Assert.NotNull(exception);
            Assert.Equal("", exception.Message);
        }

        [Fact]
        public void Constructor_WithNullInnerException_HandlesCorrectly()
        {
            // Arrange & Act
            var exception = new CommandException("Test", null!);

            // Assert
            Assert.NotNull(exception);
            Assert.Equal("Test", exception.Message);
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void CommandException_IsException()
        {
            // Arrange & Act
            var exception = new CommandException("Test");

            // Assert
            Assert.IsAssignableFrom<SystemException>(exception);
        }

        [Fact]
        public void CanThrowAndCatch_CommandException()
        {
            // Arrange
            string expectedMessage = "Command failed";

            // Act
            CommandException caughtException = null!;
            try
            {
                throw new CommandException(expectedMessage);
            }
            catch (CommandException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.Equal(expectedMessage, caughtException.Message);
        }

        [Fact]
        public void CanThrowAndCatch_AsBaseException()
        {
            // Arrange
            string expectedMessage = "Command failed";

            // Act
            SystemException caughtException = null!;
            try
            {
                throw new CommandException(expectedMessage);
            }
            catch (SystemException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.IsType<CommandException>(caughtException);
            Assert.Equal(expectedMessage, caughtException.Message);
        }

        [Fact]
        public void Constructor_WithComplexInnerException_PreservesStack()
        {
            // Arrange
            string testMessage = "Outer error";
            SystemException innerException;
            try
            {
                try
                {
                    throw new DivideByZeroException();
                }
                catch (DivideByZeroException ex)
                {
                    throw new InvalidOperationException("Inner", ex);
                }
            }
            catch (InvalidOperationException ex)
            {
                innerException = ex;
            }

            // Act
            var exception = new CommandException(testMessage, innerException);

            // Assert
            Assert.Equal(testMessage, exception.Message);
            Assert.IsType<InvalidOperationException>(exception.InnerException);
            Assert.IsType<DivideByZeroException>(exception.InnerException!.InnerException);
        }
    }
}
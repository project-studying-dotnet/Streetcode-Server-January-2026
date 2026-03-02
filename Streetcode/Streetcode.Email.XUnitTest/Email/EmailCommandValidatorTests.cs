namespace Streetcode.Email.XUnitTest.MediatR.Feedback
{
    using FluentAssertions;
    using FluentValidation.TestHelper;
    using Streetcode.Email.BLL.DTO;
    using Streetcode.Email.BLL.MediatR.Email;
    using Streetcode.Resources;
    using Xunit;

    public class EmailCommandValidatorTests
    {
        private readonly SendEmailCommandValidator validator;

        public EmailCommandValidatorTests()
        {
            this.validator = new SendEmailCommandValidator();
        }

        [Fact]
        public void ShouldReturnError_IfFeedbackIsNull()
        {
            // Arrange
            var command = new SendEmailCommand(null!);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.email)
                .WithErrorMessage(Messages.Error_CommandDataRequired);
        }

        [Fact]
        public void ShouldHaveError_WhenFeedbackDTOIsInvalid()
        {
            // Arrange
            var invalidDto = new EmailDTO
            {
                From = "invalid-email",
                Content = "123"
            };
            var command = new SendEmailCommand(invalidDto);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.email.From);
            result.ShouldHaveValidationErrorFor(x => x.email.Content);
        }

        [Fact]
        public void ShouldNotHaveErrors_WhenCommandIsValid()
        {
            // Arrange
            var validDto = new EmailDTO
            {
                From = "test@gmail.com",
                Content = "Valid message content"
            };
            var command = new SendEmailCommand(validDto);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
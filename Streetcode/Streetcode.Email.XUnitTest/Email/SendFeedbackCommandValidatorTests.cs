namespace Streetcode.Email.XUnitTest.MediatR.Feedback
{
    using FluentAssertions;
    using FluentValidation.TestHelper;
    using Streetcode.Email.BLL.DTO;
    using Streetcode.Email.BLL.MediatR.Feedback;
    using Streetcode.Resources;
    using Xunit;

    public class SendFeedbackCommandValidatorTests
    {
        private readonly SendFeedbackCommandValidator validator;

        public SendFeedbackCommandValidatorTests()
        {
            this.validator = new SendFeedbackCommandValidator();
        }

        [Fact]
        public void ShouldReturnError_IfFeedbackIsNull()
        {
            // Arrange
            var command = new SendFeedbackCommand(null!);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Feedback)
                .WithErrorMessage(Messages.Error_CommandDataRequired);
        }

        [Fact]
        public void ShouldHaveError_WhenFeedbackDTOIsInvalid()
        {
            // Arrange
            var invalidDto = new EmailDTO
            {
                Email = "invalid-email",
                Message = "123"
            };
            var command = new SendFeedbackCommand(invalidDto);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Feedback.Email);
            result.ShouldHaveValidationErrorFor(x => x.Feedback.Message);
        }

        [Fact]
        public void ShouldNotHaveErrors_WhenCommandIsValid()
        {
            // Arrange
            var validDto = new EmailDTO
            {
                Email = "test@gmail.com",
                Message = "Valid message content"
            };
            var command = new SendFeedbackCommand(validDto);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
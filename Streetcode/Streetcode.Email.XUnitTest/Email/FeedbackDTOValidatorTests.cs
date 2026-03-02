namespace Streetcode.Email.XUnitTest.MediatR.Feedback
{
    using FluentValidation.TestHelper;
    using Streetcode.Email.BLL.DTO;
    using Streetcode.Email.BLL.MediatR.Email;
    using Xunit;

    public class FeedbackDTOValidatorTests
    {
        private readonly EmailDTOValidator validator;

        public FeedbackDTOValidatorTests()
        {
            this.validator = new EmailDTOValidator();
        }

        [Fact]
        public void ShouldHaveError_WhenEmailIsEmpty()
        {
            // Arrange
            var model = new EmailDTO { Email = string.Empty };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void ShouldHaveError_WhenEmailIsInvalid()
        {
            // Arrange
            var model = new EmailDTO { Email = "not-an-email" };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorCode("EmailValidator");
        }

        [Fact]
        public void ShouldHaveError_WhenMessageIsTooShort()
        {
            // Arrange
            var model = new EmailDTO
            {
                Email = "test@gmail.com",
                Message = "123"
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Message);
        }

        [Fact]
        public void ShouldHaveError_WhenMessageExceedsMaxLength()
        {
            // Arrange
            var model = new EmailDTO
            {
                Email = "test@gmail.com",
                Message = new string('a', 101)
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Message);
        }

        [Fact]
        public void ShouldNotHaveAnyValidationErrors_WhenDTOIsValid()
        {
            // Arrange
            var model = new EmailDTO
            {
                Email = "test@gmail.com",
                Message = "Hello World!"
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
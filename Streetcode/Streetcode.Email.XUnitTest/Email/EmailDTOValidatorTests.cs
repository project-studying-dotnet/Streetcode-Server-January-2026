namespace Streetcode.Email.XUnitTest.MediatR.Feedback
{
    using FluentValidation.TestHelper;
    using Streetcode.Email.BLL.DTO;
    using Streetcode.Email.BLL.MediatR.Email;
    using Xunit;

    public class EmailDTOValidatorTests
    {
        private readonly EmailDTOValidator validator;

        public EmailDTOValidatorTests()
        {
            this.validator = new EmailDTOValidator();
        }

        [Fact]
        public void ShouldHaveError_WhenEmailIsEmpty()
        {
            // Arrange
            var model = new EmailDTO { From = string.Empty };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.From);
        }

        [Fact]
        public void ShouldHaveError_WhenEmailIsInvalid()
        {
            // Arrange
            var model = new EmailDTO { From = "not-an-email" };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.From)
                  .WithErrorCode("EmailValidator");
        }

        [Fact]
        public void ShouldHaveError_WhenMessageIsTooShort()
        {
            // Arrange
            var model = new EmailDTO
            {
                From = "test@gmail.com",
                Content = "123"
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Content);
        }

        [Fact]
        public void ShouldHaveError_WhenMessageExceedsMaxLength()
        {
            // Arrange
            var model = new EmailDTO
            {
                From = "test@gmail.com",
                Content = new string('a', 101)
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Content);
        }

        [Fact]
        public void ShouldNotHaveAnyValidationErrors_WhenDTOIsValid()
        {
            // Arrange
            var model = new EmailDTO
            {
                From = "test@gmail.com",
                Content = "Hello World!"
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
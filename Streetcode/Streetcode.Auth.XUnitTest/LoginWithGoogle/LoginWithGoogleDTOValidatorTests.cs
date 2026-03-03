namespace Streetcode.Auth.XUnitTest.LoginWithGoogle
{
    using FluentValidation.TestHelper;
    using Streetcode.Auth.BLL.DTO.Auth;
    using Streetcode.Auth.BLL.MediatR.LoginWithGoogle;
    using Xunit;

    public class LoginWithGoogleDTOValidatorTests
    {
        private readonly LoginWithGoogleDTOValidator validator;

        public LoginWithGoogleDTOValidatorTests()
        {
            this.validator = new LoginWithGoogleDTOValidator();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ShouldHaveError_WhenEmailIsEmpty(string email)
        {
            // Arrange
            var model = new LoginWithGoogleDTO { Email = email };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Email is required.");
        }

        [Theory]
        [InlineData("plainaddress")]
        [InlineData("#@%^%#$@#$@#.com")]
        [InlineData("@example.com")]
        [InlineData("email.example.com")]
        public void ShouldHaveError_WhenEmailIsInvalidFormat(string email)
        {
            // Arrange
            var model = new LoginWithGoogleDTO { Email = email };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Invalid email format.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ShouldHaveError_WhenNameIsEmpty(string name)
        {
            // Arrange
            var model = new LoginWithGoogleDTO { Name = name, Email = "test@gmail.com" };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage("Name is required.");
        }

        [Fact]
        public void ShouldNotHaveError_WhenDTOIsValid()
        {
            // Arrange
            var model = new LoginWithGoogleDTO
            {
                Email = "google.user@gmail.com",
                Name = "Ivan Ivanov",
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
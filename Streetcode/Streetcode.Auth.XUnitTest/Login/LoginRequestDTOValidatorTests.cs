namespace Streetcode.Auth.XUnitTest.Login
{
    using FluentValidation.TestHelper;
    using Streetcode.Auth.BLL.DTO.Auth;
    using Streetcode.Auth.BLL.MediatR.Login;

    public class LoginRequestDTOValidatorTests
    {
        private readonly LoginRequestDTOValidator validator;

        public LoginRequestDTOValidatorTests()
        {
            this.validator = new LoginRequestDTOValidator();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ShouldHaveError_WhenEmailIsEmpty(string email)
        {
            // Arrange
            var model = new LoginRequestDTO { Email = email };

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
            var model = new LoginRequestDTO { Email = email };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Invalid email format.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ShouldHaveError_WhenPasswordIsEmpty(string password)
        {
            // Arrange
            var model = new LoginRequestDTO { Password = password };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("Password is required.");
        }

        [Fact]
        public void ShouldNotHaveError_WhenDTOIsValid()
        {
            // Arrange
            var model = new LoginRequestDTO
            {
                Email = "correct@email.com",
                Password = "AnyStrongPassword123!",
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

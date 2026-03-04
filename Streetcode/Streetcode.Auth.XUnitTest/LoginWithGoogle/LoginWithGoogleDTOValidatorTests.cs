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
            var model = new LoginWithGoogleDTO { Email = email };
            var result = this.validator.TestValidate(model);

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
            var model = new LoginWithGoogleDTO { Email = email };
            var result = this.validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Invalid email format.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ShouldHaveError_WhenNameIsEmpty(string name)
        {
            var model = new LoginWithGoogleDTO { Name = name, Email = "test@gmail.com", Surname = "Test" };
            var result = this.validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage("Name is required.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ShouldHaveError_WhenSurnameIsEmpty(string surname)
        {
            // Arrange
            var model = new LoginWithGoogleDTO
            {
                Surname = surname,
                Email = "test@gmail.com",
                Name = "John"
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Surname)
                  .WithErrorMessage("Surname is required.");
        }

        [Fact]
        public void ShouldNotHaveError_WhenDTOIsValid()
        {
            // Arrange
            var model = new LoginWithGoogleDTO
            {
                Email = "google.user@gmail.com",
                Name = "John",
                Surname = "Smith"
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
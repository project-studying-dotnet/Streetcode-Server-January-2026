namespace Streetcode.Auth.XUnitTest.Register
{
    using FluentValidation.TestHelper;
    using Streetcode.Auth.BLL.DTO.Auth;
    using Streetcode.Auth.BLL.MediatR.Register;

    public class RegisterRequestDTOValidatorTests
    {
        private readonly RegisterRequestDTOValidator validator;

        public RegisterRequestDTOValidatorTests()
        {
            this.validator = new RegisterRequestDTOValidator();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ShouldHaveError_WhenNameIsEmpty(string name)
        {
            // Arrange
            var model = new RegisterRequestDTO { Name = name };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage("First name is required.");
        }

        [Fact]
        public void ShouldHaveError_WhenNameExceedsMaxLength()
        {
            // Arrange
            var model = new RegisterRequestDTO { Name = new string('a', 101) };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage("First name cannot exceed 100 characters.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ShouldHaveError_WhenSurnameIsEmpty(string surname)
        {
            // Arrange
            var model = new RegisterRequestDTO { Surname = surname };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Surname)
                  .WithErrorMessage("Last name is required.");
        }

        [Fact]
        public void ShouldHaveError_WhenSurnameExceedsMaxLength()
        {
            // Arrange
            var model = new RegisterRequestDTO { Surname = new string('a', 151) };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Surname)
                  .WithErrorMessage("Last name cannot exceed 150 characters.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ShouldHaveError_WhenEmailIsEmpty(string email)
        {
            // Arrange
            var model = new RegisterRequestDTO { Email = email };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Email is required.");
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("@domain.com")]
        public void ShouldHaveError_WhenEmailIsInvalid(string email)
        {
            // Arrange
            var model = new RegisterRequestDTO { Email = email };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Invalid email address format.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ShouldHaveError_WhenPhoneNumberIsEmpty(string phone)
        {
            // Arrange
            var model = new RegisterRequestDTO { PhoneNumber = phone };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
                  .WithErrorMessage("Phone number is required.");
        }

        [Theory]
        [InlineData("0991234567")]
        [InlineData("+38099123456")]
        [InlineData("+3809912345678")]
        [InlineData("+123456789012")]
        [InlineData("abcdefghijk")]
        public void ShouldHaveError_WhenPhoneNumberIsInvalidFormat(string phone)
        {
            // Arrange
            var model = new RegisterRequestDTO { PhoneNumber = phone };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
                  .WithErrorMessage("Phone number must start with +380 and contain 12 digits.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ShouldHaveError_WhenPasswordIsEmpty(string password)
        {
            // Arrange
            var model = new RegisterRequestDTO { Password = password };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("Password is required.");
        }

        [Fact]
        public void ShouldHaveError_WhenPasswordIsTooShort()
        {
            // Arrange
            var model = new RegisterRequestDTO { Password = "Short" };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("Password must be at least 6 characters long.");
        }

        [Fact]
        public void ShouldHaveError_WhenPasswordHasNoUppercase()
        {
            // Arrange
            var model = new RegisterRequestDTO { Password = "password123!" };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("Password must contain at least one uppercase letter.");
        }

        [Fact]
        public void ShouldHaveError_WhenPasswordHasNoLowercase()
        {
            // Arrange
            var model = new RegisterRequestDTO { Password = "PASSWORD123!" };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("Password must contain at least one lowercase letter.");
        }

        [Fact]
        public void ShouldHaveError_WhenPasswordHasNoDigit()
        {
            // Arrange
            var model = new RegisterRequestDTO { Password = "Password!" };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("Password must contain at least one number.");
        }

        [Fact]
        public void ShouldHaveError_WhenPasswordHasNoSpecialChar()
        {
            // Arrange
            var model = new RegisterRequestDTO { Password = "Password123" };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("Password must contain at least one special character.");
        }

        [Fact]
        public void ShouldNotHaveError_WhenDTOIsValid()
        {
            // Arrange
            var model = new RegisterRequestDTO
            {
                Name = "John",
                Surname = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "+380991234567",
                Password = "StrongPassword1!",
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

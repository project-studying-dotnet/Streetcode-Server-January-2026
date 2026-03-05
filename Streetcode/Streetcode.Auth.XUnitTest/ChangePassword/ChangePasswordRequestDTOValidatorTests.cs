namespace Streetcode.Auth.XUnitTest.ChangePassword
{
    using FluentValidation.TestHelper;
    using Streetcode.Auth.BLL.DTO.Auth;
    using Streetcode.Auth.BLL.MediatR.ChangePassword;
    using Xunit;

    public class ChangePasswordRequestDTOValidatorTests
    {
        private readonly ChangePasswordRequestDTOValidator validator;

        public ChangePasswordRequestDTOValidatorTests()
        {
            this.validator = new ChangePasswordRequestDTOValidator();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ShouldHaveError_WhenCurrentPasswordIsEmpty(string password)
        {
            var model = new ChangePasswordRequestDTO { CurrentPassword = password };
            var result = this.validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CurrentPassword)
                  .WithErrorMessage("Password is required.");
        }

        [Fact]
        public void ShouldHaveError_WhenNewPasswordIsTooShort()
        {
            var model = new ChangePasswordRequestDTO { NewPassword = "Short" };
            var result = this.validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.NewPassword)
                  .WithErrorMessage("Password must be at least 6 characters long.");
        }

        [Fact]
        public void ShouldHaveError_WhenNewPasswordHasNoUppercase()
        {
            var model = new ChangePasswordRequestDTO { NewPassword = "password123!" };
            var result = this.validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.NewPassword)
                  .WithErrorMessage("Password must contain at least one uppercase letter.");
        }

        [Fact]
        public void ShouldHaveError_WhenNewPasswordHasNoLowercase()
        {
            var model = new ChangePasswordRequestDTO { NewPassword = "PASSWORD123!" };
            var result = this.validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.NewPassword)
                  .WithErrorMessage("Password must contain at least one lowercase letter.");
        }

        [Fact]
        public void ShouldHaveError_WhenNewPasswordHasNoDigit()
        {
            var model = new ChangePasswordRequestDTO { NewPassword = "Password!" };
            var result = this.validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.NewPassword)
                  .WithErrorMessage("Password must contain at least one number.");
        }

        [Fact]
        public void ShouldHaveError_WhenNewPasswordHasNoSpecialChar()
        {
            var model = new ChangePasswordRequestDTO { NewPassword = "Password123" };
            var result = this.validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.NewPassword)
                  .WithErrorMessage("Password must contain at least one special character.");
        }

        [Fact]
        public void ShouldHaveError_WhenNewPasswordIsSameAsCurrent()
        {
            var model = new ChangePasswordRequestDTO
            {
                CurrentPassword = "StrongPassword1!",
                NewPassword = "StrongPassword1!"
            };
            var result = this.validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.NewPassword)
                  .WithErrorMessage("New password cannot be the same as the current password.");
        }

        [Fact]
        public void ShouldNotHaveError_WhenDTOIsValid()
        {
            var model = new ChangePasswordRequestDTO
            {
                CurrentPassword = "OldPassword1!",
                NewPassword = "NewPassword2?"
            };
            var result = this.validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
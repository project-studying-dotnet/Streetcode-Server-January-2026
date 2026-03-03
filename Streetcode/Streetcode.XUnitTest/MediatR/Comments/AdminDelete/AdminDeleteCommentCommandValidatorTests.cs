namespace Streetcode.XUnitTest.MediatR.Comments.Delete
{
    using FluentValidation.TestHelper;
    using Streetcode.BLL.MediatR.Streetcode.Comments.AdminDelete;
    using Streetcode.Resources;
    using Xunit;

    public class AdminDeleteCommentCommandValidatorTests
    {
        private readonly AdminDeleteCommentCommandValidator validator;

        public AdminDeleteCommentCommandValidatorTests()
        {
            this.validator = new AdminDeleteCommentCommandValidator();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ShouldHaveError_WhenIdIsInvalid(int id)
        {
            // Arrange
            var command = new AdminDeleteCommentCommand(id);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage(string.Format(Messages.Error_PropertyMustBeGreaterThanZero, nameof(AdminDeleteCommentCommand.Id)));
        }

        [Fact]
        public void ShouldNotHaveError_WhenCommandIsValid()
        {
            // Arrange
            var command = new AdminDeleteCommentCommand(10);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

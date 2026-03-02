namespace Streetcode.XUnitTest.MediatR.Comments.Delete
{
    using FluentValidation.TestHelper;
    using Streetcode.BLL.MediatR.Streetcode.Comments.Delete;
    using Streetcode.Resources;
    using Xunit;

    public class DeleteCommentCommandValidatorTests
    {
        private readonly DeleteCommentCommandValidator validator;

        public DeleteCommentCommandValidatorTests()
        {
            this.validator = new DeleteCommentCommandValidator();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ShouldHaveError_WhenIdIsInvalid(int id)
        {
            // Arrange
            var command = new DeleteCommentCommand(id, "user-123");

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage(string.Format(Messages.Error_PropertyMustBeGreaterThanZero, nameof(DeleteCommentCommand.Id)));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShouldHaveError_WhenUserIdIsEmpty(string? userId)
        {
            // Arrange
            var command = new DeleteCommentCommand(1, userId!);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserId)
                .WithErrorMessage(string.Format(Messages.Error_PropertyIsRequired, "UserId"));
        }

        [Fact]
        public void ShouldNotHaveError_WhenCommandIsValid()
        {
            // Arrange
            var command = new DeleteCommentCommand(10, "valid-user-id");

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

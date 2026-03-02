namespace Streetcode.XUnitTest.MediatR.Comments.Update
{
    using FluentValidation.TestHelper;
    using Streetcode.BLL.DTO.Streetcode.Comments;
    using Streetcode.BLL.MediatR.Streetcode.Comments.Update;
    using Streetcode.Resources;
    using Xunit;

    public class UpdateCommentCommandValidatorTests
    {
        private readonly UpdateCommentCommandValidator validator;

        public UpdateCommentCommandValidatorTests()
        {
            this.validator = new UpdateCommentCommandValidator();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShouldHaveError_WhenUserIdIsEmpty(string? userId)
        {
            // Arrange
            var dto = new UpdateCommentDTO { Id = 1, TextContent = "Test" };
            var command = new UpdateCommentCommand(dto, userId!);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserId)
                .WithErrorMessage(string.Format(Messages.Error_PropertyIsRequired, "UserId"));
        }

        [Fact]
        public void ShouldHaveError_WhenCommentDTOIsNull()
        {
            // Arrange
            var command = new UpdateCommentCommand(null!, "user-123");

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Comment)
                .WithErrorMessage(Messages.Error_CommandDataRequired);
        }

        [Fact]
        public void ShouldHaveError_WhenChildDTOIsInvalid()
        {
            // Arrange
            var invalidDto = new UpdateCommentDTO { Id = 0, TextContent = "Valid text" };
            var command = new UpdateCommentCommand(invalidDto, "user-123");

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor("Comment.Id");
        }

        [Fact]
        public void ShouldNotHaveError_WhenCommandIsValid()
        {
            // Arrange
            var validDto = new UpdateCommentDTO { Id = 1, TextContent = "Valid text" };
            var command = new UpdateCommentCommand(validDto, "user-123");

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

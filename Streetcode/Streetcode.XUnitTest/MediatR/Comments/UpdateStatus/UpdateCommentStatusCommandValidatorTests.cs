namespace Streetcode.XUnitTest.MediatR.Comments.UpdateStatus
{
    using FluentValidation.TestHelper;
    using Streetcode.BLL.DTO.Streetcode.Comments;
    using Streetcode.BLL.MediatR.Streetcode.Comments.UpdateStatus;
    using Streetcode.DAL.Enums;
    using Streetcode.Resources;
    using Xunit;

    public class UpdateCommentStatusCommandValidatorTests
    {
        private readonly UpdateCommentStatusCommandValidator validator;

        public UpdateCommentStatusCommandValidatorTests()
        {
            this.validator = new UpdateCommentStatusCommandValidator();
        }

        [Fact]
        public void ShouldHaveError_WhenDtoIsNull()
        {
            // Arrange
            var command = new UpdateCommentStatusCommand(null!);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Comment)
                .WithErrorMessage(Messages.Error_CommandDataRequired);
        }

        [Fact]
        public void ShouldHaveError_WhenChildDtoIsInvalid()
        {
            // Arrange
            var invalidDto = new UpdateCommentStatusDTO { Id = 0, Status = CommentStatus.Approved };
            var command = new UpdateCommentStatusCommand(invalidDto);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor("Comment.Id");
        }

        [Fact]
        public void ShouldNotHaveError_WhenCommandIsValid()
        {
            // Arrange
            var validDto = new UpdateCommentStatusDTO { Id = 1, Status = CommentStatus.Approved };
            var command = new UpdateCommentStatusCommand(validDto);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
namespace Streetcode.XUnitTest.MediatR.Comments.UpdateStatus
{
    using FluentValidation.TestHelper;
    using Streetcode.BLL.DTO.Streetcode.Comments;
    using Streetcode.BLL.MediatR.Streetcode.Comments.UpdateStatus;
    using Streetcode.DAL.Enums;
    using Streetcode.Resources;
    using Streetcode.Shared.Extensions;
    using Xunit;

    public class UpdateCommentStatusDTOValidatorTests
    {
        private readonly UpdateCommentStatusDTOValidator validator;

        public UpdateCommentStatusDTOValidatorTests()
        {
            this.validator = new UpdateCommentStatusDTOValidator();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ShouldHaveError_WhenIdIsInvalid(int id)
        {
            // Arrange
            var model = new UpdateCommentStatusDTO { Id = id, Status = CommentStatus.Approved };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage(string.Format(Messages.Error_PropertyMustBeGreaterThanZero, nameof(UpdateCommentStatusDTO.Id)));
        }

        [Theory]
        [InlineData((CommentStatus)999)]
        [InlineData((CommentStatus)(-1))]
        public void ShouldHaveError_WhenStatusIsInvalid(CommentStatus status)
        {
            // Arrange
            var model = new UpdateCommentStatusDTO { Id = 1, Status = status };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Status)
                .WithErrorMessage(Messages.Error_InvalidEnumValue.Format(nameof(UpdateCommentStatusDTO.Status)));
        }

        [Fact]
        public void ShouldNotHaveError_WhenDtoIsValid()
        {
            // Arrange
            var model = new UpdateCommentStatusDTO
            {
                Id = 1,
                Status = CommentStatus.Approved,
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
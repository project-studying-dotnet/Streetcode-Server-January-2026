namespace Streetcode.XUnitTest.MediatR.Comments.Create
{
    using FluentValidation.TestHelper;
    using Streetcode.BLL.DTO.Streetcode.Comments;
    using Streetcode.BLL.MediatR.Streetcode.Comments.Create;
    using Streetcode.Resources;
    using Xunit;

    public class CreateCommentCommandValidatorTests
    {
        private readonly CreateCommentCommandValidator validator;

        public CreateCommentCommandValidatorTests()
        {
            this.validator = new CreateCommentCommandValidator();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShouldHaveError_WhenUserIdIsEmpty(string? userId)
        {
            // Arrange
            var dto = new CreateCommentDTO { TextContent = "Test", StreetcodeId = 1 };
            var command = new CreateCommentCommand(dto, userId!);

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
            var command = new CreateCommentCommand(null!, "user-id");

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
            var invalidDto = new CreateCommentDTO { TextContent = "", StreetcodeId = 1 };
            var command = new CreateCommentCommand(invalidDto, "user-id");

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor("Comment.TextContent");
        }

        [Fact]
        public void ShouldNotHaveError_WhenCommandIsValid()
        {
            // Arrange
            var validDto = new CreateCommentDTO { TextContent = "Valid text", StreetcodeId = 1 };
            var command = new CreateCommentCommand(validDto, "user-id");

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

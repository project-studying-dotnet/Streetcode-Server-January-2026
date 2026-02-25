namespace Streetcode.XUnitTest.MediatR.Comments.Update
{
    using FluentValidation.TestHelper;
    using Streetcode.BLL.DTO.Streetcode.Comments;
    using Streetcode.BLL.MediatR.Streetcode.Comments.Update;
    using Streetcode.Resources;
    using Xunit;

    public class UpdateCommentDTOValidatorTests
    {
        private readonly UpdateCommentDTOValidator validator;

        public UpdateCommentDTOValidatorTests()
        {
            this.validator = new UpdateCommentDTOValidator();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ShouldHaveError_WhenIdIsInvalid(int id)
        {
            // Arrange
            var model = new UpdateCommentDTO { Id = id, TextContent = "Valid text" };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage(string.Format(Messages.Error_PropertyMustBeGreaterThanZero, nameof(UpdateCommentDTO.Id)));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShouldHaveError_WhenTextContentIsEmpty(string? textContent)
        {
            // Arrange
            var model = new UpdateCommentDTO { Id = 1, TextContent = textContent! };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TextContent)
                .WithErrorMessage(string.Format(Messages.Error_PropertyIsRequired, nameof(UpdateCommentDTO.TextContent)));
        }

        [Fact]
        public void ShouldHaveError_WhenTextContentExceedsMaxLength()
        {
            // Arrange
            var longText = new string('a', 251);
            var model = new UpdateCommentDTO { Id = 1, TextContent = longText };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TextContent)
                .WithErrorMessage(string.Format(Messages.Error_PropertyMustNotExceedCharacters, nameof(UpdateCommentDTO.TextContent), 250));
        }

        [Fact]
        public void ShouldNotHaveError_WhenDTOIsValid()
        {
            // Arrange
            var model = new UpdateCommentDTO
            {
                Id = 1,
                TextContent = "Valid update text",
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

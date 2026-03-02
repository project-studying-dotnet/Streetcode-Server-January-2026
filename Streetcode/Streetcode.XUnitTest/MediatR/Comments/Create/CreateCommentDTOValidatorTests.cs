namespace Streetcode.XUnitTest.MediatR.Comments.Create
{
    using FluentValidation.TestHelper;
    using Streetcode.BLL.DTO.Streetcode.Comments;
    using Streetcode.BLL.MediatR.Streetcode.Comments.Create;
    using Streetcode.Resources;
    using Xunit;

    public class CreateCommentDTOValidatorTests
    {
        private readonly CreateCommentDTOValidator validator;

        public CreateCommentDTOValidatorTests()
        {
            this.validator = new CreateCommentDTOValidator();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShouldHaveError_WhenTextContentIsEmpty(string? textContent)
        {
            // Arrange
            var model = new CreateCommentDTO { TextContent = textContent! };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TextContent)
                .WithErrorMessage(string.Format(Messages.Error_PropertyIsRequired, nameof(CreateCommentDTO.TextContent)));
        }

        [Fact]
        public void ShouldHaveError_WhenTextContentExceedsMaxLength()
        {
            // Arrange
            var longText = new string('a', 251);
            var model = new CreateCommentDTO { TextContent = longText };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TextContent)
                .WithErrorMessage(string.Format(Messages.Error_PropertyMustNotExceedCharacters, nameof(CreateCommentDTO.TextContent), 250));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ShouldHaveError_WhenStreetcodeIdIsInvalid(int streetcodeId)
        {
            // Arrange
            var model = new CreateCommentDTO { StreetcodeId = streetcodeId };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.StreetcodeId)
                .WithErrorMessage(string.Format(Messages.Error_PropertyMustBeGreaterThanZero, nameof(CreateCommentDTO.StreetcodeId)));
        }

        [Fact]
        public void ShouldNotHaveError_WhenDTOIsValid()
        {
            // Arrange
            var model = new CreateCommentDTO
            {
                StreetcodeId = 1,
                TextContent = "Valid comment text",
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

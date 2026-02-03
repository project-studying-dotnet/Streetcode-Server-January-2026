namespace Streetcode.XUnitTest.MediatR.Fact.Create
{
    using FluentValidation.TestHelper;
    using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
    using Streetcode.BLL.MediatR.Streetcode.Fact.Create;
    using Xunit;

    public class CreateFactDTOValidatorTests
    {
        private readonly CreateFactDTOValidator validator;

        public CreateFactDTOValidatorTests()
        {
            this.validator = new CreateFactDTOValidator();
        }

        [Fact]
        public void ShouldHaveError_WhenTitleIsEmpty()
        {
            // Arrange
            var model = new CreateFactDTO { Title = string.Empty };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Title is required.");
        }

        [Fact]
        public void ShouldHaveError_WhenTitleExceedsMaxLength()
        {
            // Arrange
            var model = new CreateFactDTO { Title = new string('a', 69) };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Title length must not exceed 68 characters.");
        }

        [Fact]
        public void ShouldHaveError_WhenFactContentIsEmpty()
        {
            // Arrange
            var model = new CreateFactDTO { FactContent = string.Empty };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.FactContent)
                  .WithErrorMessage("Fact content is required.");
        }

        [Fact]
        public void ShouldHaveError_WhenFactContentExceedsMaxLength()
        {
            // Arrange
            var model = new CreateFactDTO { FactContent = new string('a', 601) };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.FactContent)
                  .WithErrorMessage("Fact content length must not exceed 600 characters.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ShouldHaveError_WhenImageIdIsInvalid(int imageId)
        {
            // Arrange
            var model = new CreateFactDTO { ImageId = imageId };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ImageId)
                  .WithErrorMessage("ImageId must be greater than 0.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ShouldHaveError_WhenStreetcodeIdIsInvalid(int streetcodeId)
        {
            // Arrange
            var model = new CreateFactDTO { StreetcodeId = streetcodeId };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.StreetcodeId)
                  .WithErrorMessage("StreetcodeId must be greater than 0.");
        }

        [Fact]
        public void ShouldHaveError_WhenImageDescriptionExceedsMaxLength()
        {
            // Arrange
            var model = new CreateFactDTO { ImageDescription = new string('a', 201) };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ImageDescription)
                  .WithErrorMessage("Image description must not exceed 200 characters.");
        }

        [Fact]
        public void ShouldNotHaveError_WhenImageDescriptionIsEmptyOrNull()
        {
            // Arrange
            var modelEmpty = new CreateFactDTO { ImageDescription = string.Empty };
            var modelNull = new CreateFactDTO { ImageDescription = null };

            // Act & Assert
            var resultEmpty = this.validator.TestValidate(modelEmpty);
            var resultNull = this.validator.TestValidate(modelNull);

            resultEmpty.ShouldNotHaveValidationErrorFor(x => x.ImageDescription);
            resultNull.ShouldNotHaveValidationErrorFor(x => x.ImageDescription);
        }

        [Fact]
        public void ShouldBeValid_WhenAllPropertiesAreCorrect()
        {
            // Arrange
            var model = new CreateFactDTO
            {
                Title = "Valid Title",
                FactContent = "Valid Content",
                ImageId = 1,
                StreetcodeId = 1,
                ImageDescription = "Valid Description",
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
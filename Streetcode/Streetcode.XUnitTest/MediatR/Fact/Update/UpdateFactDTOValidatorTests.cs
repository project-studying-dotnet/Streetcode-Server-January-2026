namespace Streetcode.XUnitTest.MediatR.Fact.Update
{
    using FluentValidation.TestHelper;
    using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
    using Streetcode.BLL.MediatR.Streetcode.Fact.Update;
    using Xunit;

    public class UpdateFactDTOValidatorTests
    {
        private readonly UpdateFactDTOValidator validator;

        public UpdateFactDTOValidatorTests()
        {
            this.validator = new UpdateFactDTOValidator();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ShouldHaveError_WhenIdIsInvalid(int id)
        {
            // Arrange
            var model = new UpdateFactDTO { Id = id };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Fact Id must be greater than 0.");
        }

        [Fact]
        public void ShouldHaveError_WhenTitleIsEmpty()
        {
            // Arrange
            var model = new UpdateFactDTO { Title = string.Empty };

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
            var model = new UpdateFactDTO { Title = new string('a', 69) };

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
            var model = new UpdateFactDTO { FactContent = string.Empty };

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
            var model = new UpdateFactDTO { FactContent = new string('a', 601) };

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
            var model = new UpdateFactDTO { ImageId = imageId };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ImageId)
                  .WithErrorMessage("ImageId must be greater than 0.");
        }

        [Fact]
        public void ShouldHaveError_WhenImageDescriptionExceedsMaxLength()
        {
            // Arrange
            var model = new UpdateFactDTO { ImageDescription = new string('a', 201) };

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
            var modelEmpty = new UpdateFactDTO { ImageDescription = string.Empty };
            var modelNull = new UpdateFactDTO { ImageDescription = null };

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
            var model = new UpdateFactDTO
            {
                Id = 1,
                Title = "Valid Title",
                FactContent = "Valid Content",
                ImageId = 1,
                ImageDescription = "Valid Description",
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

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
                  .WithErrorMessage(Messages.Error_PropertyIsRequired.Format(nameof(CreateFactDTO.Title)));
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
                  .WithErrorMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(nameof(CreateFactDTO.Title), 68));
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
                  .WithErrorMessage(Messages.Error_PropertyIsRequired.Format(nameof(CreateFactDTO.FactContent)));
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
                  .WithErrorMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(nameof(CreateFactDTO.FactContent), 600));
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
                  .WithErrorMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(CreateFactDTO.ImageId)));
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
                  .WithErrorMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(CreateFactDTO.StreetcodeId)));
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
                  .WithErrorMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(nameof(CreateFactDTO.ImageDescription), 200));
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
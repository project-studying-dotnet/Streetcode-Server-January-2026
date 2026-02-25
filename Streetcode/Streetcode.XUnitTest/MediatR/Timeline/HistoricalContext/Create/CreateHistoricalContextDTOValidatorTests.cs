using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.MediatR.Timeline.HistoricalContext.Create;
using Streetcode.Resources;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.Timeline.HistoricalContext.Create
{
    public class CreateHistoricalContextDTOValidatorTests
    {
        private readonly HistoricalContextCreateDTOValidator validator;

        public CreateHistoricalContextDTOValidatorTests()
        {
            this.validator = new HistoricalContextCreateDTOValidator();
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDTOIsValid()
        {
            // Arrange
            var validDTO = new CreateHistoricalContextDTO
            {
                Title = "Valid Title",
            };

            // Act
            var result = this.validator.TestValidate(validDTO);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void ShouldHaveError_WhenTitleIsEmpty()
        {
            // Arrange
            var dto = new CreateHistoricalContextDTO { Title = string.Empty };
            var expectedError = string.Format(Messages.Error_PropertyIsRequired, nameof(dto.Title));

            // Act
            var result = this.validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage(expectedError);
        }

        [Fact]
        public void ShouldHaveError_WhenTitleExceedsMaximumLength()
        {
            // Arrange
            var dto = new CreateHistoricalContextDTO { Title = new string('a', 51) };
            var expectedError = string.Format(Messages.Error_PropertyMustNotExceedCharacters, nameof(dto.Title), 50);

            // Act
            var result = this.validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage(expectedError);
        }
    }
}

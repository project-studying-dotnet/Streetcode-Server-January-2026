using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.MediatR.Timeline.HistoricalContext.Update;
using Streetcode.Resources;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.Timeline.HistoricalContext.Update
{
    public class UpdateHistoricalContextDTOValidatorTests
    {
        private readonly HistoricalContextDTOUpdateValidator validator;

        public UpdateHistoricalContextDTOValidatorTests()
        {
            this.validator = new HistoricalContextDTOUpdateValidator();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ShouldHaveError_WhenIdIsZeroOrLess(int invalidId)
        {
            // Arrange
            var DTO = new UpdateHistoricalContextDTO { Id = invalidId };
            var expectedError = string.Format(Messages.Error_PropertyMustBeGreaterThanZero, nameof(DTO.Id));

            // Act
            var result = this.validator.TestValidate(DTO);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage(expectedError);
        }

        [Fact]
        public void ShouldHaveError_WhenTitleIsEmpty()
        {
            // Arrange
            var DTO = new UpdateHistoricalContextDTO { Title = string.Empty };
            var expectedError = string.Format(Messages.Error_PropertyIsRequired, nameof(DTO.Title));

            // Act
            var result = this.validator.TestValidate(DTO);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage(expectedError);
        }

        [Fact]
        public void ShouldHaveError_WhenTitleExceedsMaximumLength()
        {
            // Arrange
            var DTO = new UpdateHistoricalContextDTO { Title = new string('a', 51) };
            var expectedError = string.Format(Messages.Error_PropertyMustNotExceedCharacters, nameof(DTO.Title), 50);

            // Act
            var result = this.validator.TestValidate(DTO);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage(expectedError);
        }


        [Fact]
        public void ShouldNotHaveError_WhenDTOIsValid()
        {
            // Arrange
            var DTO = new UpdateHistoricalContextDTO
            {
                Id = 1,
                Title = "Valid Updated Title"
            };

            // Act
            var result = this.validator.TestValidate(DTO);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

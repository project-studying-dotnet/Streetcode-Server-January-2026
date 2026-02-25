namespace Streetcode.XUnitTest.MediatR.Timeline.TimelineItem.Update
{
    using FluentValidation.TestHelper;
    using Streetcode.BLL.DTO.Timeline.TimelineItem;
    using Streetcode.BLL.MediatR.Timeline.TimelineItem.Update;
    using Streetcode.DAL.Enums;
    using Streetcode.Resources;
    using Xunit;

    public class UpdateTimelineItemDTOValidatorTests
    {
        private readonly TimelineItemUpdateDTOValidator validator;

        public UpdateTimelineItemDTOValidatorTests()
        {
            this.validator = new TimelineItemUpdateDTOValidator();
        }

        [Fact]
        public void ShouldHaveError_WhenIdIsZeroOrLess()
        {
            // Arrange
            var model = new UpdateTimelineItemDTO { Id = 0 };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage(string.Format(Messages.Error_PropertyMustBeGreaterThanZero, nameof(UpdateTimelineItemDTO.Id)));
        }

        [Fact]
        public void ShouldHaveError_WhenTitleIsEmpty()
        {
            // Arrange
            var model = new UpdateTimelineItemDTO { Title = string.Empty };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title)
                .WithErrorMessage(string.Format(Messages.Error_PropertyIsRequired, nameof(UpdateTimelineItemDTO.Title)));
        }

        [Fact]
        public void ShouldHaveError_WhenTitleExceedsMaxLength()
        {
            // Arrange
            var model = new UpdateTimelineItemDTO { Title = new string('a', 101) };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title)
                .WithErrorMessage(string.Format(Messages.Error_PropertyMustNotExceedCharacters, nameof(UpdateTimelineItemDTO.Title), 100));
        }

        [Fact]
        public void ShouldHaveError_WhenDescriptionExceedsMaxLength()
        {
            // Arrange
            var model = new UpdateTimelineItemDTO { Description = new string('a', 601) };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description)
                .WithErrorMessage(string.Format(Messages.Error_AdditionalTextMustNotExceedCharacters, 600));
        }

        [Fact]
        public void ShouldHaveError_WhenDateIsEmpty()
        {
            // Arrange
            var model = new UpdateTimelineItemDTO { Date = default };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Date)
                .WithErrorMessage(string.Format(Messages.Error_PropertyIsRequired, nameof(UpdateTimelineItemDTO.Date)));
        }

        [Fact]
        public void ShouldHaveError_WhenDateViewPatternIsInvalid()
        {
            // Arrange
            var model = new UpdateTimelineItemDTO { DateViewPattern = (DateViewPattern)999 };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            var expectedMessage = string.Format(Messages.Error_EntitiesNotFound, nameof(UpdateTimelineItemDTO.DateViewPattern));
            result.ShouldHaveValidationErrorFor(x => x.DateViewPattern)
                .WithErrorMessage(expectedMessage);
        }

        [Fact]
        public void ShouldHaveError_WhenStreetcodeIdIsZeroOrLess()
        {
            // Arrange
            var model = new UpdateTimelineItemDTO { StreetcodeId = 0 };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.StreetcodeId)
                .WithErrorMessage(string.Format(Messages.Error_PropertyMustBeGreaterThanZero, nameof(UpdateTimelineItemDTO.StreetcodeId)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ShouldHaveError_WhenHistoricalContextIdsAreInvalid(int invalidId)
        {
            // Arrange
            var model = new UpdateTimelineItemDTO { HistoricalContextIds = new List<int> { 1, invalidId } };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrors()
                .WithErrorMessage(string.Format(Messages.Error_PropertyMustBeGreaterThanZero, nameof(UpdateTimelineItemDTO.HistoricalContextIds)));
        }

        [Fact]
        public void ShouldNotHaveError_WhenModelIsValid()
        {
            // Arrange
            var model = new UpdateTimelineItemDTO
            {
                Id = 1,
                Title = "Updated Title",
                Description = "Updated Description",
                Date = DateTime.Now,
                DateViewPattern = DateViewPattern.DateMonthYear,
                StreetcodeId = 1,
                HistoricalContextIds = new List<int> { 1 }
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

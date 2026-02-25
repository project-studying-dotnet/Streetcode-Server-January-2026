namespace Streetcode.XUnitTest.MediatR.Timeline.TimelineItem.Create
{
    using FluentValidation.TestHelper;
    using Streetcode.BLL.DTO.Timeline.TimelineItem;
    using Streetcode.BLL.MediatR.Timeline.TimelineItem.Create;
    using Streetcode.DAL.Enums;
    using Streetcode.Resources;
    using Xunit;

    public class CreateTimelineItemDTOValidatorTests
    {
        private readonly TimelineItemCreateDTOValidator validator;

        public CreateTimelineItemDTOValidatorTests()
        {
            this.validator = new TimelineItemCreateDTOValidator();
        }

        [Fact]
        public void ShouldHaveError_WhenTitleIsEmpty()
        {
            // Arrange
            var model = new CreateTimelineItemDTO { Title = string.Empty };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title)
                .WithErrorMessage(string.Format(Messages.Error_PropertyIsRequired, nameof(CreateTimelineItemDTO.Title)));
        }

        [Fact]
        public void ShouldHaveError_WhenTitleExceedsMaxLength()
        {
            // Arrange
            var model = new CreateTimelineItemDTO { Title = new string('a', 101) };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title)
                .WithErrorMessage(string.Format(Messages.Error_PropertyMustNotExceedCharacters, nameof(CreateTimelineItemDTO.Title), 100));
        }

        [Fact]
        public void ShouldHaveError_WhenDescriptionExceedsMaxLength()
        {
            // Arrange
            var model = new CreateTimelineItemDTO { Description = new string('a', 601) };

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
            var model = new CreateTimelineItemDTO { Date = default };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Date)
                .WithErrorMessage(string.Format(Messages.Error_PropertyIsRequired, nameof(CreateTimelineItemDTO.Date)));
        }

        [Fact]
        public void ShouldHaveError_WhenStreetcodeIdIsZeroOrLess()
        {
            // Arrange
            var model = new CreateTimelineItemDTO { StreetcodeId = 0 };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.StreetcodeId)
                .WithErrorMessage(string.Format(Messages.Error_PropertyMustBeGreaterThanZero, nameof(CreateTimelineItemDTO.StreetcodeId)));
        }

        [Fact]
        public void ShouldHaveError_WhenHistoricalContextIdsContainInvalidId()
        {
            // Arrange
            var model = new CreateTimelineItemDTO { HistoricalContextIds = new List<int> { 1, 0, -5 } };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.HistoricalContextIds)
                .WithErrorMessage(string.Format(Messages.Error_PropertyMustBeGreaterThanZero, nameof(CreateTimelineItemDTO.HistoricalContextIds)));
        }

        [Fact]
        public void ShouldNotHaveError_WhenModelIsValid()
        {
            // Arrange
            var model = new CreateTimelineItemDTO
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.Now,
                DateViewPattern = 0,
                StreetcodeId = 1,
                HistoricalContextIds = new List<int> { 1, 2 }
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

namespace Streetcode.XUnitTest.MediatR.Timeline.TimelineItem.Create
{
    using FluentAssertions;
    using FluentValidation.TestHelper;
    using Streetcode.BLL.DTO.Timeline.TimelineItem;
    using Streetcode.BLL.MediatR.Timeline.TimelineItem.Create;
    using Streetcode.Resources;
    using Xunit;

    public class CreateTimelineItemCommandValidatorTests
    {
        private readonly CreateTimelineItemCommandValidator validator;

        public CreateTimelineItemCommandValidatorTests()
        {
            this.validator = new CreateTimelineItemCommandValidator();
        }

        [Fact]
        public void ShouldReturnError_IfTimelineItemIsNull()
        {
            // Arrange
            var command = new CreateTimelineItemCommand(null!);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(x => x.TimelineItem)
                .WithErrorMessage(Messages.Error_CommandDataRequired);
        }

        [Fact]
        public void ShouldReturnOk_IfTimelineItemIsValid()
        {
            // Arrange
            var timelineItem = new CreateTimelineItemDTO
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Date = DateTime.Now,
                DateViewPattern = 0,
                StreetcodeId = 1,
                HistoricalContextIds = new List<int> { 1, 2 },
            };

            var command = new CreateTimelineItemCommand(timelineItem);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

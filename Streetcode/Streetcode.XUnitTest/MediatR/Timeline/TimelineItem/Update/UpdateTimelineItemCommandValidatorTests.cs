namespace Streetcode.XUnitTest.MediatR.Timeline.TimelineItem.Update
{
    using FluentValidation.TestHelper;
    using Streetcode.BLL.DTO.Timeline.TimelineItem;
    using Streetcode.BLL.MediatR.Timeline.TimelineItem.Update;
    using Streetcode.Resources;
    using Xunit;

    public class UpdateTimelineItemCommandValidatorTests
    {
        private readonly UpdateTimelineItemCommandValidator validator;

        public UpdateTimelineItemCommandValidatorTests()
        {
            this.validator = new UpdateTimelineItemCommandValidator();
        }

        [Fact]
        public void ShouldHaveError_WhenTimelineItemIsNull()
        {
            // Arrange
            var command = new UpdateTimelineItemCommand(null!);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TimelineItem)
                .WithErrorMessage(Messages.Error_CommandDataRequired);
        }

        [Fact]
        public void ShouldNotHaveError_WhenTimelineItemIsNotNull()
        {
            // Arrange
            var dto = new UpdateTimelineItemDTO
            {
                Id = 1,
                Title = "Valid Title",
                Date = DateTime.UtcNow,
                StreetcodeId = 1,
            };
            var command = new UpdateTimelineItemCommand(dto);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.TimelineItem);
        }

        [Fact]
        public void ShouldInvokeChildValidator()
        {
            // Arrange
            var DTO = new UpdateTimelineItemDTO { Title = string.Empty };
            var command = new UpdateTimelineItemCommand(DTO);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrors();
        }
    }
}

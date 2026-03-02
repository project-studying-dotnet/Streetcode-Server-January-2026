using FluentValidation.TestHelper;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Delete;
using Streetcode.Resources;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.Timeline.TimelineItem.Delete
{
        public class DeleteTimelineItemCommandValidatorTests
        {
            private readonly DeleteTimelineItemCommandValidator validator;

            public DeleteTimelineItemCommandValidatorTests()
            {
                this.validator = new DeleteTimelineItemCommandValidator();
            }

            [Fact]
            public void ShouldHaveError_WhenIdIsZero()
            {
                // Arrange
                var command = new DeleteTimelineItemCommand(0);

                // Act
                var result = this.validator.TestValidate(command);

                // Assert
                result.ShouldHaveValidationErrorFor(x => x.Id)
                    .WithErrorMessage(string.Format(Messages.Error_PropertyMustBeGreaterThanZero, nameof(DeleteTimelineItemCommand.Id)));
            }

            [Fact]
            public void ShouldHaveError_WhenIdIsNegative()
            {
                // Arrange
                var command = new DeleteTimelineItemCommand(-1);

                // Act
                var result = this.validator.TestValidate(command);

                // Assert
                result.ShouldHaveValidationErrorFor(x => x.Id)
                    .WithErrorMessage(string.Format(Messages.Error_PropertyMustBeGreaterThanZero, nameof(DeleteTimelineItemCommand.Id)));
            }

            [Fact]
            public void ShouldNotHaveError_WhenIdIsGreaterThanZero()
            {
                // Arrange
                var command = new DeleteTimelineItemCommand(1);

                // Act
                var result = this.validator.TestValidate(command);

                // Assert
                result.ShouldNotHaveValidationErrorFor(x => x.Id);
            }
        }
}

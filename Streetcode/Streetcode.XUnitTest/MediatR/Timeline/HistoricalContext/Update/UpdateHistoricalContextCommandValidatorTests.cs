namespace Streetcode.XUnitTest.MediatR.Timeline.HistoricalContext.Update
{
    using FluentAssertions;
    using FluentValidation.TestHelper;
    using Streetcode.BLL.DTO.Timeline.HistoricalContext;
    using Streetcode.BLL.MediatR.Timeline.HistoricalContext.Update;
    using Streetcode.Resources;
    using Xunit;

    public class UpdateHistoricalContextCommandValidatorTests
    {
        private readonly UpdateHistoricalContextCommandValidator validator;

        public UpdateHistoricalContextCommandValidatorTests()
        {
            this.validator = new UpdateHistoricalContextCommandValidator();
        }

        [Fact]
        public void ShouldReturnError_IfHistoricalContextIsNull()
        {
            // Arrange
            var command = new UpdateHistoricalContextCommand(null!);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(x => x.HistoricalContext)
            .WithErrorMessage(Messages.Error_CommandDataRequired);
        }

        [Fact]
        public void ShouldReturnOk_IfContextValid()
        {
            // Arrange
            var historicalContext = new UpdateHistoricalContextDTO
            {
                Id = 1,
                Title = "Valid Title",
            };

            var command = new UpdateHistoricalContextCommand(historicalContext);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

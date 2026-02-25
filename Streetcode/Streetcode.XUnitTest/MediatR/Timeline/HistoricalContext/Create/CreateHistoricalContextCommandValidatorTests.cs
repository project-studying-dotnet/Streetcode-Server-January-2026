namespace Streetcode.XUnitTest.MediatR.Timeline.HistoricalContext.Create
{
    using FluentAssertions;
    using FluentValidation.TestHelper;
    using Streetcode.BLL.DTO.Timeline.HistoricalContext;
    using Streetcode.BLL.MediatR.Timeline.HistoricalContext.Create;
    using Streetcode.Resources;
    using Xunit;

    public class CreateHistoricalContextCommandValidatorTests
    {
        private readonly CreateHistoricalContextCommandValidator validator;

        public CreateHistoricalContextCommandValidatorTests()
        {
            this.validator = new CreateHistoricalContextCommandValidator();
        }

        [Fact]
        public void ShouldReturnError_IfHistoricalContextIsNull()
        {
            // Arrange
            var command = new CreateHistoricalContextCommand(null!);

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
            var historicalContext = new CreateHistoricalContextDTO
            {
                Title = "Valid Title",
            };

            var command = new CreateHistoricalContextCommand(historicalContext);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

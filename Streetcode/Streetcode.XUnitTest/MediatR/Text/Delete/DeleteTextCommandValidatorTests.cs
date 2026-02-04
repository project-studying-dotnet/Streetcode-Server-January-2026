
namespace Streetcode.XUnitTest.MediatR.Text.Delete
{
    using FluentValidation.TestHelper;
    using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
    using Streetcode.BLL.MediatR.Streetcode.Text.Delete;
    using Xunit;

    public class DeleteTextCommandValidatorTests
    {

        private readonly DeleteTextCommandValidator validator;

        public DeleteTextCommandValidatorTests()
        {
            this.validator = new DeleteTextCommandValidator();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_HaveError_IfIdIsInvalid(int id)
        {
            // Arrange
            var command = new DeleteTextCommand(id);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("StreetcodeId must be greater than zero");
        }


        [Fact]
        public void ShouldHaveSuccess_IfCommandIsValid()
        {
            // Arrange
            int id = 1;
            var validDto = new TextBaseDTO
            {
                StreetcodeId = 1,
                Title = "Valid Title",
                TextContent = "Some valid content",
                AdditionalText = null,
            };
            var command = new DeleteTextCommand(id);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

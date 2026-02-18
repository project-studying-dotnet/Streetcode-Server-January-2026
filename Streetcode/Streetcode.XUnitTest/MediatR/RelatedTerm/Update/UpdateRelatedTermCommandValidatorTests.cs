using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Create;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Update;
using Streetcode.Resources;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.RelatedTerm.Update;

public class UpdateRelatedTermCommandValidatorTests
{
    private readonly UpdateRelatedTermCommandValidator validator;

    public UpdateRelatedTermCommandValidatorTests()
    {
        this.validator = new UpdateRelatedTermCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenRelatedTermIsNull()
    {
        // Arrange
        var command = new UpdateRelatedTermCommand(null!);

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdateRelatedTerm)
            .WithErrorMessage(Messages.Error_CommandDataRequired);
    }

    [Fact]
    public void ShouldPass_WhenRelatedTermIsPresent()
    {
        // Arrange
        var updateRelatedTermDTO = new UpdateRelatedTermDTO
        {
            Id = 1,
            Word = "test",
        };

        var command = new UpdateRelatedTermCommand(updateRelatedTermDTO);

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
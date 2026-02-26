using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Create;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Update;
using Streetcode.Resources;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.RelatedTerm.Create;

public class CreateRelatedTermCommandValidatorTests
{
    private readonly CreateRelatedTermCommandValidator validator;

    public CreateRelatedTermCommandValidatorTests()
    {
        this.validator = new CreateRelatedTermCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenRelatedTermIsNull()
    {
        // Arrange
        var command = new CreateRelatedTermCommand(null!);

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CreateRelatedTerm)
            .WithErrorMessage(Messages.Error_CommandDataRequired);
    }

    [Fact]
    public void ShouldPass_WhenRelatedTermIsPresent()
    {
        // Arrange
        var createRelatedTermDTO = new CreateRelatedTermDTO
        {
            Word = "test",
            TermId = 1,
        };

        var command = new CreateRelatedTermCommand(createRelatedTermDTO);

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
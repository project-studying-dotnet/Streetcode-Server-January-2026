using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Update;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.RelatedTerm.Update;

public class UpdateRelatedTermDTOValidatorTests
{
    private readonly UpdateRelatedTermDTOValidator validator;

    public UpdateRelatedTermDTOValidatorTests()
    {
        this.validator = new UpdateRelatedTermDTOValidator();
    }

    [Fact]
    public void ShouldPass_WhenRelatedTermIsValid()
    {
        // Arrange
        var updateRelatedTermDTO = new UpdateRelatedTermDTO
        {
            Id = 1,
            Word = "test",
        };

        // Act
        var result = this.validator.TestValidate(updateRelatedTermDTO);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveError_WhenWordIsNull()
    {
        // Arrange
        var updateRelatedTermDTO = new UpdateRelatedTermDTO
        {
            Id = 1,
            Word = null!,
        };

        // Act
        var result = this.validator.TestValidate(updateRelatedTermDTO);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Word)
            .WithErrorMessage(Messages.Error_PropertyIsRequired.Format(nameof(UpdateRelatedTermDTO.Word)));
    }

    [Fact]
    public void ShouldHaveError_WhenWordExceedsFiftyCharacters()
    {
        // Arrange
        var updateRelatedTermDTO = new UpdateRelatedTermDTO
        {
            Id = 1,
            Word = new string('a', 51),
        };

        // Act
        var result = this.validator.TestValidate(updateRelatedTermDTO);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Word)
            .WithErrorMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(
                nameof(UpdateRelatedTermDTO.Word),
                50));
    }

    [Fact]
    public void ShouldHaveError_WhenIdIsLessThanOne()
    {
        // Arrange
        var updateRelatedTermDTO = new UpdateRelatedTermDTO
        {
            Id = 0,
            Word = "test",
        };

        // Act
        var result = this.validator.TestValidate(updateRelatedTermDTO);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(
                nameof(UpdateRelatedTermDTO.Id)));
    }
}
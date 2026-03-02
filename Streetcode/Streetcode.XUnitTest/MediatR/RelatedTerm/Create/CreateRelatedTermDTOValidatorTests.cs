using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Create;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;
using Xunit;

namespace Streetcode.XUnitTest.MediatR.RelatedTerm.Create;

public class CreateRelatedTermDTOValidatorTests
{
    private readonly CreateRelatedTermDTOValidator validator;

    public CreateRelatedTermDTOValidatorTests()
    {
        this.validator = new CreateRelatedTermDTOValidator();
    }

    [Fact]
    public void ShouldPass_WhenRelatedTermIsValid()
    {
        // Arrange
        var createRelatedTermDTO = new CreateRelatedTermDTO
        {
            Word = "test",
            TermId = 1,
        };

        // Act
        var result = this.validator.TestValidate(createRelatedTermDTO);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveError_WhenWordIsNull()
    {
        // Arrange
        var createRelatedTermDTO = new CreateRelatedTermDTO
        {
            Word = null!,
            TermId = 1,
        };

        // Act
        var result = this.validator.TestValidate(createRelatedTermDTO);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Word)
            .WithErrorMessage(Messages.Error_PropertyIsRequired.Format(nameof(CreateRelatedTermDTO.Word)));
    }

    [Fact]
    public void ShouldHaveError_WhenWordExceedsFiftyCharacters()
    {
        // Arrange
        var createRelatedTermDTO = new CreateRelatedTermDTO
        {
            Word = new string('a', 51),
            TermId = 1,
        };

        // Act
        var result = this.validator.TestValidate(createRelatedTermDTO);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Word)
            .WithErrorMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(
                nameof(CreateRelatedTermDTO.Word),
                50));
    }

    [Fact]
    public void ShouldHaveError_WhenTermIdIsLessThanOne()
    {
        // Arrange
        var createRelatedTermDTO = new CreateRelatedTermDTO
        {
            Word = "test",
            TermId = 0,
        };

        // Act
        var result = this.validator.TestValidate(createRelatedTermDTO);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TermId)
            .WithErrorMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(
                nameof(CreateRelatedTermDTO.TermId)));
    }
}
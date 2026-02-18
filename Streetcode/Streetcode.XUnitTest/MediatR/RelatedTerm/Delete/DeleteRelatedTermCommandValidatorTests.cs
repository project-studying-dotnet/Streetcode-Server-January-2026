using FluentValidation.TestHelper;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Delete;
using Streetcode.Resources;
using Xunit;
using Streetcode.Shared.Extensions;

namespace Streetcode.XUnitTest.MediatR.RelatedTerm.Delete;

public class DeleteRelatedTermCommandValidatorTests
{
    private readonly DeleteRelatedTermCommandValidator validator;

    public DeleteRelatedTermCommandValidatorTests()
    {
        this.validator = new DeleteRelatedTermCommandValidator();
    }

    [Fact]
    public void ShouldPass_WhenRelatedTermIsValid()
    {
        // Arrange
        var command = new DeleteRelatedTermCommand("test", 1);

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveError_WhenWordIsNull()
    {
        // Arrange
        var command = new DeleteRelatedTermCommand(null!, 1);

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Word)
            .WithErrorMessage(Messages.Error_PropertyIsRequired.Format(nameof(DeleteRelatedTermCommand.Word)));
    }

    [Fact]
    public void ShouldHaveError_WhenWordExceedsFiftyCharacters()
    {
        // Arrange
        var command = new DeleteRelatedTermCommand(new string('a', 51), 1);

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Word)
            .WithErrorMessage(Messages.Error_PropertyMustNotExceedCharacters.Format(
                nameof(DeleteRelatedTermCommand.Word),
                50));
    }

    [Fact]
    public void ShouldHaveError_WhenTermIdIsLessThanOne()
    {
        // Arrange
        var command = new DeleteRelatedTermCommand("test", 0);

        // Act
        var result = this.validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TermId)
            .WithErrorMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(
                nameof(DeleteRelatedTermCommand.TermId)));
    }
}
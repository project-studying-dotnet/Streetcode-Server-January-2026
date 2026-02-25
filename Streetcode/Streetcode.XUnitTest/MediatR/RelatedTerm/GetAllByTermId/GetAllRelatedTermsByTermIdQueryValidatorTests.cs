using FluentValidation.TestHelper;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.GetAllByTermId;
using Streetcode.Resources;
using Xunit;
using Streetcode.Shared.Extensions;

namespace Streetcode.XUnitTest.MediatR.RelatedTerm.GetAllByTermId;

public class GetAllRelatedTermsByTermIdQueryValidatorTests
{
    private readonly GetAllRelatedTermsByTermIdQueryValidator validator;

    public GetAllRelatedTermsByTermIdQueryValidatorTests()
    {
        this.validator = new GetAllRelatedTermsByTermIdQueryValidator();
    }

    [Fact]
    public void ShouldPass_WhenRelatedTermIsValid()
    {
        // Arrange
        var query = new GetAllRelatedTermsByTermIdQuery(1);

        // Act
        var result = this.validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveError_WhenTermIdIsLessThanOne()
    {
        // Arrange
        var query = new GetAllRelatedTermsByTermIdQuery(0);

        // Act
        var result = this.validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TermId)
            .WithErrorMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(
                nameof(GetAllRelatedTermsByTermIdQuery.TermId)));
    }
}
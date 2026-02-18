using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.XUnitTest.MediatR.Fact.UpdateOrder
{
    using FluentValidation.TestHelper;
    using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
    using Streetcode.BLL.MediatR.Streetcode.Fact.UpdateOrder;
    using Xunit;

    public class UpdateOrderFactDTOListValidatorTests
    {
        private readonly UpdateOrderFactDTOListValidator validator;

        public UpdateOrderFactDTOListValidatorTests()
        {
            this.validator = new UpdateOrderFactDTOListValidator();
        }

        [Fact]
        public void ShouldHaveError_WhenListIsEmpty()
        {
            // Arrange
            var model = new List<UpdateFactOrderDTO>();

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x)
                  .WithErrorMessage(Messages.Error_FactsListEmpty);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ShouldHaveError_WhenItemIdIdInvalid(int invalidId)
        {
            // Arrange
            var model = new List<UpdateFactOrderDTO>
            {
                new () { Id = 1, Order = 1 },
                new () { Id = invalidId, Order = 2 },
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor($"x[{1}].TermId")
                  .WithErrorMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(UpdateFactOrderDTO.Id)));
        }

        [Fact]
        public void ShouldHaveError_WhenItemOrderIsInvalid()
        {
            // Arrange
            var model = new List<UpdateFactOrderDTO>
            {
                new () { Id = 1, Order = -5 },
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor("x[0].Order")
                  .WithErrorMessage(Messages.Error_PropertyMustBeEqualOrGreaterThanZero.Format(nameof(UpdateFactOrderDTO.Order)));
        }

        [Fact]
        public void ShouldBeValid_WhenListAndItemsAreCorrect()
        {
            // Arrange
            var model = new List<UpdateFactOrderDTO>
            {
                new () { Id = 1, Order = 1 },
                new () { Id = 2, Order = 0 },
                new () { Id = 3, Order = 100 },
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
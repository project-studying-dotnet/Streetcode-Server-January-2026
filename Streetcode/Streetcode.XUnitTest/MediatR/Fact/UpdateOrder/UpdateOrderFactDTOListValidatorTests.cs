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
                  .WithErrorMessage("Facts list cannot be empty.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ShouldHaveError_WhenItemIdIdInvalid(int invalidId)
        {
            // Arrange
            var model = new List<UpdateFactOrderDTO>
            {
                new() { Id = 1, Order = 1 },
                new () { Id = invalidId, Order = 2 },
            };

            // Act
            var result = this.validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor($"x[{1}].Id")
                  .WithErrorMessage("Fact Id must be greater than 0.");
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
                  .WithErrorMessage("Order must be 0 or greater.");
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
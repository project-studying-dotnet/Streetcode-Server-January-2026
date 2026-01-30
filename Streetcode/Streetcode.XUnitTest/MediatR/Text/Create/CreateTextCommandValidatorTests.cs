// <copyright file="CreateTextCommandValidatorTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Streetcode.XUnitTest.MediatR.Text.Create
{
    using FluentValidation.TestHelper;
    using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
    using Streetcode.BLL.MediatR.Streetcode.Entity.Create;
    using Xunit;

    public class CreateTextCommandValidatorTests
    {
        private readonly CreateTextCommandValidator validator;

        public CreateTextCommandValidatorTests()
        {
            this.validator = new CreateTextCommandValidator();
        }

        [Fact]
        public void ShouldHaveError_WhenTextIsNull()
        {
            // Arrange
            var command = new CreateTextCommand(null!);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Text)
                  .WithErrorMessage("TextDataRequired");
        }

        [Fact]
        public void ShouldPass_WhenTextIsValid()
        {
            // Arrange
            var dto = new TextBaseDTO
            {
                Title = "Test title",
                TextContent = "Some content",
                AdditionalText = "Additional",
                StreetcodeId = 123,
            };

            var command = new CreateTextCommand(dto);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

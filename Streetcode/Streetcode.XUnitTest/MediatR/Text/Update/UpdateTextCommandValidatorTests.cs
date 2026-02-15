// <copyright file="UpdateTextCommandValidatorTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.XUnitTest.MediatR.Text.Update
{
    using FluentValidation.TestHelper;
    using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
    using Streetcode.BLL.MediatR.Streetcode.Text.Update;
    using Xunit;

    public class UpdateTextCommandValidatorTests
    {
        private readonly UpdateTextCommandValidator validator;

        public UpdateTextCommandValidatorTests()
        {
            this.validator = new UpdateTextCommandValidator();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_HaveError_IfIdIsInvalid(int id)
        {
            // Arrange
            var model = new TextUpdateDTO { StreetcodeId = id };
            var command = new UpdateTextCommand(model);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Text.StreetcodeId)
                  .WithErrorMessage(Messages.Error_PropertyMustBeGreaterThanZero.Format(nameof(TextUpdateDTO.StreetcodeId)));
        }

        [Fact]
        public void ShouldHaveError_IfTextIsNull()
        {
            // Arrange
            var model = new TextUpdateDTO { StreetcodeId = 1, TextContent = null!};
            var command = new UpdateTextCommand(model);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Text.TextContent)
                  .WithErrorMessage(Messages.Error_PropertyIsRequired.Format(nameof(TextUpdateDTO.TextContent)));
        }

        [Fact]
        public void ShouldHaveError_IfChildValidatorFails()
        {
            // Arrange
            var invalidDTO = new TextUpdateDTO
            {
                StreetcodeId = 1,
                Title = "123",
                TextContent = string.Empty,
            };

            var command = new UpdateTextCommand(invalidDTO);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Text.TextContent);
        }

        [Fact]
        public void ShouldHaveSuccess_IfCommandIsValid()
        {
            // Arrange
            var validDto = new TextUpdateDTO
            {
                Id = 1,
                StreetcodeId = 1,
                Title = "Valid Title",
                TextContent = "Some valid content",
                AdditionalText = null,
            };
            var command = new UpdateTextCommand(validDto);

            // Act
            var result = this.validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

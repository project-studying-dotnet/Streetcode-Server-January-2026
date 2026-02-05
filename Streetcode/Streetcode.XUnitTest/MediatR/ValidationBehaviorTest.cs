namespace Streetcode.XUnitTest.MediatR
{
    using FluentAssertions;
    using FluentResults;
    using FluentValidation;
    using FluentValidation.Results;
    using global::MediatR;
    using Moq;
    using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
    using Streetcode.BLL.MediatR;
    using Streetcode.BLL.MediatR.Streetcode.Text.Create;
    using Xunit;

    public class ValidationBehaviorTest
    {
        [Fact]
        public async Task Handle_ShouldReturnFailResult_WhenValidationFails()
        {
            // Arrange
            var request = new CreateTextCommand(new TextCreateDTO { Title = "" });
            var failures = new List<ValidationFailure> { new("Title", "Title is required") };

            var validatorMock = new Mock<IValidator<CreateTextCommand>>();
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CreateTextCommand>>(), default))
                         .ReturnsAsync(new ValidationResult(failures));

            var behavior = new ValidatorBehavior<CreateTextCommand, Result<TextDTO>>(new[] { validatorMock.Object });

            var nextDelegateMock = new Mock<RequestHandlerDelegate<Result<TextDTO>>>();

            // Act
            var result = await behavior.Handle(request, nextDelegateMock.Object, TestContext.Current.CancellationToken);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == "Title is required");
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccessResult_WhenValidationSucceeds()
        {
            // Arrange
            var request = new CreateTextCommand(new TextCreateDTO 
            {
                Title = "string",
                TextContent = "string",
                StreetcodeId = 1,
            });

            var failures = new List<ValidationFailure> { new("Title", "Title is required") };

            var validatorMock = new Mock<IValidator<CreateTextCommand>>();
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CreateTextCommand>>(), default))
                         .ReturnsAsync(new ValidationResult(failures));

            var behavior = new ValidatorBehavior<CreateTextCommand, Result<TextDTO>>(new[] { validatorMock.Object });

            var nextDelegateMock = new Mock<RequestHandlerDelegate<Result<TextDTO>>>();

            // Act
            var result = await behavior.Handle(request, nextDelegateMock.Object, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
    }
}

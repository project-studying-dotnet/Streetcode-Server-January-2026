// <copyright file="GetParsedTextAdminPreviewHandlerTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Streetcode.XUnitTest.MediatR.Text.GetParsed
{
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Streetcode.BLL;
    using Streetcode.BLL.Interfaces.Text;
    using Streetcode.BLL.MediatR.Streetcode.Entity.GetParsed;
    using Xunit;

    public class GetParsedTextAdminPreviewHandlerTests
    {
        private readonly Mock<ITextService> mockTextService;
        private readonly GetParsedTextAdminPreviewHandler handler;

        public GetParsedTextAdminPreviewHandlerTests()
        {
            mockTextService = new Mock<ITextService>();
            handler = new GetParsedTextAdminPreviewHandler(mockTextService.Object);
        }

        [Fact]
        public async Task Handler_ShouldReturnTrue_IfTextIsParsed()
        {
            // Arrange
            const string textToParse = "Sample text to parse";
            const string parsedText = "<p>Sample text to parse</p>";
            var query = new GetParsedTextForAdminPreviewCommand(textToParse);

            mockTextService
                .Setup(s => s.AddTermsTag(textToParse))
                .ReturnsAsync(parsedText);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(parsedText);
        }

        [Fact]
        public async Task Handler_ShouldReturnFail_IfTextServiceFails()
        {
            // Arrange
            var text = "Sample text to parse";
            string errorMsg = "text was not parsed successfully";
            var query = new GetParsedTextForAdminPreviewCommand(text);

            mockTextService
                .Setup(s => s.AddTermsTag(text))
                .Returns(Task.FromResult<string?>(null));

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(s => s.Message == errorMsg);
        }
    }
}

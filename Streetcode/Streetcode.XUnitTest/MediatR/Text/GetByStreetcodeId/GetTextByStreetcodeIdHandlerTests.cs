// <copyright file="GetTextByStreetcodeIdHandlerTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore.Query;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.XUnitTest.MediatR.Text.GetByStreetcodeId
{
    using System.Linq.Expressions;
    using AutoMapper;
    using FluentAssertions;
    using Moq;
    using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.Interfaces.Text;
    using Streetcode.BLL.MediatR.Streetcode.Text.GetByStreetcodeId;
    using Streetcode.DAL.Entities.Streetcode;
    using Streetcode.DAL.Entities.Streetcode.TextContent;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Xunit;

    public class GetTextByStreetcodeIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> mockRepoWrapper;
        private readonly Mock<IMapper> mockMapper;
        private readonly Mock<ILoggerService> mockLogger;
        private readonly Mock<ITextService> mockTextService;
        private readonly GetTextByStreetcodeIdHandler handler;

        public GetTextByStreetcodeIdHandlerTests()
        {
            this.mockRepoWrapper = new Mock<IRepositoryWrapper>();
            this.mockMapper = new Mock<IMapper>();
            this.mockLogger = new Mock<ILoggerService>();
            this.mockTextService = new Mock<ITextService>();

            this.handler = new GetTextByStreetcodeIdHandler(
                this.mockRepoWrapper.Object,
                this.mockMapper.Object,
                this.mockTextService.Object,
                this.mockLogger.Object);
        }

        [Fact]
        public async Task Handler_ShouldReturnOk_WhenTextExists()
        {
            // Arrange
            int streetcodeId = 123;
            var text = new Text { Id = 123, TextContent = "content", StreetcodeId = streetcodeId };
            var textDTO = new TextDTO { Id = 123, TextContent = "parsed" };
            var query = new GetTextByStreetcodeIdQuery(streetcodeId);

            this.mockRepoWrapper
                .Setup(r => r.TextRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Text, bool>>>(),
                    It.IsAny<Func<IQueryable<Text>, IIncludableQueryable<Text, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(text);

            this.mockTextService
                .Setup(t => t.AddTermsTag(It.IsAny<string>()))
                .ReturnsAsync("parsed");

            this.mockMapper
                .Setup(m => m.Map<TextDTO?>(text))
                .Returns(textDTO);

            // Act
            var result = await this.handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.TextContent.Should().BeEquivalentTo("parsed");
        }

        [Fact]
        public async Task Handle_ShouldReturnOk_WhenTextExists_ButStreedCodeNoMatter()
        {
            // Arrange
            int streetcodeId = 123;
            var text = new Text { Id = 1, StreetcodeId = streetcodeId };
            var textDTO = new TextDTO { Id = 1 };

            this.mockRepoWrapper
                .Setup(r => r.TextRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Text, bool>>>(),
                    It.IsAny<Func<IQueryable<Text>, IIncludableQueryable<Text, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(text);

            this.mockTextService
                .Setup(t => t.AddTermsTag(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            this.mockMapper
                .Setup(m => m.Map<TextDTO?>(text))
                .Returns(textDTO);

            // Act
            var result = await this.handler.Handle(new GetTextByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(1)]
        public async Task Handler_ShouldReturnFail_IfTextNotExists(int streetcodeId)
        {
            // Arrange
            var query = new GetTextByStreetcodeIdQuery(streetcodeId);
            var errorMsg = Messages.Error_EntityWithStreetcodeIdNotFound.Format(nameof(Text), streetcodeId);

            this.mockRepoWrapper
                .Setup(r => r.TextRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Text, bool>>>(),
                    It.IsAny<Func<IQueryable<Text>, IIncludableQueryable<Text, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((Text?)null);

            // Act
            var result = await this.handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == errorMsg);
        }
    }
}
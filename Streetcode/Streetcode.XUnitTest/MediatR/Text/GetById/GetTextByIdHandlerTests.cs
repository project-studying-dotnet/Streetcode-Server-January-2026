// <copyright file="GetTextByIdHandlerTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Streetcode.XUnitTest.MediatR.Text.GetById
{
    using System.Linq.Expressions;
    using AutoMapper;
    using FluentAssertions;
    using Moq;
    using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.MediatR.Streetcode.Text.GetById;
    using Streetcode.DAL.Entities.Streetcode.TextContent;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Xunit;

    public class GetTextByIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> mockRepoWrapper;
        private readonly Mock<IMapper> mockMapper;
        private readonly Mock<ILoggerService> mockLogger;
        private readonly GetTextByIdHandler handler;

        public GetTextByIdHandlerTests()
        {
            this.mockRepoWrapper = new Mock<IRepositoryWrapper>();
            this.mockMapper = new Mock<IMapper>();
            this.mockLogger = new Mock<ILoggerService>();

            this.handler = new GetTextByIdHandler(
                this.mockRepoWrapper.Object,
                this.mockMapper.Object,
                this.mockLogger.Object);
        }

        [Fact]
        public async Task Handler_ShouldReturnOk_WhenTextExists()
        {
            // Arrange
            int id = 123;
            var text = new Text { Id = id, Title = "content" };
            var textDTO = new TextDTO { Id = id, Title = "content" };
            var query = new GetTextByIdQuery(id);

            this.mockRepoWrapper
                .Setup(r => r.TextRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Text, bool>>>(), null))
                .ReturnsAsync(text);

            this.mockMapper
                .Setup(m => m.Map<TextDTO>(text))
                .Returns(textDTO);

            // Act
            var result = await this.handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(textDTO);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(1)]
        public async Task Handle_ShouldReturnFail_IfContentNotExists(int id)
        {
            // Arrange
            var query = new GetTextByIdQuery(id);
            string errorMsg = $"Cannot find any text with corresponding id: {id}";

            this.mockRepoWrapper
                .Setup(r => r.TextRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Text, bool>>>(), null))
                .ReturnsAsync((Text?)null);

            // Act
            var result = await this.handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Contain(errorMsg);

            this.mockLogger.Verify(l => l.LogError(It.IsAny<object>(), errorMsg), Times.Once);
        }
    }
}

// <copyright file="DeleteTextHandlerTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore.Query;

namespace Streetcode.XUnitTest.MediatR.Text.Delete
{
    using FluentAssertions;
    using FluentResults;
    using global::MediatR;
    using Moq;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.MediatR.Streetcode.Text.Delete;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;
    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using TextEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Text;

    public class DeleteTextHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly DeleteTextHandler handler;

        public DeleteTextHandlerTests()
        {
            this.repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            this.loggerMock = new Mock<ILoggerService>();
            this.handler = new DeleteTextHandler(
                this.repositoryWrapperMock.Object,
                this.loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WhenTextNotFound_ShouldReturnFailure()
        {
            // Arrange
            int id = 1;
            string errorMsg = $"No text found with Id {id}";
            var textRepoMock = new Mock<ITextRepository>(MockBehavior.Strict);

            this.repositoryWrapperMock
                .Setup(r => r.TextRepository)
                .Returns(textRepoMock.Object);

            textRepoMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TextEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TextEntity>, IIncludableQueryable<TextEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((TextEntity)null!);

            var command = new DeleteTextCommand(1);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be(errorMsg);
            textRepoMock.Verify(
                r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TextEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TextEntity>, IIncludableQueryable<TextEntity, object>>>(),
                    It.IsAny<bool>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenTextFound_ShouldReturnOk()
        {
            // Arrange
            var textEntity = new TextEntity { Id = 1, TextContent = "Sample content" };

            var textRepoMock = new Mock<ITextRepository>(MockBehavior.Strict);

            this.repositoryWrapperMock
                .Setup(r => r.TextRepository)
                .Returns(textRepoMock.Object);

            textRepoMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TextEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TextEntity>, IIncludableQueryable<TextEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(textEntity);
            textRepoMock.Setup(r => r.Delete(It.IsAny<TextEntity>()));

            this.repositoryWrapperMock
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            var command = new DeleteTextCommand(1);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Should().BeOfType<Result<Unit>>();

            textRepoMock.Verify(
                r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TextEntity>, IIncludableQueryable<TextEntity, object>>>(),
                It.IsAny<bool>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenSaveChangesFails_ShouldReturnFailure()
        {
            // Arrange
            string errorMsg = "Error while saving changes to database";
            var textEntity = new TextEntity { Id = 1, TextContent = "Sample content" };

            var textRepoMock = new Mock<ITextRepository>(MockBehavior.Strict);

            this.repositoryWrapperMock
                .Setup(r => r.TextRepository)
                .Returns(textRepoMock.Object);

            textRepoMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TextEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TextEntity>, IIncludableQueryable<TextEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(textEntity);
            textRepoMock.Setup(r => r.Delete(It.IsAny<TextEntity>()));

            this.repositoryWrapperMock
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(0);

            var command = new DeleteTextCommand(1);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Be(errorMsg);
        }
    }
}

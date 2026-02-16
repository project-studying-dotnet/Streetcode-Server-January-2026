// <copyright file="UpdateTextHandlerTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.XUnitTest.MediatR.Text.Update
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AutoMapper;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore.Query;
    using Moq;
    using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.MediatR.Streetcode.Text.Update;
    using Streetcode.DAL.Entities.Streetcode.TextContent;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;
    using Xunit;
    using TextEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Text;

    public class UpdateTextHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepoWrapper;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly UpdateTextHandler _handler;

        public UpdateTextHandlerTests()
        {
            _mockRepoWrapper = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();

            _handler = new UpdateTextHandler(
                _mockRepoWrapper.Object,
                _mockMapper.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_WhenTextNotFound_ShouldReturnFail()
        {
            // Arrange
            var id = 1;
            var command = new UpdateTextCommand(new TextUpdateDTO { Id = id });
            string expectedErrorMsg = Messages.Error_EntityWithIdNotFound.Format(nameof(Text), id);

            _mockRepoWrapper
                .Setup(r => r.TextRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Text, Text>>>(),
                    It.IsAny<Expression<Func<Text, bool>>>(),
                    It.IsAny<Func<IQueryable<Text>, IIncludableQueryable<Text, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((Text?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == expectedErrorMsg);
            _mockLogger.Verify(x => x.LogError(It.IsAny<object>(), expectedErrorMsg), Times.Once);
            _mockRepoWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenSaveChangesFails_ShouldReturnFail()
        {
            // Arrange
            var streetcodeId = 1;
            var updateDto = new TextUpdateDTO { StreetcodeId = streetcodeId };
            var existingText = new Text { Id = streetcodeId };
            var command = new UpdateTextCommand(updateDto);

            _mockRepoWrapper
                .Setup(r => r.TextRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Text, Text>>>(),
                    It.IsAny<Expression<Func<Text, bool>>>(),
                    It.IsAny<Func<IQueryable<Text>, IIncludableQueryable<Text, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(existingText);

            _mockRepoWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WhenUpdateSuccessful_ShouldReturnUpdatedDto()
        {
            // Arrange
            var textRepoMock = new Mock<ITextRepository>(MockBehavior.Strict);
            var existing = new TextEntity { Id = 10, AdditionalText = "Old" };
            var update = new TextUpdateDTO { AdditionalText = "New!" };
            var mapped = new TextEntity { Id = 10, AdditionalText = "New!" };
            var mappedDto = new TextDTO { Id = 10, AdditionalText = "New!" };

            _mockRepoWrapper
                .Setup(r => r.TextRepository)
                .Returns(textRepoMock.Object);

            textRepoMock
                .Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TextEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TextEntity>, IIncludableQueryable<TextEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(existing);

            _mockMapper.Setup(m => m.Map(update, existing))
                .Returns(mapped);
            textRepoMock.Setup(r => r.Update(mapped))
                .Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TextEntity>)null);
            _mockRepoWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<TextDTO>(mapped))
                .Returns(mappedDto);

            var command = new UpdateTextCommand(update);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(mappedDto);
        }
    }
}

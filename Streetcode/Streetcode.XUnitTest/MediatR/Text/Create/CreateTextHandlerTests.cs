// <copyright file="CreateTextHandlerTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Streetcode.XUnitTest.MediatR.Text.Create
{
    using AutoMapper;
    using FluentAssertions;
    using Moq;
    using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
    using Streetcode.BLL.Interfaces.Logging;
    using Streetcode.BLL.MediatR.Streetcode.Entity.Create;
    using Streetcode.DAL.Repositories.Interfaces.Base;
    using Xunit;
    using TextEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Text;

    public class CreateTextHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<IMapper> mapperMock;
        private readonly Mock<ILoggerService> loggerMock;
        private readonly CreateTextHandler handler;

        public CreateTextHandlerTests()
        {
            this.repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            this.mapperMock = new Mock<IMapper>();
            this.loggerMock = new Mock<ILoggerService>();
            this.handler = new CreateTextHandler(
                this.repositoryWrapperMock.Object,
                this.mapperMock.Object,
                this.loggerMock.Object);
        }

        [Fact]
        public async Task Handle_IfCreateSuccessful_ShouldReturnCreatedDTO()
        {
            // Arrange
            var textCreateDTO = new TextBaseDTO { Title = "Test Title", TextContent = "Test Content" };
            var entityText = new TextEntity { Id = 1, Title = "Test Title" };
            var expectedTextDto = new TextDTO { Id = 1, Title = "Test Title" };

            var command = new CreateTextCommand(textCreateDTO);


            mapperMock.Setup(m => m.Map<TextEntity>(It.IsAny<TextBaseDTO>()))
                       .Returns(entityText);

            repositoryWrapperMock.Setup(r => r.TextRepository.CreateAsync(It.IsAny<TextEntity>()))
                                   .ReturnsAsync(entityText);

            repositoryWrapperMock.Setup(r => r.SaveChangesAsync())
                                   .ReturnsAsync(1);

            mapperMock.Setup(m => m.Map<TextDTO>(It.IsAny<TextEntity>()))
                       .Returns(expectedTextDto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedTextDto);

            loggerMock.Verify(x => x.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_IfSaveChangesFails_ShouldReturnFail()
        {
            // Arrange
            string errorMsg = "Error while saving changes to database";
            var textCreateDTO = new TextBaseDTO { Title = "Test Title" };
            var command = new CreateTextCommand(textCreateDTO);

            var entityText = new TextEntity { Id = 1, Title = "Test Title" };

            mapperMock.Setup(m => m.Map<TextEntity>(It.IsAny<TextBaseDTO>()))
                       .Returns(entityText);

            repositoryWrapperMock.Setup(r => r.TextRepository.CreateAsync(It.IsAny<TextEntity>()))
                                   .ReturnsAsync(entityText);

            repositoryWrapperMock.Setup(r => r.SaveChangesAsync())
                                   .ReturnsAsync(0);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == errorMsg);

            mapperMock.Verify(m => m.Map<TextDTO>(It.IsAny<TextEntity>()), Times.Never);

        }
    }
}
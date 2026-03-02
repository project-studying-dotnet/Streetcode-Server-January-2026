

namespace Streetcode.Email.XUnitTest.MediatR.Feedback
{
    using AutoMapper;
    using FluentAssertions;
    using Hangfire;
    using Hangfire.Common;
    using Hangfire.States;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Streetcode.Email.BLL.DTO;
    using Streetcode.Email.BLL.Interfaces;
    using Streetcode.Email.BLL.MediatR.Email;
    using Streetcode.Email.DAL.Persistence;
    using Streetcode.Email.BLL.Mapping;
    using Streetcode.Resources;
    public class SendFeedbackHandlerTests : IDisposable
    {
        private readonly EmailDbContext dbContext;
        private readonly Mock<ILogger<SendEmailHandler>> mockLogger;
        private readonly Mock<IBackgroundJobClient> mockBackgroundJob;
        private readonly IMapper mapper;
        private readonly SendEmailHandler handler;

        public SendFeedbackHandlerTests()
        {
            var options = new DbContextOptionsBuilder<EmailDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            this.dbContext = new EmailDbContext(options);
            this.mockLogger = new Mock<ILogger<SendEmailHandler>>();
            this.mockBackgroundJob = new Mock<IBackgroundJobClient>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new EmailProfile());
            });
            this.mapper = new Mapper(configuration);

            this.handler = new SendEmailHandler(
                this.dbContext,
                this.mapper,
                this.mockLogger.Object,
                this.mockBackgroundJob.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnOk_WhenFeedbackIsSavedSuccessfully()
        {
            // Arrange
            var feedbackDto = new EmailDTO { Email = "test@gmail.com", Message = "Valid message" };
            var command = new SendEmailCommand(feedbackDto);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            this.mockBackgroundJob.Verify(x => x.Create(
                It.Is<Job>(j => j.Method.Name == nameof(IEmailService.SendEmailAsync)),
                It.IsAny<EnqueuedState>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_IfDatabaseSaveFails()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<EmailDbContext>()
                .UseInMemoryDatabase(databaseName: "FailDatabase")
                .Options;

            var mockContext = new Mock<EmailDbContext>(options) { CallBase = true };

            mockContext
                .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var failHandler = new SendEmailHandler(
                mockContext.Object,
                this.mapper,
                this.mockLogger.Object,
                this.mockBackgroundJob.Object);

            var command = new SendEmailCommand(new EmailDTO { Email = "fail@test.com", Message = "fail message" });

            // Act
            var result = await failHandler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == Messages.Error_FailedToCreateEntity);

            this.mockBackgroundJob.Verify(x => x.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()), Times.Never);
        }

        public void Dispose()
        {
            this.dbContext.Database.EnsureDeleted();
            this.dbContext.Dispose();
        }
    }
}
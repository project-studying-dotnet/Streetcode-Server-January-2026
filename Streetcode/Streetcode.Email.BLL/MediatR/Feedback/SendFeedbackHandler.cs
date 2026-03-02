using AutoMapper;
using FluentResults;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using Streetcode.Email.BLL.DTO;
using Streetcode.Email.BLL.Interfaces;
using Streetcode.Email.DAL.Persistence;
using Streetcode.Resources;
using FeedbackEntity = Streetcode.Email.DAL.Entities.Email;

namespace Streetcode.Email.BLL.MediatR.Feedback
{
    public class SendFeedbackHandler : IRequestHandler<SendFeedbackCommand, Result<Unit>>
    {
        private readonly EmailDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<SendFeedbackHandler> _logger;
        private readonly IBackgroundJobClient _backgroundJob;

        public SendFeedbackHandler(EmailDbContext context, IMapper mapper, ILogger<SendFeedbackHandler> logger, IBackgroundJobClient backgroundJob)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _backgroundJob = backgroundJob;
        }

        public async Task<Result<Unit>> Handle(SendFeedbackCommand request, CancellationToken cancellationToken)
        {
            var feedbackEntity = _mapper.Map<FeedbackEntity>(request.Feedback);

            _context.Feedbacks.Add(feedbackEntity);

            var rowsAffected = await _context.SaveChangesAsync(cancellationToken);

            if (rowsAffected <= 0)
            {
                var errorMsg = Messages.Error_FailedToCreateEntity;
                _logger.LogError(errorMsg);
                return Result.Fail(errorMsg);
            }

            _backgroundJob.Enqueue<IEmailService>(emailService =>
            emailService.SendEmailAsync(request.Feedback));

            _logger.LogInformation("Feedback saved to DB and email task enqueued for {Email}", request.Feedback.Email);

            return Result.Ok(Unit.Value);
        }
    }
}

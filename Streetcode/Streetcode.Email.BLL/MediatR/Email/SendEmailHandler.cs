using AutoMapper;
using FluentResults;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using Streetcode.Email.BLL.DTO;
using Streetcode.Email.BLL.Interfaces;
using Streetcode.Email.DAL.Persistence;
using Streetcode.Resources;
using EmailEntity = Streetcode.Email.DAL.Entities.Email;

namespace Streetcode.Email.BLL.MediatR.Email
{
    public class SendEmailHandler : IRequestHandler<SendEmailCommand, Result<Unit>>
    {
        private readonly EmailDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<SendEmailHandler> _logger;
        private readonly IBackgroundJobClient _backgroundJob;

        public SendEmailHandler(EmailDbContext context, IMapper mapper, ILogger<SendEmailHandler> logger, IBackgroundJobClient backgroundJob)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _backgroundJob = backgroundJob;
        }

        public async Task<Result<Unit>> Handle(SendEmailCommand request, CancellationToken cancellationToken)
        {
            var feedbackEntity = _mapper.Map<EmailEntity>(request.email);

            _context.Emails.Add(feedbackEntity);

            var rowsAffected = await _context.SaveChangesAsync(cancellationToken);

            if (rowsAffected <= 0)
            {
                var errorMsg = Messages.Error_FailedToCreateEntity;
                _logger.LogError(errorMsg);
                return Result.Fail(errorMsg);
            }

            _backgroundJob.Enqueue<IEmailService>(emailService =>
                emailService.SendEmailAsync(request.email));

            _logger.LogInformation("Email saved to DB and email task enqueued for {Email}", request.email.From);

            return Result.Ok(Unit.Value);
        }
    }
}

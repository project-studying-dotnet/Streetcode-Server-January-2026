using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Email;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.AdditionalContent.Email;
using Streetcode.Resources;
using Streetcode.Shared;

namespace Streetcode.BLL.MediatR.Email
{
    public class SendEmailHandler : IRequestHandler<SendEmailCommand, Result<Unit>>
    {
        private readonly IEmailService _emailService;
        private readonly ILoggerService _logger;

        public SendEmailHandler(IEmailService emailService, ILoggerService logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(SendEmailCommand request, CancellationToken cancellationToken)
        {
            var message = new Message(
                [Constants.StreetcodeContacts.Email],
                request.Email.Email,
                "FeedBack",
                request.Email.Message);

            var isResultSuccess = await _emailService.SendEmailAsync(message);

            if (isResultSuccess)
            {
                return Result.Ok(Unit.Value);
            }

            var errorMsg = Messages.Error_FailedToSendEmail;
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}

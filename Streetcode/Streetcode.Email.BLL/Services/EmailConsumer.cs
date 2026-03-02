using MassTransit;
using MediatR;
using Streetcode.Email.BLL.DTO;
using Streetcode.Email.BLL.MediatR.Email;
using Streetcode.Shared.Contracts;

namespace Streetcode.Email.BLL.Services
{
    public class EmailConsumer : IConsumer<IEmailMessage>
    {
        private readonly IMediator _mediator;
        public EmailConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<IEmailMessage> context)
        {
            var message = context.Message;

            var email = new EmailDTO
            {
                From = message.From,
                Content = message.Content
            };

            var command = new SendEmailCommand(email);

            await _mediator.Send(command);
        }
    }
}


using MassTransit;
using MediatR;
using Streetcode.Email.BLL.DTO;
using Streetcode.Email.BLL.MediatR.Feedback;
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

            var feedback = new EmailDTO
            {
                Email = message.Email,
                Message = message.Message
            };

            var command = new SendFeedbackCommand(feedback);

            await _mediator.Send(command);
        }
    }
}


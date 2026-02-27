using MassTransit;
using MediatR;
using Streetcode.Email.BLL.MediatR.Feedback;

namespace Streetcode.Email.BLL.Services
{
    public class EmailConsumer : IConsumer<SendFeedbackCommand>
    {
        private readonly IMediator _mediator;
        public EmailConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task Consume(ConsumeContext<SendFeedbackCommand> context)
        {
            await _mediator.Send(context.Message);
        }
    }
}


using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Email;
using Streetcode.Shared.Contracts;

namespace Streetcode.WebApi.Controllers.Email
{
    public class EmailController : BaseApiController
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public EmailController(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Send([FromBody] EmailDTO email)
        {
            await _publishEndpoint.Publish<IEmailMessage>(
                new
                {
                    email.From,
                    email.Content
                });

            return Accepted();
        }
    }
}

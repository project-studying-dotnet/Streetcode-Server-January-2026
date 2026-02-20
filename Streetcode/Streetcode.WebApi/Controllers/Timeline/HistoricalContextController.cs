using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.MediatR.Timeline.HistoricalContext.Create;
using Streetcode.BLL.MediatR.Timeline.HistoricalContext.Delete;
using Streetcode.BLL.MediatR.Timeline.HistoricalContext.GetAll;
using Streetcode.BLL.MediatR.Timeline.HistoricalContext.Update;
using Streetcode.Shared.Enums;

namespace Streetcode.WebApi.Controllers.Timeline
{
    public class HistoricalContextController : BaseApiController
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            return HandleResult(await Mediator.Send(new GetAllHistoricalContextQuery()));
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Administrator))]
        public async Task<IActionResult> Create([FromBody] CreateHistoricalContextDTO historicalContext)
        {
            return HandleResult(await Mediator.Send(new CreateHistoricalContextCommand(historicalContext)));
        }

        [HttpPut]
        [Authorize(Roles = nameof(UserRole.Administrator))]
        public async Task<IActionResult> Update([FromBody] UpdateHistoricalContextDTO historicalContext)
        {
            return HandleResult(await Mediator.Send(new UpdateHistoricalContextCommand(historicalContext)));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = nameof(UserRole.Administrator))]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            return HandleResult(await Mediator.Send(new DeleteHistoricalContextCommand(id)));
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Team;
using Streetcode.BLL.MediatR.Team.Create;
using Streetcode.BLL.MediatR.Team.Position.GetAll;
using Streetcode.Shared.Enums;

namespace Streetcode.WebApi.Controllers.Team
{
    public class PositionController : BaseApiController
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            return HandleResult(await Mediator.Send(new GetAllPositionsQuery()));
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Administrator))]
        public async Task<IActionResult> Create([FromBody] PositionDTO position)
        {
            return HandleResult(await Mediator.Send(new CreatePositionQuery(position)));
        }
    }
}

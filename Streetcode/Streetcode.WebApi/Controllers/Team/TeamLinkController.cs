using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Team;
using Streetcode.BLL.MediatR.Team.TeamMembersLinks.Create;
using Streetcode.BLL.MediatR.Team.TeamMembersLinks.GetAll;
using Streetcode.Shared.Enums;

namespace Streetcode.WebApi.Controllers.Team
{
    public class TeamLinkController : BaseApiController
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            return HandleResult(await Mediator.Send(new GetAllTeamLinkQuery()));
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Administrator))]
        public async Task<IActionResult> Create([FromBody] TeamMemberLinkDTO teamMemberLink)
        {
            return HandleResult(await Mediator.Send(new CreateTeamLinkQuery(teamMemberLink)));
        }
    }
}

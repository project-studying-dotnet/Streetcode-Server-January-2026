using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Streetcode.Comments;
using Streetcode.BLL.MediatR.Streetcode.Comments.Create;
using Streetcode.BLL.MediatR.Streetcode.Comments.Delete;
using Streetcode.BLL.MediatR.Streetcode.Comments.GetByStreetcodeId;
using Streetcode.BLL.MediatR.Streetcode.Comments.Update;
using Streetcode.Resources;

namespace Streetcode.WebApi.Controllers.Streetcode.Comments;

public class CommentController : BaseApiController
{
    [HttpGet("{streetcodeId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByStreetcodeId([FromRoute] int streetcodeId)
    {
        return HandleResult(await Mediator.Send(new GetCommentsByStreetcodeIdQuery(streetcodeId)));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateCommentDTO createCommentDTO)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(Messages.Error_MissingUserId);
        }

        return HandleResult(await Mediator.Send(new CreateCommentCommand(createCommentDTO, userId)));
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] UpdateCommentDTO updateCommentDTO)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(Messages.Error_MissingUserId);
        }

        return HandleResult(await Mediator.Send(new UpdateCommentCommand(updateCommentDTO, userId)));
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(Messages.Error_MissingUserId);
        }

        return HandleResult(await Mediator.Send(new DeleteCommentCommand(id, userId)));
    }
}

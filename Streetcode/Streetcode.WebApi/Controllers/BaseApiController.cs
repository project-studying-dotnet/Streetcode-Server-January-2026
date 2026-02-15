using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Streetcode.Resources;

namespace Streetcode.WebApi.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class BaseApiController : ControllerBase
{
    private IMediator? _mediator;

    protected IMediator Mediator => _mediator ??=
        HttpContext.RequestServices.GetService<IMediator>() !;

    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value is null
                ? NotFound(Messages.Error_FoundResultMatchingNull)
                : Ok(result.Value);
        }

        return BadRequest(result.Reasons);
    }
}
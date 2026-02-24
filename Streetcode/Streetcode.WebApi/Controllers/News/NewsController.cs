using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.MediatR.News.Create;
using Streetcode.BLL.MediatR.News.Delete;
using Streetcode.BLL.MediatR.News.GetAll;
using Streetcode.BLL.MediatR.News.GetById;
using Streetcode.BLL.MediatR.News.GetByUrl;
using Streetcode.BLL.MediatR.News.GetNewsAndLinksByUrl;
using Streetcode.BLL.MediatR.News.SortedByDateTime;
using Streetcode.BLL.MediatR.News.Update;
using Streetcode.Shared.Enums;

namespace Streetcode.WebApi.Controllers.News;

public class NewsController : BaseApiController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        return HandleResult(await Mediator.Send(new GetAllNewsQuery()));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new GetNewsByIdQuery(id)));
    }

    [HttpGet("{url}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByUrl([FromRoute] string url)
    {
        return HandleResult(await Mediator.Send(new GetNewsByUrlQuery(url)));
    }

    [HttpGet("getNewsAndLinks/{url}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetNewsAndLinksByUrl([FromRoute] string url)
    {
        return HandleResult(await Mediator.Send(new GetNewsAndLinksByUrlQuery(url)));
    }

    [HttpGet("sortedByDateTime")]
    [AllowAnonymous]
    public async Task<IActionResult> GetNewsAndLinksByUrl()
    {
        return HandleResult(await Mediator.Send(new SortedByDateTimeQuery()));
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Administrator))]
    public async Task<IActionResult> Create([FromBody] NewsDTO news)
    {
        return HandleResult(await Mediator.Send(new CreateNewsCommand(news)));
    }

    [HttpPut]
    [Authorize(Roles = nameof(UserRole.Administrator))]
    public async Task<IActionResult> Update([FromBody] NewsDTO news)
    {
        return HandleResult(await Mediator.Send(new UpdateNewsCommand(news)));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Administrator))]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new DeleteNewsCommand(id)));
    }
}

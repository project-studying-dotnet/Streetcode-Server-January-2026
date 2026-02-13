using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.MediatR.Streetcode.Fact.Create;
using Streetcode.BLL.MediatR.Streetcode.Fact.Delete;
using Streetcode.BLL.MediatR.Streetcode.Fact.GetAll;
using Streetcode.BLL.MediatR.Streetcode.Fact.GetById;
using Streetcode.BLL.MediatR.Streetcode.Fact.GetByStreetcodeId;
using Streetcode.BLL.MediatR.Streetcode.Fact.Update;
using Streetcode.BLL.MediatR.Streetcode.Fact.UpdateOrder;
using Streetcode.Shared.Enums;

namespace Streetcode.WebApi.Controllers.Streetcode.TextContent;

public class FactController : BaseApiController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        return HandleResult(await Mediator.Send(new GetAllFactsQuery()));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new GetFactByIdQuery(id)));
    }

    [HttpGet("getByStreetcodeId/{streetcodeId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByStreetcodeId([FromRoute] int streetcodeId)
    {
        return HandleResult(await Mediator.Send(new GetFactByStreetcodeIdQuery(streetcodeId)));
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Administrator))]
    public async Task<IActionResult> Create([FromBody] CreateFactDTO fact)
    {
        return HandleResult(await Mediator.Send(new CreateFactCommand(fact)));
    }

    [HttpPut]
    [Authorize(Roles = nameof(UserRole.Administrator))]
    public async Task<IActionResult> Update([FromBody] UpdateFactDTO fact)
    {
        return HandleResult(await Mediator.Send(new UpdateFactCommand(fact)));
    }

    [HttpPut("update-order")]
    [Authorize(Roles = nameof(UserRole.Administrator))]
    public async Task<IActionResult> UpdateOrder([FromBody] List<UpdateFactOrderDTO> facts)
    {
        return HandleResult(await Mediator.Send(new UpdateOrderFactCommand(facts)));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Administrator))]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new DeleteFactCommand(id)));
    }
}

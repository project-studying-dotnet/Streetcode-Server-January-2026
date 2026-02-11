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

namespace Streetcode.WebApi.Controllers.Streetcode.TextContent;

public class FactController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return HandleResult(await Mediator.Send(new GetAllFactsQuery()));
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new GetFactByIdQuery(id)));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("getByStreetcodeId/{streetcodeId:int}")]
    public async Task<IActionResult> GetByStreetcodeId([FromRoute] int streetcodeId)
    {
        return HandleResult(await Mediator.Send(new GetFactByStreetcodeIdQuery(streetcodeId)));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateFactDTO fact)
    {
        return HandleResult(await Mediator.Send(new CreateFactCommand(fact)));
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateFactDTO fact)
    {
        return HandleResult(await Mediator.Send(new UpdateFactCommand(fact)));
    }

    [HttpPut("update-order")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateOrder([FromBody] List<UpdateFactOrderDTO> facts)
    {
        return HandleResult(await Mediator.Send(new UpdateOrderFactCommand(facts)));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new DeleteFactCommand(id)));
    }
}

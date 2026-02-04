using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
using Streetcode.BLL.MediatR.Streetcode.Text.Create;
using Streetcode.BLL.MediatR.Streetcode.Text.Delete;
using Streetcode.BLL.MediatR.Streetcode.Text.GetAll;
using Streetcode.BLL.MediatR.Streetcode.Text.GetById;
using Streetcode.BLL.MediatR.Streetcode.Text.GetByStreetcodeId;
using Streetcode.BLL.MediatR.Streetcode.Text.GetParsed;
using Streetcode.BLL.MediatR.Streetcode.Text.Update;

namespace Streetcode.WebApi.Controllers.Streetcode.TextContent;

public class TextController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return HandleResult(await Mediator.Send(new GetAllTextsQuery()));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new GetTextByIdQuery(id)));
    }

    [HttpGet("{streetcodeId:int}")]
    public async Task<IActionResult> GetByStreetcodeId([FromRoute] int streetcodeId)
    {
        return HandleResult(await Mediator.Send(new GetTextByStreetcodeIdQuery(streetcodeId)));
    }

    [HttpGet]
    public async Task<IActionResult> GetParsedText([FromQuery] string text)
    {
        return HandleResult(await Mediator.Send(new GetParsedTextForAdminPreviewCommand(text)));
    }

    [HttpPost]
    public async Task<IActionResult> CreateText([FromBody] TextCreateDTO textBaseDTO)
    {
        return HandleResult(await Mediator.Send(new CreateTextCommand(textBaseDTO)));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateText([FromBody] TextUpdateDTO textBaseDTO)
    {
        return HandleResult(await Mediator.Send(new UpdateTextCommand(textBaseDTO)));
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteText([FromQuery] int id)
    {
        return HandleResult(await Mediator.Send(new DeleteTextCommand(id)));
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Create;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Delete;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.GetAllByTermId;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Update;

namespace Streetcode.WebApi.Controllers.Streetcode.TextContent
{
    public class RelatedTermController : BaseApiController
    {
        [HttpGet("{termId:int}")]
        public async Task<IActionResult> GetByTermId([FromRoute] int termId)
        {
            return HandleResult(await Mediator.Send(new GetAllRelatedTermsByTermIdQuery(termId)));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRelatedTermDTO createRelatedTerm)
        {
            return HandleResult(await Mediator.Send(new CreateRelatedTermCommand(createRelatedTerm)));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateRelatedTermDTO updateRelatedTerm)
        {
            return HandleResult(await Mediator.Send(new UpdateRelatedTermCommand(updateRelatedTerm)));
        }

        [HttpDelete("{word}/{termId:int}")]
        public async Task<IActionResult> Delete([FromRoute] string word, [FromRoute] int termId)
        {
            return HandleResult(await Mediator.Send(new DeleteRelatedTermCommand(word, termId)));
        }
    }
}

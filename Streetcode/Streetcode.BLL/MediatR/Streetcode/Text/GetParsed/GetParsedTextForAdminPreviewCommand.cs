using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Streetcode.Entity.GetParsed
{
    public record GetParsedTextForAdminPreviewCommand(string textToParse) : IRequest<Result<string>>
    {
    }
}

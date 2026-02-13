using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Cache;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetShortById
{
    public record GetStreetcodeShortByIdQuery(int Id) : IRequest<Result<StreetcodeShortDTO>>;
}

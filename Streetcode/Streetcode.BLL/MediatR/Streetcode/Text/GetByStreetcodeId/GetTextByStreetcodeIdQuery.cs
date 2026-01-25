using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Entity.GetByStreetcodeId;

public record GetTextByStreetcodeIdQuery(int StreetcodeId) : IRequest<Result<TextDTO?>>;
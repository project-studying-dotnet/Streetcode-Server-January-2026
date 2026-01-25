using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Entity.GetById;

public record GetTextByIdQuery(int Id) : IRequest<Result<TextDTO>>;

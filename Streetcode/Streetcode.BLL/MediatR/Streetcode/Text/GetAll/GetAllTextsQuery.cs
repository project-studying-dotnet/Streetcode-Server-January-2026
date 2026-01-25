using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Entity.GetAll;

public record GetAllTextsQuery : IRequest<Result<IEnumerable<TextDTO>>>;
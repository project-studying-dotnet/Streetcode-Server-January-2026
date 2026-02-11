using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Cache;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetById;

public record GetStreetcodeByIdQuery(int Id) : IRequest<Result<StreetcodeDTO>>;
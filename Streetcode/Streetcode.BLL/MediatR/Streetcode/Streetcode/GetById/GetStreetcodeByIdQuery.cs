using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Cache;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetById;

public record GetStreetcodeByIdQuery(int Id) : IRequest<Result<StreetcodeDTO>>, ICachableQuery
{
    public string CacheKey => $"Streetcode_{Id}";

    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(30);

    public bool BypassCache => false;
}
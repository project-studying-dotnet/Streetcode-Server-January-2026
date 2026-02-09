using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Cache;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetShortById
{
    public record GetStreetcodeShortByIdQuery(int id) : IRequest<Result<StreetcodeShortDTO>>, ICachableQuery
    {
        public string CacheKey => $"Streetcode_{id}";

        public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(30);

        public bool BypassCache => false;
    }
}

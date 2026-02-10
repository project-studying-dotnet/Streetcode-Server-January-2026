using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetCount
{
    public class GetStreetcodesCountHander : IRequestHandler<GetStreetcodesCountQuery,
        Result<int>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public GetStreetcodesCountHander(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(GetStreetcodesCountQuery request, CancellationToken cancellationToken)
        {
            var streetcodes = await _repositoryWrapper.StreetcodeRepository.GetAllAsync();

            if (streetcodes.Any())
            {
                return Result.Ok(streetcodes.Count());
            }

            var errorMsg = Messages.Error_EntitiesNotFound.Format(nameof(StreetcodeContent));
            _logger.LogError(request, errorMsg);
            return Result.Fail(errorMsg);
        }
    }
}

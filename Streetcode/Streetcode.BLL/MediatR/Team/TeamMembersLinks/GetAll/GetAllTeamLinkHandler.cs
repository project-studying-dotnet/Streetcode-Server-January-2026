using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Team;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Team;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Team.TeamMembersLinks.GetAll
{
    public class GetAllTeamLinkHandler : IRequestHandler<GetAllTeamLinkQuery, Result<IEnumerable<TeamMemberLinkDTO>>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public GetAllTeamLinkHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<TeamMemberLinkDTO>>> Handle(GetAllTeamLinkQuery request, CancellationToken cancellationToken)
        {
            var teamLinks = await _repositoryWrapper
                .TeamLinkRepository
                .GetAllAsync();

            if (teamLinks.Any())
            {
                return Result.Ok(_mapper.Map<IEnumerable<TeamMemberLinkDTO>>(teamLinks));
            }

            var errorMsg = Messages.Error_EntitiesNotFound.Format(nameof(TeamMemberLink));
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}

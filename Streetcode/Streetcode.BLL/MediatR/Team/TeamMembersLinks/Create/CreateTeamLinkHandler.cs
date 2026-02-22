using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Team;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Team;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Team.TeamMembersLinks.Create
{
    public class CreateTeamLinkHandler : IRequestHandler<CreateTeamLinkQuery, Result<TeamMemberLinkDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repository;
        private readonly ILoggerService _logger;

        public CreateTeamLinkHandler(IMapper mapper, IRepositoryWrapper repository, ILoggerService logger)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<TeamMemberLinkDTO>> Handle(CreateTeamLinkQuery request, CancellationToken cancellationToken)
        {
            var teamMemberLink = _mapper.Map<TeamMemberLink>(request.TeamMember);

            if (teamMemberLink is null)
            {
                var errorConvertMsg = Messages.Error_ConvertNullToEntity.Format(nameof(TeamMemberLink));
                _logger.LogError(request, errorConvertMsg);
                return Result.Fail(new Error(errorConvertMsg));
            }

            var createdTeamLink = await _repository.TeamLinkRepository.CreateAsync(teamMemberLink);

            var resultIsSuccess = await _repository.SaveChangesAsync() > 0;

            if (resultIsSuccess)
            {
                return Result.Ok(_mapper.Map<TeamMemberLinkDTO>(createdTeamLink));
            }

            var errorFailedToCreateMsg = Messages.Error_FailedToCreateEntity.Format(nameof(TeamMemberLink));
            _logger.LogError(request, errorFailedToCreateMsg);
            return Result.Fail(new Error(errorFailedToCreateMsg));
        }
    }
}

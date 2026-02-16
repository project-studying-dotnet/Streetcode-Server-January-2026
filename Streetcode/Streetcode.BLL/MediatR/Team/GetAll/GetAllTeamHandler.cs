using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Team;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Team.GetAll
{
    public class GetAllTeamHandler : IRequestHandler<GetAllTeamQuery, Result<IEnumerable<TeamMemberDTO>>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public GetAllTeamHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<TeamMemberDTO>>> Handle(GetAllTeamQuery request, CancellationToken cancellationToken)
        {
            var teams = await _repositoryWrapper.TeamRepository
                .GetAllAsync(include: x => x
                    .Include(x => x.Positions)
                    .Include(x => x.TeamMemberLinks));

            if (teams.Any())
            {
                return Result.Ok(_mapper.Map<IEnumerable<TeamMemberDTO>>(teams));
            }

            var errorMsg = Messages.Error_EntitiesNotFound.Format(nameof(DAL.Entities.Team.TeamMember));
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}
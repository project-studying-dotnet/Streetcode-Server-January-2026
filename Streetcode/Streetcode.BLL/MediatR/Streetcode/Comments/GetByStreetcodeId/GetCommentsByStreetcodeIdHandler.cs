using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Streetcode.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.GetByStreetcodeId
{
    public class GetCommentsByStreetcodeIdHandler : IRequestHandler<GetCommentsByStreetcodeIdQuery, Result<IEnumerable<CommentDTO>>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public GetCommentsByStreetcodeIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<CommentDTO>>> Handle(GetCommentsByStreetcodeIdQuery request, CancellationToken cancellationToken)
        {
            var comments = await _repositoryWrapper.CommentRepository.GetAllAsync(
                predicate: c => c.StreetcodeId == request.StreetcodeId,
                include: x => x.Include(c => c.User));

            var sortedComments = comments.OrderByDescending(c => c.CreatedAt);

            return Result.Ok(_mapper.Map<IEnumerable<CommentDTO>>(sortedComments));
        }
    }
}

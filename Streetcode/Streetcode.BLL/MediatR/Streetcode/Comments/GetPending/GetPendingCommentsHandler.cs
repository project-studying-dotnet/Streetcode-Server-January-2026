using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Streetcode.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.GetPending
{
    public class GetPendingCommentsHandler : IRequestHandler<GetPendingCommentsQuery, Result<IEnumerable<CommentDTO>>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public GetPendingCommentsHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<CommentDTO>>> Handle(GetPendingCommentsQuery request, CancellationToken cancellationToken)
        {
            var comments = await _repositoryWrapper.CommentRepository.GetAllAsync(
                predicate: c => c.Status == CommentStatus.Pending,
                include: x => x.Include(c => c.User));

            if (!comments.Any())
            {
                return Result.Ok(Enumerable.Empty<CommentDTO>());
            }

            var sortedComments = comments.OrderBy(c => c.CreatedAt);

            var dtos = _mapper.Map<IEnumerable<CommentDTO>>(sortedComments);

            return Result.Ok(dtos);
        }
    }
}

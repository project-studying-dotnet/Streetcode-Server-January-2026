using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Streetcode.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Enums;
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
                predicate: c => c.StreetcodeId == request.StreetcodeId
                             && c.Status == CommentStatus.Approved,
                include: x => x.Include(c => c.User));

            var dtos = _mapper.Map<IEnumerable<CommentDTO>>(comments).ToList();

            var dictionary = dtos.ToDictionary(x => x.Id);

            foreach (var dto in dtos)
            {
                if (dto.ParentId.HasValue && dictionary.TryGetValue(dto.ParentId.Value, out var parent))
                {
                    parent.Replies.Add(dto);
                }
            }

            var rootComments = dtos
                .Where(c => c.ParentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            SortReplies(rootComments);

            return Result.Ok((IEnumerable<CommentDTO>)rootComments);
        }

        private static void SortReplies(List<CommentDTO> comments)
        {
            foreach (var comment in comments)
            {
                if (comment.Replies != null && comment.Replies.Any())
                {
                    comment.Replies = comment.Replies
                        .OrderBy(c => c.CreatedAt)
                        .ToList();

                    SortReplies(comment.Replies);
                }
            }
        }
    }
}

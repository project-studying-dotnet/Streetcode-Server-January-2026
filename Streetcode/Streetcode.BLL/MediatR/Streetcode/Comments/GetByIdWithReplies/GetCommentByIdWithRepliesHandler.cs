using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Streetcode.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Streetcode.Comments;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.GetByIdWithReplies
{
    public class GetCommentByIdWithRepliesHandler : IRequestHandler<GetCommentByIdWithRepliesQuery, Result<CommentDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public GetCommentByIdWithRepliesHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CommentDTO>> Handle(GetCommentByIdWithRepliesQuery request, CancellationToken cancellationToken)
        {
            var targetComment = await _repositoryWrapper.CommentRepository
                .GetFirstOrDefaultAsync(c => c.Id == request.Id);

            if (targetComment is null)
            {
                var errorNotFoundMsg = Messages.Error_EntityWithIdNotFound.Format(nameof(Comment), request.Id);
                _logger.LogError(request, errorNotFoundMsg);
                return Result.Fail(new Error(errorNotFoundMsg));
            }

            var allComments = await _repositoryWrapper.CommentRepository.GetAllAsync(
                predicate: c => c.StreetcodeId == targetComment.StreetcodeId,
                include: x => x.Include(c => c.User));

            var dtos = _mapper.Map<IEnumerable<CommentDTO>>(allComments).ToList();
            var dictionary = dtos.ToDictionary(x => x.Id);

            foreach (var dto in dtos)
            {
                if (dto.ParentId.HasValue && dictionary.TryGetValue(dto.ParentId.Value, out var parent))
                {
                    parent.Replies.Add(dto);
                }
            }

            var resultComment = dictionary[request.Id];

            resultComment.Replies = resultComment.Replies.OrderBy(c => c.CreatedAt).ToList();
            SortReplies(resultComment.Replies);

            return Result.Ok(resultComment);
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

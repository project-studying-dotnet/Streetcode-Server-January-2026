using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Streetcode.Comments;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.AdminDelete
{
    public class AdminDeleteCommentHandler : IRequestHandler<AdminDeleteCommentCommand, Result<Unit>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public AdminDeleteCommentHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(AdminDeleteCommentCommand request, CancellationToken cancellationToken)
        {
            var targetComment = await _repositoryWrapper.CommentRepository
                .GetFirstOrDefaultAsync(t => t.Id == request.Id);

            if (targetComment is null)
            {
                var errorNotFoundMsg = Messages.Error_EntityWithIdNotFound.Format(nameof(Comment), request.Id);
                _logger.LogError(request, errorNotFoundMsg);
                return Result.Fail(new Error(errorNotFoundMsg));
            }

            var allComments = await _repositoryWrapper.CommentRepository
                .GetAllAsync(c => c.StreetcodeId == targetComment.StreetcodeId);

            var childrenLookup = allComments
                .Where(c => c.ParentId.HasValue)
                .GroupBy(c => c.ParentId.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            var commentsToDelete = new List<Comment>();
            var stack = new Stack<Comment>();

            stack.Push(targetComment);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                commentsToDelete.Add(current);

                if (childrenLookup.TryGetValue(current.Id, out var children))
                {
                    foreach (var child in children)
                    {
                        stack.Push(child);
                    }
                }
            }

            _repositoryWrapper.CommentRepository.DeleteRange(commentsToDelete);

            var successSave = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (successSave)
            {
                return Result.Ok(Unit.Value);
            }

            var errorMsg = Messages.Error_FailedToDeleteEntity.Format(nameof(Comment));
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}

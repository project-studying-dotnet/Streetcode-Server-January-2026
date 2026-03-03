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
            var comment = await _repositoryWrapper.CommentRepository
                .GetFirstOrDefaultAsync(t => t.Id == request.Id);

            if (comment is null)
            {
                var errorNotFoundMsg = Messages.Error_EntityWithIdNotFound.Format(nameof(Comment), request.Id);
                _logger.LogError(request, errorNotFoundMsg);
                return Result.Fail(new Error(errorNotFoundMsg));
            }

            var allComments = await _repositoryWrapper.CommentRepository
                .GetAllAsync(c => c.StreetcodeId == comment.StreetcodeId);

            var commentLookup = allComments
                .Where(c => c.ParentId.HasValue)
                .GroupBy(c => c.ParentId.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            var commentsToDelete = new List<Comment>();
            var commentsStack = new Stack<Comment>();

            commentsStack.Push(comment);

            while (commentsStack.Count > 0)
            {
                var current = commentsStack.Pop();
                commentsToDelete.Add(current);

                if (commentLookup.TryGetValue(current.Id, out var children))
                {
                    foreach (var child in children)
                    {
                        commentsStack.Push(child);
                    }
                }
            }

            _repositoryWrapper.CommentRepository.DeleteRange(commentsToDelete);

            var success = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (success)
            {
                return Result.Ok(Unit.Value);
            }

            var errorMsg = Messages.Error_FailedToDeleteEntity.Format(nameof(Comment));
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}

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

namespace Streetcode.BLL.MediatR.Streetcode.Comments.Update
{
    public class UpdateCommentHandler : IRequestHandler<UpdateCommentCommand, Result<CommentDTO>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;

        public UpdateCommentHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CommentDTO>> Handle(UpdateCommentCommand command, CancellationToken cancellationToken)
        {
            var comment = await _repositoryWrapper.CommentRepository
                .GetFirstOrDefaultAsync(
                    predicate: f => f.Id == command.Comment.Id,
                    include: x => x.Include(c => c.User));

            if (comment == null)
            {
                var errorNotFoundMsg = Messages.Error_EntityWithIdNotFound.Format(
                    nameof(Comment), command.Comment.Id);

                _logger.LogError(command, errorNotFoundMsg);
                return Result.Fail(new Error(errorNotFoundMsg));
            }

            if (comment.UserId != command.UserId)
            {
                var errorAuthMsg = Messages.Error_UserNotCommentOwner;
                _logger.LogError(command, errorAuthMsg);
                return Result.Fail(new Error(errorAuthMsg));
            }

            comment = _mapper.Map(command.Comment, comment);
            comment.UpdatedAt = DateTime.UtcNow;

            _repositoryWrapper.CommentRepository.Update(comment);
            var successSave = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (successSave)
            {
                return Result.Ok(_mapper.Map<CommentDTO>(comment));
            }

            var errorMsg = Messages.Error_FailedToUpdateEntity.Format(nameof(Comment));
            _logger.LogError(command, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}

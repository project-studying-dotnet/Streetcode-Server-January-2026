using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Streetcode.Comments;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Streetcode.Comments.Create
{
    public class CreateCommentHandler : IRequestHandler<CreateCommentCommand, Result<CommentDTO>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;

        public CreateCommentHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CommentDTO>> Handle(CreateCommentCommand command, CancellationToken cancellationToken)
        {
            var comment = _mapper.Map<Comment>(command.Comment);

            comment.UserId = command.UserId;
            comment.CreatedAt = DateTime.UtcNow;

            var createdComment = await _repositoryWrapper.CommentRepository.CreateAsync(comment);
            var successSave = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (successSave)
            {
                var user = await _repositoryWrapper.UserRepository
                    .GetFirstOrDefaultAsync(u => u.Id == command.UserId);

                createdComment.User = user;

                return Result.Ok(_mapper.Map<CommentDTO>(createdComment));
            }

            var errorMsg = Messages.Error_FailedToCreateEntity.Format(nameof(Comment));
            _logger.LogError(command, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}

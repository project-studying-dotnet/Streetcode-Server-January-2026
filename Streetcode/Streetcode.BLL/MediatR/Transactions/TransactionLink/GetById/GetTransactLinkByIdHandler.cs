using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Transactions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.Resources;
using Streetcode.Shared.Extensions;

namespace Streetcode.BLL.MediatR.Transactions.TransactionLink.GetById;

public class GetTransactLinkByIdHandler : IRequestHandler<GetTransactLinkByIdQuery, Result<TransactLinkDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetTransactLinkByIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<TransactLinkDTO>> Handle(GetTransactLinkByIdQuery request, CancellationToken cancellationToken)
    {
        var transactLink = await _repositoryWrapper.TransactLinksRepository
            .GetFirstOrDefaultAsync(f => f.Id == request.Id);

        if (transactLink is not null)
        {
            return Result.Ok(_mapper.Map<TransactLinkDTO>(transactLink));
        }

        var errorMsg = Messages.Error_EntityWithIdNotFound.Format(
            nameof(DAL.Entities.Transactions.TransactionLink),
            request.Id);

        _logger.LogError(request, errorMsg);
        return Result.Fail(new Error(errorMsg));
    }
}
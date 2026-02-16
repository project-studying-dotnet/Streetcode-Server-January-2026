using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.News.Delete
{
    public record DeleteNewsCommand(int Id) : IRequest<Result<Unit>>;
}

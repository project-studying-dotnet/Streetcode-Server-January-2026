using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.News;

namespace Streetcode.BLL.MediatR.News.Update
{
    public record UpdateNewsCommand(NewsDTO News) : IRequest<Result<NewsDTO>>;
}

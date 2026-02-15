using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Sources;

namespace Streetcode.BLL.MediatR.Sources.SourceLinkCategory.GetCategoryContentByStreetcodeId
{
    public record GetCategoryContentByStreetcodeIdQuery(int StreetcodeId, int CategoryId)
        : IRequest<Result<StreetcodeCategoryContentDTO>>;
}

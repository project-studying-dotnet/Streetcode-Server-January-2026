using AutoMapper;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.DAL.Entities.Streetcode.TextContent;

namespace Streetcode.BLL.Mapping.Streetcode.TextContent;

public class FactProfile : Profile
{
    public FactProfile()
    {
        CreateMap<Fact, FactDTO>()
            .ForMember(
                dest => dest.ImageDescription,
                opt => opt.MapFrom(src => src.Image != null && src.Image.ImageDetails != null
                    ? src.Image.ImageDetails.Title
                    : null))
            .ReverseMap();

        CreateMap<CreateFactDTO, Fact>();
        CreateMap<UpdateFactDTO, Fact>();
        CreateMap<UpdateFactOrderDTO, Fact>();
    }
}

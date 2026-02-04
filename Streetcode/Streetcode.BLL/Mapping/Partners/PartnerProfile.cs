using AutoMapper;
using Streetcode.BLL.DTO.Partners;
using Streetcode.DAL.Entities.Partners;

namespace Streetcode.BLL.Mapping.Partners;

public class PartnerProfile : Profile
{
    public PartnerProfile()
    {
        CreateMap<CreatePartnerDTO, Partner>()
            .ForPath(dest => dest.Streetcodes, opt => opt.Ignore());
        CreateMap<UpdatePartnerDTO, Partner>()
            .ForPath(dest => dest.Streetcodes, opt => opt.Ignore())
            .ForPath(dest => dest.PartnerSourceLinks, opt => opt.Ignore());
        CreateMap<Partner, PartnerDTO>()
            .ForPath(dto => dto.TargetUrl!.Title, conf => conf.MapFrom(ol => ol.UrlTitle))
            .ForPath(dto => dto.TargetUrl!.Href, conf => conf.MapFrom(ol => ol.TargetUrl));
        CreateMap<Partner, PartnerShortDTO>().ReverseMap();
    }
}

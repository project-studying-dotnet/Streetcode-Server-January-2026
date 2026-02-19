using AutoMapper;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.DTO.Timeline.TimelineItem;
using Streetcode.DAL.Entities.Timeline;

namespace Streetcode.BLL.Mapping.Timeline;

public class TimelineItemProfile : Profile
{
    public TimelineItemProfile()
    {
        CreateMap<TimelineItem, TimelineItemDTO>().ReverseMap();

        CreateMap<TimelineItem, TimelineItemDTO>()
            .ForMember(dest => dest.HistoricalContexts, opt => opt.MapFrom(x => x.HistoricalContextTimelines
                .Select(x => new HistoricalContextDTO
                {
                    Id = x.HistoricalContextId,
                    Title = x.HistoricalContext.Title
                }).ToList()));

        CreateMap<CreateTimelineItemDTO, TimelineItem>()
            .ForMember(dest => dest.HistoricalContextTimelines, opt => opt.Ignore());

        CreateMap<UpdateTimelineItemDTO, TimelineItem>()
            .ForMember(dest => dest.HistoricalContextTimelines, opt => opt.Ignore())
            .ForMember(dest => dest.Streetcode, opt => opt.Ignore());

        CreateMap<CreateHistoricalContextDTO, HistoricalContext>();
        CreateMap<UpdateHistoricalContextDTO, HistoricalContext>()
            .ForMember(dest => dest.HistoricalContextTimelines, opt => opt.Ignore());
        CreateMap<HistoricalContext, HistoricalContextDTO>();
    }
}

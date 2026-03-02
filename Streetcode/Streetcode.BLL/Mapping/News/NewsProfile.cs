using AutoMapper;
using Streetcode.BLL.DTO.News;

namespace Streetcode.BLL.Mapping.News
{
    public class NewsProfile : Profile
    {
        public NewsProfile()
        {
            CreateMap<DAL.Entities.News.News, NewsDTO>().ReverseMap();
        }
    }
}

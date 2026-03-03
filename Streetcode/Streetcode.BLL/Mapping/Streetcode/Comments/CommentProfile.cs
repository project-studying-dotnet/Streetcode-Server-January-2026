using AutoMapper;
using Streetcode.BLL.DTO.Streetcode.Comments;
using Streetcode.DAL.Entities.Streetcode.Comments;

namespace Streetcode.BLL.Mapping.Streetcode.Comments
{
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            CreateMap<Comment, CommentDTO>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src =>
                    src.User != null ? $"{src.User.Name} {src.User.Surname}" : "Unknown User"))
                .ForMember(dest => dest.ParentCommentId, opt => opt.MapFrom(src => src.ParentCommentId))
                .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.Replies));

            CreateMap<CreateCommentDTO, Comment>()
                .ForMember(dest => dest.ParentCommentId, opt => opt.MapFrom(src => src.ParentCommentId));

            CreateMap<UpdateCommentDTO, Comment>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.StreetcodeId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ParentCommentId, opt => opt.Ignore());
        }
    }
}
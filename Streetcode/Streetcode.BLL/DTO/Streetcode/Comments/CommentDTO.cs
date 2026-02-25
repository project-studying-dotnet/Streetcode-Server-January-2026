namespace Streetcode.BLL.DTO.Streetcode.Comments
{
    public class CommentDTO
    {
        public int Id { get; set; }
        public string TextContent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int StreetcodeId { get; set; }
        public string UserId { get; set; }
        public string UserFullName { get; set; }
    }
}

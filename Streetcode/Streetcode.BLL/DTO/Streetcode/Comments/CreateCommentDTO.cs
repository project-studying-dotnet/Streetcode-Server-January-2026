namespace Streetcode.BLL.DTO.Streetcode.Comments
{
    public class CreateCommentDTO
    {
        public string TextContent { get; set; }

        public int StreetcodeId { get; set; }

        public int? ParentId { get; set; }
    }
}

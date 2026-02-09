namespace Streetcode.BLL.DTO.Streetcode.TextContent.Fact;

public class FactDTO
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string FactContent { get; set; }
    public int ImageId { get; set; }
    public string? ImageDescription { get; set; }
    public int Order { get; set; }
}

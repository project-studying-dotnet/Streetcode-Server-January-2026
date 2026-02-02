namespace Streetcode.BLL.DTO.Streetcode.TextContent.Fact;

public class CreateFactDTO
{
    public string Title { get; set; }
    public string FactContent { get; set; }
    public int ImageId { get; set; }
    public string? ImageDescription { get; set; }
    public int StreetcodeId { get; set; }
}
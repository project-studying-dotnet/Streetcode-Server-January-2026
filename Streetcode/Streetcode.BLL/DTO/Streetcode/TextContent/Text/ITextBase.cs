namespace Streetcode.BLL.DTO.Streetcode.TextContent.Text
{
    public interface ITextBase
    {
        public string Title { get; }
        public string TextContent { get; }
        public string? AdditionalText { get; }

        public int StreetcodeId { get; }
    }
}

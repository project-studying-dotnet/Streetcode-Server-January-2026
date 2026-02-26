namespace Streetcode.BLL.DTO.News
{
    public class NewsDTOWithURLs
    {
        public NewsDTO News { get; set; } = new ();

        public string? PrevNewsUrl { get; set; }

        public string? NextNewsUrl { get; set; }

        public RandomNewsDTO? RandomNews { get; set; } = new ();
    }
}

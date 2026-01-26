using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.DTO.Streetcode.TextContent.Text
{
    public class TextUpdateDTO
    {
        public string Title { get; set; }
        public string TextContent { get; set; }
        public string? AdditionalText { get; set; }

        public int StreetcodeId { get; set; }
    }
}

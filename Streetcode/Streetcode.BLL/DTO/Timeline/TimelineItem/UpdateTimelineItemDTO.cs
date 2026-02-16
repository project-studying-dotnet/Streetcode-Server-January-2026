using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.DTO.Timeline.TimelineItem
{

    public class UpdateTimelineItemDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string DateViewPattern { get; set; }
        public int StreetcodeId { get; set; }
        public IEnumerable<int> HistoricalContextIds { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCardProtocolAPI.Command.Models
{
    public class TimeGroupModel
    {
        public int Index { get; set; }

        public List<DayTimeModel> DayTimeList { get; set; }
    }

    public class DayTimeModel
    {
        public DayOfWeek DayWeek { get; set; }
        public List<TimeSegment> TimeSegmentList { get; set; }
    }

    public class TimeSegment
    {
        public string Begin { get; set; }
        public string End { get; set; }
    }

}

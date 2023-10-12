using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCardProtocolAPI.Command.Models
{
    public class PersonAddRangeInfo
    {
        public List<string> SNList { get; set; }

        public List<PersonModel> PersonList { get; set; }
    }
}

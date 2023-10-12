using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCardProtocolAPI.Command.Models
{
    public class KeepAliveTransaction:CardRecord
    {
        

        public DateTime KeepAliveTime { get; set; }
    }
}

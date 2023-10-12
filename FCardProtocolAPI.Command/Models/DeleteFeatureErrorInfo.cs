using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCardProtocolAPI.Command.Models
{
    public class DeleteFeatureErrorInfo
    {
        public string SN { get; set; }
        public bool State { get; set; }
        public string Message { get; set; }
    }
}

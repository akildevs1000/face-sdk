using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCardProtocolAPI.Command.Models
{
    public class DeleteFeatureCodeModel
    {
        public uint UserCode { get; set; }

        public int[] Fingerprint { get; set; }

        public int[] Palm { get; set; }

        public int[] Photo { get; set; }
    }
}

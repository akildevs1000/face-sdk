using DoNetDrive.Protocol.Fingerprint.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCardProtocolAPI.Command.Models
{
    public class PersonAddRangeErrorInfo
    {
        public string SN { get; set; }
        public bool State { get; set; }
        public string Message { get; set; }
        public List<PersonAddErrorInfo> UserList { get; set; }

    }

    public class PersonAddErrorInfo
    {
        public uint UserCode { get; set; }
        public string Message { get; set; }
    }

    public class PersonListInfo
    {
        public List<PersonAddErrorInfo> ErrorList { get; set; }
        public List<Person > PersonList { get; set; }
    }

}

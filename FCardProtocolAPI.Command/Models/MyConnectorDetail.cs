using DoNetDrive.Core.Connector;
using DoNetDrive.Core.Connector.TCPClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCardProtocolAPI.Command.Models
{
    public class MyConnectorDetail
    {
        public INConnectorDetail ConnectorDetail { get; set; }

        public string SN { get; set; }

        public DateTime KeepAliveTime { get; set; }
        public bool IsClient { get; set; } = true;

        public bool IsTCP { get; set; }
        [System.Text.Json.Serialization.JsonInclude]
        public bool FristConnection { get; set; }
    }
}

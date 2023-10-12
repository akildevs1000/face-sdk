using DoNetDrive.Core.Connector;
using DoNetDrive.Protocol.Door8800;
using DoNetDrive.Protocol.Transaction;
using DotNetty.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCardProtocolAPI.Command
{
    public class MyRequestHandle: Door8800RequestHandle
    {
        private string ConnectKey;
        private DateTime KeepaliveTime;
        internal INConnectorDetail ConnDetail;

        public MyRequestHandle(IByteBufferAllocator allocator,
            Func<string, byte, byte, AbstractTransaction> factory, INConnectorDetail iDetail)
            : base(allocator, factory)
        {
            ConnectKey = iDetail.GetKey();
            KeepaliveTime = DateTime.Now;
            ConnDetail = (INConnectorDetail)iDetail.Clone();
        }
        //接收
        public override void DisposeRequest(INConnector connector, IByteBuffer msg)
        {
            Console.WriteLine(ByteBufferUtil.HexDump(msg)); 
            base.DisposeRequest(connector, msg);
            KeepaliveTime = DateTime.Now;
        }
        //超时
        internal bool CheckTimeout()
        {
            var iSec = (DateTime.Now - KeepaliveTime).TotalSeconds;
            return (iSec > 180);
        }
    }
}

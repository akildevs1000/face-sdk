using DoNetDrive.Core;
using DoNetDrive.Core.Command;
using DoNetDrive.Core.Connector;
using DoNetDrive.Core.Connector.TCPServer;
using DoNetDrive.Core.Connector.UDP;
using DoNetDrive.Protocol.Door8800;
using DoNetDrive.Protocol.Fingerprint.Data.Transaction;
using DoNetDrive.Protocol.Fingerprint.SystemParameter;
using DoNetDrive.Protocol.Fingerprint.Transaction;
using DoNetDrive.Protocol.OnlineAccess;
using DoNetDrive.Protocol.Transaction;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace FCardProtocolAPI.Command
{
    public class ListenerDemo
    {
        //全局定义
        ConnectorAllocator _connectorAllocator = ConnectorAllocator.GetAllocator();
        string mServerIP = "192.168.1.110"; //本机IP
        int mServerPort = 9001;//本机端口
        ConcurrentDictionary<string, INConnectorDetail> ClientConnectorList = new();//用于保存设备的连接信息
        ConcurrentDictionary<string, string> DeviceList = new();//用于保存设备与连接信息管理
        /// <summary>
        /// 
        /// </summary>
        public ListenerDemo()
        {
            _connectorAllocator.ClientOnline += _connectorAllocator_ClientOnline;
            _connectorAllocator.ClientOffline += _connectorAllocator_ClientOffline;
            _connectorAllocator.TransactionMessage += _connectorAllocator_TransactionMessage;
        }
        /// <summary>
        /// 消息推送监听
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="EventData"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void _connectorAllocator_TransactionMessage(INConnectorDetail connector, DoNetDrive.Core.Data.INData EventData)
        {
            CheckOpenForciblyConnect(connector);//检查长连接是否打开
            Door8800Transaction fcTrn = EventData as Door8800Transaction;
            var SN = fcTrn.SN;
            AddDevice(connector, SN);//添加设备
            switch (fcTrn.CmdIndex)
            {
                case 0x01:
                    //0x01 识别记录 
                     var cardTransaction = EventData as CardTransaction;
                    break;
                case 0x04:
                    //0x04 体温记录
                    var bodyTemperatureTransaction = EventData as BodyTemperatureTransaction;
                    break;
                case 0x02:
                    //0x02门磁记录
                    var doorSensorTransaction = EventData as DoorSensorTransaction;
                    break;
                case 0x03:
                    //0x03 系统记录
                    var systemTransaction = EventData as SystemTransaction;
                    break;
                case 0x22:
                case 0xA0:
                    //保活消息和测试连接消息
                    //回复响应
                    var sndConntmsg = new SendConnectTestResponse(GetCommandDetail(SN));
                    _connectorAllocator.AddCommand(sndConntmsg);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 检查长连接是否打开
        /// </summary>
        /// <param name="connector"></param>
        private void CheckOpenForciblyConnect(INConnectorDetail connector)
        {
            var conn = _connectorAllocator.GetConnector(connector);
            if (conn != null)
            {
                if (!conn.IsForciblyConnect())
                {
                    conn.OpenForciblyConnect();//保存长连接
                }
            }
        }
        /// <summary>
        /// 添加设备
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="SN"></param>
        private void AddDevice(INConnectorDetail connector, string SN)
        {
            var key = connector.GetKey();
            if (ClientConnectorList.ContainsKey(key))
            {
                if (!DeviceList.ContainsKey(SN))
                {
                    DeviceList.TryAdd(SN, key);
                }
                else
                {
                    DeviceList[SN] = key;
                }
            }
        }

        /// <summary>
        /// 设备下线监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _connectorAllocator_ClientOffline(object sender, ServerEventArgs e)
        {
            INConnector inc = sender as INConnector;
            var key = inc.GetKey();
            if (ClientConnectorList.ContainsKey(key))
            {
                //删除连接对象
                ClientConnectorList.Remove(key, out _);
                Console.WriteLine("设备离线");
            }
        }
        /// <summary>
        /// 设备上线监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void _connectorAllocator_ClientOnline(object sender, ServerEventArgs e)
        {
            INConnector inc = sender as INConnector;
            var connectorType = inc.GetConnectorType();
            var key = inc.GetKey();
            if (connectorType == ConnectorType.TCPServerClient || connectorType == ConnectorType.UDPClient)
            {
                var fC8800Request = new Door8800RequestHandle(DotNetty.Buffers.UnpooledByteBufferAllocator.Default, RequestHandleFactory);
                inc.RemoveRequestHandle(typeof(Door8800RequestHandle));//先删除，防止已存在就无法添加。
                inc.AddRequestHandle(fC8800Request);
                if (!ClientConnectorList.ContainsKey(key))
                {
                    ClientConnectorList.TryAdd(key, inc.GetConnectorDetail());//添加连接对象
                }
            }
        }
        /// <summary>
        /// 解析器处理工厂
        /// </summary>
        /// <param name="sn"></param>
        /// <param name="cmdIndex"></param>
        /// <param name="cmdPar"></param>
        /// <returns></returns>
        private static AbstractTransaction RequestHandleFactory(string sn, byte cmdIndex, byte cmdPar)
        {
            if (cmdIndex >= 1 && cmdIndex <= 4)
            {    //记录消息          
                return ReadTransactionDatabaseByIndex.NewTransactionTable[cmdIndex]();
            }
            if (cmdIndex == 0x22)
            {
                //心跳消息
                return new DoNetDrive.Protocol.Door.Door8800.Data.Transaction.KeepaliveTransaction();
            }

            if (cmdIndex == 0xA0)
            {
                //连接测试消息
                return new DoNetDrive.Protocol.Door.Door8800.Data.Transaction.ConnectMessageTransaction();
            }
            return null;
        }
        /// <summary>
        /// 监听UDP
        /// </summary>
        public void ListenerUDP()
        {
            var udp = new UDPServerDetail(mServerIP, mServerPort); //创建UDPServer 对象
            _connectorAllocator.OpenForciblyConnect(udp);//启动UDP Server
        }
        /// <summary>
        /// 监听TCP
        /// </summary>
        public void ListenerTCP()
        {
            var tcp = new TCPServerDetail(mServerIP, mServerPort);
            _connectorAllocator.OpenForciblyConnect(tcp);//启动TCP Server
        }

        /// <summary>
        /// 获取命令详情对接
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        public INCommandDetail GetCommandDetail(string sn)
        {
            if (DeviceList.ContainsKey(sn))
            {
                var key = DeviceList[sn];
                if (ClientConnectorList.ContainsKey(key))
                {
                    var cnt = ClientConnectorList[key];
                    var result = new OnlineAccessCommandDetail(cnt, sn, "ffffffff");
                    return result;
                }
            }
            return default;
        }
    }
}

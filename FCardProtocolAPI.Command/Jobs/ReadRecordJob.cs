using DoNetDrive.Common.Extensions;
using DoNetDrive.Core.Command;
using DoNetDrive.Core.Connector;
using DoNetDrive.Protocol.Door.Door8800.Transaction;
using DoNetDrive.Protocol.Fingerprint.AdditionalData;
using DoNetDrive.Protocol.Transaction;
using FCardProtocolAPI.Command.Models;
using FCardProtocolAPI.Common;
using FluentScheduler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FCardProtocolAPI.Command.Jobs
{
    public class ReadRecordJob : IJob
    {
        public void Execute()
        {
            if (CommandAllocator.WebSockets.IsEmpty || CommandAllocator.ClientList.IsEmpty)
            {
                return;
            }
            var keys = CommandAllocator.ClientList.Keys;
            var tasks = new List<Task>();
            foreach (var key in keys)
            {
                if (CommandAllocator.ClientList.TryGetValue(key, out var value))
                {
                    if (string.IsNullOrWhiteSpace(value.SN))
                        continue;
                    tasks.Add(ReadRecord(detail: value));
                }
            }
            Task.WaitAll(tasks.ToArray());
        }
        /// <summary>
        /// 读取记录
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        public static async Task ReadRecord(MyConnectorDetail detail)
        {
            var commandName = CommandAllocator.GetDeviceType(detail.SN);
            if (commandName != CommandAllocator.Door8900HName && commandName != CommandAllocator.FingerprintCommandName)
            {
                return;
            }
            var cmdDtl = CommandAllocator.GetCommandDetail(detail.SN, detail.ConnectorDetail);
            var transactionList = new List<CardRecord>();
            TransactionDatabaseDetailBase readTransaction;
            if (commandName.Equals(CommandAllocator.Door8900HName))
            {
                readTransaction = new DoorDatabaseDetail(cmdDtl, detail.SN);
            }
            else
            {
                readTransaction = new FingerprintDatabaseDetail(cmdDtl, detail.SN);
            }
            var databaseDetail = await readTransaction.GetDatabaseDetail();
            if (databaseDetail == null)
                return;
            var records = await readTransaction.ReadRecord(databaseDetail);
            transactionList.AddRange(records);
            await Send(cmdDtl, transactionList);
        }
        /// <summary>
        /// 发送记录
        /// </summary>
        /// <param name="transactionList"></param>
        /// <returns></returns>
        private static async Task Send(INCommandDetail cmdDtl, List<CardRecord> transactionList)
        {
            if (!transactionList.Any())
            {
                return;
            }
            foreach (var item in transactionList)
            {
                TransactionType type = TransactionType.CardTransaction;
                if (item is FaceTransaction faceTransaction)
                {
                    type = TransactionType.FaceTransaction;
                    if (faceTransaction.Photo == 1)
                    {
                        faceTransaction.RecordImage = await ReadRecordImage(cmdDtl, (uint)faceTransaction.RecordNumber);
                    }
                }
                var result = CommandAllocator.GetResult(CommandStatus.Succeed, type, item);
                await SendTransactionToWebSocket(result);
                item.Release();
            }
        }
        /// <summary>
        /// 
        /// 发送记录到websocket
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private static async Task SendTransactionToWebSocket(IFcardCommandResult result)
        {
            var message = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result, CommandAllocator.Settings));
            var buf = new ArraySegment<byte>(message);
            var keys = CommandAllocator.WebSockets.Keys;
            foreach (var key in keys)
            {
                if (CommandAllocator.WebSockets.TryGetValue(key, out var webSocket))
                {
                    await webSocket.SendAsync(buf, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        /// <summary>
        /// 读取图片
        /// </summary>
        /// <param name="cmdDtl"></param>
        /// <param name="recordNumber"></param>
        /// <returns></returns>
        private async static Task<byte[]> ReadRecordImage(INCommandDetail cmdDtl, uint recordNumber)
        {
            var cmd = new ReadFile(cmdDtl, new ReadFile_Parameter(recordNumber, 3, 1));
            await CommandAllocator.Allocator.AddCommandAsync(cmd);
            if (cmd.getResult() is ReadFile_Result result && result.FileHandle != 0 && result.Result)
            {
                return result.FileDatas;
            }
            return null;
        }
    }
}

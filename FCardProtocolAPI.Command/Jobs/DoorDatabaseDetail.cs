using DoNetDrive.Common;
using DoNetDrive.Common.Extensions;
using DoNetDrive.Core.Command;
using DoNetDrive.Protocol.Door.Door8800.Data;
using DoNetDrive.Protocol.Door.Door8800.Transaction;
using FCardProtocolAPI.Command.Models;
using FCardProtocolAPI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FCardProtocolAPI.Command.Jobs
{
    public class DoorDatabaseDetail : TransactionDatabaseDetailBase
    {
        INCommandDetail cmdDtl;
        string sn;
        public DoorDatabaseDetail(INCommandDetail cmdDtl, string sn)
        {
            this.cmdDtl = cmdDtl;
            this.sn = sn;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<TransactionDatabaseDetail> GetDatabaseDetail()
        {
            try
            {
                var cmd = new ReadTransactionDatabaseDetail(cmdDtl);
                await CommandAllocator.Allocator.AddCommandAsync(cmd);
                var result = (ReadTransactionDatabaseDetail_Result)cmd.getResult();
                return result.DatabaseDetail;
            }
            catch
            {

                return null;
            }
        }

        public async Task<List<CardRecord>> ReadRecord(TransactionDatabaseDetail transactionDatabase)
        {
            var transactionList = new Dictionary<int, CardRecord>();
            for (int i = 0; i < 6; i++)
            {
                var transactionDetail = transactionDatabase.ListTransaction[i];
                if (transactionDetail.WriteIndex - transactionDetail.ReadIndex <= 0)
                {
                    continue;
                }
                int type = i + 1;
                var result = await ReadTransactionDataBase(type);
                if (result == null)
                    return transactionList.Values.ToList();
                foreach (var item in result.TransactionList)
                {
                    try
                    {
                        CardRecord record;
                        if (!transactionList.ContainsKey(item.SerialNumber))
                        {
                            transactionList.Add(item.SerialNumber, new CardRecord());
                        }
                        record = transactionList[item.SerialNumber];
                        record.RecordNumber = item.SerialNumber;
                        record.RecordDate = item.TransactionDate;
                        record.RecordType = item.TransactionType;
                        record.RecordCode = item.TransactionCode;
                        record.RecordMsg = MessageType.CardTransactionCodeList[item.TransactionType][item.TransactionCode];
                        record.SN = sn;
                        if (type == 1)
                        {
                            var transaction = item as DoNetDrive.Protocol.Door.Door89H.Data.CardTransaction;
                            record.Accesstype = (byte)(transaction.IsEnter() ? 1 : 2);
                            if (CommandAllocator.PasswordCodeTable.Contains(item.TransactionCode))
                            {
                                var buf = transaction.BigCard.toBytes(9);
                                record.CardData = StringUtil.ByteToHex(buf.Copy(0, 4));
                            }
                            else
                            {
                                record.CardData = transaction.BigCard.UInt64Value.ToString();
                            }
                            record.Door = transaction.DoorNum();
                        }
                    }
                    catch 
                    {
                    }
                }
            }
            return transactionList.Values.ToList();
        }

        private async Task<ReadTransactionDatabase_Result> ReadTransactionDataBase(int type)
        {
            try
            {
                var parameter = new ReadTransactionDatabase_Parameter((e_TransactionDatabaseType)type, 60);
                var cmd = new DoNetDrive.Protocol.Door.Door89H.Transaction.ReadTransactionDatabase(cmdDtl, parameter);
                await CommandAllocator.Allocator.AddCommandAsync(cmd);
                var result = (ReadTransactionDatabase_Result)cmd.getResult();
                return result;
            }
            catch
            {

                return null;
            }
        }
    }
}

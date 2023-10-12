using DoNetDrive.Common.Extensions;
using DoNetDrive.Core.Command;
using DoNetDrive.Protocol.Door.Door8800.Data;
using DoNetDrive.Protocol.Door.Door8800.Transaction;
using DoNetDrive.Protocol.Fingerprint.AdditionalData;
using DoNetDrive.Protocol.Transaction;
using FCardProtocolAPI.Command.Models;
using FCardProtocolAPI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCardProtocolAPI.Command.Jobs
{
    public class FingerprintDatabaseDetail : TransactionDatabaseDetailBase
    {
        INCommandDetail cmdDtl;
        string sn;
        public FingerprintDatabaseDetail(INCommandDetail cmdDtl, string sn)
        {
            this.cmdDtl = cmdDtl;
            this.sn = sn;
        }
        public async Task<TransactionDatabaseDetail> GetDatabaseDetail()
        {
            try
            {
                var cmd = new DoNetDrive.Protocol.Fingerprint.Transaction.ReadTransactionDatabaseDetail(cmdDtl);
                await CommandAllocator.Allocator.AddCommandAsync(cmd);
                var result = (DoNetDrive.Protocol.Fingerprint.Transaction.ReadTransactionDatabaseDetail_Result)cmd.getResult();
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
            for (int i = 0; i < 4; i++)
            {
                var transactionDetail = transactionDatabase.ListTransaction[i];
                if (transactionDetail.WriteIndex - transactionDetail.ReadIndex <= 0)
                {
                    continue;
                }
                int type = i + 1;
                var result = await ReadFaceTransactionDataBase(type);
                if (result == null)
                    return transactionList.Values.ToList();
                foreach (var item in result.TransactionList)
                {
                    try
                    {
                        FaceTransaction record;
                        if (!transactionList.ContainsKey(item.SerialNumber))
                        {
                            transactionList.Add(item.SerialNumber, new FaceTransaction());
                        }
                        record = (FaceTransaction)transactionList[item.SerialNumber];
                        record.SN = sn;
                        record.RecordCode = item.TransactionCode;
                        record.RecordNumber = item.SerialNumber;
                        record.RecordDate = item.TransactionDate;
                        if (record.RecordType == 0)
                            record.RecordType = item.TransactionType;
                        if (type != 4)
                        {
                            record.RecordMsg = MessageType.TransactionCodeNameList[item.TransactionType][item.TransactionCode];
                        }
                        if (type == 1)//认证记录
                        {
                            var cardtrn = (DoNetDrive.Protocol.Fingerprint.Data.Transaction.CardTransaction)item;
                            record.Accesstype = cardtrn.Accesstype;
                            record.Photo = cardtrn.Photo;
                            record.UserCode = cardtrn.UserCode;
                        }
                        else if (type == 4)//体温记录
                        {
                            var cardtrn = (DoNetDrive.Protocol.Fingerprint.Data.Transaction.BodyTemperatureTransaction)item;
                            record.BodyTemperature = (double)cardtrn.BodyTemperature / 10;
                            record.RecordDate = DateTime.Now;
                        }
                    }
                    catch { }
                }
            }
            return transactionList.Values.ToList();
        }


        /// <summary>
        /// 读取人脸指纹机记录
        /// </summary>
        /// <param name="cmdDtl"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private async Task<ReadTransactionDatabase_Result> ReadFaceTransactionDataBase(int type)
        {
            try
            {
                var parameter = new DoNetDrive.Protocol.Fingerprint.Transaction.ReadTransactionDatabase_Parameter(type, 60);
                var cmd = new DoNetDrive.Protocol.Fingerprint.Transaction.ReadTransactionDatabase(cmdDtl, parameter);
                await CommandAllocator.Allocator.AddCommandAsync(cmd);
                var result = (DoNetDrive.Protocol.Door.Door8800.Transaction.ReadTransactionDatabase_Result)cmd.getResult();
                return result;
            }
            catch
            {
                return null;
            }
        }




    }
}

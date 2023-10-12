using DoNetDrive.Core.Command;
using DoNetDrive.Protocol.Door.Door8800.Data;
using FCardProtocolAPI.Command.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCardProtocolAPI.Command.Jobs
{
    public interface TransactionDatabaseDetailBase
    {
        Task<TransactionDatabaseDetail> GetDatabaseDetail();

        Task<List<CardRecord>> ReadRecord(TransactionDatabaseDetail transactionDatabase);
    }
}

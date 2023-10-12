using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCardProtocolAPI.Command
{
    public enum TransactionType
    {
        /// <summary>
        /// 
        /// </summary>
        DefaultTransaction=0,
        /// <summary>
        /// 刷卡
        /// </summary>
        CardTransaction,
        /// <summary>
        /// 心跳
        /// </summary>
        KeepAliveTransaction,
        /// <summary>
        /// 人脸
        /// </summary>
        FaceTransaction
    }
}

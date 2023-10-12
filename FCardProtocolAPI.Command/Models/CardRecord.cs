using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCardProtocolAPI.Command.Models
{
    public class CardRecord
    {
        /// <summary>
        /// 记录编号
        /// </summary>
        public int RecordNumber { get; set; }
        /// <summary>
        /// 出入类型：1--表示进门；2--表示出门
        /// </summary>
        public byte Accesstype { get; set; }
        /// <summary>
        /// 记录时间
        /// </summary>
        public DateTime RecordDate { get; set; }
        /// <summary>
        /// 记录类型
        /// </summary>
        public int RecordType { get; set; }
        /// <summary>
        /// 记录消息
        /// </summary>
        public string RecordMsg { get; set; }
        /// <summary>
        /// 记录代码
        /// </summary>
        public int RecordCode { get; set; }
        /// <summary>
        /// 卡号
        /// </summary>
        public string CardData { get; set; }
        /// <summary>
        /// 门号
        /// </summary>
        public int Door { get; set; } = 1;
        public string SN { get; set; }
        public virtual void Release()
        {

        }

    }
}

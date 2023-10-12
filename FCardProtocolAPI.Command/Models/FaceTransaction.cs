using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCardProtocolAPI.Command.Models
{
    public class FaceTransaction : CardRecord
    {


        /// <summary>
        /// 用户号
        /// </summary>
        public uint UserCode { get; set; }

        /// <summary>
        /// 是否包含照片
        /// </summary>
        public byte Photo { get; set; }

        /// <summary>
        /// 记录图片
        /// </summary>
        public byte[] RecordImage { get; set; }


        /// <summary>
        /// 体温
        /// </summary>
        public double BodyTemperature { get; set; }

        public override void Release()
        {
            RecordImage = null;
        }

    }
}

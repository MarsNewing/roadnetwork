using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.RoadSign
{
    class Kerb
    {
        /// <summary>
        /// 规定了数据模型，请不要在其他类中直接读取数据
        /// </summary>
        //public const string KerbIDNm = "KerbID";
        //public const string ArcIDNm = "ArcID";
        //public const string SerialNm = "Serial";


        //public const string OtherNm = "Other";

        public const string KerbName = "Kerb";

        public int KerbID { get; set; }
        public int ArcID { get; set; }
        public int Serial { get; set; }


        public int Other { get; set; }

        /// <summary>
        /// 复制实体
        /// </summary>
        /// <returns></returns>
        public Kerb Copy()
        {
            Kerb kerb = new Kerb();

            kerb.KerbID = this.KerbID;
            kerb.Serial = this.Serial;
            kerb.ArcID = this.ArcID;

            kerb.Other = this.Other;

            return kerb;
        }
    }
}

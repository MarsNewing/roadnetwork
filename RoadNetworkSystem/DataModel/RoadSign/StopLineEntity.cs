using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.RoadSign
{
    class StopLineEntity
    {
        /// <summary>
        /// 规定了数据模型，请不要在其他类中直接读取数据
        /// </summary>
        //public const string StopLineIDNm = "StopLineID";
        //public const string NodeIDNm = "NodeID";
        //public const string ArcIDNm = "ArcID";

        //public const string LaneIDNm = "LaneID";
        //public const string StyleIDNm = "StyleID";
        //public const string OtherNm = "Other";
        public const string StopLineName = "StopLine";



        public int StopLineID { get; set; }
        public int NodeID { get; set; }
        public int ArcID { get; set; }

        public int LaneID { get; set; }
        public int StyleID { get; set; }
        public int Other { get; set; }

        /// <summary>
        /// 复制实体
        /// </summary>
        /// <returns></returns>
        public StopLineEntity Copy()
        {
            StopLineEntity sl = new StopLineEntity();

            sl.StopLineID = this.StopLineID;
            sl.NodeID = this.NodeID;
            sl.ArcID = this.ArcID;

            sl.LaneID = this.LaneID;
            sl.StyleID = this.StyleID;
            sl.Other = this.Other;

            return sl;
        }
    }
}

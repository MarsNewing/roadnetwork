using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.RoadSign
{
    class StopLine
    {
        /// <summary>
        /// 规定了数据模型，请不要在其他类中直接读取数据
        /// </summary>
        //public const string StopLineIDNm = "StopLineID";
        //public const string NodeIDNm = "NodeID";
        //public const string ArcIDNm = "ArcID";

        //public const string LaneIDNm = "LaneID";
        //public const string Boundary.STYLEID_NAME = "StyleID";
        //public const string OtherNm = "Other";
        public const string StopLineName = "StopLine";

        public const string StopLineIDNm = "StopLineID";
        public const string STYLEID_NAME = "StyleID";
        public const string NodeIDNm = "NodeID";


        public const string LaneIDNm = "LaneID";
        public const string ArcIDNm = "ArcID";
        public const string OtherNm = "Other";


        public const int STOPLINESTYLE = -247;

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
        public StopLine Copy()
        {
            StopLine sl = new StopLine();

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

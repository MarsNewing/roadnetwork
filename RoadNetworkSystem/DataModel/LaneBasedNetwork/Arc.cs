using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.LaneBasedNetwork
{
    public class Arc
    {
        public const string ArcFeatureName = "Arc";
        public int ArcID { get; set; }
        public int LaneNum { get; set; }
        public int FlowDir { get; set; }

        public int LinkID { get; set; }
        public int Other { get; set; }

        public const string ArcIDNm = "ArcID";
        public const string LaneNumNm = "LaneNum";
        public const string LinkIDNm = "LinkID";

        public const string FlowDirNm = "FlowDir";
        public const string OtherNm = "Other";

        /// <summary>
        /// 复制实体
        /// </summary>
        /// <returns></returns>
        public Arc Copy()
        {
            Arc arc = new Arc();

            arc.ArcID = this.ArcID;
            arc.LaneNum = this.LaneNum;
            arc.FlowDir = this.FlowDir;

            arc.LinkID = this.LinkID;
            arc.Other = this.Other;

            return arc;
        }
    }
}

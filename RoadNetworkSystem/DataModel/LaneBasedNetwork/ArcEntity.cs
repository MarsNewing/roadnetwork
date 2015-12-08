using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.LaneBasedNetwork
{
    class ArcEntity
    {
        public const string ArcFeatureName = "Arc";
        public int ArcID { get; set; }
        public int LaneNum { get; set; }
        public int FlowDir { get; set; }

        public int LinkID { get; set; }
        public int Other { get; set; }

        /// <summary>
        /// 复制实体
        /// </summary>
        /// <returns></returns>
        public ArcEntity Copy()
        {
            ArcEntity arc = new ArcEntity();

            arc.ArcID = this.ArcID;
            arc.LaneNum = this.LaneNum;
            arc.FlowDir = this.FlowDir;

            arc.LinkID = this.LinkID;
            arc.Other = this.Other;

            return arc;
        }
    }
}

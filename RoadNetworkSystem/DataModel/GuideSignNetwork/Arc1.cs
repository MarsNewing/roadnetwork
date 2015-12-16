using RoadNetworkSystem.DataModel.Road;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.GuideSignNetwork
{
    class Arc1:LinkMaster
    {
        public const string Arc1Name = "Arc1";
        /// <summary>
        /// 复制实体
        /// </summary>
        /// <returns></returns>
        public Arc1 Copy()
        {
            Arc1 Arc1 = new Arc1();
            Arc1.RoadType = this.RoadType;
            Arc1.RoadName = this.RoadName;
            Arc1.FNodeID = this.FNodeID;

            Arc1.TNodeID = this.TNodeID;
            Arc1.RelID = this.RelID;
            Arc1.ID = this.ID;

            Arc1.RoadLevel = this.RoadLevel;
            Arc1.FlowDir = this.FlowDir;

            Arc1.Other = this.Other;

            return Arc1;
        }
    }
}

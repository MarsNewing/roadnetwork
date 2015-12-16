using RoadNetworkSystem.DataModel.Road;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.LaneBasedNetwork
{
    class Link:LinkMaster
    {

        public const string LinkName = "Link";

        public const int FLOWDIR_DOUBLE = 0;
        public const int FLOWDIR_SAME = 1;
        public const int FLOWDIR_OPPOSITION = -1;

        //public int ID { get; set; }
        //public int FNodeID { get; set; }
        //public int TNodeID { get; set; }

        //public int RelID { get; set; }
        //public string RoadName { get; set; }
        //public int RoadType { get; set; }

        //public int RoadLevel { get; set; }
        //public int FlowDir { get; set; }
        //public int Other { get; set; }

        /// <summary>
        /// 复制实体
        /// </summary>
        /// <returns></returns>
        public Link Copy(LinkMaster linkMasterEty)
        {
            Link link = new Link();

            link.ID = linkMasterEty.ID;
            link.FNodeID = linkMasterEty.FNodeID;
            link.TNodeID = linkMasterEty.TNodeID;

            link.RelID = linkMasterEty.RelID;
            link.RoadType = linkMasterEty.RoadType;
            link.RoadName = linkMasterEty.RoadName;

            link.FlowDir = linkMasterEty.FlowDir;
            link.Other = linkMasterEty.Other;

            return link;
        }

        public enum 道路类型 { 
            高速路=0,
            快速路=1,
            主干道=2,
            次干道=3,
            支路=4,
            辅道=5,
            匝道=6,
            渠化道=7 
        }
    }
}

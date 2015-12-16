using RoadNetworkSystem.DataModel.Road;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.LaneBasedNetwork
{
    class Segment:LinkMaster
    {
        public const string SegmentName = "Segment";

        /// <summary>
        /// 规定了数据模型，请不要在其他类中直接读取数据
        /// </summary>
        //public const string RoadSegmentIDNm = "RoadSegmentID";
        //public const string FSegmentNodeIDNm = "FSegmentNodeID";
        //public const string TSegmentNodeIDNm = "TSegmentNodeID";
        //public const string RoadTypeNm = "RoadType";
        //public const string FlowDirNm = "TrafficDir";
        //public const string RoadIDNm = "RoadID";
        //public const string RoadNameNm = "RoadName";
        //public const string OtherNm = "Other";

        //public int ID { get; set; }
        //public int FNodeID { get; set; }
        //public int TNodeID { get; set; }

        //public string RoadName { get; set; }
        //public int RoadType { get; set; }
        //public int RelID { get; set; }

        //public int FlowDir { get; set; }
        //public int RoadLevel { get; set; }
        //public int Other { get; set; }

        /// <summary>
        /// 复制实体
        /// </summary>
        /// <returns></returns>
        public Segment Copy(LinkMaster linkMasterEty)
        {
            Segment link = new Segment();

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
    }
}

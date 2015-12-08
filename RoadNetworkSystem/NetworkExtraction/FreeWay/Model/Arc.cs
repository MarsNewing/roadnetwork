using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.NetworkExtraction.FreeWay.Model
{
    class Arc
    {
        public int ArcID { get; set; }
        public int FArcNodeID { get; set; }
        public int TArcNodeID { get; set; }


        public int LaneNum { get; set; }
        public int RoadID { get; set; }
        public int RoadType { get; set; }

        public string DistrictName { get; set; }
        public int SegmentNum { get; set; }


        public const string FEATURE_ARC_NAME =      "Arc";

        public const string FIELDE_ARC_ID =        "ArcID";
        public const string FIELDE_FARC_NODE_ID =   "FNodeID";
        public const string FIELDE_TARC_NODE_ID =   "TNodeID";

        public const string FIELDE_LANE_NUM =       "LaneNum";
        public const string FIELDE_ROAD_ID =        "RoadID";
        public const string FIELDE_ROAD_TYPE =      "RoadType";

        public const string FIELDE_DISTRICT_NAME =  "DistrictName";
        public const string FIELDE_SEGMENT_NUM =    "SegmentNum";
    }
}

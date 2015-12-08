using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.NetworkExtraction.FreeWay.Model
{
    class SegNode
    {
        public Int32 SegNodeID { get; set; }
        public String SegNodeLandmark { get; set; }
        public Int32 RoadID { get; set; }

        public const string FEATURE_SEGNODE_NAME =          "SegNode";

        public const string FIELDE_SEG_NODE_ID =            "SegNodeID";
        public const string FIELDE_SEG_NODE_LANEMARK_ID =   "SegNodeLandmark";
        public const string FIELDE_ROAD_ID =                "RoadID";

    }
}

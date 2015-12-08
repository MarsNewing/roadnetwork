using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.NetworkExtraction.FreeWay.Model
{
    class Node
    {

        public int NodeID { get; set; }
        public string NodeType { get; set; }
        public int AdjArcAngs { get; set; }

        public string AdjArcIDs { get; set; }
        public string ConRoadName { get; set; }

        public const string FEATURE_NODE_NAME =     "Node";

        public const string FIELDE_NODE_ID =        "NodeID";
        public const string FIELDE_NODE_TYPE =      "NodeType";

        public const string FIELDE_ADJ_ARC_ANGS =   "AdjArcAngs";
        public const string FIELDE_ADJ_ARC_IDS =    "AdjArcIDs";

    }
}

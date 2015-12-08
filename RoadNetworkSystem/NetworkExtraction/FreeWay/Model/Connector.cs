using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.NetworkExtraction.FreeWay.Model
{
    class Connector
    {
        public int ConnectorID { get; set; }
        public int FromLaneID { get; set; }
        public int ToLaneID { get; set; }

        public int NodeID { get; set; }

        public const string FEATURE_CONNECTOR = "Connector";

        public const string FIELDE_CONNECTOR_ID = "ConnectorID";

        public const string FIELDE_FROM_LANE_ID = "FromLaneID";
        public const string FIELDE_TO_LANE_ID = "ToLaneID";
        public const string FIELDE_NODE_ID = "NodeID";
    }
}

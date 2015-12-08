using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.NetworkExtraction.FreeWay.Model
{
    class Road
    {
        public int RoadID { get; set; }
        public string RoadName { get; set; }
        public int RoadCode { get; set; }
       
        public int FlowDir { get; set; }
        public int RoadType { get; set; }
        public int Other { get; set; }

        public const string FEATURE_ROAD_NAME =     "Road";
        
        public const string FIELDE_ROAD_ID =        "RoadID";
        public const string FIELDE_ROAD_NAME =      "RoadName";
        public const string FIELDE_ROAD_CODE =      "RoadCode";

        public const string FIELDE_FLOW_DIR =       "FlowDir";
        public const string FIELDE_ROAD_TYPE =      "RoadType";
        public const string FIELDE_OTHER =          "Other";


        public enum ROAD_TYPE_ENUM
        {
            高速路 = 0,
            快速路 = 1,
            主干道 = 2,
            次干道 = 3,
            支路 = 4,
            辅道 = 5,
            匝道 = 6,
            渠化道 = 7 
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.NetworkExtraction.FreeWay.Model
{
    class Lane
    {
        public int LaneID { get; set; }
        public int Type { get; set; }
        public int SegmentID { get; set; }

        public int Serial { get; set; }

        public const string FEATURE_LANE = "Lane";

        public const string FIELDE_LANE_ID = "LaneID";
        public const string FIELDE_TYPE = "Type";
        public const string FIELDE_SEGMENT_ID = "SegmentID";
        public const string FIELDE_SERIAL = "Serial";

        public enum LANE_TYPE
        {
            普通车道,
            应急车道
        }
    }
}

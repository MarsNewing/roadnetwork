using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.NetworkExtraction.FreeWay.Model
{
    class Segment
    {

        public Int32 SegmentID { get; set; }
        public Int32 FSegNodeID { get; set; }
        public Int32 TSegNodeID { get; set; }

        public Int32 SegType { get; set; }
        public Int32 ArcID { get; set; }
        public Int32 ArcSerial { get; set; }
        
        public Int32 LaneNum { get; set; }


        public const string FEATURE_SEGMENT_NAME =  "Segment";

        public const string FIELDE_SEGMENT_ID =     "SegmentID";
        public const string FIELDE_FSEG_NODE_ID =   "FSegNodeID";
        public const string FIELDE_TSEG_NODE_ID =   "TSegNodeID";

        public const string FIELDE_SEG_TYPE =       "SegType";
        public const string FIELDE_ARC_ID =         "ArcID";
        public const string FIELDE_ARC_SERIAL =     "ArcSerial";

        public const string FIELDE_ARC_LANENUM =     "LaneNum";

    }
}

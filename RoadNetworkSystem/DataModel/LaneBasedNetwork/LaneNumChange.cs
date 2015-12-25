using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.DataModel.LaneBasedNetwork
{
    class LaneNumChange
    {
        public const string LaneNumChangeName = "LaneNumChange";
        public int FromBreakPointID { get; set; }
        public int ToBreakPointID { get; set; }
        public int LaneNum { get; set; }

        public int RoadID { get; set; }
        public int DoneFlag { get; set; }
        public int FlowDir { get; set; }

        public const string FromBreakPointID_Name = "FromBreakPointID";
        public const string ToBreakPointID_Name = "ToBreakPointID";
        public const string LaneNum_Name = "LaneNum";

        public const string RoadID_Name = "RoadID";
        public const string DoneFlag_Name = "DoneFlag";
        public const string FlowDir_Name = "FlowDir";

        public const int DONEFLAG_UNDO = 0;
        public const int DONEFLAG_DONE = 1;


    }
}

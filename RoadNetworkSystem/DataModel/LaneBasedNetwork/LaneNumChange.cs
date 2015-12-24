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
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.Road
{
    class LinkMasterEntity
    {
        public int ID { get; set; }
        public int FNodeID { get; set; }
        public int TNodeID { get; set; }

        
        public string RoadName { get; set; }
        public int RelID { get; set; }
        public int RoadType { get; set; }

        public int RoadLevel { get; set; }
        public int FlowDir { get; set; }
        public int Other { get; set; }
    }

}

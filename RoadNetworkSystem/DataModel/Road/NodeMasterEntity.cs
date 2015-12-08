using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.Road
{
    public class NodeMasterEntity
    {

        public int ID { get; set; }
        public int CompositeType { get; set; }
        public int NodeType { get; set; }

        public string AdjIDs { get; set; }
        public string NorthAngles { get; set; }
        public string ConnState { get; set; }

        public int Other { get; set; }
             

      }
}

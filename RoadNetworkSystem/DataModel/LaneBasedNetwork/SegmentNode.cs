using RoadNetworkSystem.DataModel.Road;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.LaneBasedNetwork
{
    public class SegmentNode : NodeMaster
    {
        public const string RoadSegmentNodeName = "SegmentNode";
        //public int ID { get; set; }
        //public int CompositeType { get; set; }
        //public int NodeType { get; set; }

        //public string AdjIDs { get; set; }
        //public string NorthAngles { get; set; }
        //public string ConnState { get; set; }

        //public int Other { get; set; }

        /// <summary>
        /// 复制实体
        /// </summary>
        /// <returns></returns>
        public SegmentNode Copy(NodeMaster nodeMasterEty)
        {
            SegmentNode Node = new SegmentNode();
            Node.ConnState = nodeMasterEty.ConnState;
            Node.AdjIDs = nodeMasterEty.AdjIDs;
            Node.CompositeType = nodeMasterEty.CompositeType;

            Node.NodeType = nodeMasterEty.NodeType;
            Node.NorthAngles = nodeMasterEty.NorthAngles;
            Node.ID = nodeMasterEty.ID;

            Node.Other = nodeMasterEty.Other;

            return Node;
        }
    }
}

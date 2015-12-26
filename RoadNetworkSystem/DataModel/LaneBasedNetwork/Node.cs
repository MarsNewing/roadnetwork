using RoadNetworkSystem.DataModel.Road;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.LaneBasedNetwork
{
     public class Node : NodeMaster
    {
         public const string NodeName = "Node";

         public const int NODE_TYPE_JUNCDTION = 1;
         public const int NODE_TYPE_NO_JUNCDTION = 0;

         public const int NODE_TYPE_PLANE_INTERSECTION = 1;
         public const int NODE_TYPE_PLANE_OVERPASS = 2;

        /// <summary>
        /// 复制实体
        /// </summary>
        /// <returns></returns>
        public Node Copy(NodeMaster nodeMasterEty)
        {
            Node Node = new Node();
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

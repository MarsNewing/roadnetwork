using RoadNetworkSystem.DataModel.Road;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.LaneBasedNetwork
{
     public class Node : NodeMaster
    {
         public const string NodeName = "Node";



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

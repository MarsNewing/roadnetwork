using RoadNetworkSystem.DataModel.Road;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.LaneBasedNetwork
{
     public class NodeEntity : NodeMasterEntity
    {
         public const string NodeName = "Node";



        /// <summary>
        /// 复制实体
        /// </summary>
        /// <returns></returns>
        public NodeEntity Copy(NodeMasterEntity nodeMasterEty)
        {
            NodeEntity Node = new NodeEntity();
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

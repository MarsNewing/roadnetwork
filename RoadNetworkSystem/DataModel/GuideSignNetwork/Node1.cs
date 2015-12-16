using RoadNetworkSystem.DataModel.Road;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.GuideSignNetwork
{
    class Node1 : NodeMaster
    {

        public const string Node1Name = "Node1";

        /// <summary>
        /// 复制实体
        /// </summary>
        /// <returns></returns>
        public Node1 Copy()
        {
            Node1 Node = new Node1();
            Node.ConnState = this.ConnState;
            Node.AdjIDs = this.AdjIDs;
            Node.CompositeType = this.CompositeType;

            Node.NodeType = this.NodeType;
            Node.NorthAngles = this.NorthAngles;
            Node.ID = this.ID;

            Node.Other = this.Other;

            return Node;
        }
    }
}

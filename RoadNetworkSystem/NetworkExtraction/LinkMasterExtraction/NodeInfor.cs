using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.NetworkExtraction.LinkMasterExtraction
{
    /// <summary>
    /// Node的信息，包含点、道路类型集、是否是交叉口、NodeID
    /// </summary>
    class NodeInfor
    {
        public NodeInfor(int nodeID, ESRI.ArcGIS.Geometry.IPoint pPt, string roadtypeset, Boolean isInterchange)
        {

            this.Point = pPt;
            this.RoadTypeSet = roadtypeset;
            this.IsJunction = isInterchange;
            this.NodeID = nodeID;
        }

        /// <summary>
        /// 节点的几何
        /// </summary>
        public ESRI.ArcGIS.Geometry.IPoint Point { get; set; }

        /// <summary>
        /// 结点类型,记录结点所属路段的RoadType
        /// </summary>
        public string RoadTypeSet { get; set; }

        /// <summary>
        /// 是否为交叉口
        /// </summary>
        public Boolean IsJunction { get; set; }

        /// <summary>
        /// 结点的ID
        /// </summary>
        public int NodeID { get; set; }
    }
}

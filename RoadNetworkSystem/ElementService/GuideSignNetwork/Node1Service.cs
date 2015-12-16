using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.NetworkElement.MasterLayer;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.NetworkElement.GuideSignNetwork
{
    class Node1Service:NodeMasterService
    {

        private IFeatureClass _pFeaClsNode1;
        private int _id;

        public Node1Service(IFeatureClass pFeaClsNode1,int node1ID,IPoint pnt):base(pFeaClsNode1,node1ID,pnt)
        {
            _pFeaClsNode1 = pFeaClsNode1;
            _id = node1ID;

            base.NodeIDNm = "Node1ID";
            base.CompositeTypeNm = "CompositeType";
            base.NodeTypeNm = "NodeType";

            base.AdjIDsNm = "AdjArc1IDs";
            base.NorthAnglesNm = "NorthAngles";
            base.ConnStateNm = "ConnState";

            base.OtherNm = "Other";

            if (node1ID > 0)
            {
                base.NodeMasterFea = GetFeature();
            }
            else
            {
                base.NodeMasterFea = null;
            }

        }

        
    }
}

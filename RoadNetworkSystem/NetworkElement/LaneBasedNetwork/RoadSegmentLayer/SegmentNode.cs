using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.NetworkElement.MasterLayer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace RoadNetworkSystem.NetworkElement.LaneBasedNetwork.RoadSegmentLayer
{
    class SegmentNode:NodeMaster
    {
        

        private IFeatureClass _pFeaClsRSNode;
        private int _id;

        public SegmentNode(IFeatureClass pFeaClsRSNode,int rsNodeID,IPoint pnt):base(pFeaClsRSNode,rsNodeID,pnt)
        {
            try
            {
                if (pFeaClsRSNode != null)
                {
                    _pFeaClsRSNode = pFeaClsRSNode;
                }
                else
                {
                    _pFeaClsRSNode = null;
                }

                _id = rsNodeID;

                base.NodeIDNm = "SegmentNodeID";
                base.CompositeTypeNm = "CompositeType";
                base.NodeTypeNm = "NodeType";

                base.AdjIDsNm = "AdjSegmentIDs";
                base.NorthAnglesNm = "NorthAngles";
                base.ConnStateNm = "ConnState";

                base.OtherNm = "Other";
                if (rsNodeID > 0)
                {
                    base.NodeMasterFea = GetFeature();
                }
                else
                {
                    base.NodeMasterFea = null;
                }
            }
            catch(Exception ex) {MessageBox.Show(ex.ToString()); }
        }


      
    }
}

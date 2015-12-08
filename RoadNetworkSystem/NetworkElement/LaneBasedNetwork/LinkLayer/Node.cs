using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.NetworkElement.MasterLayer;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer
{
    class Node:NodeMaster
    {
        private IFeatureClass _pFeaClsNode;
        private int _id;
       
        public Node(IFeatureClass pFeaClsNode,int nodeID,IPoint pnt):base(pFeaClsNode,nodeID,pnt)
        {
            _pFeaClsNode = pFeaClsNode;
            _id = nodeID;

            base.NodeIDNm = "NodeID";
            base.CompositeTypeNm = "CompositeType";
            base.NodeTypeNm = "NodeType";

            base.AdjIDsNm = "AdjLinkIDs";
            base.NorthAnglesNm = "NorthAngles";
            base.ConnStateNm = "ConnState";

            base.OtherNm = "Other";
            if (nodeID > 0)
            {
                base.NodeMasterFea = GetFeature();
            }
            else
            {
                base.NodeMasterFea = null;
            }
        }

        public void UpdateConnState()
        {

            
            IFeature pFeatureNode = GetFeature();
            string adjLink = Convert.ToString(pFeatureNode.get_Value(_pFeaClsNode.FindField(AdjIDsNm)));
            string[] adjLinkArr = adjLink.Split('\\');
            string conState = "";
            //LaneConnWorkSpace laneConnMngr = new LaneConnWorkSpace(_mdbPath);

            //for (int i = 0; i < adjLinkArr.Length; i++)
            //{
            //    int curLinkID = Convert.ToInt32(adjLinkArr[i]);
            //    for (int j = 0; j < adjLinkArr.Length; j++)
            //    {
            //        int nextLink = Convert.ToInt32(adjLinkArr[j]);
            //        bool existedFlag = laneConnMngr.IsExistedConnByLink(curLinkID, nextLink);
            //        char conFlag;
            //        if (existedFlag == true)
            //        {
            //            conFlag = '1';
            //        }
            //        else
            //        {
            //            conFlag = '0';
            //        }
            //        conState = conState + conFlag;
            //    }
            //}
            pFeatureNode.set_Value(_pFeaClsNode.FindField("LinkConnState"), conState);
            pFeatureNode.Store();
        }
    }
}

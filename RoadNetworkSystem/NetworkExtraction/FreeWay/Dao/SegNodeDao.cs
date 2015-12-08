using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.NetworkExtraction.FreeWay.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.NetworkExtraction.FreeWay.Dao
{
    class SegNodeDao
    {
        private IFeatureClass _pFeaCls;
        public SegNodeDao(IFeatureClass pFeaCls)
        {
            _pFeaCls = pFeaCls;
        }

        public int CreateSegNode(SegNode segNode,IPoint pnt)
        {
            IFeature pFeature = _pFeaCls.CreateFeature();

            pFeature.set_Value(_pFeaCls.FindField(SegNode.FIELDE_SEG_NODE_ID), pFeature.OID);
            if (segNode.RoadID != null)
            {
                pFeature.set_Value(_pFeaCls.FindField(SegNode.FIELDE_ROAD_ID), segNode.RoadID);
            }
            if (segNode.SegNodeLandmark != null)
            {
                pFeature.set_Value(_pFeaCls.FindField(SegNode.FIELDE_SEG_NODE_LANEMARK_ID), segNode.SegNodeLandmark);
            }
            pFeature.Shape = pnt;
            pFeature.Store();
            return pFeature.OID;
        }


        public SegNode GetSegNode(IFeature pFeature)
        {

            SegNode segNode = new SegNode();
            segNode.SegNodeID = Convert.ToInt32(pFeature.get_Value(_pFeaCls.FindField(SegNode.FIELDE_SEG_NODE_ID)));
            segNode.RoadID = Convert.ToInt32(pFeature.get_Value(_pFeaCls.FindField(SegNode.FIELDE_ROAD_ID)));
            segNode.SegNodeLandmark = Convert.ToString(pFeature.get_Value(_pFeaCls.FindField(SegNode.FIELDE_SEG_NODE_LANEMARK_ID)));
            return segNode;
        }
    }
}

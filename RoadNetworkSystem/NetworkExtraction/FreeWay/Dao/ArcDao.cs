using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoadNetworkSystem.NetworkExtraction.FreeWay.Model;

namespace ArcNetworkSystem.NetworkExtraction.FreeWay.Dao
{
    class ArcDao
    {
         private IFeatureClass _pFeaCls;
         public ArcDao(IFeatureClass pFeaCls)
        {
            _pFeaCls = pFeaCls;
        }

        public Arc getArc(IFeature pFeature)
        {
            Arc arc = new Arc();
            arc.ArcID = Convert.ToInt32(pFeature.get_Value(_pFeaCls.FindField(Arc.FIELDE_ARC_ID)));
            arc.DistrictName = Convert.ToString(pFeature.get_Value(_pFeaCls.FindField(Arc.FIELDE_DISTRICT_NAME)));
            arc.FArcNodeID = Convert.ToInt32(pFeature.get_Value(_pFeaCls.FindField(Arc.FIELDE_FARC_NODE_ID)));

            arc.LaneNum = Convert.ToInt32(pFeature.get_Value(_pFeaCls.FindField(Arc.FIELDE_LANE_NUM)));
            arc.RoadID = Convert.ToInt32(pFeature.get_Value(_pFeaCls.FindField(Arc.FIELDE_ROAD_ID)));

            return arc;
        }
    }
}

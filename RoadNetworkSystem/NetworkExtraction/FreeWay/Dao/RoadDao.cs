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
    class RoadDao
    {
         private IFeatureClass _pFeaCls;
         public RoadDao(IFeatureClass pFeaCls)
        {
            _pFeaCls = pFeaCls;
        }

        public Road getRoad(IFeature pFeature)
        {
            Road road = new Road();
            road.FlowDir = Convert.ToInt32(pFeature.get_Value(_pFeaCls.FindField(Road.FIELDE_FLOW_DIR)));
            road.RoadName = Convert.ToString(pFeature.get_Value(_pFeaCls.FindField(Road.FIELDE_ROAD_NAME)));
            if(_pFeaCls.FindField(Road.FIELDE_ROAD_NAME) >= 0 )
            {
                //road.RoadCode = Convert.ToInt32(pFeature.get_Value(_pFeaCls.FindField(Road.FIELDE_ROAD_CODE)));
            }
            road.RoadID = Convert.ToInt32(pFeature.get_Value(_pFeaCls.FindField(Road.FIELDE_ROAD_ID)));
            road.RoadType = Convert.ToInt32(pFeature.get_Value(_pFeaCls.FindField(Road.FIELDE_ROAD_TYPE)));

            return road;
        }


        public IFeature getRoadFeature(int roadId)
        {
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = Road.FIELDE_ROAD_ID + " = " + roadId;
            IFeatureCursor cursor = _pFeaCls.Search(filter, false);
            IFeature fea = cursor.NextFeature();
            return fea;
        }
    }
}

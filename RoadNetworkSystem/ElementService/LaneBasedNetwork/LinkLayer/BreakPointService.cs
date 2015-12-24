using ESRI.ArcGIS.Geodatabase;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.ElementService.LaneBasedNetwork.LinkLayer
{
    class BreakPointService
    {
        IFeatureClass _pFeaClsBreakPoint;
        int _breakPointId;
        public BreakPointService(IFeatureClass pFeaClsBreakPoint,int breakPointId)
        {
            _pFeaClsBreakPoint = pFeaClsBreakPoint;
            _breakPointId = breakPointId;
        }

        public IFeature getBreakPointFeature()
        {
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = BreakPoint.BreakPointIDName + " = " + _breakPointId;
            IFeatureCursor cursor = _pFeaClsBreakPoint.Search(filter, false);
            return cursor.NextFeature();
        }

        public BreakPoint getBreakPoint(IFeature pFeature)
        {
            BreakPoint breakPoint = new BreakPoint();
            breakPoint.BreakPointID = Convert.ToInt32(pFeature.get_Value(pFeature.Fields.FindField(BreakPoint.BreakPointIDName)));
            return breakPoint;
        }
    }
}

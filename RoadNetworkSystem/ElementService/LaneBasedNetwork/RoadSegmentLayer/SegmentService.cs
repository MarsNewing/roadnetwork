using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.NetworkElement.MasterLayer;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.NetworkElement.LaneBasedNetwork.RoadSegmentLayer
{
    class SegmentService:LinkMasterService
    {

        private IFeatureClass _pFeaClsRoadSegment;
        private int _id;

        public SegmentService(IFeatureClass pFeaClsRoadSegment, int Id):base(pFeaClsRoadSegment,Id)
        {
            _pFeaClsRoadSegment = pFeaClsRoadSegment;
            _id = Id;
            
            base.IDNm = "SegmentID";
            base.FNodeIDNm = "FSegNodeID";
            base.TNodeIDNm = "TSegNodeID";

            base.RoadTypeNm = "RoadType";
            base.FlowDirNm = "FlowDir";
            base.RelIDNm = "RoadID";

            base.RoadNameNm = "RoadName";
            base.OtherNm = "Other";
            if (Id <= 0)
            {
                base.FeaLink = null;
            }
            else
            {
                base.FeaLink = GetFeature();
                
            }
        }

    }
}

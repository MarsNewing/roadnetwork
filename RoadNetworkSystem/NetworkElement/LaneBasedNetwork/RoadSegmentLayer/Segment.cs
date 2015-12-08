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
    class Segment:LinkMaster
    {

        private IFeatureClass _pFeaClsRoadSegment;
        private int _id;

        public Segment(IFeatureClass pFeaClsRoadSegment, int Id):base(pFeaClsRoadSegment,Id)
        {
            _pFeaClsRoadSegment = pFeaClsRoadSegment;
            _id = Id;
            
            base.IDNm = "RoadSegmentID";
            base.FNodeIDNm = "FSegmentNodeID";
            base.TNodeIDNm = "TSegmentNodeID";

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

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.NetworkElement.MasterLayer;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer
{
    class LinkService:LinkMasterService
    {
         
        public IFeatureClass _pFeaClsLink;
        public int _linkId;

        public LinkService(IFeatureClass pFeaClsLink,int linkId):base(pFeaClsLink,linkId)
        {
            _pFeaClsLink = pFeaClsLink;
            _linkId = linkId;
            base.IDNm = "LinkID";
            base.FNodeIDNm = "FNodeID";
            base.TNodeIDNm = "TNodeID";

            base.RelIDNm = "RoadSegmentID";
            base.RoadNameNm = "RoadName";
            base.RoadTypeNm = "RoadType";

            base.FlowDirNm = "FlowDir";
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

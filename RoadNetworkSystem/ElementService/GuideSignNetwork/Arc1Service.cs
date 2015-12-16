using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.GuideSignNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.NetworkElement.MasterLayer;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.NetworkElement.GuideSignNetwork
{
    class Arc1Service:LinkMasterService
    {

        private IFeatureClass _pFeaClsArc1;
        private int _arc1Id;

        public Arc1Service(IFeatureClass pFeaClsArc1,int arc1Id):base(pFeaClsArc1,arc1Id)
        {
            _pFeaClsArc1 = pFeaClsArc1;
            _arc1Id = arc1Id;

            base.IDNm = "Arc1ID";
            base.FNodeIDNm = "FNode1ID";
            base.TNodeIDNm = "TNode1ID";

            base.RoadTypeNm = "RoadType";
            base.RoadLevelNm = "RoadLevel";
            base.FlowDirNm = "FlowDir";

            base.RelIDNm = "RoadID";
            base.RoadNameNm = "RoadName";
            base.LengthNm = "Length";
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

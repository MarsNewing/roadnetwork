using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer;
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

        /// <summary>
        /// 把Link打断成几段，新生成的Link属性不变，只是几何变化，并且不生成和修改Node
        /// </summary>
        /// <param name="preLink"></param>
        /// <param name="lines"></param>
        public void breakLinkIntoLinksWithoutNodeUpdate(IFeature preLinkFeature, List<IPolyline> lines,
            IFeatureClass pFeaClsArc)
        {
            
            LinkMaster linkMstr = GetEntity(preLinkFeature);

            Link link = new Link();
            link = link.Copy(linkMstr);

            //找到原始Link对应的Arc
            Arc sameArc = null;
            Arc oppositionArc = null;
            IFeature sameArcFeature = null;
            IFeature oppositionArcFeature = null;

            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = IDNm + " = " + link.ID;
            IFeatureCursor cursor = pFeaClsArc.Search(filter, false);
            IFeature arcFeature = cursor.NextFeature();
            while (arcFeature != null)
            {
                ArcService arcService = new ArcService(pFeaClsArc, 0);
                Arc arc = arcService.GetArcEty(arcFeature);
                if (arc.FlowDir == Link.FLOWDIR_SAME)
                {
                    sameArc = arc;
                    sameArcFeature = arcFeature;
                }
                else
                {
                    oppositionArc = arc;
                    oppositionArcFeature = arcFeature;
                }
                arcFeature = cursor.NextFeature();
            }

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

            foreach (IPolyline line in lines)
            {
                if (line.Length == 0)
                {
                    continue;
                }
                IFeature temLinkFeature = Create(link, line);
                IPolyline temLinkLine = temLinkFeature.Shape as IPolyline;
                ArcService sameArcService = new ArcService(pFeaClsArc, 0);
                if(sameArc != null)
                {
                   IPolyline sameArcLine = LineHelper.CreateLineByLRS(temLinkLine,sameArc.LaneNum *Lane.LANE_WEIDTH/2,
                       ArcService.ARC_CUT_PERCENTAGE*temLinkLine.Length,ArcService.ARC_CUT_PERCENTAGE*temLinkLine.Length);
                   sameArcService.CreateArc(sameArc, sameArcLine);
                }
            }

            preLinkFeature.Delete();
            if (sameArcFeature != null)
            {
                sameArcFeature.Delete();
            }
            if (oppositionArcFeature != null)
            {
                oppositionArcFeature.Delete();
            }
        }

    }

}

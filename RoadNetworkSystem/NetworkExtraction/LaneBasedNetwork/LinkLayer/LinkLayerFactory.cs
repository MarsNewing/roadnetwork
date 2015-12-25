using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.NetworkExtraction.LaneBasedNetwork.LinkLayer
{
    class LinkLayerFactory
    {
        IFeatureClass _pFeaClsLink;
        IFeatureClass _pFeaClsArc;
        IFeatureClass _pFeaClsNode;

        const double ARC_CUT_PERCENTAGE = 0.05;
        const double LANE_WEIDTH = 3.5;

        public LinkLayerFactory(IFeatureClass pFeaClsLink,IFeatureClass pFeaClsNode,IFeatureClass pFeaClsArc)
        {
            _pFeaClsLink = pFeaClsLink;
            _pFeaClsNode = pFeaClsNode;
            _pFeaClsArc = pFeaClsArc;
        }

        /// <summary>
        /// 线创建Link和Arc
        /// </summary>
        /// <param name="link"></param>
        /// <param name="linkLine"></param>
        /// <param name="sameDirLaneNum"></param>
        /// <param name="oppoDirLaneNum"></param>
        public void createLinkAndArcs(Link link, IPolyline linkLine, int sameDirLaneNum, int oppoDirLaneNum)
        {
            LinkService linkService = new LinkService(_pFeaClsLink, link.ID);
            IFeature linkFeature = linkService.Create(link, linkLine);
            LinkMaster linkMaster = linkService.GetEntity(linkFeature);
            link = link.Copy(linkMaster);

            if (sameDirLaneNum > 0)
            {
                Arc sameArc = new Arc();
                sameArc.LinkID = link.ID;
                sameArc.LaneNum = sameDirLaneNum;
                sameArc.FlowDir = Link.FLOWDIR_SAME;

                ArcService arcService = new ArcService(_pFeaClsArc, 0);
                IPolyline sameArcLine = LineHelper.CreateLineByLRS(linkLine, sameDirLaneNum * LANE_WEIDTH / 2,
                    linkLine.Length * ARC_CUT_PERCENTAGE, linkLine.Length * ARC_CUT_PERCENTAGE);
                IFeature sameArcFeature = arcService.CreateArc(sameArc, sameArcLine);

            }


            if (oppoDirLaneNum > 0)
            {
                Arc oppositionArc = new Arc();
                oppositionArc.LinkID = link.ID;
                oppositionArc.LaneNum = sameDirLaneNum;
                oppositionArc.FlowDir = Link.FLOWDIR_SAME;

                ArcService arcService = new ArcService(_pFeaClsArc, 0);
                IPolyline oppositionArcLine = LineHelper.CreateLineByLRS(linkLine, -oppoDirLaneNum * LANE_WEIDTH / 2,
                    linkLine.Length * ARC_CUT_PERCENTAGE, linkLine.Length * ARC_CUT_PERCENTAGE);
                IFeature oppositionArcFeature = arcService.CreateArc(oppositionArc, oppositionArcLine);
            }
        }
    }
}

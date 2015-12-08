using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.NetworkExtraction.FreeWay.Dao;
using RoadNetworkSystem.NetworkExtraction.FreeWay.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.NetworkExtraction.FreeWay.Service
{
    /// <summary>
    /// 其他道路Arc（非高速公路）Arc转换成Segment，Node转换成SegNode
    /// Road已经被打断（里程桩、Node处）
    /// </summary>
    class OtherRoadSegmentation
    {
        private IFeatureClass pFeaClsRoad;
        private IFeatureClass pFeaClsSegment;
        private IFeatureClass pFeaClsSegNode;

        private IFeatureClass pFeaClsNode;
        private IFeatureClass pFeaClsArc;

        public OtherRoadSegmentation(Dictionary<string, IFeatureClass> feaClsDic)
        {
            pFeaClsRoad = feaClsDic[Road.FEATURE_ROAD_NAME];
            pFeaClsSegment = feaClsDic[Segment.FEATURE_SEGMENT_NAME];
            pFeaClsSegNode = feaClsDic[SegNode.FEATURE_SEGNODE_NAME];

            pFeaClsNode = feaClsDic[Node.FEATURE_NODE_NAME];
            pFeaClsArc = feaClsDic[Arc.FEATURE_ARC_NAME];
        }


        public void SegmentOtherRoad()
        {
            IQueryFilter filter = new QueryFilterClass();

            Road.ROAD_TYPE_ENUM e = Road.ROAD_TYPE_ENUM.高速路;

            filter.WhereClause = Arc.FIELDE_ROAD_TYPE + "<>" + Array.IndexOf(Enum.GetValues(e.GetType()), e);
            IFeatureCursor cursor = pFeaClsArc.Search(filter, false);
            IFeature pFeature = cursor.NextFeature();

            while (pFeature != null)
            {

                int arcId = Convert.ToInt32(pFeature.get_Value(pFeaClsArc.FindField(Arc.FIELDE_ARC_ID)));

                IPolyline line = pFeature.Shape as IPolyline;
                IPoint fPnt = line.FromPoint;
                IPoint tPnt = line.ToPoint;


                int fSegNodeId = checkSegNodeExist(fPnt);
                int tSegNodeId = checkSegNodeExist(tPnt);

                if(fSegNodeId == -1)
                {

                    SegNode fSegNode = new SegNode();
                    fSegNode.RoadID = Convert.ToInt32(pFeature.get_Value(pFeaClsArc.FindField(Arc.FIELDE_ROAD_ID)));
                    SegNodeDao segNodeDao = new SegNodeDao(pFeaClsSegNode);
                    fSegNodeId = segNodeDao.CreateSegNode(fSegNode,fPnt);

                }

                
                
                if (tSegNodeId == -1)
                {
                    SegNode tSegNode = new SegNode();
                    tSegNode.RoadID = Convert.ToInt32(pFeature.get_Value(pFeaClsArc.FindField(Arc.FIELDE_ROAD_ID)));
                    SegNodeDao segNodeDao = new SegNodeDao(pFeaClsSegNode);
                    tSegNodeId = segNodeDao.CreateSegNode(tSegNode, tPnt);
                }



                Segment segment = new Segment();
                segment.ArcID = arcId;
                segment.FSegNodeID = fSegNodeId;
                segment.TSegNodeID = tSegNodeId;
                segment.LaneNum = Convert.ToInt32(pFeature.get_Value(pFeaClsArc.FindField(Arc.FIELDE_LANE_NUM)));
                SegmentDao segDao = new SegmentDao(pFeaClsSegment);
                segDao.CreateSegment(segment, pFeature.Shape as IPolyline);
                pFeature = cursor.NextFeature();
            }
        }


        private int checkSegNodeExist(IPoint pnt)
        {

            double yuzhi = 0.00001;

            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = "";
            IFeatureCursor cursor = pFeaClsSegNode.Search(filter, false);
            IFeature pFeature = cursor.NextFeature();

            while (pFeature != null)
            {
                IPoint temPnt = pFeature.Shape as IPoint;
                if ((temPnt.X - pnt.X < yuzhi && temPnt.X - pnt.X > -yuzhi) && (temPnt.Y - pnt.Y < yuzhi) && (temPnt.Y - pnt.Y > -yuzhi ))
                {
                    return Convert.ToInt32(pFeature.get_Value(pFeaClsSegNode.FindField(SegNode.FIELDE_SEG_NODE_ID)));
                }
                pFeature = cursor.NextFeature();
            }
            return -1;
        }

    }
}

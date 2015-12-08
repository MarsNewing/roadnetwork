using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.GIS.Geometry;
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
    /// 在Node点处给Segment分段
    /// </summary>
    class NodeSegmetation
    {
       
        private IFeatureClass pFeaClsRoad;
        private IFeatureClass pFeaClsSegment;
        private IFeatureClass pFeaClsSegNode;

        private IFeatureClass pFeaClsNode;
        private IFeatureClass pFeaClsArc;

        public NodeSegmetation(Dictionary<string, IFeatureClass> feaClsDic)
        {
            pFeaClsRoad = feaClsDic[Road.FEATURE_ROAD_NAME];
            pFeaClsSegment = feaClsDic[Segment.FEATURE_SEGMENT_NAME];
            pFeaClsSegNode = feaClsDic[SegNode.FEATURE_SEGNODE_NAME];

            pFeaClsNode = feaClsDic[Node.FEATURE_NODE_NAME];
            pFeaClsArc = feaClsDic[Arc.FEATURE_ARC_NAME];
        }


        /// <summary>
        /// 利用Node打断Segment
        /// </summary>
        public void SegmentSegment()
        {
            /*
             *  1.遍历所有的Node，找到在某个阈值范围内的Segment集
             *  2.找到在Segment集中，筛选出Node所在的Segment
             *  3.打断Segment，生成新的SegNode，生成新的两段Segment（注意更新fsegnodeid，tsegnodeid）
             */


            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = "";
            IFeatureCursor cursor = pFeaClsNode.Search(filter,false);
            IFeature nodeFea = cursor.NextFeature();
            while (nodeFea != null)
            {
                List<IFeature> nearFeature = PhysicalTopology.GetFeaInCircle(pFeaClsSegment, nodeFea.Shape as IPoint, 1000);

                if (nearFeature == null || nearFeature.Count == 0)
                {
                    nodeFea = cursor.NextFeature();
                    continue;
                }

                IFeature onLineSegmentFea = null;
                foreach (IFeature pfeature in nearFeature)
                {

                    if (nodeFea.OID == 25)
                    {
                        int test = 0;
                    }

                    bool onLineFlag = PhysicalTopology.IsPointOnGon(pfeature, nodeFea);
                    if (onLineFlag == true)
                    {
                        onLineSegmentFea = pfeature;
                        breakSegment(onLineSegmentFea, nodeFea);
                        break;
                    }
                }

                nodeFea = cursor.NextFeature();
                
            }
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }


        private void breakSegment(IFeature segFeature, IFeature nodeFeature)
        {

            IPolyline preLine = new PolylineClass();
            IPolyline postLine = new PolylineClass();
            LineHelper.CutLineAtPoint(segFeature.Shape as IPolyline, nodeFeature.Shape as IPoint, out preLine, out postLine);

            int fSegNodeId0 = Convert.ToInt32(segFeature.get_Value(pFeaClsSegment.FindField(Segment.FIELDE_FSEG_NODE_ID)));
            int tSegNodeId0 = Convert.ToInt32(segFeature.get_Value(pFeaClsSegment.FindField(Segment.FIELDE_TSEG_NODE_ID)));

            SegNode segNode = new SegNode();
            SegNodeDao segNodeDao = new SegNodeDao(pFeaClsSegNode);

            int segNodeId = segNodeDao.CreateSegNode(segNode, nodeFeature.Shape as IPoint);

            Segment preSegment = new Segment();
            preSegment.TSegNodeID = segNodeId;
            preSegment.FSegNodeID = fSegNodeId0;
            SegmentDao segDao = new SegmentDao(pFeaClsSegment);
            segDao.CreateSegment(preSegment, preLine);

            Segment postSegment = new Segment();
            postSegment.FSegNodeID = segNodeId;
            postSegment.TSegNodeID = tSegNodeId0;
            segDao.CreateSegment(postSegment, postLine);


            //删除Segment
            segFeature.Delete();
        }

        private List<IFeature> findSegmentInCircle(IFeature nodeFeature)
        {
            return null;
        }
    }
}

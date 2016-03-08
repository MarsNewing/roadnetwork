using ArcNetworkSystem.NetworkExtraction.FreeWay.Dao;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.GIS.GeoDatabase.Dataset;
using RoadNetworkSystem.GIS.Geometry;
using RoadNetworkSystem.NetworkExtraction.FreeWay.Dao;
using RoadNetworkSystem.NetworkExtraction.FreeWay.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RoadNetworkSystem.NetworkExtraction.FreeWay.Service
{
    class Segmentation
    {
        private IFeatureClass pFeaClsRoad;
        private IFeatureClass pFeaClsSegment;
        private IFeatureClass pFeaClsSegNode;

        private IFeatureClass pFeaClsNode;
        private IFeatureClass pFeaClsArc;


        Dictionary<String, IFeatureClass> feaClsDic;

        public Segmentation(string mdbPath)
        {
            initFeatureClass(mdbPath);
        }

        private void initFeatureClass(string mdbPath)
        {
            feaClsDic = new Dictionary<string, IFeatureClass>();
            List<IFeatureClass> feaClsList = new List<IFeatureClass>();
            feaClsList.Add(pFeaClsRoad);
            feaClsList.Add(pFeaClsSegment);
            feaClsList.Add(pFeaClsSegNode);

            feaClsList.Add(pFeaClsNode);
            feaClsList.Add(pFeaClsArc);

            List<string> feaNames = new List<string>();
            feaNames.Add(Road.FEATURE_ROAD_NAME);
            feaNames.Add(Segment.FEATURE_SEGMENT_NAME);
            feaNames.Add(SegNode.FEATURE_SEGNODE_NAME);

            feaNames.Add(Node.FEATURE_NODE_NAME);
            feaNames.Add(Arc.FEATURE_ARC_NAME);

            feaClsList = FeatureClassHelper.GetFeaClsesInAccess(mdbPath, feaNames);
            pFeaClsRoad = feaClsList[0];
            pFeaClsSegment = feaClsList[1];
            pFeaClsSegNode = feaClsList[2];

            pFeaClsNode = feaClsList[3];
            pFeaClsArc = feaClsList[4];


            feaClsDic.Add(Road.FEATURE_ROAD_NAME, pFeaClsRoad);
            feaClsDic.Add(Segment.FEATURE_SEGMENT_NAME, pFeaClsSegment);
            feaClsDic.Add(SegNode.FEATURE_SEGNODE_NAME, pFeaClsSegNode);


            feaClsDic.Add(Node.FEATURE_NODE_NAME, pFeaClsNode);
            feaClsDic.Add(Arc.FEATURE_ARC_NAME, pFeaClsArc);
        }

        public void SegmentRoad()
        {
            MileSegmentation mileSegmentation = new MileSegmentation(feaClsDic);
            mileSegmentation.SegmentRoad();

            NodeSegmetation nodesegmentation = new NodeSegmetation(feaClsDic);
            nodesegmentation.SegmentSegment();

            //更新打断后生成Segment的ArcId和LaneNum
            updateSegmentationLaneNum();

            OtherRoadSegmentation otherRoadSegmentation = new OtherRoadSegmentation(feaClsDic);
            otherRoadSegmentation.SegmentOtherRoad();
        }


        private void updateSegmentationLaneNum()
        {

            SegmentDao segDao =  new SegmentDao(pFeaClsSegment);
            ArcDao arcDao = new ArcDao(pFeaClsArc);

            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = "";
            IFeatureCursor cursor = pFeaClsSegment.Search(filter, false);
            IFeature pfeature = cursor.NextFeature();
            while (pfeature != null)
            {
                IPolyline line = pfeature.Shape as IPolyline;
                IPointCollection pntClt = line as IPointCollection;
                IPoint middlePnt;
                if (pntClt.PointCount > 2)
                {
                    middlePnt = pntClt.get_Point(((int)pntClt.PointCount / 2));
                }
                else
                {
                    middlePnt = line.FromPoint;
                }

                IFeature nearestArc = null;
                Double distance = -1;
                PhysicalTopology.GetClosestFeature(pFeaClsArc, middlePnt, ref nearestArc, ref distance);

                if (distance > 10)
                {
                    MessageBox.Show(pfeature.OID.ToString());
                }


                if (nearestArc != null && PhysicalTopology.IsPointOnGon(nearestArc, pfeature))
                {
                    
                    Segment segment = segDao.getSegment(pfeature);
                    Arc arc = arcDao.getArc(nearestArc);
                    segment.LaneNum = arc.LaneNum;
                    segment.ArcID = arc.ArcID;

                    segDao.updateSegment(segment, pfeature);
                }
                pfeature = cursor.NextFeature();
            }
        }
    }
}

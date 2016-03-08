using ESRI.ArcGIS.Geodatabase;
using RoadNetworkSystem.ADO.Access;
using RoadNetworkSystem.GIS.GeoDatabase.Dataset;
using RoadNetworkSystem.NetworkExtraction.FreeWay.Dao;
using RoadNetworkSystem.NetworkExtraction.FreeWay.Model;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.NetworkExtraction.FreeWay.Service
{
    class LaneService
    {
        private IFeatureClass pFeaClsRoad;
        private IFeatureClass pFeaClsSegment;
        private IFeatureClass pFeaClsSegNode;

        private IFeatureClass pFeaClsNode;
        private IFeatureClass pFeaClsArc;

        private static OleDbConnection _conn;
        public LaneService(string mdbPath)
        {
            initFeatureClass(mdbPath);
            if (_conn == null)
            {
                _conn = AccessHelper.OpenConnection(mdbPath);
            }
        }

        private void initFeatureClass(string mdbPath)
        {
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

        }


        

        public void CreateLane()
        {

            SegmentDao segmentDao = new SegmentDao(pFeaClsSegment);

            IDataset ds = pFeaClsSegment.FeatureDataset;
            string path = ds.Workspace.PathName;

            LaneDao laneDao = new LaneDao(_conn);
            //创建Lane车道表（手动）
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = "";
            IFeatureCursor cursor = pFeaClsSegment.Search(filter, false);
            IFeature pFeature = cursor.NextFeature();
            while (pFeature != null)
            {
                Segment segment = segmentDao.getSegment(pFeature);
                for (int i = 0; i < segment.LaneNum; i++)
                {
                    Lane lane = new Lane();
                    lane.SegmentID = segment.SegmentID;
                    if (i == segment.LaneNum - 1)
                    {
                        Lane.LANE_TYPE e = Lane.LANE_TYPE.应急车道;
                        //System.Enum.GetName(typeof(LinkEntity.道路类型), o).ToString();
                        lane.Type = Array.IndexOf(Enum.GetValues(e.GetType()), e) + 1;
                    }
                    else
                    {
                        Lane.LANE_TYPE e = Lane.LANE_TYPE.普通车道;
                        //System.Enum.GetName(typeof(LinkEntity.道路类型), o).ToString();
                        lane.Type = Array.IndexOf(Enum.GetValues(e.GetType()), e) + 1;
                    }
                    lane.Serial = i + 1;
                    laneDao.InsertLane(lane);
                }
                pFeature = cursor.NextFeature();
            }
        }
    }
}

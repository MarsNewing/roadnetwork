using ArcNetworkSystem.NetworkExtraction.FreeWay.Dao;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.ADO.Access;
using RoadNetworkSystem.GIS.GeoDatabase.Dataset;
using RoadNetworkSystem.GIS.Geometry;
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
    class ConnectorService
    {
        private IFeatureClass pFeaClsRoad;
        private IFeatureClass pFeaClsSegment;
        private IFeatureClass pFeaClsSegNode;

        private IFeatureClass pFeaClsNode;
        private IFeatureClass pFeaClsArc;

        private ITable pTableConntion;
        private static OleDbConnection connection;

        public ConnectorService(string mdbPath)
        {
            if (connection != null)
            {
                connection = AccessHelper.OpenConnection(mdbPath);
            }
            initFeatureClass(mdbPath);
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

            feaClsList = FeatureClassHelper.GetFeaClsInAccess(mdbPath, feaNames);
            pFeaClsRoad = feaClsList[0];
            pFeaClsSegment = feaClsList[1];
            pFeaClsSegNode = feaClsList[2];

            pFeaClsNode = feaClsList[3];
            pFeaClsArc = feaClsList[4];


            pTableConntion = FeatureClassHelper.GetTableByName(mdbPath, "Connection");
        }

        public void CreateConnector()
        {

            ConnectorDao connectorDao = new ConnectorDao(connection);

            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = "";
            ICursor cursor = pTableConntion.Search(filter, false);
            IRow row = cursor.NextRow();
            while (row != null)
            {
                int fromArcId = Convert.ToInt32(row.get_Value(pTableConntion.FindField("FromArcID")));
                int toArcId = Convert.ToInt32(row.get_Value(pTableConntion.FindField("ToArcID")));
                int nodeId = Convert.ToInt32(row.get_Value(pTableConntion.FindField("NodeID")));

                IFeature fromArcFea = pFeaClsArc.GetFeature(fromArcId);
                IFeature toArcFea = pFeaClsArc.GetFeature(toArcId);
                ArcDao arcDao = new ArcDao(pFeaClsArc);
                RoadDao roadDao = new RoadDao(pFeaClsRoad);

                Arc fromArc = arcDao.getArc(fromArcFea);
                Arc toArc = arcDao.getArc(toArcFea);

                int fromRoadID = fromArc.RoadID;
                int toRoadID = toArc.RoadID;

                
                Segment fromSegment = getFromSegment((fromArcFea.Shape as IPolyline).ToPoint,fromArc.ArcID);
                Segment toSegment = getToSegment((toArcFea.Shape as IPolyline).FromPoint, toArc.ArcID);

                if (fromSegment == null || toSegment == null)
                {
                    row = cursor.NextRow();
                    continue;
                }

                IFeature fromRoadFea = roadDao.getRoadFeature(fromRoadID);
                if(fromRoadFea == null)
                {
                    row = cursor.NextRow();
                    continue;
                }
                Road fromRoad = roadDao.getRoad(fromRoadFea);

                IFeature toRoadFea = roadDao.getRoadFeature(toRoadID);
                if (toRoadFea == null)
                {
                    row = cursor.NextRow();
                    continue;
                }
                Road toRoad = roadDao.getRoad(toRoadFea);


                //主流路线
                if (fromRoad.RoadID == toRoadID ||
                    ((fromRoad.RoadName != null ||
                        fromRoad.RoadName.Equals("") == false) &&
                            (fromRoad.RoadName.Equals(toRoad.RoadName) ||
                            fromRoad.RoadName.StartsWith(toRoad.RoadName))))
                {
                    for (int i = 1; i < fromArc.LaneNum - 1; i++)
                    {
                        Connector connector = new Connector();
                        connector.NodeID = nodeId;

                        Lane fromLane = (new LaneDao(connection)).SearchLane(fromSegment.SegmentID, i);

                        connector.FromLaneID = fromLane.LaneID;

                        Lane toLane = (new LaneDao(connection)).SearchLane(toSegment.SegmentID, i);
                        connector.FromLaneID = toLane.LaneID;

                        connectorDao.InsertLane(connector);
                    }
                }
                else
                {
                        Connector connector = new Connector();
                        connector.NodeID = nodeId;

                        Lane fromLane = (new LaneDao(connection)).SearchLane(fromSegment.SegmentID, fromArc.LaneNum - 1);

                        connector.FromLaneID = fromLane.LaneID;

                        Lane toLane = (new LaneDao(connection)).SearchLane(toSegment.SegmentID, 1);
                        connector.FromLaneID = toLane.LaneID;

                        connectorDao.InsertLane(connector);
                }

                row = cursor.NextRow();
            }
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

        }

        private Segment getFromSegment(IPoint nodePnt,int arcId)
        {
            SegmentDao segDao = new SegmentDao(pFeaClsSegment);

            IFeature segNodeFea = null;
            double distance = 0;
            
            PhysicalTopology.GetClosestFeature(pFeaClsSegNode, nodePnt, ref segNodeFea, ref distance);

            if (segNodeFea == null)
                return null;
            SegNodeDao segNodeDao = new SegNodeDao(pFeaClsSegNode);

            SegNode segNode =segNodeDao.GetSegNode(segNodeFea);

            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = Segment.FIELDE_TSEG_NODE_ID + " = "+segNode.SegNodeID;
            IFeatureCursor cusror = pFeaClsSegment.Search(filter,false);
            IFeature pfeature = cusror.NextFeature();
            while (pfeature != null)
            {
                Segment seg = segDao.getSegment(pfeature);
                if (seg.ArcID == arcId)
                {
                    return seg;
                }
                pfeature = cusror.NextFeature();
                
            }
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

            return null;
        }


        private Segment getToSegment(IPoint node, int arcId)
        {
            IFeature segNodeFea = null;
            double distance = 0;
            SegmentDao segDao = new SegmentDao(pFeaClsSegment);

            PhysicalTopology.GetClosestFeature(pFeaClsSegNode, node, ref segNodeFea, ref distance);
            SegNodeDao segNodeDao = new SegNodeDao(pFeaClsSegNode);


            if (segNodeFea == null)
                return null;

            SegNode segNode = segNodeDao.GetSegNode(segNodeFea);

            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = Segment.FIELDE_FSEG_NODE_ID + " = " + segNode.SegNodeID;
            IFeatureCursor cusror = pFeaClsSegment.Search(filter, false);
            IFeature pfeature = cusror.NextFeature();
            while (pfeature != null)
            {
                Segment seg = segDao.getSegment(pfeature);
                if (seg.ArcID == arcId)
                {
                    return seg;
                }
                pfeature = cusror.NextFeature();

            }
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

            return null;
        }
    }
}

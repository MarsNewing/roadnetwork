using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.RoadSegmentLayer;
using RoadNetworkSystem.NetworkElement.RoadLayer;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.ElementService.LaneBasedNetwork.LinkLayer
{
    class LaneNumChangeService
    {
        private OleDbConnection _conn;
        
        public LaneNumChangeService(OleDbConnection conn)
        {
            _conn = conn;
        }

        /// <summary>
        /// 获取给定起始和终止BreakPoint的LaneNumChange
        /// </summary>
        /// <param name="fromBreakPointId"></param>
        /// <param name="toBreakPointId"></param>
        /// <returns></returns>
        public LaneNumChange GetLaneNumChange(int fromBreakPointId, int toBreakPointId)
        {
                string sql = "Select * from " + LaneNumChange.LaneNumChangeName +
                    " where " + LaneNumChange.FromBreakPointID_Name + " = " + fromBreakPointId +
                    " and  " + LaneNumChange.ToBreakPointID_Name + " = " + toBreakPointId;
                OleDbCommand cmd = new OleDbCommand(sql, _conn);
                OleDbDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    int laneNumChangeId = Convert.ToInt32(reader[LaneNumChange.LaneNumChangeID_Name]);
                    int laneNum = Convert.ToInt32(reader[LaneNumChange.LaneNum_Name]);
                    int done = Convert.ToInt32(reader[LaneNumChange.DoneFlag_Name]);
                    int flowDir = Convert.ToInt32(reader[LaneNumChange.FlowDir_Name]);

                    LaneNumChange laneNumChange = new LaneNumChange();
                    laneNumChange.LaneNumChangeID = laneNumChangeId;
                    laneNumChange.FromBreakPointID = fromBreakPointId;
                    laneNumChange.ToBreakPointID = toBreakPointId;
                    laneNumChange.LaneNum = laneNum;
                    laneNumChange.DoneFlag = done;

                    laneNumChange.FlowDir = flowDir;

                    reader.Close();
                    reader.Dispose();
                    return laneNumChange;
                }
                else
                {
                    reader.Close();
                    reader.Dispose();
                    cmd.Dispose();

                    return null;
                }

        }
        

        /// <summary>
        /// 获取给定起始和终止BreakPoint的相反方向的LaneNumChange
        /// </summary>
        /// <param name="fromBreakPointId"></param>
        /// <param name="toBreakPointId"></param>
        /// <returns></returns>
        public LaneNumChange GetOppositeDirectionLaneNumChange(int segmentId,int oppositionDir,int fromBreakPointId, int toBreakPointId)
        {
            string sql;
            if (fromBreakPointId > 0 && toBreakPointId > 0)
            {
                sql = "Select * from " + LaneNumChange.LaneNumChangeName +
                " where " + LaneNumChange.FromBreakPointID_Name + " = " + toBreakPointId +
                " and  " + LaneNumChange.ToBreakPointID_Name + " = " + fromBreakPointId +
                " and  " + LaneNumChange.FlowDir_Name + " = " + oppositionDir +
                " and  " + LaneNumChange.DoneFlag_Name + " = " + LaneNumChange.DONEFLAG_UNDO;
            }
            else if (fromBreakPointId > 0)
            {
                sql = "Select * from " + LaneNumChange.LaneNumChangeName +
                " where " + LaneNumChange.SegmentID_Name + " = " + segmentId +
                " and  " + LaneNumChange.ToBreakPointID_Name + " = " + fromBreakPointId +
                " and  " + LaneNumChange.FlowDir_Name + " = " + oppositionDir +
                " and  " + LaneNumChange.DoneFlag_Name + " = " + LaneNumChange.DONEFLAG_UNDO;
            }
            else if (toBreakPointId > 0)
            {
                sql = "Select * from " + LaneNumChange.LaneNumChangeName +
                " where " + LaneNumChange.FromBreakPointID_Name + " = " + toBreakPointId +
                " and  " + LaneNumChange.SegmentID_Name + " = " + segmentId +
                " and  " + LaneNumChange.FlowDir_Name + " = " + oppositionDir +
                " and  " + LaneNumChange.DoneFlag_Name + " = " + LaneNumChange.DONEFLAG_UNDO;
            }
            else
            {
                sql = "Select * from " + LaneNumChange.LaneNumChangeName +
                " where " + LaneNumChange.SegmentID_Name + " = " + segmentId +
                " and  " + LaneNumChange.FlowDir_Name + " = " + oppositionDir +
                " and  " + LaneNumChange.DoneFlag_Name + " = " + LaneNumChange.DONEFLAG_UNDO;
            }
            OleDbCommand cmd = new OleDbCommand(sql, _conn);
            OleDbDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int laneNum = Convert.ToInt32(reader[LaneNumChange.LaneNum_Name]);
                int done = Convert.ToInt32(reader[LaneNumChange.DoneFlag_Name]);

                int flowDir = Convert.ToInt32(reader[LaneNumChange.FlowDir_Name]);
                LaneNumChange laneNumChange = new LaneNumChange();

                laneNumChange.LaneNumChangeID = Convert.ToInt32(reader[LaneNumChange.LaneNumChangeID_Name]);
                if (reader[LaneNumChange.ToBreakPointID_Name] != DBNull.Value)
                {
                    laneNumChange.ToBreakPointID = Convert.ToInt32(reader[LaneNumChange.ToBreakPointID_Name]);
                }

                if (reader[LaneNumChange.FromBreakPointID_Name] != DBNull.Value)
                {
                    laneNumChange.FromBreakPointID = Convert.ToInt32(reader[LaneNumChange.FromBreakPointID_Name]);
                }
                
                
                
                laneNumChange.LaneNum = laneNum;
                laneNumChange.DoneFlag = done;
                laneNumChange.SegmentID = segmentId;

                laneNumChange.FlowDir = flowDir;
                reader.Close();
                reader.Dispose();
                return laneNumChange;
            }
            else
            {
                reader.Close();
                reader.Dispose();
                cmd.Dispose();

                return null;
            }

        }

        /// <summary>
        /// 判断LaneNumChange表示的方向与Road的数字化方向是否相同
        /// </summary>
        /// <param name="roadLine"></param>
        /// <param name="currentLaneNumChange"></param>
        /// <returns></returns>
        public bool isCurrentLaneNumChangeSameDirection(IPolyline roadLine, 
            LaneNumChange currentLaneNumChange,IFeatureClass pFeaClsBreakPoint)
        {
            IPoint fromPointPoint = new PointClass();


            IPoint toPointPoint = new PointClass();
            BreakPointService breakPointService = new BreakPointService(pFeaClsBreakPoint, 0);
            breakPointService.getBreakPointPoints(roadLine, currentLaneNumChange.FromBreakPointID, currentLaneNumChange.ToBreakPointID, 
                currentLaneNumChange.FlowDir == Link.FLOWDIR_SAME ? true : false,
                ref fromPointPoint, ref toPointPoint);

            double vector_x_LaneNumChange = toPointPoint.X - fromPointPoint.X;
            double vector_y_LaneNumChange = toPointPoint.Y - fromPointPoint.Y;

            double vector_x_Road = roadLine.ToPoint.X - roadLine.FromPoint.X;
            double vector_y_Road = roadLine.ToPoint.Y - roadLine.FromPoint.Y;

            double product = vector_x_LaneNumChange * vector_x_Road + vector_y_LaneNumChange * vector_y_Road;
            if (product > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public void UpdateLaneNumChangeDoneFlag(LaneNumChange laneNumChange)
        {
            string sql = "Update " + LaneNumChange.LaneNumChangeName +
                " set " + LaneNumChange.DoneFlag_Name + " = " + laneNumChange.DoneFlag +
                " where " + LaneNumChange.LaneNumChangeID_Name + " = " + laneNumChange.LaneNumChangeID;
            OleDbCommand cmd = new OleDbCommand(sql, _conn);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }



        public void CreateLaneNumChangesFromSegment(IFeatureClass FeaClsSegment)
        {
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = "";

            IFeatureCursor cursor = FeaClsSegment.Search(filter, false);
            IFeature pFeature = cursor.NextFeature();
            int laneNumChangeId = 1;
            while (pFeature != null)
            {
                
                SegmentService segmentService = new SegmentService(FeaClsSegment, 0);
                LinkMaster linkMaster = segmentService.GetEntity(pFeature);
                int segmentId = linkMaster.ID;
                int laneNum = RoadService.GetRoadLaneNumDefault(linkMaster.RoadType);
                string sql = "Insert into " + LaneNumChange.LaneNumChangeName +
                    " (" + LaneNumChange.LaneNumChangeID_Name + "," +
                    LaneNumChange.FlowDir_Name +        "," +
                    LaneNumChange.DoneFlag_Name +       "," +
                    LaneNumChange.LaneNum_Name +        "," +
                    LaneNumChange.SegmentID_Name +        
                    ")" + " Values (" +
                    laneNumChangeId +               "," +
                    1 +                             "," +
                    LaneNumChange.DONEFLAG_UNDO +   "," +
                    laneNum +                       "," +
                    segmentId +                        ")";

                OleDbCommand cmd = new OleDbCommand(sql,_conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();

                laneNumChangeId++;

                sql = "Insert into " + LaneNumChange.LaneNumChangeName +
                    " (" + LaneNumChange.LaneNumChangeID_Name + "," +
                    LaneNumChange.FlowDir_Name + "," +
                    LaneNumChange.DoneFlag_Name + "," +
                    LaneNumChange.LaneNum_Name + "," +
                    LaneNumChange.SegmentID_Name +
                    ")" + " Values (" +
                    laneNumChangeId + "," +
                    -1 + "," +
                    LaneNumChange.DONEFLAG_UNDO + "," +
                    laneNum + "," +
                    segmentId + ")";
                cmd = new OleDbCommand(sql, _conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                laneNumChangeId++;
                pFeature = cursor.NextFeature();
            }
        }
    }
}

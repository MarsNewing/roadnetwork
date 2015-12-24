using RoadNetworkSystem.DataModel.LaneBasedNetwork;
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
                    int laneNum = Convert.ToInt32(reader[LaneNumChange.LaneNum_Name]);
                    int done = Convert.ToInt32(reader[LaneNumChange.DoneFlag_Name]);

                    LaneNumChange laneNumChange = new LaneNumChange();
                    laneNumChange.FromBreakPointID = fromBreakPointId;
                    laneNumChange.ToBreakPointID = toBreakPointId;
                    laneNumChange.LaneNum = laneNum;
                    laneNumChange.DoneFlag = done;


                    reader.Close();
                    reader.Dispose();
                    return laneNumChange;
                }
                else
                {
                    reader.Close();
                    reader.Dispose();

                    return null;
                }

        }
        

        /// <summary>
        /// 获取给定起始和终止BreakPoint的相反方向的LaneNumChange
        /// </summary>
        /// <param name="fromBreakPointId"></param>
        /// <param name="toBreakPointId"></param>
        /// <returns></returns>
        public LaneNumChange GetOppositeDirectionLaneNumChange(int fromBreakPointId, int toBreakPointId)
        {
            string sql = "Select * from " + LaneNumChange.LaneNumChangeName +
                " where " + LaneNumChange.FromBreakPointID_Name + " = " + toBreakPointId +
                " and  " + LaneNumChange.ToBreakPointID_Name + " = " + fromBreakPointId + 
                " and  " + LaneNumChange.DoneFlag_Name + " = " + LaneNumChange.DONEFLAG_UNDO ;
            OleDbCommand cmd = new OleDbCommand(sql, _conn);
            OleDbDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int laneNum = Convert.ToInt32(reader[LaneNumChange.LaneNum_Name]);
                int done = Convert.ToInt32(reader[LaneNumChange.DoneFlag_Name]);

                LaneNumChange laneNumChange = new LaneNumChange();
                laneNumChange.FromBreakPointID = fromBreakPointId;
                laneNumChange.ToBreakPointID = toBreakPointId;
                laneNumChange.LaneNum = laneNum;
                laneNumChange.DoneFlag = done;


                reader.Close();
                reader.Dispose();
                return laneNumChange;
            }
            else
            {
                reader.Close();
                reader.Dispose();

                return null;
            }

        }
    }
}
